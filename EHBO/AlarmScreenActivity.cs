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

namespace EHBO
{
    [Activity(Label = "AlarmScreenActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class AlarmScreenActivity : Activity
    {
        PowerManager.WakeLock wl;
        Vibrator vibro;

        MediaPlayer music;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetContentView(Resource.Layout.Wakescreen_Layout);
            FindViewById<Button>(Resource.Id.StopAlarmButton).Click += StopAlarm;
            FindViewById<Button>(Resource.Id.SnoozeButton).Click += Snooze;


            vibro = (Vibrator)GetSystemService(Context.VibratorService);

            base.OnCreate(savedInstanceState);





            // Create your application here
        }
        /// <summary>
        /// Gets called when the Stop alarm button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StopAlarm(object sender, EventArgs e)
        {
            //Cancel the vibration
            vibro.Cancel();
            music.Stop();
        }

        /// <summary>
        /// Gets called when the snooze button is pressed
        /// </summary>
        /// 
      

        void Snooze(object sender, EventArgs e)
        {

        }

        void WakeMeUpInside()
        {
            //Vibrate for 50 seconds. Have fun!    
            vibro.Vibrate(50000);
            music.Start();
        }

        void ImAwake()
        {
            // Release a wakelock, if we are still using wakelocks. Otherwise, delete this
            wl.Release();
        }


        /// <summary>
        /// Gets called rougly at the start of loading this activity . Sets the flags that wake the screen, bypass the screenlock , etcetera
        /// </summary>
        public override void OnAttachedToWindow()
        {
            Window.AddFlags(WindowManagerFlags.ShowWhenLocked |
                            WindowManagerFlags.KeepScreenOn |
                            WindowManagerFlags.DismissKeyguard |
                            WindowManagerFlags.TurnScreenOn);
        }

    }
}