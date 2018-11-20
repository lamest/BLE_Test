using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace standard_lib
{
    public interface IBluetooth : INotifyPropertyChanged
    {
        Task<bool> Disconnect(IDevice device);
        Task<bool> Connect(IDevice device);
        bool CheckAvailability();
        bool CheckPermissions();
        BleState CheckState();
        event DeviceDiscoveredHandler DeviceDiscovered;
        event DeviceStateChangedHandler StateChanged;
        void SetScanMode(ScanMode scanMode);
        Task ScanAsync();
    }

    public delegate void DeviceStateChangedHandler(object sender, BluetoothStateChangedArgs e);
    public delegate void DeviceDiscoveredHandler(object sender, DeviceEventArgs e);

    public enum BleState
    {
        Enabled,
        Disabled
    }

    public enum ScanMode
    {
        LowEnergy,
        LowLatency
    }
}
