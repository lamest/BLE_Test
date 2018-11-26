using System;
using System.Linq;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using standard_lib;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace BLETest.Droid
{
    [Activity(Label = "BLETest", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity, IPermissions
    {
        private static readonly string[] _requiredPermissions =
        {
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        public void Request()
        {
            ActivityCompat.RequestPermissions(this, _requiredPermissions, 0);
        }

        public event EventHandler OnRequestResult;

        public bool Check()
        {
            var value = _requiredPermissions.All(p =>
                ContextCompat.CheckSelfPermission(this, p) == Permission.Granted);
            return value;
        }

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Permissions.SetInstance(this);

            AppCenter.Start("bc3f2ae7-f40f-448e-91aa-88c0c6df8fd9", typeof(Analytics), typeof(Crashes));

            Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            OnRequestResult?.Invoke(null, null);
        }
    }
}