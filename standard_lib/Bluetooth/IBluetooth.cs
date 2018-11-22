using System.ComponentModel;

namespace standard_lib.Bluetooth
{
    public interface IBluetooth : INotifyPropertyChanged
    {
        bool IsAvailable { get; }
        bool IsPermitted { get; }
        bool IsOn { get; }
        event DeviceDiscoveredHandler DeviceDiscovered;
        void RequestPermissions();
        void Scan();
        void StopScan();
    }

    public delegate void DeviceDiscoveredHandler(object sender, DeviceEventArgs e);
}