using System.ComponentModel;
using System.Threading.Tasks;

namespace standard_lib.Bluetooth
{
    public interface IBluetooth : INotifyPropertyChanged
    {
        bool IsAvailable { get; }
        bool IsPermitted { get; }
        BluetoothState State { get; }
        Task<bool> Disconnect(IDevice device);
        Task<bool> Connect(IDevice device);
        event DeviceDiscoveredHandler DeviceDiscovered;
        void RequestPermissions();
        void Scan();
        void StopScan();
    }

    public delegate void DeviceDiscoveredHandler(object sender, DeviceEventArgs e);
}