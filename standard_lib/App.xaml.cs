using standard_lib.Bluetooth;
using Xamarin.Forms;

namespace BLETest
{
    public partial class App : Application
    {
        public App(IBluetooth bluetooth)
        {
            InitializeComponent();
            Bluetooth = bluetooth;
            MainPage = new MainPage();
        }

        public static IBluetooth Bluetooth { get; private set; }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}