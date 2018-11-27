using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using standard_lib;
using Xamarin.Forms;

namespace BLETest
{
    public class MainPageViewModel : BindableBase
    {
        private readonly double _disappearingTime = 3;
        private readonly IBluetoothLE _manager;
        private bool _isBtOn;
        private bool _isPermitted;

        public MainPageViewModel()
        {
            var manager = Bluetooth.Current;
            if (!manager.IsAvailable)
                throw new Exception("BLE is not available.");

            _manager = manager;
            IsBTOn = _manager.IsOn;
            IsPermitted = Permissions.Instance.Check();
            Permissions.Instance.OnRequestResult += UpdatePermitted;
            if (IsBTOn)
                StartScan();
            _manager.Adapter.DeviceDiscovered += OnDeviceDiscovered;
            _manager.StateChanged += OnStateChanged;

            Devices = new ObservableCollection<IDeviceInTest>();
            StartScanCommand = new Command(StartStacExecute);
            RequestPermissionsCommand = new Command(RequestPermissionsCommandExecute);
        }

        public Command RequestPermissionsCommand { get; set; }

        public bool IsPermitted
        {
            get => _isPermitted;
            set => SetProperty(ref _isPermitted, value);
        }

        public Command StartScanCommand { get; set; }


        public ObservableCollection<IDeviceInTest> Devices { get; set; }

        public bool IsBTOn
        {
            get => _isBtOn;
            set => SetProperty(ref _isBtOn, value);
        }

        private void RequestPermissionsCommandExecute()
        {
            Permissions.Instance.Request();
        }

        private void UpdatePermitted(object sender, EventArgs e)
        {
            IsPermitted = Permissions.Instance.Check();
            IsBTOn = _manager.IsOn;
            if (IsBTOn)
                StartScan();
        }

        private void StartStacExecute(object obj)
        {
            StartScan();
        }

        private void StartScan()
        {
            _manager.Adapter.ScanMode = ScanMode.LowLatency;
            var cts = new CancellationTokenSource();
            _manager.Adapter.StartScanningForDevicesAsync(allowDuplicatesKey: false, cancellationToken: cts.Token);
        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => UpdateDevices(e));
        }

        private void UpdateDevices(DeviceEventArgs e)
        {
            var devicesToRemove = new List<IDeviceInTest>();
            var all = true;
            foreach (var device in Devices)
            {
                if (device.ID == e.Device.Id)
                {
                    all = false;
                    device.DiscoveryTimer.Restart();
                }

                if (device.DiscoveryTimer.Elapsed > TimeSpan.FromMinutes(_disappearingTime))
                    devicesToRemove.Add(device);
            }

            if (all)
            {
                var testDevice = CreateDevice(e.Device);
                Devices.Insert(0, testDevice);
            }

            foreach (var device in devicesToRemove)
            {
                _manager.Adapter.DisconnectDeviceAsync(device.Device);
                Devices.Remove(device);
            }
        }

        private IDeviceInTest CreateDevice(IDevice device)
        {
            var deviceInTest = new DeviceInTest(device, _manager.Adapter);
            return deviceInTest;
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            IsBTOn = e.NewState == BluetoothState.On;
            if (IsBTOn)
                StartScan();
        }
    }

    internal class DeviceInTest : BindableBase, IDeviceInTest
    {
        private static readonly MethodInfo[] _testMethods = typeof(Tests).GetMethods(BindingFlags.Public | BindingFlags.Static);
        private readonly IAdapter _adapter;
        private string _error;
        private bool _isTesting;
        private volatile int _isTestRunning;
        private bool _isTestSuccessful;
        private CancellationTokenSource _tokenSource;


        public DeviceInTest(IDevice device, IAdapter adapter)
        {
            Device = device;
            _adapter = adapter;
            DiscoveryTimer = new Stopwatch();
            DiscoveryTimer.Start();
            StartTestCommand = new Command(async () => await TestAsync().ConfigureAwait(false));
            DisconnectCommand = new Command(async () => await DisconnectAsync().ConfigureAwait(false));
        }

        public bool IsTesting
        {
            get => _isTesting;
            set => SetProperty(ref _isTesting, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }


        public Guid ID => Device.Id;
        public string Name => Device.Name;
        public IDevice Device { get; }

        public Command StartTestCommand { get; set; }
        public Command DisconnectCommand { get; set; }

        public async Task TestAsync()
        {
            if (Interlocked.CompareExchange(ref _isTestRunning, 1, 0) == 1)
                return;

            _tokenSource = new CancellationTokenSource(30000);

            var tcs = new TaskCompletionSource<bool>();

            try
            {
                _tokenSource.Token.Register(() => tcs.TrySetCanceled());

                Analytics.TrackEvent(
                    TrackerEvents.TestStarted,
                    new Dictionary<string, string>
                    {
                        {"ID", Device.Id.ToString()},
                        {"Name", Device.Name}
                    });

                IsTesting = true;

                var connectionTask = _adapter.ConnectToDeviceAsync(Device);
                if (await Task.WhenAny(tcs.Task, connectionTask).ConfigureAwait(false) == connectionTask)
                {
                    await connectionTask;
                }
                else
                {
                    connectionTask.SuppressExceptions();
                    await _adapter.DisconnectDeviceAsync(Device).ConfigureAwait(false);
                    throw new TestException("Timeout");
                }

                await Task.Delay(1500).ConfigureAwait(false);
                foreach (var method in _testMethods)
                {
                    var task = (Task) method.Invoke(null, new object[] {Device, _adapter});
                    if (await Task.WhenAny(tcs.Task, task).ConfigureAwait(false) == task)
                    {
                        await task;
                    }
                    else
                    {
                        task.SuppressExceptions();
                        await _adapter.DisconnectDeviceAsync(Device).ConfigureAwait(false);
                        throw new TestException("Timeout");
                    }
                }

                Error = string.Empty;
                IsTestSuccessful = true;

                Analytics.TrackEvent(
                    TrackerEvents.TestSuccessful,
                    new Dictionary<string, string>
                    {
                        {"ID", Device.Id.ToString()},
                        {"Name", Device.Name}
                    });
            }
            catch (TestException ex1)
            {
                Error = ex1.Message;
            }
            catch (Exception ex)
            {
                Error = "Phone error: " + ex.Message;
            }
            finally
            {
                tcs.TrySetCanceled();
                if (Error != string.Empty)
                    Analytics.TrackEvent(
                        TrackerEvents.TestError,
                        new Dictionary<string, string>
                        {
                            {"ID", Device.Id.ToString()},
                            {"Name", Device.Name}
                        });
                IsTesting = false;
                _isTestRunning = 0;
                try
                {
                    await _adapter.DisconnectDeviceAsync(Device);
                }
                catch (Exception)
                {
                }
            }
        }

        public async Task DisconnectAsync()
        {
            _tokenSource.Cancel();
            await _adapter.DisconnectDeviceAsync(Device).ConfigureAwait(false);
        }

        public bool IsTestSuccessful
        {
            get => _isTestSuccessful;
            set => SetProperty(ref _isTestSuccessful, value);
        }

        public Stopwatch DiscoveryTimer { get; set; }
    }

    public static class TrackerEvents
    {
        public static string TestError = "TestError";
        public static string TestSuccessful = "TestSuccessful";
        public static string TestStarted = "TestStarted";
    }

    public interface IDeviceInTest
    {
        Guid ID { get; }
        string Name { get; }
        IDevice Device { get; }
        Command StartTestCommand { get; set; }
        Command DisconnectCommand { get; set; }
        bool IsTestSuccessful { get; }
        Stopwatch DiscoveryTimer { get; set; }
        Task TestAsync();
        Task DisconnectAsync();
    }
}