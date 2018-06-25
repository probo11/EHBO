// Arduino Domotica server with Klik-Aan-Klik-Uit-controller 
//
// By Sibbele Oosterhaven, Computer Science NHL, Leeuwarden
// V1.2, 16/12/2016, published on BB. Works with Xamarin (App: Domotica)
//
// Hardware: Arduino Uno, Ethernet shield W5100; RF transmitter on RFpin; debug LED for serverconnection on ledPin
// The Ethernet shield uses pin 10, 11, 12 and 13
// Use Ethernet2.h libary with the (new) Ethernet board, model 2
// IP address of server is based on DHCP. No fallback to static IP; use a wireless router
// Arduino server and smartphone should be in the same network segment (192.168.1.x)
// 
// Supported kaku-devices
// https://eeo.tweakblogs.net/blog/11058/action-klik-aan-klik-uit-modulen (model left)
// kaku Action device, old model (with dipswitches); system code = 31, device = 'A' 
// system code = 31, device = 'A' true/false
// system code = 31, device = 'B' true/false
//
// // https://eeo.tweakblogs.net/blog/11058/action-klik-aan-klik-uit-modulen (model right)
// Based on https://github.com/evothings/evothings-examples/blob/master/resources/arduino/arduinoethernet/arduinoethernet.ino.
// kaku, Action, new model, codes based on Arduino -> Voorbeelden -> RCsw-2-> ReceiveDemo_Simple
//   on      off       
// 1 2210415 2210414   replace with your own codes
// 2 2210413 2210412
// 3 2210411 2210410
// 4 2210407 2210406
//
// https://github.com/hjgode/homewatch/blob/master/arduino/libraries/NewRemoteSwitch/README.TXT
// kaku, Gamma, APA3, codes based on Arduino -> Voorbeelden -> NewRemoteSwitch -> ShowReceivedCode
// 1 Addr 25542022 unit 0 on/off, period: 270us   replace with your own code
// 2 Addr 25542022 unit 1 on/off, period: 270us
// 3 Addr 25542022 unit 2 on/off, period: 270us

// Supported KaKu devices -> find, download en install corresponding libraries
#define unitCodeApa3      25542022  // replace with your own code
//#define unitCodeActionOld 31        // replace with your own code
//#define unitCodeActionNew 2210406   // replace with your own code

// Include files.
#include <SPI.h>                  // Ethernet shield uses SPI-interface
#include <Ethernet.h>             // Ethernet library (use Ethernet2.h for new ethernet shield v2)
#include <NewRemoteTransmitter.h> // Remote Control, Gamma, APA3

#include <SimpleTimer.h>

// the timer object
SimpleTimer timer;

// Set Ethernet Shield MAC address  (check yours)
byte mac[] = { 0x40, 0x6c, 0x8f, 0x36, 0x84, 0x8a }; // Ethernet adapter shield S. Oosterhaven
int ethPort = 3300;                                  // Take a free port (check your router)

#define RFPin        3  // output, pin to control the RF-sender (and Click-On Click-Off-device)
#define switchPin    7  // input, connected to some kind of inputswitch
#define ledPin       2  // output, led used for "connect state": blinking = searching; continuously = connected
#define infoPin      4  // output, more information
#define analogPin    0  // sensor value

EthernetServer server(ethPort);              // EthernetServer instance (listening on port <ethPort>).
NewRemoteTransmitter apa3Transmitter(unitCodeApa3, RFPin, 260, 3);  // APA3 (Gamma) remote, use pin <RFPin> 
//ActionTransmitter actionTransmitter(RFPin);  // Remote Control, Action, old model (Impulse), use pin <RFPin>
//RCSwitch mySwitch = RCSwitch();            // Remote Control, Action, new model (on-off), use pin <RFPin>

char actionDevice = 'A';                 // Variable to store Action Device id ('A', 'B', 'C')
bool pinState = false;                   // Variable to store actual pin state
bool pinChange = false;                  // Variable to store actual pin change
int  sensorValue = 0;                    // Variable to store actual sensor value
int kaku_unit = 0;                       //kaku unit nummer
String bufferGlobal = "";
bool OK = false;
int threshold = 200;
bool opdr5 = false;

int sensorReadInterval = 500;

int WaterSensor = 0; //Watersensor variable
bool GenoegWater;

void setup()
{
   Serial.begin(9600);
   //while (!Serial) { ; }               // Wait for serial port to connect. Needed for Leonardo only.

   Serial.println("Domotica project, Arduino Domotica Server\n");
   
   //Init I/O-pins
   pinMode(switchPin, INPUT);            // hardware switch, for changing pin state
   pinMode(RFPin, OUTPUT);
   pinMode(ledPin, OUTPUT);
   pinMode(infoPin, OUTPUT);
   
   
   //Default states
   digitalWrite(switchPin, HIGH);        // Activate pullup resistors (needed for input pin)
   digitalWrite(RFPin, LOW);
   digitalWrite(ledPin, LOW);
   digitalWrite(infoPin, LOW);

   //Try to get an IP address from the DHCP server.
   if (Ethernet.begin(mac) == 0)
   {
      Serial.println("Could not obtain IP-address from DHCP -> do nothing");
      while (true){     // no point in carrying on, so do nothing forevermore; check your router
      }
   }
   
   Serial.print("LED (for connect-state and pin-state) on pin "); Serial.println(ledPin);
   Serial.print("Input switch on pin "); Serial.println(switchPin);
   Serial.println("Ethernetboard connected (pins 10, 11, 12, 13 and SPI)");
   Serial.println("Connect to DHCP source in local network (blinking led -> waiting for connection)");
   
   //Start the ethernet server.
   server.begin();

   // Print IP-address and led indication of server state
   Serial.print("Listening address: ");
   Serial.print(Ethernet.localIP());
   
   // for hardware debug: LED indication of server state: blinking = waiting for connection
   int IPnr = getIPComputerNumber(Ethernet.localIP());   // Get computernumber in local network 192.168.1.3 -> 3)
   Serial.print(" ["); Serial.print(IPnr); Serial.print("] "); 
   Serial.print("  [Testcase: telnet "); Serial.print(Ethernet.localIP()); Serial.print(" "); Serial.print(ethPort); Serial.println("]");
   signalNumber(ledPin, IPnr);

   readSensors();
}    

void loop()
{
   // Listen for incomming connection (app)
   EthernetClient ethernetClient = server.available();
   if (!ethernetClient) {
      blink(ledPin);
      return; // wait for connection and blink LED
   }

   Serial.println("Application connected");
   digitalWrite(ledPin, LOW);

   // Do what needs to be done while the socket is connected.
   while (ethernetClient.connected()) 
   {
      checkEvent(switchPin, pinState);          // update pin state


   //sensor timer
   timer.run();
        
      // Activate pin based op pinState
      if (pinChange) {
         if (pinState) { digitalWrite(ledPin, HIGH); switchDefault(true); }
         else { switchDefault(false); digitalWrite(ledPin, LOW); }
         pinChange = false;
      }
   
      // Execute when byte is received.
      while (ethernetClient.available())
      {
         char inByte = ethernetClient.read();   // Get byte from the client.

         bufferGlobal += inByte;
         if (inByte == '#')
         {
          OK = true;
          Serial.print(bufferGlobal);
          Serial.println("\n OK \n");
          break;
         }
       
         //executeCommand(inByte);                // Wait for command to execute
         inByte = NULL;                         // Reset the read byte.
      } 
      if (OK)
      {
        byte sCount = bufferGlobal.indexOf("-");
        bufferGlobal.remove(sCount);
        char cmd = bufferGlobal[1];
        bufferGlobal.remove(0,2);
        executeCommand(cmd);
        bufferGlobal = "";
      }
   }
   Serial.println("Application disonnected");
}


//LDR en Ultrasone Sensor
void readSensors(){ 
  sensorValue = readSensor(0, 100);         // update sensor value
  
  timer.setTimeout(sensorReadInterval, readSensors);
  
  sensorValue = analogRead(analogPin); // read the value from the sensor
  delay(150);
  
  //drempel waardes
  if(sensorValue <300){
    GenoegWater = false;
  }
  else{
    GenoegWater = true;
  }
}



// Choose and switch your Kaku device, state is true/false (HIGH/LOW)
void switchDefault(bool state)
{   
   apa3Transmitter.sendUnit(kaku_unit, state);          // APA3 Kaku (Gamma)    //u2 = 0, u1 = 1, u0 =2  werkt niet          
   delay(100);
}

// Implementation of (simple) protocol between app and Arduino
// Request (from app) is single char ('a', 's', 't', 'i' etc.)
// Response (to app) is 4 chars  (not all commands demand a response)
void executeCommand(char cmd)
{     
  char buf[4] = {'\0', '\0', '\0', '\0'};
  
  // Command protocol
  //10 letters defineren voor snelheid
  //Serial.print("["); Serial.print(cmd); Serial.print("] -> ");
  switch (cmd) { 
    case 's': // Report switch state to the app
      if (pinState) { server.write(" ON\n"); Serial.println("Pin state is ON"); }  // always send 4 chars
      else { server.write("OFF\n"); Serial.println("Pin state is OFF"); }
    break;
    /*koffie aan/uit*/
    /*
    case 'k': // Toggle state; If state is already ON then turn it OFF
      if (pinState) { pinState = false; Serial.println("Set pin state to \"OFF\""); }
      else { pinState = true; Serial.println("Set pin state to \"ON\""); } 
      kaku_unit = 0; 
      pinChange = true; */
      case 'k':
      pinMode(7, HIGH);
    break;
    
    /*licht aaan uit*/
    case 'l': // Toggle state; If state is already ON then turn it OFF
      if (pinState) { pinState = false; Serial.println("Set pin state to \"OFF\""); }
      else { pinState = true; Serial.println("Set pin state to \"ON\""); }  
      pinChange = true; 
      kaku_unit = 1;
    break;    
    case 'v': // Toggle state; If state is already ON then turn it OFF
      if (pinState) { pinState = false; Serial.println("Set pin state to \"OFF\""); }
      else { pinState = true; Serial.println("Set pin state to \"ON\""); }  
      pinChange = true; 
      kaku_unit = 2;
    break;
    
    case 'w': //sensor waardes van watersensor naar app
      intToCharBuf(WaterSensor, buf, 4);                // convert to charbuffer
    
      if (GenoegWater = true){  
        server.println("1");
      } else if (GenoegWater = false){
        server.println("0");
      }          
      server.write(buf, 4);                             // response is always 4 chars (\n included)
      Serial.print("Waterniveau Status: "); Serial.println(buf);
    break;  
                   
    case 'i':    
      digitalWrite(infoPin, HIGH);
    break;

    case 'k': //koffiezetapparaat aan/uit
      digitalWrite(5, HIGH);
    break;

    case 'a': //koffie daadwerkelijk zetten
      digitalWrite(6, HIGH);
    break;
    
    default:
      digitalWrite(infoPin, LOW);
    break;
  }
}

// read value from pin pn, return value is mapped between 0 and mx-1
int readSensor(int pn, int mx)
{
  return map(analogRead(pn), 0, 1023, 0, mx-1);    
}

// Convert int <val> char buffer with length <len>
void intToCharBuf(int val, char buf[], int len)
{
   String s;
   s = String(val);                        // convert tot string
   if (s.length() == 1) s = "0" + s;       // prefix redundant "0" 
   if (s.length() == 2) s = "0" + s;  
   s = s + "\n";                           // add newline
   s.toCharArray(buf, len);                // convert string to char-buffer
}

// Check switch level and determine if an event has happend
// event: low -> high or high -> low
void checkEvent(int p, bool &state)
{
   static bool swLevel = false;       // Variable to store the switch level (Low or High)
   static bool prevswLevel = false;   // Variable to store the previous switch level

   swLevel = digitalRead(p);
   if (swLevel)
      if (prevswLevel) delay(1);
      else {               
         prevswLevel = true;   // Low -> High transition
         state = true;
         pinChange = true;
      } 
   else // swLevel == Low
      if (!prevswLevel) delay(1);
      else {
         prevswLevel = false;  // High -> Low transition
         state = false;
         pinChange = true;
      }
}

// blink led on pin <pn>
void blink(int pn)
{
  digitalWrite(pn, HIGH); 
  delay(100); 
  digitalWrite(pn, LOW); 
  delay(100);
}

// Visual feedback on pin, based on IP number, used for debug only
// Blink ledpin for a short burst, then blink N times, where N is (related to) IP-number
void signalNumber(int pin, int n)
{
   int i;
   for (i = 0; i < 30; i++)
       { digitalWrite(pin, HIGH); delay(20); digitalWrite(pin, LOW); delay(20); }
   delay(1000);
   for (i = 0; i < n; i++)
       { digitalWrite(pin, HIGH); delay(300); digitalWrite(pin, LOW); delay(300); }
    delay(1000);
}

// Convert IPAddress tot String (e.g. "192.168.1.105")
String IPAddressToString(IPAddress address)
{
    return String(address[0]) + "." + 
           String(address[1]) + "." + 
           String(address[2]) + "." + 
           String(address[3]);
}

// Returns B-class network-id: 192.168.1.3 -> 1)
int getIPClassB(IPAddress address)
{
    return address[2];
}

// Returns computernumber in local network: 192.168.1.3 -> 3)
int getIPComputerNumber(IPAddress address)
{
    return address[3];
}

// Returns computernumber in local network: 192.168.1.105 -> 5)
int getIPComputerNumberOffset(IPAddress address, int offset)
{
    return getIPComputerNumber(address) - offset;
}
