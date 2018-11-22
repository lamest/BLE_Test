using System;
using System.Collections.Generic;

namespace standard_lib.Bluetooth
{
    public interface IService
    {
        Guid Id { get; }
        IList<ICharacteristic> Characteristics { get; }
    }
}