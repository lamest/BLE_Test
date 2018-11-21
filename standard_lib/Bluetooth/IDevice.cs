using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace standard_lib.Bluetooth
{
    public interface IDevice
    {
        Guid Id { get; set; }
        string Name { get; set; }
        Task<IList<IService>> DiscoverServicesAsync();
    }
}
