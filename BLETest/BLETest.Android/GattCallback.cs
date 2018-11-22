using System;
using System.Diagnostics;
using Android.Bluetooth;

namespace BLETest.Droid
{
    public class GattCallback : BluetoothGattCallback
    {
        private readonly IGattOwner _owner;

        public GattCallback(IGattOwner owner)
        {
            _owner = owner;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            if (!gatt.Device.Address.Equals(_owner.Address))
            {
                Trace.TraceInformation(
                    $"Gatt callback for device {_owner.Address} was called for device with address {gatt.Device.Address}. This shoud not happen. Please log an issue.");
                return;
            }

            Trace.TraceInformation($"OnConnectionStateChange: GattStatus: {status}");
            switch (newState)
            {
                case ProfileState.Disconnected:
                    // Close GATT regardless, else we can accumulate zombie gatts.
                    Trace.TraceInformation($"Disconnected '{_owner.Address}'");
                    _owner.Disconnected(status);
                    break;
                case ProfileState.Connecting:
                    Trace.TraceInformation("Connecting");
                    break;
                case ProfileState.Connected:
                    Trace.TraceInformation($"Connected '{_owner.Address}'");
                    _owner.Update(gatt.Device, gatt, status);
                    break;
                case ProfileState.Disconnecting:
                    Trace.TraceInformation("Disconnecting");
                    break;
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);
            Trace.TraceInformation($"OnServicesDiscovered: {status.ToString()}");
            _owner.OnServicesDiscovered(status);
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic,
            GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);
            Trace.TraceInformation("OnCharacteristicRead: value {0}; status {1}",
                characteristic.GetValue().ToHexString(), status);
            _owner.CharacteristicRead(status, characteristic);
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);
            _owner.CharacteristicChanged(characteristic);
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic,
            GattStatus status)
        {
            base.OnCharacteristicWrite(gatt, characteristic, status);
            Trace.TraceInformation("OnCharacteristicWrite: value {0} status {1}",
                characteristic.GetValue().ToHexString(), status);
            _owner.CharacteristicWrite(status, characteristic);
        }

        public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor,
            GattStatus status)
        {
            base.OnDescriptorWrite(gatt, descriptor, status);
            Trace.TraceInformation("OnDescriptorWrite: {0}", descriptor.GetValue()?.ToHexString());
            _owner.DescriptorWrite(status, descriptor);
        }

        public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
        {
            base.OnDescriptorRead(gatt, descriptor, status);
            Trace.TraceInformation("OnDescriptorRead: {0}", descriptor.GetValue()?.ToHexString());
            _owner.DescriptorRead(status, descriptor);
        }

        private Exception GetExceptionFromGattStatus(GattStatus status)
        {
            Exception exception = null;
            switch (status)
            {
                case GattStatus.Failure:
                case GattStatus.InsufficientAuthentication:
                case GattStatus.InsufficientEncryption:
                case GattStatus.InvalidAttributeLength:
                case GattStatus.InvalidOffset:
                case GattStatus.ReadNotPermitted:
                case GattStatus.RequestNotSupported:
                case GattStatus.WriteNotPermitted:
                    exception = new Exception(status.ToString());
                    break;
                case GattStatus.Success:
                    break;
            }

            return exception;
        }
    }
}