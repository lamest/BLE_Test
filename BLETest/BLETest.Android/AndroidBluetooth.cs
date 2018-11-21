using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using BLETest;
using BLETest.Droid;
using standard_lib;
using standard_lib.Bluetooth;
using Xamarin.Forms;
using Application = Android.App.Application;
using ScanMode = standard_lib.Bluetooth.ScanMode;

[assembly: Dependency(typeof(AndroidBluetooth))]
public class AndroidBluetooth : BindableBase, IBluetooth
{
    private static readonly string[] RequiredPermissions =
    {
        Manifest.Permission.Bluetooth,
        Manifest.Permission.BluetoothAdmin,
        Manifest.Permission.AccessCoarseLocation,
        Manifest.Permission.AccessFineLocation
    };

    private readonly Context _context;
    private readonly BluetoothManager _bluetoothManager;
    private bool _isPermitted;
    private int _requestId;
    private readonly Api21BleScanCallback _scanCallback;
    private ScanMode _scanMode;
    private bool _isOn;

    public AndroidBluetooth()
    {
        _context = Application.Context;
        if (!_context.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
        {
            IsAvailable = false;
            return;
        }

        var statusChangeReceiver = new BluetoothStatusBroadcastReceiver(UpdateState);
        _context.RegisterReceiver(statusChangeReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));

        _bluetoothManager = (BluetoothManager)_context.GetSystemService(Context.BluetoothService);
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

    public async Task<bool> Connect(IDevice device)
    {
        var d = (BleDevice) device;
        return await d.Connect().ConfigureAwait(false);
    }

    public void RequestPermissions()
    {
        _requestId++;
        ActivityCompat.RequestPermissions((Activity)_context, RequiredPermissions, _requestId);
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

    public void CheckPermissions()
    {
        IsPermitted =
            RequiredPermissions.Any(p => ContextCompat.CheckSelfPermission(_context, p) != Permission.Granted);
    }

    private void UpdateState(State state)
    {
        IsOn = state == State.On;
    }

    public void HandleDiscoveredDevice(IDevice device)
    {
        DeviceDiscovered?.Invoke(this, new DeviceEventArgs(device));
    }

    public void OnRequestPermissionsResult()
    {
        CheckPermissions();
    }
}