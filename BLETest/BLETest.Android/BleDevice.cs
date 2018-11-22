using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Java.Util;
using standard_lib.Bluetooth;

namespace BLETest.Droid
{
    public class BleDevice : BindableBase, IEventBasedDevice, IGattOwner
    {
        private readonly GattCallback _gattCallback;
        private BluetoothDevice _device;
        private BluetoothGatt _gatt;
        public Stopwatch Stopwatch { get; }
        private int _rssi;
        private ScanRecord _scanRecord;
        public string Address => _device.Address;


        public BleDevice(BluetoothDevice device, int rssi, ScanRecord scanRecord)
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            _device = device;
            _rssi = rssi;
            _scanRecord = scanRecord;

            _gattCallback = new GattCallback(this);
        }

        public Guid Id => ParseDeviceId(_device.Address);
        public string Name => _device.Name;

        public void DiscoverServices()
        {
            var errorCode = _gatt.DiscoverServices();
            if (!errorCode) throw new Exception("Fail to start service discovery");
        }

        public void Connect()
        {
            /*_gatt = */
            var t = BluetoothDevice.ConnectGatt(Application.Context, false, _gattCallback);
            if (t == null) throw new Exception("Unknown error");
        }

        public void Disconnect()
        {
            _gatt.Disconnect();
            _gatt.Close();
            _gatt = null;
        }

        public void WriteCharacteristic(IService service, ICharacteristic characteristic, byte[] value)
        {
            throw new NotImplementedException();
        }

        public void ReadCharacteristic(IService service, ICharacteristic characteristic)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<Guid> Connected;
        public event EventHandler<Guid> Disconnected;
        public event EventHandler<IList<IService>> ServicesDiscovered;
        public event EventHandler<ICharacteristic> CharacteristicWrite;
        public event EventHandler<ICharacteristic> CharacteristicRead;

        private Guid ParseDeviceId(string uuid)
        {
            var deviceGuid = new byte[16];
            var macWithoutColons = uuid.Replace(":", "");
            var macBytes = Enumerable.Range(0, macWithoutColons.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
                .ToArray();
            macBytes.CopyTo(deviceGuid, 10);
            return new Guid(deviceGuid);
        }


        #region IGattOwner

        void IGattOwner.Disconnected(GattStatus status)
        {
            throw new NotImplementedException();
        }

        void IGattOwner.OnServicesDiscovered(GattStatus status)
        {
            var services = _gatt.Services.Select(x => (IService) new BleService(x));
            ServicesDiscovered?.Invoke(this, (IList<IService>) services);
        }

        void IGattOwner.CharacteristicRead(GattStatus status, BluetoothGattCharacteristic characteristic)
        {
            throw new NotImplementedException();
        }

        void IGattOwner.CharacteristicChanged(BluetoothGattCharacteristic characteristic)
        {
            throw new NotImplementedException();
        }

        void IGattOwner.CharacteristicWrite(GattStatus status, BluetoothGattCharacteristic characteristic)
        {
            throw new NotImplementedException();
        }

        void IGattOwner.DescriptorWrite(GattStatus status, BluetoothGattDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        string IGattOwner.Address => _device.Address;

        void IGattOwner.Update(BluetoothDevice nativeDevice, BluetoothGatt gatt, GattStatus status)
        {
            _gatt?.Close();

            _gatt = gatt;
            _device = nativeDevice;
        }

        #endregion
    }
}