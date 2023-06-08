// Includes for MPU6050 and Bluetooth
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
// #include "BluetoothSerial.h"
#include <WiFi.h>
#define LED_PIN 2 

Adafruit_MPU6050 mpu;
// BluetoothSerial SerialBT;
const char *ssid = "12345678";  // Enter your WiFi Name
const char *pass = "12345678";  // Enter your WiFi Password
WiFiServer server(80);

float angleX = 0;
float angleY = 0;
float angleZ = 0;
const int buttonPin = 4;

void setup(void) {
  Serial.begin(115200);
  pinMode(LED_PIN, OUTPUT);
  // SerialBT.begin("ESP32test");


  while (!Serial)
    delay(10);
  Serial.println("Adafruit MPU6050 test!");

  // Try to initialize!
  if (!mpu.begin()) {
    Serial.println("Failed to find MPU6050 chip");
    while (1) {
      delay(10);
    }
  }
  Serial.println("MPU6050 Found!");
  mpu.setGyroRange(MPU6050_RANGE_250_DEG);

  Serial.println("");

  // WIFI
  Serial.println("Connecting to ");
  Serial.println(ssid);
  WiFi.begin(ssid, pass);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");  // print ... till not connected
  }
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address is : ");
  Serial.println(WiFi.localIP());
  server.begin();
  digitalWrite(LED_PIN, HIGH);  
  delay(200);                  
  digitalWrite(LED_PIN, LOW);   
  delay(200);
  digitalWrite(LED_PIN, HIGH);  
  delay(200);                  
  digitalWrite(LED_PIN, LOW);   
  


  delay(100);
}

void loop() {
  /* Get new sensor events with the readings */
  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  if (g.gyro.x > 0.1 || g.gyro.x < -0.1) {
    angleX += g.gyro.x * 57.295779513 * 0.02;
  }
  if (g.gyro.y > 0.1 || g.gyro.y < -0.1) {
    angleY += g.gyro.y * 57.295779513 * 0.02;
  }
  if (g.gyro.z > 0.1 || g.gyro.z < -0.1) {
    angleZ += g.gyro.z * 57.295779513 * 0.02;
  }

  int buttonState = digitalRead(buttonPin);


  // Serial.print("Rotation X: ");
  // Serial.print(angleX);

  // Create data string
  String data = String(angleZ) + String(",") + String(angleY) + String(",") + String(buttonState);
  Serial.println(data);

  // Send data over Bluetooth
  //SerialBT.println(data);

  WiFiClient client = server.available();
  if (client) {
    Serial.println("new client");
    String currentLine = "";  //Storing the incoming data in the string
    while (client.connected()) {
      if (client.available())  //if there is some client data available
      {
        char c = client.read();  // read a byte
        if (c == '\n')           // check for newline character,
        {
          if (currentLine.length() == 0)  //if line is blank it means its the end of the client HTTP request
          {
            client.println("HTTP/1.1 200 OK");          // Start of HTTP response
            client.println("Content-Type: text/html");  // The type of data that's being returned
            client.println("Connection: close");        // Signal that all data has been sent
            client.println();                           // blank line before sending HTML
            client.println("<html><title> ESP32 WebServer</title></html>");
            client.println(data);
            break;  // break out of the while loop:
          } else {  // if you got a newline, then clear currentLine:
            currentLine = "";
          }
        } else if (c != '\r') {  // if you got anything else but a carriage return character,
          currentLine += c;      // add it to the end of the currentLine
        }
      }
    }
  }


  delay(20);
}