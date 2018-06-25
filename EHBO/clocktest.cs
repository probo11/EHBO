using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;



//using static Android.Widget.Toast;

namespace EHBO
{
    [Activity(Label = "Set alarm", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Alarmcontroller : Activity
    {
        Toast repeating;
        EditText timertext;
        TimePicker timeselector;
        AlarmManager am;
        PowerManager.WakeLock wl;
        Vibrator vibro;

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Alarmcontroller_Layout);
            FindViewById<Button>(Resource.Id.oneshotAlarm).Click += StartAlarm;
            FindViewById<Button>(Resource.Id.stoprepeatingAlarm).Click += StopAlarm;
            timeselector = FindViewById<TimePicker>(Resource.Id.timePicker);
            //timertext = FindViewById<EditText>(Resource.Id.timertext);
            vibro = (Vibrator)GetSystemService(Context.VibratorService);
            Java.Lang.Boolean q = new Java.Lang.Boolean(false);

            timeselector.SetIs24HourView(q);


        }

        /// <summary>
        /// Gets executed when the 'Set' button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartAlarm(object sender, EventArgs e)
        {
            // When the alarm goes off, we want to broadcast an Intent to our
            // BroadcastReceiver.  Here we make an Intent with an explicit class
            // name to have our own receiver (which has been published in
            // AndroidManifest.xml) instantiated and called, and then create an
            // IntentSender to have the intent executed as a broadcast.

            int timeHour = Convert.ToInt32(timeselector.Hour);
            int timeMinutes = Convert.ToInt32(timeselector.Minute);




            //else if(timeHour)
            //{

            //}


            //if (timeHour < r.Hour)
            //{
            //    if (timeMinutes < r.Minute)
            //    {

            //    }

            //}


            am = (AlarmManager)GetSystemService(AlarmService);
            Intent oneshotIntent = new Intent(this, typeof(OneShotAlarm));
            PendingIntent source = PendingIntent.GetBroadcast(this, 0, oneshotIntent, 0);


            // Check if we should set the time for later today or tomorrow
            DateTime r = DateTime.Now;
            Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
            if (timeHour < r.Hour || timeHour == r.Hour && timeMinutes < r.Minute)
            {
                // Aantal uren is kleiner, zet de timer voor morgen


                calendar.Set(CalendarField.HourOfDay, timeHour);
                calendar.Set(CalendarField.Minute, timeMinutes);
                calendar.Set(CalendarField.DayOfWeek, r.Day - 1);


            }
            else
            {
                // Zet wekker voor vandaag
                calendar.Set(CalendarField.HourOfDay, timeHour);
                calendar.Set(CalendarField.Minute, timeMinutes);

            }
            am.Set(AlarmType.RtcWakeup, calendar.TimeInMillis, source);
            // Tiny vibration for happtic feedback
           // vibro.Vibrate(50);

            //Use a calendar to convert hours and minutes to Java calendar. Set the alarm using the calendar


            // Tell the user about what we did.

            repeating = Toast.MakeText(this, "Alarm set for: ", ToastLength.Long);

        }


        void StopAlarm(object sender, EventArgs e)
        {
            // Add code to cancel the pending alarm here

        }
    }
    [BroadcastReceiver(Enabled = true)]
    public class OneShotAlarm : BroadcastReceiver
    {
        /// <summary>
        /// Receives the OneShot alarm Broadcast when it gets sent 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="intent"></param>
        public override void OnReceive(Context context, Intent intent)
        {
            //Not sure if i can use this  context here, might work buggy
            ContextWrapper p = new ContextWrapper(context);
            Intent q = new Intent(context, typeof(AlarmScreenActivity));
            q.AddFlags(ActivityFlags.NewTask);
            p.StartActivity(q);

        }
    }

}
