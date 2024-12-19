import { Component, OnInit, ViewChild } from '@angular/core';
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule, MatOption } from '@angular/material/autocomplete';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { map, Observable, Observer, of, startWith } from 'rxjs';

import { BaseChartDirective } from 'ng2-charts';
import { ChartData, ChartType } from 'chart.js';

import { API_URL } from './environment/env.prod';
import { ClientInputModel, ClientViewModel, ClientSimpleViewModel, Session } from './interfaces/Client';
import { AverageOfClientsViewModel } from './interfaces/AverageOfClientsViewModel';
import { FormControlSetToClient } from './interfaces/FormControlSetToClient';
import { SelectOption } from './interfaces/selectOption';
import { Measure } from './interfaces/measure';

import mqtt, { MqttClient } from 'mqtt';

const BROKER_URL: string = 'wss://acd37fac47394e56a264fd86e4cb8366.s1.eu.hivemq.cloud:8884/mqtt';
const BROKER_USER_CLIEND_ID: string = 'FrontClient'
const BROKER_USER_NAME: string = 'FrontUser';
const BROKER_USER_PASSWORD: string = 'FrontPassw0rd';
const BROKER_SUBSCRIBE_TOPIC: string = 'topic_sensor_loadcell';
const MQTT_CONNECT_EVENT = "connect"
const MQTT_RECEIVE_MESSAGE_EVENT = "message"

@Component({
  selector: 'sta-root',
  standalone: true,
  imports: [
    RouterOutlet, 
    CommonModule, 
    MatTabsModule, 
    MatTableModule, 
    MatIconModule, 
    FormsModule, 
    MatFormFieldModule,
    MatInputModule, 
    MatAutocompleteModule, 
    ReactiveFormsModule, 
    AsyncPipe, 
    MatProgressBarModule,
    BaseChartDirective
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  @ViewChild(BaseChartDirective) chart!: BaseChartDirective;

  dashboardAverages!: AverageOfClientsViewModel;
  dashboardDataSource: ClientSimpleViewModel[] = [];
  dashboardDisplayedColumns: string[] = ['name', 'cpf', 'action' ];
  
  isClientSessionsInformationModalOpen: boolean = false;
  clientSessionsDataSource: Session[] = [];
  clientSessionsDisplayedColumns: string[] = ['number', 'date', 'averageResult', 'results' ];
  
  clientSessionsChartType: ChartType = 'line';
  clientSessionsChartLegend: boolean = true;
  clientSessionsChartData: ChartData<'line'> = {
    labels: [],
    datasets: [
      {
        label: 'Força Média Ao Longo Das Sessões',
        data: [],
        borderColor: '#36eb4e',
        backgroundColor: '#9ef59b',
      }
    ]
  };

  isClientInformationModalOpen: boolean = false;
  modalClientInformation!: FormControlSetToClient;

  searchClientControl: FormControl = new FormControl('');
  searchClientOptions: SelectOption[] = [];
  searchClientFilteredOptions!: Observable<SelectOption[]>;

  isMeasuring: boolean = false;
  measureDataSource: MatTableDataSource<Measure> = new MatTableDataSource<Measure>();
  measureDisplayedColumns: string[] = ['number', 'force' ];

  mqttClient: MqttClient|null = null;

  constructor(private httpClient: HttpClient) {}
  
  ngOnInit(): void {
    this.modalClientInformation = {
      clientId: new FormControl(0),
      clientName: new FormControl(''),
      clientCpf: new FormControl('')
    };

    this.searchClientFilteredOptions = this.searchClientControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterClient(value || '')),
    );
    
    this.loadDashboard();
  }

  openClientSessionsInformationModal(id: number): void {
    this.loadClientById(id);

    this.isClientSessionsInformationModalOpen = true;
  }

  closeClientSessionsInformationModal(): void {
    this.isClientSessionsInformationModalOpen = false;
  }

  onSelectClient(htmlOption: MatOption) {
    this.searchClientControl.setValue(this.searchClientOptions.find(o => o.id == htmlOption.value)?.value);
  }

  createClient(): void {
    this.openClientInformationModal();
  }

  editClient(): void {
    let clientId = this.searchClientOptions.find(c => c.value == this.searchClientControl.value)?.id ?? 0;
    let clientSelected = this.dashboardDataSource.find(c => c.id == clientId);

    if (!!clientSelected) {
      this.modalClientInformation.clientId.setValue(clientSelected.id);
      this.modalClientInformation.clientName.setValue(clientSelected.name);
      this.modalClientInformation.clientCpf.setValue(clientSelected.cpf);

      this.openClientInformationModal();
    } else {
      alert ("Erro, não foi possível obter os dados do cliente selecionado!");
    }
  }

  saveClient() {
    let id = this.modalClientInformation.clientId.value;
    let name = this.modalClientInformation.clientName.value;
    let cpf = this.modalClientInformation.clientCpf.value;

    let client: ClientInputModel = {
      name,
      cpf
    };

    let isNewClient = !id || id == 0;
    if (isNewClient) {
      this.httpClient.post(API_URL, client).subscribe(this.getActionAfterPostOrPutClient());
    } else {
      this.httpClient.put(`${API_URL}/${id}`, client).subscribe(this.getActionAfterPostOrPutClient());
    }
  }

  openClientInformationModal(): void {
    this.isClientInformationModalOpen = true;
  }

  closeClientInformationModal(): void {
    this.clearClientInformationModalFields();
    this.isClientInformationModalOpen = false;
  }

  startMeasure(): void {
    let clientId = this.getSelectedClientId();

    if (!clientId) {
      alert("Erro, não foi possível obter os dados do cliente selecionado!");
      return;
    }

    this.httpClient.post(`${API_URL}/StartMeasurement/${clientId}`, null, {responseType: 'text'}).subscribe({
      next: (response) => {
        this.connectMqtt();
        this.isMeasuring = true;
      },
      error: (response) => {
        alert(response.error);
      }
    });
  }

  stopMeasure(): void {
    let clientId = this.getSelectedClientId();

    if (!clientId) {
      alert("Erro, não foi possível obter os dados do cliente selecionado!");
      return;
    }

    this.httpClient.post(`${API_URL}/StopMeasurement/${clientId}`, null, {responseType: 'text'}).subscribe({
      next: (response) => {
        this.disconnectMqtt();
        this.isMeasuring = false;

        this.loadDashboard();
      },
      error: (response) => {
        alert(response.error);
      }
    });
  }

  private loadDashboard(): void {
    this.loadDashboardAverages();
    this.loadClients();
  }

  private loadDashboardAverages(): void {
    this.httpClient.get(`${API_URL}/AverageOfClients`).subscribe({
      next: (response) => {
        this.dashboardAverages = response as AverageOfClientsViewModel;
      }
    });
  }

  private loadClients(): void {
    this.httpClient.get(API_URL).subscribe({
      next: (response) => {
        let clients = response as ClientSimpleViewModel[];
        this.dashboardDataSource = clients;

        this.searchClientOptions = [];
        clients.forEach(c => {
          this.searchClientOptions.push({ 
            id: c.id, 
            value: `${c.name} (${c.cpf})`
          });
        });
      }
    });
  }

  private loadClientById(id: number): void {
    this.httpClient.get(`${API_URL}/${id}`).subscribe({
      next: (response) => {
        let client = response as ClientViewModel;
        
        let graphicLabels: number[] = [];
        let grapichData: number[] = [];
        let clientSessions: Session[] = [];

        client.sessions.forEach(s => {
          graphicLabels.push(s.number);
          grapichData.push(s.averageResult);
          
          clientSessions.push({
            id: s.id, 
            date: `${s.date.toString().split("T")[0]} ${s.date.toString().split("T")[1].split("+")[0]}`, 
            number: s.number, 
            averageResult: s.averageResult, 
            results: s.results.map(r => r.value).join(' - ')})
        });

        this.updateGraphic(graphicLabels, grapichData);

        this.clientSessionsDataSource = clientSessions;
      }
    });
  }

  private updateGraphic(labels: number[], data: number[]): void {
    this.clientSessionsChartData.labels = labels;
    this.clientSessionsChartData.datasets[0].data = data;

    this.chart?.chart?.update();
  }

  private getActionAfterPostOrPutClient(): Partial<Observer<Object>> {
    let partialObserver = {
      next: (response: any) => {
        this.closeClientInformationModal();

        alert("The client was added successfully");
        this.loadDashboard();        

        let clientSaved = response as ClientSimpleViewModel;
        this.searchClientControl.setValue(`${clientSaved.name} (${clientSaved.cpf})`);
      },
      error: (response: any) => {
        alert(`Error to add client: "${response.error}"`);
      }
    };

    return partialObserver;
  }

  private getSelectedClientId(): number {
    return this.searchClientOptions.find(c => c.value == this.searchClientControl.value)?.id ?? 0;
  }

  private filterClient(value: any): SelectOption[] {
    if (!isNaN(value)) {
      return this.searchClientOptions;
    }

    this.clearMeasures();

    const filterValue = value.toLowerCase();
    return this.searchClientOptions.filter(option => option.value.toLowerCase().includes(filterValue));
  }  

  private clearClientInformationModalFields() {
    this.modalClientInformation.clientId.reset();
    this.modalClientInformation.clientName.reset();
    this.modalClientInformation.clientCpf.reset();
  }

  private clearMeasures(): void {
    if (this.measureDataSource.data.length == 0) {
      return;
    }

    this.measureDataSource.data = [];
  }

  private connectMqtt(): void {
    if (this.mqttClient && this.mqttClient.connected) {
      return;
    }

    this.mqttClient = mqtt.connect(BROKER_URL, {
      clientId: BROKER_USER_CLIEND_ID,
      username: BROKER_USER_NAME,
      password: BROKER_USER_PASSWORD
    });

    this.mqttClient.on(MQTT_CONNECT_EVENT, () => {
      this.mqttClient?.subscribe(`${BROKER_SUBSCRIBE_TOPIC}/#`);
    });    

    this.clearMeasures();
    this.mqttClient.on(MQTT_RECEIVE_MESSAGE_EVENT, (topic, message) => {
      let tempData = this.measureDataSource.data;
      tempData.push({
        number: tempData.length + 1, 
        force: message.toString()
      });

      this.measureDataSource.data = tempData;
    });
  }

  private disconnectMqtt(): void {
    if (!this.mqttClient || this.mqttClient.disconnected) {
      return;
    }

    this.mqttClient.unsubscribe(BROKER_SUBSCRIBE_TOPIC);
    this.mqttClient.end();
  }
}
