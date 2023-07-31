#include <Arduino.h>
#include <WiFi.h>
#include <PubSubClient.h>

// Credentials left out for obvious security reasons
WiFiClient wifiClient;
const char ssid[] = "ssid";
const char pass[] = "pass";

PubSubClient mqttClient(wifiClient);
const char brokerIp[] = "192.168.0.182";
const int brokerPort = 1883;
const char brokerUser[] = "andreas";
const char brokerPass[] = "pass";

// For initial communication test
const char testTopic[] = "flashTest";
const String testMessage = "FLASH";
const int ledPin = 2;

// ===== Function Declarations =====

void messageCallback(char *topic, byte *payload, unsigned int length);
void connectToMqttBroker();

void setup()
{
  Serial.begin(9600);

  // For initial communication test
  pinMode(ledPin, OUTPUT);

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

  String receivedTopic(topic);
  String message("X", length);
  for (unsigned int i = 0; i < length; i++)
  {
    message.setCharAt(i, (char)payload[i]);
  }

  if (receivedTopic == String(testTopic))
  {
    Serial.println("Correct topic");
    digitalWrite(ledPin, HIGH);
    delay(200);
    digitalWrite(ledPin, LOW);
    delay(200);

    if (message == testMessage)
    {
      Serial.println("Correct payload");
      digitalWrite(ledPin, HIGH);
      delay(200);
      digitalWrite(ledPin, LOW);
    }
    else
    {
      Serial.println("Unknown payload.");
    }
  }
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
      mqttClient.subscribe(testTopic);
    }
    else
    {
      Serial.printf("Failed to connect. RC=%d\n", mqttClient.state());
      Serial.println("Try again in 5 seconds.");
      delay(5000);
    }
  }
}