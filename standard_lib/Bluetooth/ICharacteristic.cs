using System;

namespace standard_lib.Bluetooth
{
    public interface ICharacteristic
    {
        Guid Id { get; set; }
    }
}