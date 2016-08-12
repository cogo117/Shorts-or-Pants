using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Shorts_or_Pants
{
    [Activity(Label = "Shorts or Pants", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        Button Play;
        Button Exit;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //link the buttons to the xml versions
            Play = (Button)FindViewById(Resource.Id.btnPlay);
            Exit = (Button)FindViewById(Resource.Id.btnExit);

            //if the user clicks Play
            Play.Click += OnClickPlay;

            //if the user clicks exit, run the OnClickExit Function
            Exit.Click += OnClickExit;
        }
        
        void OnClickPlay(object sender, EventArgs e)
        {
            //Creates a explicit intent variable linking to the Game file
            var intent = new Intent(this, typeof(Game));

            //Starts a new Activity with the newly created intent variable
            base.StartActivity(intent);
        }

        void OnClickExit(object sender, EventArgs e)
        {
            //Closes the app
            Finish();
        }
    }
}

