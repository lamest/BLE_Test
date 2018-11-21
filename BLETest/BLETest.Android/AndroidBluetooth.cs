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
using ScanMode = standard_lib.Bluetooth.ScanMode;

public class AndroidBluetooth : BindableBase, IBluetooth
{
    private static readonly string[] RequiredPermissions =
    {
        Manifest.Permission.Bluetooth,
        Manifest.Permission.BluetoothAdmin,
        Manifest.Permission.AccessCoarseLocation,
        Manifest.Permission.AccessFineLocation
    };

    private readonly Activity _context;
    private BluetoothManager _bluetoothManager;
    private int _requestId;
    private Api21BleScanCallback _scanCallback;
    private ScanMode _scanMode;
    private volatile BluetoothState _state;

    public AndroidBluetooth(Activity context)
    {
        _context = context;
        InitializeNative();
    }

    public ScanMode Mode
    {
        get => _scanMode;
        set => SetProperty(ref _scanMode, value);
    }

    public event DeviceDiscoveredHandler DeviceDiscovered;

    public bool IsPermitted { get; }

    public BluetoothState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
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

    public bool IsAvailable { get; private set; }

    public void RequestPermissions()
    {
        _requestId++;
        ActivityCompat.RequestPermissions(_context, RequiredPermissions, _requestId);
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

    public bool CheckAvailability()
    {
        var state = State;
        return state != BluetoothState.Unavailable &&
               state != BluetoothState.Unknown;
    }

    private bool CheckPermissions()
    {
        IsAvailable =
            RequiredPermissions.All(p => ContextCompat.CheckSelfPermission(_context, p) == Permission.Granted);
        return IsAvailable;
    }

    private void InitializeNative()
    {
        var ctx = Application.Context;
        if (!ctx.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
        {
            State = BluetoothState.Unavailable;
            return;
        }

        var statusChangeReceiver = new BluetoothStatusBroadcastReceiver(UpdateState);
        ctx.RegisterReceiver(statusChangeReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));

        _bluetoothManager = (BluetoothManager) ctx.GetSystemService(Context.BluetoothService);

        State = _bluetoothManager?.Adapter.State.ToBluetoothState() ?? BluetoothState.Unavailable;

        _scanCallback = new Api21BleScanCallback(this);
    }

    private void UpdateState(BluetoothState state)
    {
        State = state;
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