using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using standard_lib;
using Xamarin.Forms;

namespace BLETest
{
    public partial class App : Application
    {
        public App(IBluetooth bluetooth)
        {
            InitializeComponent();

            Bluetooth = bluetooth;
            MainPage = new BLETest.MainPage();
        }

        public static IBluetooth Bluetooth { get; set; }

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
