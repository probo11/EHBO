using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Runtime;
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
        Button WekkerInstellen;
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
        public bool aan;

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
            WekkerInstellen = FindViewById<Button>(Resource.Id.WekkerInstellen);

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

            WekkerInstellen.Click += (sender, e) =>
            {

                Intent intent = new Intent(this, typeof(Alarmcontroller));
                StartActivity(intent);

            };

        }

        //auto connect
        public void autoConnect_Click(object sender, EventArgs e)
        {
            AutoConnect();
        }

        private void Set_Click(object sender, EventArgs e)
        {
            aan = true;
            //time = tijd.Text;
            //timer = new Timer();
            //timer.Interval = 1000;
            //timer.Elapsed += Timer_Elapsed; // 1 seconds
            //timer.Start();
            //btnset.Enabled = false;
            //count = 0;
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

        

        public void WakeMeUp()
        {
            socket.Send(System.Text.Encoding.ASCII.GetBytes("$k---------#"));
            socket.Send(System.Text.Encoding.ASCII.GetBytes("$l---------#"));
            if (aan == true)
            {
                time = "60000";
                timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += Timer_Elapsed; // 1 seconds
                timer.Start();
                btnset.Enabled = false;
                count = 0;
            }
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
                        socket.Send(System.Text.Encoding.ASCII.GetBytes("$a---------#"));
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
                if (socket == null) // create new socket
                {
                    UpdateConnectionState(1, "Connecting...");
                    try  // to connect to the server (Arduino).
                    {
                        socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(prt)));
                        if (socket.Connected)
                        {
                            UpdateConnectionState(2, "Connected");
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
                checkbox1.Enabled = true;
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
            Color color = Color.Red;        // default color
            //Set "Connect" button label according to connection state.
            if (state == 1)
            {
                butConText = "Please wait";
                color = Color.Orange;
            }
            else
            if (state == 2)
            {
                butConText = "Disconnect";
                color = Color.Green;
            }

            //Edit the control's properties on the UI thread
            RunOnUiThread(() =>
            {
                textViewServerConnect.Text = text;
                if (butConText != null)  // text existst
                {
                    autoConnect.Text = butConText;
                    textViewServerConnect.SetTextColor(color);
                }
            });
        }
    }
}
