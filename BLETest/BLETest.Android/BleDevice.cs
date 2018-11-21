using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using standard_lib.Bluetooth;

namespace BLETest.Droid
{
    public class BleDevice : IDevice
    {
        private BluetoothDevice _device;
        private int _rssi;
        private ScanRecord _scanRecord;

        public BleDevice(BluetoothDevice device, int rssi, ScanRecord scanRecord)
        {
            _device = device;
            _rssi = rssi;
            _scanRecord = scanRecord;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public Task<IList<IService>> DiscoverServicesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Connect()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}