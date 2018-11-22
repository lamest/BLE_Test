using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace standard_lib.Bluetooth
{
    public interface IDevice : INotifyPropertyChanged
    {
        Guid Id { get; set; }
        string Name { get; set; }
        Task<IList<IService>> DiscoverServices();
        Task<IList<ICharacteristic>> DiscoverCharacteristics(IService service);
        Task Connect();
        Task Disconnect();
        Task WriteCharacteristic(IService service, ICharacteristic characteristic, byte[] value);
        Task<byte[]> ReadCharacteristic(IService service, ICharacteristic characteristic);
    }
}