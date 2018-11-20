using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace standard_lib.Bluetooth
{
    public interface ICharacteristic
    {
        Guid Id { get; set; }
        Task<byte[]> ReadAsync();
        Task<bool> WriteAsync(byte[] bytes);
    }
}
