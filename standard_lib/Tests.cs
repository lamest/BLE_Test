using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using standard_lib;
using standard_lib.Bluetooth;

namespace BLETest
{
    public static class Tests
    {
        private static readonly string ServiceTemplate = "f119{0}-71a4-11e6-bdf4-0800200c9a66";

        //private static string _defaultPass = "000000";
        private static readonly byte[] _defaultPassBytes = {0x30, 0x30, 0x30, 0x30, 0x30, 0x30};

        public static async Task Test1(IDevice device, IBluetooth bluetooth)
        {
            await bluetooth.Connect(device).ConfigureAwait(false);
            await Task.Delay(1500).ConfigureAwait(false);
            var services = await device.DiscoverServicesAsync().RunUntillAsync().ConfigureAwait(false);
            InsureServices(services);

            var movService = services.First(x => x.Id == GuidCollection.MOVEMENT_SERV_UUID);
            var chars = await movService.GetCharacteristicsAsync();
            InsureMovementServiceChars(chars);

            var output = chars.First(x => x.Id == GuidCollection.MOVEMENT_UID_UUID);
            var outputResult = await output.ReadAsync();
            if (outputResult.Length != 0) throw new Exception($"fail to read {GuidCollection.MOVEMENT_UID_UUID}");
            var passChar = chars.First(x => x.Id == GuidCollection.MOVEMENT_PASS_UUID);

            var writeResult = await passChar.WriteAsync(_defaultPassBytes).ConfigureAwait(false);
            if (!writeResult) throw new Exception($"fail to write password to {GuidCollection.MOVEMENT_PASS_UUID}");

            outputResult = await output.ReadAsync().ConfigureAwait(false);
            if (outputResult.Length == 0) throw new Exception($"fail to read {GuidCollection.MOVEMENT_UID_UUID}");
        }

        private static void InsureMovementServiceChars(IList<ICharacteristic> chars)
        {
            try
            {
                var acsData = chars.First(x => x.Id == GuidCollection.MOVEMENT_ACSDATA_UUID);
                var backSide = chars.First(x => x.Id == GuidCollection.MOVEMENT_BACKSIDE_UUID);
                var history = chars.First(x => x.Id == GuidCollection.MOVEMENT_HISTORY_UUID);
                var cmd = chars.First(x => x.Id == GuidCollection.MOVEMENT_CMD_UUID);
                var tap = chars.First(x => x.Id == GuidCollection.MOVEMENT_TAP_UUID);
                var uid = chars.First(x => x.Id == GuidCollection.MOVEMENT_UID_UUID);
                var pass = chars.First(x => x.Id == GuidCollection.MOVEMENT_PASS_UUID);
            }
            catch (Exception ex)
            {
                throw new Exception("One of MovementServiceChars not found", ex);
            }
        }

        private static void InsureServices(IList<IService> services)
        {
            var movService = services.FirstOrDefault(x => x.Id == GuidCollection.MOVEMENT_SERV_UUID);
            if (movService == default(IService))
                throw new Exception($"There is no {nameof(GuidCollection.MOVEMENT_SERV_UUID)} service");
        }

        private static class GuidCollection
        {
            // (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
            public static readonly Guid MOVEMENT_SERV_UUID = Guid.Parse(string.Format(ServiceTemplate, "6F50"));
            public static readonly Guid MOVEMENT_ACSDATA_UUID = Guid.Parse(string.Format(ServiceTemplate, "6F51"));
            public static readonly Guid MOVEMENT_BACKSIDE_UUID = Guid.Parse(string.Format(ServiceTemplate, "6F52"));
            public static readonly Guid MOVEMENT_HISTORY_UUID = Guid.Parse(string.Format(ServiceTemplate, "6F53"));
            public static readonly Guid MOVEMENT_CMD_UUID = Guid.Parse(string.Format(ServiceTemplate, "6F54"));
            public static readonly Guid MOVEMENT_TAP_UUID = Guid.Parse(string.Format(ServiceTemplate, "6F55"));
            public static readonly Guid MOVEMENT_UID_UUID = Guid.Parse(string.Format(ServiceTemplate, "6F56"));
            public static readonly Guid MOVEMENT_PASS_UUID = Guid.Parse(string.Format(ServiceTemplate, "6F57"));

//#define MOVEMENT_SERV_UUID                    0x6F50 // UID самого сервис
//#define MOVEMENT_ACSDATA_UUID             0x6F51 // UID сервиса акселерометра(сырые данные с акселерометра)
//#define MOVEMENT_BACKSIDE_UUID               0x6F52 // UID сервиса стороны кубика
//#define MOVEMENT_HISTORY_UUID             0x6F53 // UID сервиса история
//#define MOVEMENT_CMD_UUID                     0x6F54 // UID сервиса команд
//#define MOVEMENT_TAP_UUID                     0x6F55 // UID сервиса tap
//#define MOVEMENT_UID_UUID                                    0x6F56 // UID unique ID
//#define MOVEMENT_PASS_UUID                                0x6F57 // UID password

            public static readonly Guid CalibrationVersionCharachteristic =
                Guid.Parse("F1196F56-71A4-11E6-BDF4-0800200C9A66");
        }
    }
}