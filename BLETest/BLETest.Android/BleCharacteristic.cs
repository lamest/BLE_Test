using System;
using Android.Bluetooth;
using standard_lib.Bluetooth;

namespace BLETest.Droid
{
    public class BleCharacteristic : ICharacteristic
    {
        public BleCharacteristic(BluetoothGattCharacteristic characteristic)
        {
            Id = Guid.ParseExact(characteristic.Uuid.ToString(), "d");
        }

        public Guid Id { get; set; }
    }
}