using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace standard_lib.Bluetooth
{
    public interface IService
    {
        Guid Id { get; set; }
        Task<IList<ICharacteristic>> GetCharacteristicsAsync();
    }
}
