using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using standard_lib;
using standard_lib.Bluetooth;
using ScanMode = standard_lib.Bluetooth.ScanMode;

namespace BLETest.Droid
{
    public class AndroidBluetooth : BindableBase, IBluetooth, IScanOwner
    {
        private static readonly string[] RequiredPermissions =
        {
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        private readonly BluetoothManager _bluetoothManager;

        private readonly Context _context;
        private readonly IPermissions _permissions;
        private readonly Api21BleScanCallback _scanCallback;
        private bool _isOn;
        private bool _isPermitted;
        private ScanMode _scanMode;
        private object _deviceRegistration=new object();
        private IList<BleDevice> _devices;
        private static TimeSpan _removeTime = TimeSpan.FromMinutes(1);


        public AndroidBluetooth(IPermissions permissions)
        {

            _devices = new List<BleDevice>();
            _context = Application.Context;
            _permissions = permissions;
            _permissions.OnRequestResult += OnRequestPermissionsResult;
            if (!_context.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
            {
                IsAvailable = false;
                return;
            }

            var statusChangeReceiver = new BluetoothStatusBroadcastReceiver(UpdateState);
            _context.RegisterReceiver(statusChangeReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));

            _bluetoothManager = (BluetoothManager) _context.GetSystemService(Context.BluetoothService);
            if (_bluetoothManager == null)
            {
                IsAvailable = false;
                return;
            }

            IsAvailable = true;

            IsOn = _bluetoothManager.Adapter.State == State.On;

            _scanCallback = new Api21BleScanCallback(this);

            Mode = ScanMode.LowLatency;
            CheckPermissions();
        }

        public ScanMode Mode
        {
            get => _scanMode;
            private set => SetProperty(ref _scanMode, value);
        }

        public event DeviceDiscoveredHandler DeviceDiscovered;

        public bool IsAvailable { get; }

        public bool IsPermitted
        {
            get => _isPermitted;
            private set => SetProperty(ref _isPermitted, value);
        }

        public bool IsOn
        {
            get => _isOn;
            private set => SetProperty(ref _isOn, value);
        }

        public async Task<bool> Disconnect(IDevice device)
        {
            var d = (BleDevice) device;
            return await d.Disconnect().ConfigureAwait(false);
        }

        public void Connect(IDevice device)
        {
            var d = (BleDevice) device;
            d.Connect();
            RegisterDevice(d);
        }

        private void RegisterDevice(BleDevice bleDevice)
        {
            lock (_deviceRegistration)
            {
                _devices.Add(bleDevice);
                bleDevice.Disconnected += OnDeviceDisconnected;
            }
        }

        private void OnDeviceDisconnected(object sender, Guid e)
        {
            lock (_deviceRegistration)
            {
                var device = _devices.FirstOrDefault(x => x.Id == e);
                if (device != null)
                {
                    device.Disconnected -= OnDeviceDisconnected;
                    _devices.Remove(device);
                }
            }
        }

        public void RequestPermissions()
        {
            _permissions.Request(RequiredPermissions);
        }

        public void Scan()
        {
            var ssb = new ScanSettings.Builder();
            ssb.SetScanMode(Mode.ToNative());
            //ssb.SetCallbackType(ScanCallbackType.AllMatches);

            if (_bluetoothManager.Adapter.BluetoothLeScanner != null)
            {
                Trace.TraceInformation($"Starting a scan for devices. ScanMode: {Mode}");
                _bluetoothManager.Adapter.BluetoothLeScanner.StartScan(null, ssb.Build(), _scanCallback);
            }
            else
            {
                Trace.TraceInformation("Scan failed. Bluetooth is probably off");
            }
        }

        public void StopScan()
        {
            _bluetoothManager.Adapter.BluetoothLeScanner.StopScan(_scanCallback);
        }

        private void CheckPermissions()
        {
            var value = _permissions.Check(RequiredPermissions);
            IsPermitted = value;
        }

        void IScanOwner.HandleDiscoveredDevice(BluetoothDevice device, int rssi, ScanRecord scanRecord)
        {
            BleDevice discoveredDevice = null;
            lock (_deviceRegistration)
            {
                var devicesToRemove=new List<BleDevice>(_devices.Count);
                foreach (var d in _devices)
                {
                    if (d.Address == device.Address)
                    {
                        d.Stopwatch.Restart();
                        discoveredDevice = d;
                    }
                    else if (d.Stopwatch.Elapsed > _removeTime)
                    {
                        devicesToRemove.Add(d);
                    }
                }

                foreach (var remove in devicesToRemove)
                {
                    _devices.Remove(remove);
                }

                if (discoveredDevice == null)
                {
                    discoveredDevice = new BleDevice(device, rssi, scanRecord);
                    RegisterDevice(discoveredDevice);
                }
            }

            DeviceDiscovered?.Invoke(this, new DeviceEventArgs(discoveredDevice));
        }

        private void UpdateState(State state)
        {
            IsOn = state == State.On;
        }

        private void OnRequestPermissionsResult(object sender, EventArgs e)
        {
            CheckPermissions();
        }
    }
}