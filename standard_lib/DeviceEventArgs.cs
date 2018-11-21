using standard_lib.Bluetooth;

namespace standard_lib
{
    public class DeviceEventArgs
    {
        public DeviceEventArgs(IDevice device)
        {
            Device = device;
        }
        public IDevice Device { get; }
    }
}