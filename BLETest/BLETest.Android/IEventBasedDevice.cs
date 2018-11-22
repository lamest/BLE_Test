using System;
using System.Collections.Generic;
using System.ComponentModel;
using standard_lib.Bluetooth;

namespace BLETest.Droid
{
    public interface IEventBasedDevice : INotifyPropertyChanged
    {
        Guid Id { get; }
        string Name { get; }
        void DiscoverServices();
        void Connect();
        void Disconnect();
        void WriteCharacteristic(IService service, ICharacteristic characteristic, byte[] value);
        void ReadCharacteristic(IService service, ICharacteristic characteristic);
        event EventHandler<Guid> Connected;
        event EventHandler<Guid> Disconnected;
        event EventHandler<IList<IService>> ServicesDiscovered;
        event EventHandler<ICharacteristic> CharacteristicWrite;
        event EventHandler<ICharacteristic> CharacteristicRead;
    }
}