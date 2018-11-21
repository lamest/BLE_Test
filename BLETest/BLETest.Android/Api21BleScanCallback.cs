using System.Diagnostics;
using Android.Bluetooth.LE;

namespace BLETest.Droid
{
    public class Api21BleScanCallback : ScanCallback
    {
        private readonly AndroidBluetooth _bluetooth;

        public Api21BleScanCallback(AndroidBluetooth bluetooth)
        {
            _bluetooth = bluetooth;
        }

        public override void OnScanFailed(ScanFailure errorCode)
        {
            Trace.TraceInformation("Adapter: Scan failed with code {0}", errorCode);
            base.OnScanFailed(errorCode);
        }

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            /* Might want to transition to parsing the API21+ ScanResult, but sort of a pain for now 
            List<AdvertisementRecord> records = new List<AdvertisementRecord>();
            records.Add(new AdvertisementRecord(AdvertisementRecordType.Flags, BitConverter.GetBytes(result.ScanRecord.AdvertiseFlags)));
            if (!string.IsNullOrEmpty(result.ScanRecord.DeviceName))
            {
                records.Add(new AdvertisementRecord(AdvertisementRecordType.CompleteLocalName, Encoding.UTF8.GetBytes(result.ScanRecord.DeviceName)));
            }
            for (int i = 0; i < result.ScanRecord.ManufacturerSpecificData.Size(); i++)
            {
                int key = result.ScanRecord.ManufacturerSpecificData.KeyAt(i);
                var arr = result.ScanRecord.GetManufacturerSpecificData(key);
                byte[] data = new byte[arr.Length + 2];
                BitConverter.GetBytes((ushort)key).CopyTo(data,0);
                arr.CopyTo(data, 2);
                records.Add(new AdvertisementRecord(AdvertisementRecordType.ManufacturerSpecificData, data));
            }

            foreach(var uuid in result.ScanRecord.ServiceUuids)
            {
                records.Add(new AdvertisementRecord(AdvertisementRecordType.UuidsIncomplete128Bit, uuid.Uuid.));
            }

            foreach(var key in result.ScanRecord.ServiceData.Keys)
            {
                records.Add(new AdvertisementRecord(AdvertisementRecordType.ServiceData, result.ScanRecord.ServiceData));
            }*/

            var device = new BleDevice(result.Device, result.Rssi, result.ScanRecord);

            //Device device;
            //if (result.ScanRecord.ManufacturerSpecificData.Size() > 0)
            //{
            //    int key = result.ScanRecord.ManufacturerSpecificData.KeyAt(0);
            //    byte[] mdata = result.ScanRecord.GetManufacturerSpecificData(key);
            //    byte[] mdataWithKey = new byte[mdata.Length + 2];
            //    BitConverter.GetBytes((ushort)key).CopyTo(mdataWithKey, 0);
            //    mdata.CopyTo(mdataWithKey, 2);
            //    device = new Device(result.Device, null, null, result.Rssi, mdataWithKey);
            //}
            //else
            //{
            //    device = new Device(result.Device, null, null, result.Rssi, new byte[0]);
            //}

            _bluetooth.HandleDiscoveredDevice(device);
        }
    }
}