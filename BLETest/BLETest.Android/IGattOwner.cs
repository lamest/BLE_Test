using Android.Bluetooth;

namespace BLETest.Droid
{
    public interface IGattOwner
    {
        string Address { get; }
        void Update(BluetoothDevice nativeDevice, BluetoothGatt gatt, GattStatus status);
        void Disconnected(GattStatus status);
        void OnServicesDiscovered(GattStatus status);
        void CharacteristicRead(GattStatus status, BluetoothGattCharacteristic characteristic);
        void CharacteristicChanged(BluetoothGattCharacteristic characteristic);
        void CharacteristicWrite(GattStatus status, BluetoothGattCharacteristic characteristic);
        void DescriptorWrite(GattStatus status, BluetoothGattDescriptor descriptor);
    }
}