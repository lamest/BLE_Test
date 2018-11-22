using System;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using BLETest.Droid;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using standard_lib;
using standard_lib.Bluetooth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Application = Android.App.Application;

namespace BLETest.Droid
{
    [Activity(Label = "BLETest", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity, IPermissions
    {
        public void Request(string[] requiredPermissions)
        {
            ActivityCompat.RequestPermissions(this, requiredPermissions, 0);
        }

        public event EventHandler OnRequestResult;

        public bool Check(string[] requiredPermissions)
        {
            var value = requiredPermissions.All(p =>
                ContextCompat.CheckSelfPermission(Application.Context, p) == Permission.Granted);
            return value;
        }

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            AppCenter.Start("bc3f2ae7-f40f-448e-91aa-88c0c6df8fd9", typeof(Analytics));
            Forms.Init(this, bundle);
            Analytics.TrackEvent(AnalitycsEvents.AppStarted);
            var bluetooth = new AndroidBluetooth(this);
            LoadApplication(new App(bluetooth));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            OnRequestResult?.Invoke(null, null);
        }
    }
}