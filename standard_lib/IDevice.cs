using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using standard_lib.Bluetooth;

namespace standard_lib
{
    public interface IDevice
    {
        Guid Id { get; set; }
        string Name { get; set; }
        Task<IList<IService>> DiscoverServicesAsync();
    }
}
