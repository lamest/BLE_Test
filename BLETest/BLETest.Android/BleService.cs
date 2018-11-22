using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using standard_lib.Bluetooth;

namespace BLETest.Droid
{
    public class BleService : IService
    {
        public BleService(BluetoothGattService service)
        {
            Id = Guid.ParseExact(service.Uuid.ToString(), "d");
            Characteristics = service.Characteristics.Select(x => (ICharacteristic)new BleCharacteristic(x)).ToList();
        }

        public Guid Id { get; }
        public IList<ICharacteristic> Characteristics { get; }
    }
}