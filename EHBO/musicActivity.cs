using Android.App;
using Android.Widget;
using Android.OS;
using Android.Media;
using Android.Net;
using Android.Content.PM;
using Android.Content;
using System.Timers;

namespace EHBO
{
    [Activity(Label = "Eerste Hulp Bij Opstaan", ScreenOrientation = ScreenOrientation.Portrait)]
    public class musicActivity : Activity
    {
        Timer tim;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.musicLayout);

            Spinner selectMusic = FindViewById<Spinner>(Resource.Id.selectMusic);
            Button chooseMusic = FindViewById<Button>(Resource.Id.musicButton);
            selectMusic.ItemSelected += SelectMusic_ItemSelected;
            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.musicList, Android.Resource.Layout.SimpleSpinnerItem);
            selectMusic.Adapter = adapter;

            chooseMusic.Click += (sender, e) =>
            {
                MainActivity.music.Stop();
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            };
        }
        private void SelectMusic_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            if (spinner.GetItemAtPosition(e.Position).ToString() == "Life")
            {
                MainActivity.music.Stop();
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Life);
                MainActivity.music.Start();
                tim = new Timer() { Interval = 5000, Enabled = true };
                tim.Elapsed += (obj, args) =>
                {
                    MainActivity.music.Stop();
                    tim.Stop();
                };
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Tomaten")
            {
                MainActivity.music.Stop();
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Tomaten);
                MainActivity.music.Start();
                tim = new Timer() { Interval = 5000, Enabled = true };
                tim.Elapsed += (obj, args) =>
                {
                    MainActivity.music.Stop();
                    tim.Stop();
                };
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Villian")
            {
                MainActivity.music.Stop();
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Villian);
                MainActivity.music.Start();
                tim = new Timer() { Interval = 5000, Enabled = true };
                tim.Elapsed += (obj, args) =>
                {
                    MainActivity.music.Stop();
                    tim.Stop();
                };
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Wakker")
            {
                MainActivity.music.Stop();
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Wakker);
                MainActivity.music.Start();
                tim = new Timer() { Interval = 5000, Enabled = true };
                tim.Elapsed += (obj, args) =>
                {
                    MainActivity.music.Stop();
                    tim.Stop();
                };
            }
        }
    }
}