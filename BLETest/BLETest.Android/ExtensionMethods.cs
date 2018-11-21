using System;
using Android.Bluetooth;
using Android.OS;
using standard_lib.Bluetooth;
using ScanMode = standard_lib.Bluetooth.ScanMode;
using AndroidScanMode = Android.Bluetooth.LE.ScanMode;

namespace BLETest.Droid
{
    public static class ExtensionMethods
    {
        public static BluetoothState ToBluetoothState(this State state)
        {
            switch (state)
            {
                case State.Connected:
                case State.Connecting:
                case State.Disconnected:
                case State.Disconnecting:
                    return BluetoothState.On;
                case State.Off:
                    return BluetoothState.Off;
                case State.On:
                    return BluetoothState.On;
                case State.TurningOff:
                    return BluetoothState.TurningOff;
                case State.TurningOn:
                    return BluetoothState.TurningOn;
                default:
                    return BluetoothState.Unknown;
            }
        }
        public static AndroidScanMode ToNative(this ScanMode scanMode)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                throw new InvalidOperationException("Scan modes are not implemented in API lvl < 21.");

            switch (scanMode)
            {
                case ScanMode.Passive:
                    if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                    {
                        //Trace.Message("Scanmode Passive is not supported on API lvl < 23. Falling back to LowPower.");
                        return AndroidScanMode.LowPower;
                    }
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