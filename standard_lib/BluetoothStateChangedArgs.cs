using System;
using System.Collections.Generic;
using System.Text;

namespace standard_lib
{
    public class BluetoothStateChangedArgs
    {
        public BleState OldState { get; set; }
        public BleState NewState { get; set; }

    }
}
