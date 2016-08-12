using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using Android.Locations;
using Android.Net;
using Java.Net;
using Java.IO;
using System.Net;

namespace Shorts_or_Pants
{   //The addition here stops the app from restarting upon change of orientation
    [Activity(Label = "Game", ConfigurationChanges =Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class Game : Activity, ISensorEventListener, ILocationListener
    {
        TextView TempShow;
        ImageView Outcome;

        private SensorManager mSensorManager;
        private Sensor Thermometer;

        float Temperature = 13;

        LocationManager locMgr;

        double Longitude, Latitude = 0;

        const string OWMAPPID = "a939d206b4507444cf6d420cd50dad47";

        string WeatherData = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Game);

            //Link the variables to their respective visual components
            TempShow = (TextView)FindViewById(Resource.Id.txtTempShow);
            Outcome = (ImageView)FindViewById(Resource.Id.ImgOutcome);
        }

        protected override void OnResume()
        {
            base.OnResume();

            //Check to see if the SensorManager is working
            if ((mSensorManager = (SensorManager)GetSystemService(SensorService)) != null)
            {
                //Try to access the ambient temperature Sensor
                if ((Thermometer = mSensorManager.GetDefaultSensor(SensorType.AmbientTemperature)) != null)
                {
                    //Set up the listener
                    mSensorManager.RegisterListener(this, mSensorManager.GetDefaultSensor(SensorType.AmbientTemperature), SensorDelay.Normal);
                }
                //If the ambient temperature cannot be accessed
                else
                {
                    //Attempt to access the temperature sensor
                    if ((Thermometer = mSensorManager.GetDefaultSensor(SensorType.Temperature)) != null)
                    {
                        //Set up the listener
                        mSensorManager.RegisterListener(this, mSensorManager.GetDefaultSensor(SensorType.Temperature), SensorDelay.Normal);
                    }
                    //If neither Thermometers can be accessed get your location from the Google Plat services loaction API
                    else
                    {
                        //Attempts to access the LocationManager and checks if it was succesful
                        if ((locMgr = GetSystemService(Context.LocationService) as LocationManager) != null)
                        {
                            string Provider = LocationManager.GpsProvider;

                            if (locMgr.IsProviderEnabled(Provider))
                            {
                                locMgr.RequestLocationUpdates(Provider, 600000 /*600 seconds, 10 mins*/, 10000 /*10km*/, this);
                            }
                            //IsProviderEnabled fails
                            else
                            {
                                TempShow.Text = Provider + " is not available. Does the device have location services available?";
                            }
                        }
                    }
                }
            }
        }

        protected override void OnPause()
        {
            //This will stop your phone from recieving updates is this activity is 
            base.OnPause();
            locMgr.RemoveUpdates(this);
        }

        //*************************************************************************************Reading the file
        public void ReadingTheData()
        {
            //Regex temp = new Regex("temp");
            Match match = Regex.Match(WeatherData, "temp");
            WeatherData = WeatherData.Substring(match.Index + 6, 5);
            Temperature = float.Parse(WeatherData);
        }

        //*************************************************************************************Network Connection
        public void NetworkConnectionCheck(string OWMLATLON)
        {
            ConnectivityManager connMgr = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            NetworkInfo[] networkInfo = connMgr.GetAllNetworkInfo();

            if (networkInfo != null)
            {
                for (int i = 0; i < networkInfo.Length; i++)
                {
                    if (networkInfo[i].IsConnected)
                    {
                        Toast.MakeText(this, "The Internet is connected through " + networkInfo[i].TypeName, ToastLength.Long);
                        ConnectingToTheURL(OWMLATLON);
                        break;
                    }
                }
            }
            else
            {
                TempShow.Text = "No Network connection";
            }
        }

        private void ConnectingToTheURL(string myUrl)
        {
            //Create a request for the URL
            WebRequest request = WebRequest.Create(myUrl);
            //Get the response
            WebResponse response = request.GetResponse();
            //Display the response
            TempShow.Text = ((HttpWebResponse)response).StatusDescription;

            try
            {
                //Get the stream containing content returned by the server
                Stream In = response.GetResponseStream();
                //Open the Stream using a StreamReader for easy access
                StreamReader reader = new StreamReader(In);
                //Read the content
                WeatherData = reader.ReadToEnd();
                //Close the reader
                reader.Close();
            }
            catch (Exception e)
            {
                TempShow.Text = "Error: InputStream failed";
            }
            //Close the response connection
            response.Close();
        }

        //*************************************************************************************Checks the weather via the OpenWeatherMaps API
        void WeatherLookup(double Longitude, double Latitude)
        {
            //Create a string to access the weather
            string OWMLATLON = "http://api.openweathermap.org/data/2.5/weather?lat=" + Latitude.ToString() + "&lon=" + Longitude.ToString() + "&units=metric&APPID=" + OWMAPPID;

            //Check you are connected to the internet and download the file
            NetworkConnectionCheck(OWMLATLON);

            //Once the data has been obtained
            if (WeatherData != null)
            {
                ReadingTheData();
            }

            PicUpdate();
        }

        //*************************************************************************************Updating The Picture
        void PicUpdate()
        {
            if (Temperature > 23)
            {
                Outcome.SetImageResource(Resource.Drawable.Shorts);
            }
            else
            {
                Outcome.SetImageResource(Resource.Drawable.Pants);
            }
            TempShow.Text = Temperature.ToString() + " °C";
        }

        //*************************************************************************************Thermometer Listeners
        void ISensorEventListener.OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            //throw new NotImplementedException();
        }

        void ISensorEventListener.OnSensorChanged(SensorEvent e)
        {
            Temperature = e.Values[0];
            TempShow.Text = Temperature.ToString();
            PicUpdate();
        }

        //*************************************************************************************GPS Listeners
        public void OnLocationChanged(Location location)
        {
            Longitude = location.Longitude;
            Latitude = location.Latitude;

            WeatherLookup(Longitude, Latitude);
        }

        public void OnProviderDisabled(string provider)
        {
            Toast toast = Toast.MakeText(this, "GPS Disabled", ToastLength.Short);
        }

        public void OnProviderEnabled(string provider)
        {
            Toast toast = Toast.MakeText(this, "GPS Enabled", ToastLength.Short);
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            Toast toast = Toast.MakeText(this, provider + " is " + status, ToastLength.Long);
        }
    }
}