#include <Arduino.h>
#include <WiFi.h>
#include <PubSubClient.h>

// Credentials left out for obvious security reasons
WiFiClient wifiClient;
const char ssid[] = "ssid";
const char pass[] = "secret";

PubSubClient mqttClient(wifiClient);
const char brokerIp[] = "ip";
const int brokerPort = 1883;
const char brokerUser[] = "user";
const char brokerPass[] = "pass";

// ===== Function Declarations =====

void messageCallback(char *topic, byte *payload, unsigned int length);
void connectToMqttBroker();

void setup()
{
  Serial.begin(9600);

  // WiFi config and start
  Serial.printf("Attempting to connect to WiFi network: %s.\n", ssid);
  while (WiFi.status() != WL_CONNECTED)
  {
    Serial.print(".");
    WiFi.begin(ssid, pass);

    delay(1000);
  }
  Serial.println("\nConnection established.");
  Serial.print("IP: ");
  Serial.println(WiFi.localIP());
  Serial.printf("RSSI: %d dBm\n", WiFi.RSSI());

  // MQTT config
  mqttClient.setServer(brokerIp, brokerPort);
  mqttClient.setCallback(messageCallback);
  mqttClient.setKeepAlive(60);
}

void loop()
{
  if (!mqttClient.connected())
  {
    connectToMqttBroker();
  }
  mqttClient.loop();
}

// ===== Function Definitions =====

void messageCallback(char *topic, byte *payload, unsigned int length)
{
  Serial.printf("Message received on topic: %s with length %d.\n", topic, length);
}

void connectToMqttBroker()
{
  while (!mqttClient.connected())
  {
    Serial.printf("Attempting to connect to broker: %s.\n", brokerIp);

    if (mqttClient.connect("Random_generated_value_standin_fornow", brokerUser, brokerPass))
    {
      Serial.println("Connected to MQTT broker.");

      // Subscribe to topics
    }
    else
    {
      Serial.printf("Failed to connect. RC=%d\n", mqttClient.state());
      Serial.println("Try again in 5 seconds.");
      delay(5000);
    }
  }
}