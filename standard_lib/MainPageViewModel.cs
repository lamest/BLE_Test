using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using standard_lib;
using standard_lib.Bluetooth;
using Xamarin.Forms;

namespace BLETest
{
    public class MainPageViewModel : BindableBase
    {
        private readonly TimeSpan _disappearingTime = TimeSpan.FromMinutes(3);
        public IBluetooth Bluetooth { get; }

        public MainPageViewModel()
        {
            Bluetooth = App.Bluetooth;
            if (Bluetooth.IsAvailable)
            {
                Bluetooth.DeviceDiscovered += OnDeviceDiscovered;

                Devices = new ObservableCollection<IDeviceInTest>();
                StartScanCommand = new Command(Bluetooth.Scan);
                if (Bluetooth.IsPermitted)
                {
                    Bluetooth.Scan();
                }
            }
            RequestPermissionsCommand = new Command(RequestPermissionsCommandExecute);
        }

        private void RequestPermissionsCommandExecute(object obj)
        {
            Bluetooth.RequestPermissions();
        }

        public Command RequestPermissionsCommand { get; set; }

        public Command StartScanCommand { get; set; }

        public ObservableCollection<IDeviceInTest> Devices { get; set; }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            Device.BeginInvokeOnMainThread(()=>UpdateDevices(e));
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

                if (device.DiscoveryTimer.Elapsed > _disappearingTime)
                    devicesToRemove.Add(device);
            }

            if (all)
            {
                var testDevice = CreateDevice(e.Device);
                Devices.Insert(0, testDevice);
            }

            foreach (var device in devicesToRemove)
            {
                device.DiscoveryTimer.Stop();
                device.DisconnectAsync();
                Devices.Remove(device);
            }
        }

        private IDeviceInTest CreateDevice(IDevice device)
        {
            var deviceInTest = new DeviceInTest(device, Bluetooth);
            return deviceInTest;
        }
    }

    internal class DeviceInTest : BindableBase, IDeviceInTest
    {
        private readonly IBluetooth _bluetooth;
        private int _isTestRunning;
        private bool _isTestSuccessful;
        private bool _isTesting;
        private static readonly MethodInfo[] _testMethods = typeof(Tests).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        public DeviceInTest(IDevice device, IBluetooth bluetooth)
        {
            Device = device;
            _bluetooth = bluetooth;
            DiscoveryTimer = new Stopwatch();
            DiscoveryTimer.Start();
            StartTestCommand = new Command(async () => await TestAsync().ConfigureAwait(false));
            DisconnectCommand = new Command(async () => await DisconnectAsync().ConfigureAwait(false));
            StartTestCommand.Execute(null);
        }

        public Guid ID => Device.Id;
        public string Name => Device.Name;
        public IDevice Device { get; }

        public Command StartTestCommand { get; set; }
        public Command DisconnectCommand { get; set; }

        public async Task<bool> TestAsync()
        {
            //dont run testing on second tap
            if (Interlocked.CompareExchange(ref _isTestRunning, 1, 0) == 1)
                return false;

            var result = false;
            try
            {
                IsTesting = true;

                foreach (var method in _testMethods)
                {
                    result = await (Task<bool>)method.Invoke(null, new object[] {Device, _bluetooth});
                }

                return result;
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                IsTesting = false;
                IsTestSuccessful = result;
                _isTestRunning = 0;

                try
                {
                    await _bluetooth.Disconnect(Device);
                }
                catch (Exception)
                {}
            }
            return IsTestSuccessful;
        }

        public bool IsTesting
        {
            get { return _isTesting; }
            set { SetProperty(ref _isTesting , value); }
        }

        public async Task DisconnectAsync()
        {
            await _bluetooth.Disconnect(Device).ConfigureAwait(false);
        }

        public bool IsTestSuccessful
        {
            get => _isTestSuccessful;
            set => SetProperty(ref _isTestSuccessful, value);
        }

        public Stopwatch DiscoveryTimer { get; set; }
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
        Task<bool> TestAsync();
        Task DisconnectAsync();
    }
}