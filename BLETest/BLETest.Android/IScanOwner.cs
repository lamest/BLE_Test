using Android.Bluetooth;
using Android.Bluetooth.LE;

namespace BLETest.Droid
{
    public interface IScanOwner
    {
        void HandleDiscoveredDevice(BluetoothDevice device, int rssi, ScanRecord scanRecord);
    }
}