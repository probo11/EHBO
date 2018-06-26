#include <SPI.h>
#include <Ethernet.h>
#include <Servo.h>

#define unitCode 26604430

byte mac[] = {0x40, 0x6c, 0x8f, 0x36, 0x84, 0x8a};
IPAddress ip(192, 168, 1, 3);
EthernetServer server(5007);
bool connected = false;

Servo servo1;
const byte trigPin = 6, echoPin = 7, servoPin = 8;
byte barrierState = 0;
long duration;
int currentValUS;
bool servoVal = false;

void setup() 
{
    servo1.attach(servoPin);
    Serial.begin(9600);
    
    Ethernet.begin(mac, ip);
    Serial.print("Listening on adres: "); Serial.println(Ethernet.localIP());
    
    server.begin();

    pinMode(trigPin, OUTPUT);
    pinMode(servoPin, OUTPUT);
    pinMode(echoPin, INPUT);
    
    connected = true;
}

void loop() 
{
  int tempValUS = readDistance(trigPin, echoPin);
  
  delay(500);
  
  Serial.println(tempValUS);
    
  if (tempValUS >= 5 && tempValUS <= 20){
    servo1.write(getServoValue(90));
  }
    
  /*else{
    servo1.write(getServoValue(0));
  }*/
    

  if(!connected) return;

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
      
      Serial.println(buffer);
      String Test = String(buffer);
      Serial.println(Test[0]);
      
      switch(Test[0])
      {
        // Open/close barrier
        case 's':
          if(barrierState == 1){
            servo1.write(0);
            barrierState = 0;
          }else{
            servo1.write(90);
            barrierState = 1;
          }
          
          client.write('o');
          delay(500);
         break;

        // Read sensor values
        case 'v':
          int value1 = readDistance(trigPin, echoPin);
          client.write('v');
        break;
      } 
    }
  }
  
  delay(500);
}

int getServoValue(int v)
{
  servoVal = !servoVal;
  
  Serial.println(servoVal);
  
  if (v == 90)
    return v;
  else if (v == 0)
    return v;
  else
    return (servoVal == true) ? 90 : 0;
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
  
  // If bigger than 200 cm return -1 
  if (dist >= 200) 
    return -1;
  else 
    return dist;
}