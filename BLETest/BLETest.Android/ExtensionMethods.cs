using System;
using Android.OS;
using standard_lib.Bluetooth;
using AndroidScanMode = Android.Bluetooth.LE.ScanMode;

namespace BLETest.Droid
{
    public static class ExtensionMethods
    {
        public static AndroidScanMode ToNative(this ScanMode scanMode)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                throw new InvalidOperationException("Scan modes are not implemented in API lvl < 21.");

            switch (scanMode)
            {
                case ScanMode.Passive:
                    if (Build.VERSION.SdkInt < BuildVersionCodes.M) return AndroidScanMode.LowPower;
                    return AndroidScanMode.Opportunistic;
                case ScanMode.LowPower:
                    return AndroidScanMode.LowPower;
                case ScanMode.Balanced:
                    return AndroidScanMode.Balanced;
                case ScanMode.LowLatency:
                    return AndroidScanMode.LowLatency;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scanMode), scanMode, null);
            }
        }
    }
}