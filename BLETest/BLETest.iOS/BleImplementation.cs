using BLETest.iOS;
using CoreBluetooth;
using CoreFoundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Plugin.BLE.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(BleImplementation))]
namespace BLETest.iOS
{
    internal class BleImplementation : BleImplementationBase
    {
        private CBCentralManager _centralManager;

        protected override void InitializeNative()
        {
            _centralManager = new CBCentralManager(DispatchQueue.CurrentQueue);
            _centralManager.UpdatedState += (s, e) => State = GetState();
        }

        protected override BluetoothState GetInitialStateNative()
        {
            return GetState();
        }

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter(_centralManager);
        }

        private BluetoothState GetState()
        {
            return _centralManager.State.ToBluetoothState();
        }
    }
}