using System;
using System.Collections.Generic;
using System.Linq;
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using Plugin.BLE.iOS;
using UIKit;

namespace BLETest.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            var cb = new CBCentralManager(null, DispatchQueue.MainQueue, new CBCentralInitOptions { ShowPowerAlert = true });
            BluetoothHelper.NativeMethods = new NativeMethods();
            App.Adapter = new Adapter(cb);

            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}
