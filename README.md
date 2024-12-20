# PROJECT LINKS
- Wokwi Project: https://wokwi.com/projects/416369623457261569
- HiveMQ: https://www.hivemq.com/
- Backend: https://localhost:7205/swagger
- Frontend: http://localhost:4200
- Database: <no_link>

# HIVEMQ CLUSTER CONNECTIONS
WOKWI (PUBLISH)
- USERNAME: WokwiUser <br/>
- PASSWORD: WokwiPassw0rd

HIVEMQ (PUBLISH-SUBSCRIBE)
- USERNAME: HiveMQUser <br/>
- PASSWORD: HiveMQPassw0rd

SERVER (SUBSCRIBE)
- USERNAME: ServerUser <br/>
- PASSWORD: ServerPassw0rd

FRONT (SUBSCRIBE)
- USERNAME: FrontUser <br/>
- PASSWORD: FrontPassw0rd

# TOPICS
- NAME: topic_sensor_loadcell

# RUN APPLICATIONS
- Wokwi: access the website and run the project manually by clicking the start button
- Backend: dotnet run --project <path_to_strengthtrack.api>
- Frontend: ng serve --open

# ABOUT PROJECT
## App Name
StrengthTrack.

## Explanation
This solution automates the capture, processing and visualization of data, allowing the professional to focus exclusively on patient care, while the system organizes the information efficiently.

## Architecture
The solution architecture was divided into 6 (six) modules:
- HX711 Conversion Module (load cell): responsible for measuring the force applied by patients.
- ESP32 (microcontroller): responsible for obtaining sensor data and publishing it to a MQTT topic using Wi-Fi connection.
- HiveMQ (MQTT Broker): responsible for managing the exchange of messages between connected devices.
- Client Application (Angular): A web system that offers an user-friendly interface for the professional use to take measurements and view session results. It also provides a dashboard for monitor progress throughout the sessions.
- Backend (developed using C# .NET): responsible for processing the received data, validating the measurements and persisting the information in the database.
- Database (SQL Server): responsible for storing the relevant systema data, including patients, measurements and sessions histories.

## Information Flow
- The HX711 sensor detects the force applied by the patient. 
- The ESP32 obtains this information and publishes it to a specific MQTT topic managed by HiveMQ. 
- The Angular client and the server subscribe to the same topic where the ESP32 publishes the information, to receive the data in real time. 
- The client displays the measurement results to the professional and the patient, while the server performs the calculations and store them in SQL Server.

## Photos
![App](https://github.com/user-attachments/assets/4ebd1b62-2870-44bd-8001-6aab519456fa)
![Wokwi](https://github.com/user-attachments/assets/c0ce24dc-90da-4733-b04f-895ab4b48daf)
![App2](https://github.com/user-attachments/assets/3ecbba4e-ad90-4069-83ca-a909b74388c4)
![App3](https://github.com/user-attachments/assets/165516e7-ea47-44d1-8782-f009ac117759)
