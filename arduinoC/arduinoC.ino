#include <SPI.h>
#include <Ethernet.h>
#include <Servo.h>

#define unitCode 26604430

byte mac[] = {0x40, 0x6c, 0x8f, 0x36, 0x84, 0x8a};
IPAddress ip(192, 168, 1, 8);
EthernetServer server(5007);
bool connected = false;

Servo servo1;
const byte trigPin = 4, echoPin = 5, servoPin = 6;
bool barrierSwitch = false;
byte barrierState = 1;
long duration;

void setup() 
{
    Serial.begin(9600);
    Ethernet.begin(mac, ip);
    Serial.print("Listening on adres: "); Serial.println(Ethernet.localIP());
    
    server.begin();

    pinMode(trigPin, OUTPUT);
    pinMode(servoPin, OUTPUT);
    pinMode(echoPin, INPUT);
    servo1.attach(servoPin);
    connected = true;
    servo1.write(90);
    
    // Start up
    delay(500);
}

void loop() 
{
    int tempValUS = readDistance(trigPin, echoPin);

    if (tempValUS >= 20 || tempValUS == -1)
    {
      servo1.write(90);
      barrierState = 1;

      Serial.println("ik ben open");
      Serial.println("\n");
      

      
    }else if(tempValUS < 20 && tempValUS != -1)
    {
      servo1.write(0);
      barrierState = 0;
      Serial.println("ik ben dicht");
      Serial.println("\n");
    }
    
    
  Serial.println(tempValUS);
      Serial.println("\n");
    
    
    
    if(!connected) return;

    //Serial.print("Distance = "); Serial.println(tempValUS);

    delay(100);

    EthernetClient client = server.available();
    
    if(!client) return;
    
    while(client.connected())
    {
      char buffer[128];
      int count = 0;
      
      while(client.available())
      {
        buffer[count++] = client.read();
      }
      
      buffer[count] = 0;
      
      if(count > 0)
      {
        String Test = String(buffer);
        
        switch(Test[0])
        {
          // Open/close barrier
          case 's':
            if(barrierState == 1)
            {
              servo1.write(0);
              barrierState = 0;
              client.print(barrierState);
              delay(5000);
            }
           break;

          // Read sensor values
          case 'v':
            client.print(readDistance(trigPin, echoPin));
            client.print(">"); 
            client.print(barrierState);
          break;
        } 
      }
    }

    delay(500);
}

int readDistance(byte trig, byte echo)
{
  // The sensor is triggered by a HIGH pulse of 10 or more microseconds.
  // Give a short LOW pulse beforehand to ensure a clean HIGH pulse:
  digitalWrite(trig, LOW);
  delayMicroseconds(5);
  digitalWrite(trig, HIGH);
  delayMicroseconds(10);
  digitalWrite(trig, LOW);
  
  // Reads the echoPin, returns the sound wave travel time in microseconds
  duration = pulseIn(echo, HIGH);
  
  // Calculating the distance
  int dist = duration * 0.034/2;
  
  // If bigger than 100 cm return -1 
  if (dist >= 200) 
    return -1;
  else 
    return dist;
}

