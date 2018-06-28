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
                Finish();
            };
        }
        private void SelectMusic_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            MainActivity.music.Stop();
            PlayMusic(spinner.GetItemAtPosition(e.Position).ToString());
            MainActivity.music.Start();

            if(tim != null)
            {
                tim.Stop();
            }
            
            tim = new Timer() { Interval = 5000, Enabled = true };
            tim.Elapsed += (obj, args) =>
            {
                MainActivity.music.Stop();
                tim.Stop();
            };
        }

        public void PlayMusic(string musicn)
        {
            if (musicn == "Life")
            {
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Life);
                AlarmScreenActivity.musicFinal = MediaPlayer.Create(this, Resource.Raw.Life);
            }
            else if (musicn == "Tomaten")
            {
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Tomaten);
                AlarmScreenActivity.musicFinal = MediaPlayer.Create(this, Resource.Raw.Tomaten);
            }
            else if (musicn == "Villian")
            {
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Villian);
                AlarmScreenActivity.musicFinal = MediaPlayer.Create(this, Resource.Raw.Villian);
            }
            else if (musicn == "Wakker")
            {
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Wakker);
                AlarmScreenActivity.musicFinal = MediaPlayer.Create(this, Resource.Raw.Wakker);
            }
        }
    }
}