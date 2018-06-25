using Android.App;
using Android.Widget;
using Android.OS;
using Android.Media;
using Android.Net;
using Android.Content.PM;
using Android.Content;

namespace EHBO
{
    [Activity(Label = "music", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class musicActivity : Activity
    {
        //public static MediaPlayer music;

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
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            };
        }
        private void SelectMusic_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            if (spinner.GetItemAtPosition(e.Position).ToString() == "Discussion")
            {
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Discussion);
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Freaks")
            {
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Freaks);
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Rattlesnake")
            {
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Rattlesnake);
            }
            else if (spinner.GetItemAtPosition(e.Position).ToString() == "Sparkle")
            {
                MainActivity.music = MediaPlayer.Create(this, Resource.Raw.Sparkle);
            }
        }
    }
}