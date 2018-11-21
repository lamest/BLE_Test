using standard_lib.Bluetooth;

namespace standard_lib
{
    public struct BluetoothStateChangedArgs
    {
        public BluetoothStateChangedArgs(BluetoothState oldState, BluetoothState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public readonly BluetoothState OldState;
        public readonly BluetoothState NewState;
    }
}