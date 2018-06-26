using slagboom.Droid;
using System;
using Xamarin.Forms;
using System.Net.Sockets;
namespace slagboom
{
	public partial class MainPage : ContentPage
	{
        private int timerInterval = 1;

        public MainPage()
		{
			InitializeComponent();
            this.FindByName<Button>("openbutton").Clicked += Open;
		}

        private void Open(object sender, EventArgs e)
        {
            var buttonValue = (sender) as Button;
            try
            {
                var sockeClass = new Socketclass();

                var s = sockeClass.Open();

                sockeClass.Write(s, "s");

                var reply = sockeClass.Read(s);


                sockeClass.Close(s);

                Device.StartTimer(new TimeSpan(0, 0, timerInterval), TimerTick);

            }
            catch(Exception er)
            {
                System.Diagnostics.Debug.WriteLine(er);
            }


        }

        private string BarrierState(string s)
        {
            return (s == "0") ? "Gesloten" : "Open";
        }

        private bool TimerTick()
        {
            try
            {
                var sockeClass = new Socketclass();

                var s = sockeClass.Open();

                sockeClass.Write(s, "v");

                var reply = sockeClass.Read(s);

                string[] replyCollection = reply.Split('>');

                this.FindByName<Label>("sonarValue").Text = $"Sonar value is: {replyCollection[0].ToString()}";

                this.FindByName<Label>("slagboomValue").Text = $"Slagboom positie is: {BarrierState(replyCollection[1])}";

                sockeClass.Close(s);
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

      
    }
}
