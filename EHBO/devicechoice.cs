using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace EHBO
{
    [Activity(Label = "Eerste Hulp Bij Opstaan", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class deviceChoice
    {
        public bool koffieAan { get; set; }
        public bool lichtAan { get; set; }

        public deviceChoice(bool koffieAan, bool lichtAan)
        {
            this.koffieAan = koffieAan;
            this.lichtAan = lichtAan;
        }


    }


}