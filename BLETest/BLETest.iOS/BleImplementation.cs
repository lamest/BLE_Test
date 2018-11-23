using System;
using BLETest.iOS;
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Plugin.BLE.iOS;
using standard_lib;
using Xamarin.Forms;

[assembly: Dependency(typeof(BleImplementation))]
namespace BLETest.iOS
{
    internal class BleImplementation : BleImplementationBase, IPermissions
    {
        private CBCentralManager _centralManager;

        protected override void InitializeNative()
        {
            _centralManager = new CBCentralManager(DispatchQueue.CurrentQueue);
            _centralManager.UpdatedState += (s, e) => State = GetState();
            _centralManager.UpdatedState += (s, e) => OnRequestResult?.Invoke(null,null);
            Permissions.SetInstance(this);
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

        public void Request()
        {
        }

        public event EventHandler OnRequestResult;
        public bool Check()
        {
            return _centralManager.State != CBCentralManagerState.Unauthorized;
        }
    }
}