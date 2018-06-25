using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Runtime;
using EHBO;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Media;
using Android.Net;

namespace EHBO
{
    [Activity(Label = "Eerste Hulp Bij Opstaan", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        //variables
        //public static bool koffieAan = deviceChoice.koffieAan;
        //public static bool lichtAan = deviceChoice.lichtAan;
        //controls on GUI
        Button ToggleKoffie;
        Button ToggleLicht;
        CheckBox checkbox1;
        CheckBox checkbox2;
        public static MediaPlayer music;
        deviceChoice choice;
        Button chooseMusic;
        //timer stuff
        private Button btnCancel;
        private Button btnset;
        private TextView txtCountdown;
        private int count = 0;
        private int countdown;
        Timer timer;
        private EditText tijd;
        private string time;
        TextView textViewServerConnect;
        //snooze stuff
        private Button snooze;

        //socket connect
        Button autoConnect;

        //Timer timerClock, timerSockets;             // Timers   
        Socket socket = null;                       // Socket   
        List<Tuple<string, TextView>> commandList = new List<Tuple<string, TextView>>();  // List for commands and response places on UI
        int listIndex = 0;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            //toggle stuff
            ToggleKoffie = FindViewById<Button>(Resource.Id.ToggleKoffie);
            ToggleLicht = FindViewById<Button>(Resource.Id.ToggleLicht);
            chooseMusic = FindViewById<Button>(Resource.Id.goMusic);
            checkbox1 = FindViewById<CheckBox>(Resource.Id.checkBox1);
            checkbox2 = FindViewById<CheckBox>(Resource.Id.checkBox2);

            choice = new deviceChoice(true, true);

            //timer stuff

            btnset = FindViewById<Button>(Resource.Id.set);
            txtCountdown = FindViewById<TextView>(Resource.Id.txtCountdown);
            btnset.Click += Set_Click;
            tijd = FindViewById<EditText>(Resource.Id.tijd);

            //snooze stuff
            snooze = FindViewById<Button>(Resource.Id.snooze);
            snooze.Click += snooze_Click;
            snooze.Enabled = false;

            Spinner selectMusic = FindViewById<Spinner>(Resource.Id.selectMusic);

            selectMusic.ItemSelected += SelectMusic_ItemSelected;
            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.musicList, Android.Resource.Layout.SimpleSpinnerItem);
            selectMusic.Adapter = adapter;

            //autoconnect
            autoConnect = FindViewById<Button>(Resource.Id.autoConnect);
            autoConnect.Click += autoConnect_Click;
            textViewServerConnect = FindViewById<TextView>(Resource.Id.textViewServerConnect);

            UpdateConnectionState(4, "Disconnected");

            //koffie keuze aan of uit
            if (checkbox1.Checked == true)
            {
                choice.koffieAan = true;
            }
            else
            {
                choice.koffieAan = false;
            }

            //licht keuze aan of uit
            if (checkbox2.Checked == true)
            {
                choice.lichtAan = true;
            }
            else
            {
                choice.lichtAan = false;
            }

            //koffie aan
            if (ToggleKoffie != null)  // if koffie button exists
            {
                ToggleKoffie.Click += (sender, e) =>
                {
                    socket.Send(System.Text.Encoding.ASCII.GetBytes("$k---------#")); //send k
                };
            }

            //licht aan
            if (ToggleLicht != null)  // if licht button exists
            {
                ToggleLicht.Click += (sender, e) =>
                {
                    socket.Send(System.Text.Encoding.ASCII.GetBytes("$l---------#")); //send l
                };
            }


            chooseMusic.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(musicActivity));
                StartActivity(intent);
            };


        }

        private void SelectMusic_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            if (spinner.GetItemAtPosition(e.Position).ToString() == "Discussion")
            {
                music = MediaPlayer.Create(this, Resource.Raw.Discussion);
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Freaks")
            {
                music = MediaPlayer.Create(this, Resource.Raw.Freaks);
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Rattlesnake")
            {
                music = MediaPlayer.Create(this, Resource.Raw.Rattlesnake);
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Sparkle")
            {
                music = MediaPlayer.Create(this, Resource.Raw.Sparkle);
            }
        }


        //auto connect
        public void autoConnect_Click(object sender, EventArgs e)
        {
            //autoConnect.Click += (sender, e) =>
            //{
            AutoConnect();
            //};
        }

        private void Set_Click(object sender, EventArgs e)
        {
            time = tijd.Text;
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed; // 1 seconds
            timer.Start();
            btnset.Enabled = false;
            count = 0;
        }

        private void snooze_Click(object sender, EventArgs e)
        {
            timer.Stop();
            snooze.Enabled = false;
            btnCancel.Enabled = true;
            time = tijd.Text;
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed; // 1 seconds
            timer.Start();
            btnset.Enabled = false;
            count = 0;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (count < Convert.ToInt32(time))
            {
                count++; // increase count variable
                RunOnUiThread(() =>
                {
                    countdown = (Convert.ToInt32(time) + 1) - count;
                    int seconds = countdown % 60;
                    int minutes = countdown / 60;
                    txtCountdown.Text = minutes + ":" + seconds;
                });
            }
            else
            {
                //als countdown afgelopen is
                RunOnUiThread(() =>
                {
                    count = Convert.ToInt32(time); // Reset count variable
                    Toast.MakeText(this, "Hello", ToastLength.Short).Show();
                    timer.Stop();
                    countdown = Convert.ToInt32(time) - count;
                    int seconds = countdown % 60;
                    int minutes = countdown / 60;
                    txtCountdown.Text = minutes + ":" + seconds;
                    btnset.Enabled = true;
                    snooze.Enabled = true;

                    //toggle koffiezetapparaat
                    if (choice.koffieAan == true)
                    {
                        socket.Send(System.Text.Encoding.ASCII.GetBytes("$k---------#"));
                    }

                });
            }
        }

        //Send command to server and wait for response (blocking)
        //Method should only be called when socket existst
        public string executeCommand(string cmd)
        {
            byte[] buffer = new byte[4]; // response is always 4 bytes
            int bytesRead = 0;
            string result = "---";

            if (socket != null)
            {
                //Send command to server
                string rep = "$" + cmd.PadRight(11, '-');
                string buf = rep + "#";
                socket.Send(System.Text.Encoding.ASCII.GetBytes(buf));

                try //Get response from server
                {
                    //Store received bytes (always 4 bytes, ends with \n)
                    bytesRead = socket.Receive(buffer);  // If no data is available for reading, the Receive method will block until data is available,
                                                         //Read available bytes.              // socket.Available gets the amount of data that has been received from the network and is available to be read
                    while (socket.Available > 0) bytesRead = socket.Receive(buffer);
                    if (bytesRead == 4)
                        result = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead - 1); // skip \n
                    else result = "err";
                }
                catch (Exception exception)
                {
                    result = exception.ToString();
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }
                    UpdateConnectionState(3, result);
                }
            }
            return result;
        }

        // Connect to socket ip/prt (simple sockets)
        public void ConnectSocket(string ip, string prt)
        {
            RunOnUiThread(() =>
            {
                if (socket == null)                                       // create new socket
                {
                    UpdateConnectionState(1, "Connecting...");
                    try  // to connect to the server (Arduino).
                    {
                        socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(prt)));
                        if (socket.Connected)
                        {
                            UpdateConnectionState(2, "Connected");
                            //timerSockets.Enabled = true;                //Activate timer for communication with Arduino     
                        }
                    }
                    catch (Exception exception)
                    {
                        //timerSockets.Enabled = false;
                        if (socket != null)
                        {
                            socket.Close();
                            socket = null;
                        }
                        UpdateConnectionState(4, exception.Message);
                    }
                }
                else // disconnect socket
                {
                    socket.Close(); socket = null;
                    //timerSockets.Enabled = false;
                    UpdateConnectionState(4, "Disconnected");
                }
            });
        }

        public void CheckWater(string result)
        {
            if (result == "0")
            {
                //als er niet genoeg water in zit
                checkbox1.Checked = false;
            }
            else if (result == "1")
            {
                // als er wel genoeg water in zit

                //checkbox.enable = true;
            }

        }
        //Close the connection (stop the threads) if the application stops.
        protected override void OnStop()
        {
            base.OnStop();
        }

        //Close the connection (stop the threads) if the application is destroyed.
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }


        //Prepare the Screen's standard options menu to be displayed.
        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            //Prevent menu items from being duplicated.
            menu.Clear();

            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        //Executes an action when a menu button is pressed.
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.exit:
                    //Force quit the application.
                    System.Environment.Exit(0);
                    return true;
                case Resource.Id.abort:
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        //Check if the entered IP address is valid.
        private bool CheckValidIpAddress(string ip)
        {
            if (ip != "")
            {
                //Check user input against regex (check if IP address is not empty).
                Regex regex = new Regex("\\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\\.|$)){4}\\b");
                Match match = regex.Match(ip);
                return match.Success;
            }
            else return false;
        }

        //Check if the entered port is valid.
        private bool CheckValidPort(string port)
        {
            //Check if a value is entered.
            if (port != "")
            {
                Regex regex = new Regex("[0-9]+");
                Match match = regex.Match(port);

                if (match.Success)
                {
                    int portAsInteger = Int32.Parse(port);
                    //Check if port is in range.
                    return ((portAsInteger >= 0) && (portAsInteger <= 65535));
                }
                else return false;
            }
            else return false;
        }


        /// <summary>
        /// Tries all IP adresses in the 192.168.1 range on port 3300 , stops when it connects
        /// </summary>
        void AutoConnect()
        {
            try
            {
                for (int i = 2; i < 256; i++)
                {
                    string p = "192.168.1." + i;
                    ConnectSocket(p, "3300");
                    if (textViewServerConnect.Text == "Connected")
                    {
                        return;
                    }

                }
                //ConnectSocket("192.168.1.2","3300");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }



        }


        //Update connection state label (GUI).
        public void UpdateConnectionState(int state, string text)
        {
            // connectButton
            string butConText = "Connect";  // default text
            //bool butConEnabled = true;      // default state
            Color color = Color.Red;        // default color
            // pinButton
            //bool butPinEnabled = false;     // default state 

            //Set "Connect" button label according to connection state.
            if (state == 1)
            {
                butConText = "Please wait";
                color = Color.Orange;
                //butConEnabled = false;
            }
            else
            if (state == 2)
            {
                butConText = "Disconnect";
                color = Color.Green;
                //butPinEnabled = true;
            }



            //Edit the control's properties on the UI thread
            RunOnUiThread(() =>
            {
                textViewServerConnect.Text = text;
                if (butConText != null)  // text existst
                {

                    autoConnect.Text = butConText;
                    textViewServerConnect.SetTextColor(color);
                    //buttonConnect.Enabled = butConEnabled;
                }
                //buttonChangePinState.Enabled = butPinEnabled;
            });

        }




    }
}
