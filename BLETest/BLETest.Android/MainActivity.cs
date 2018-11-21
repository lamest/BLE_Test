using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using standard_lib;
using standard_lib.Bluetooth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace BLETest.Droid
{
    [Activity(Label = "BLETest", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        private AndroidBluetooth _bluetooth;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            AppCenter.Start("bc3f2ae7-f40f-448e-91aa-88c0c6df8fd9", typeof(Analytics));
            Forms.Init(this, bundle);
            Analytics.TrackEvent(AnalitycsEvents.AppStarted);
            _bluetooth = (AndroidBluetooth)DependencyService.Get<IBluetooth>();
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) => _bluetooth.OnRequestPermissionsResult();
    }
}