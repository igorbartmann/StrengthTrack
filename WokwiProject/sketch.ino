// External Libraries.
#include "HX711.h"
#include <LiquidCrystal_I2C.h>
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>

// Libraries Objects.
HX711 loadcell;
LiquidCrystal_I2C lcd(0x27,16,2);

WiFiClientSecure esp_client;
PubSubClient MQTT(esp_client);

// WI-FI (login) Variables.
const char *WIFI_SSID = "Wokwi-GUEST";
const char *WIFI_PASSWORD = "";

// Broker MQTT (HiveMQ) Variables.
const char *BROKER_MQTT_URL = "acd37fac47394e56a264fd86e4cb8366.s1.eu.hivemq.cloud";
#define BROKER_MQTT_PORT 8883
#define BROKER_MQTT_CLIENT_ID "WOKWI"
#define BROKER_MQTT_CLIENT_USER "WokwiUser"
#define BROKER_MQTT_CLIENT_PASSWORD "WokwiPassw0rd"
#define BROKER_MQTT_TOPIC_PUBLISH_LOAD "topic_sensor_loadcell"

static const char *root_ca PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIIFazCCA1OgAwIBAgIRAIIQz7DSQONZRGPgu2OCiwAwDQYJKoZIhvcNAQELBQAw
TzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2Vh
cmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwHhcNMTUwNjA0MTEwNDM4
WhcNMzUwNjA0MTEwNDM4WjBPMQswCQYDVQQGEwJVUzEpMCcGA1UEChMgSW50ZXJu
ZXQgU2VjdXJpdHkgUmVzZWFyY2ggR3JvdXAxFTATBgNVBAMTDElTUkcgUm9vdCBY
MTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAK3oJHP0FDfzm54rVygc
h77ct984kIxuPOZXoHj3dcKi/vVqbvYATyjb3miGbESTtrFj/RQSa78f0uoxmyF+
0TM8ukj13Xnfs7j/EvEhmkvBioZxaUpmZmyPfjxwv60pIgbz5MDmgK7iS4+3mX6U
A5/TR5d8mUgjU+g4rk8Kb4Mu0UlXjIB0ttov0DiNewNwIRt18jA8+o+u3dpjq+sW
T8KOEUt+zwvo/7V3LvSye0rgTBIlDHCNAymg4VMk7BPZ7hm/ELNKjD+Jo2FR3qyH
B5T0Y3HsLuJvW5iB4YlcNHlsdu87kGJ55tukmi8mxdAQ4Q7e2RCOFvu396j3x+UC
B5iPNgiV5+I3lg02dZ77DnKxHZu8A/lJBdiB3QW0KtZB6awBdpUKD9jf1b0SHzUv
KBds0pjBqAlkd25HN7rOrFleaJ1/ctaJxQZBKT5ZPt0m9STJEadao0xAH0ahmbWn
OlFuhjuefXKnEgV4We0+UXgVCwOPjdAvBbI+e0ocS3MFEvzG6uBQE3xDk3SzynTn
jh8BCNAw1FtxNrQHusEwMFxIt4I7mKZ9YIqioymCzLq9gwQbooMDQaHWBfEbwrbw
qHyGO0aoSCqI3Haadr8faqU9GY/rOPNk3sgrDQoo//fb4hVC1CLQJ13hef4Y53CI
rU7m2Ys6xt0nUW7/vGT1M0NPAgMBAAGjQjBAMA4GA1UdDwEB/wQEAwIBBjAPBgNV
HRMBAf8EBTADAQH/MB0GA1UdDgQWBBR5tFnme7bl5AFzgAiIyBpY9umbbjANBgkq
hkiG9w0BAQsFAAOCAgEAVR9YqbyyqFDQDLHYGmkgJykIrGF1XIpu+ILlaS/V9lZL
ubhzEFnTIZd+50xx+7LSYK05qAvqFyFWhfFQDlnrzuBZ6brJFe+GnY+EgPbk6ZGQ
3BebYhtF8GaV0nxvwuo77x/Py9auJ/GpsMiu/X1+mvoiBOv/2X/qkSsisRcOj/KK
NFtY2PwByVS5uCbMiogziUwthDyC3+6WVwW6LLv3xLfHTjuCvjHIInNzktHCgKQ5
ORAzI4JMPJ+GslWYHb4phowim57iaztXOoJwTdwJx4nLCgdNbOhdjsnvzqvHu7Ur
TkXWStAmzOVyyghqpZXjFaH3pO3JLF+l+/+sKAIuvtd7u+Nxe5AW0wdeRlN8NwdC
jNPElpzVmbUq4JUagEiuTDkHzsxHpFKVK7q4+63SM1N95R1NbdWhscdCb+ZAJzVc
oyi3B43njTOQ5yOf+1CceWxG1bQVs5ZufpsMljq4Ui0/1lvh+wjChP4kqKOJ2qxq
4RgqsahDYVvTH9w7jXbyLeiNdd8XM2w9U/t7y0Ff/9yi0GE44Za4rF2LN9d11TPA
mRGunUHBcnWEvgJBQl9nJEiU0Zsnvgc/ubhPgXRR4Xq37Z0j4r7g1SgEEzwxA57d
emyPxgcYxn/eR44/KJ4EBs+lVDR3veyJm+kXQ99b21/+jh5Xos1AnX5iItreGCc=
-----END CERTIFICATE-----
)EOF";

// Global Variables.
int currentValue = -1;
int lastValue = -1;
int force = -1;

// Method to initialize serial.
void init_serial() {
  Serial.println("Initializing Serial...");
}

// Method to initialize lcd.
void init_lcd() {
  Serial.println("LCD Initializing...");

  lcd.init();
  lcd.backlight();
  lcd.setCursor(0,0);
  lcd.print("Initializing...");
  delay(2000);

  Serial.println("LCD Read To Operate!");
}

// Method to update serial message.
void update_serial(int p_force) {
  Serial.print("Measured force: ");
  Serial.print(p_force);
  Serial.println(".");
}

// Method to update lcd message.
void update_lcd(int p_force) {
  lcd.clear();
  delay(100);

  lcd.setCursor(0,0);
  lcd.print("Measured force:");

  lcd.setCursor(0,1);
  lcd.print(p_force);
  lcd.print("g");
}

// Method to publish force in topic.
void publish_force(int p_force) {
    check_wifi_and_mqtt_connection();

    static char strForce[10] = {0};
    sprintf(strForce, "%d", force);

    MQTT.publish(BROKER_MQTT_TOPIC_PUBLISH_LOAD, strForce);
    MQTT.loop();

    Serial.print("Force \"");  
    Serial.print(strForce);  
    Serial.print("\" published to \"");
    Serial.print(BROKER_MQTT_TOPIC_PUBLISH_LOAD);
    Serial.println("\" topic!");  
}

// Method to verify loadcell value and update serial and lcd.
void verify_loadcell_and_update_serial_and_lcd() {
  currentValue = loadcell.get_units(), 1;
  if (currentValue != lastValue) {
    lastValue = currentValue;

    force = map(currentValue, 0, 21000, 0, 50000);
    update_serial(force);
    update_lcd(force);

    if (force > 0) {
      publish_force(force);
    }
  }
}

// Method to connect wifi
void connect_wifi()
{
  if (WiFi.status() == WL_CONNECTED) {
    return;
  }

  Serial.println("");  
  Serial.println("------Conexao WI-FI------");
  Serial.print("Conectando-se na rede: ");
  Serial.println(WIFI_SSID);
  Serial.println("Aguarde");

  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(100);
  }

  Serial.println();
  Serial.print("Conectado com sucesso na rede: ");
  Serial.println(WIFI_SSID);
  Serial.print("IP obtido: ");
  Serial.println(WiFi.localIP());
}

// Method to connect MQTT
void connect_mqtt()
{
  if (MQTT.connected()) {
    return;
  }
  
  esp_client.setCACert(root_ca);
  MQTT.setServer(BROKER_MQTT_URL, BROKER_MQTT_PORT);

  while (!MQTT.connected()) {
    Serial.print("* Tentando se conectar ao Broker MQTT: ");
    Serial.println(BROKER_MQTT_URL);

    if (MQTT.connect(BROKER_MQTT_CLIENT_ID, BROKER_MQTT_CLIENT_USER, BROKER_MQTT_CLIENT_PASSWORD)) {
      Serial.println("Conectado com sucesso ao broker MQTT!");
    } else {
      Serial.println("Falha ao reconectar-se ao broker MQTT! Nova tentativa de conexao em 2 segundos.");
      delay(2000);
    }
  }
}

// Method to guarantee wifi and mqtt connection.
void check_wifi_and_mqtt_connection()
{
  connect_wifi();
  connect_mqtt();
}

// Default setup method.
void setup() {
  Serial.begin(115200);
  loadcell.begin(13, 14);

  init_serial();
  init_lcd(); 

  check_wifi_and_mqtt_connection();
}

// Default loop method.
void loop() {
  verify_loadcell_and_update_serial_and_lcd();
  delay(250);
}
