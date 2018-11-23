using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using standard_lib;

namespace BLETest
{
    public static partial class Tests
    {
        private static readonly string ServiceTemplate = "f119{0}-71a4-11e6-bdf4-0800200c9a66";

        //private static string _defaultPass = "000000";
        private static readonly byte[] _defaultPassBytes = {0x30, 0x30, 0x30, 0x30, 0x30, 0x30};

        private static readonly byte[] _manufacturerIDData = {0xff, 0xff};

        public static async Task Test1(IDevice device, IAdapter adapter)
        {
            InsureVersion(device);

            var services = await device.GetServicesAsync().RunUntillAsync().ConfigureAwait(false);
            InsureServices(services);

            var timeTrackerService = services.First(x => x.Id == GuidCollection.TimeTrackerService.ServiceId);
            var timeTrackerServiceChars = await timeTrackerService.GetCharacteristicsAsync().ConfigureAwait(false);
            InsureTimeTrackerService(timeTrackerServiceChars);

            var versionService = services.First(x => x.Id == GuidCollection.VersionService.ServiceId);
            var versionServiceChars = await versionService.GetCharacteristicsAsync().ConfigureAwait(false);
            InsureVersionService(versionServiceChars);

            var accelerometerCharacteristic = timeTrackerServiceChars.First(x => x.Id == GuidCollection.TimeTrackerService.AccelerometerCharacteristic);
            var outputResult = await accelerometerCharacteristic.ReadAsync().ConfigureAwait(false);
            if (outputResult.Length != 0) throw new TestException("Passwords are not guarding AccelerometerCharacteristic");

            var passChar = timeTrackerServiceChars.First(x => x.Id == GuidCollection.TimeTrackerService.PasswordCharacteristic);
            var writeResult = await passChar.WriteAsync(_defaultPassBytes).ConfigureAwait(false);
            if (!writeResult) throw new TestException("fail to write password to PasswordCharacteristic");

            outputResult = await accelerometerCharacteristic.ReadAsync().ConfigureAwait(false);
            if (outputResult.Length == 0) throw new TestException("Default password is not accepted");

            var calibrationCharacteristic = timeTrackerServiceChars.First(x => x.Id == GuidCollection.TimeTrackerService.CalibrationVersionCharachteristic);
            await InsureCalibrationVersion(calibrationCharacteristic).ConfigureAwait(false);

            await InsureHistory(timeTrackerServiceChars).ConfigureAwait(false);

            await InsureFlags(timeTrackerServiceChars);
        }

        private static async Task InsureFlags(IList<ICharacteristic> timeTrackerServiceChars)
        {
            var commandInputCharacteristic = timeTrackerServiceChars.First(x => x.Id == GuidCollection.TimeTrackerService.CommandInputCharacteristic);
            var commandOutputCharacteristic = timeTrackerServiceChars.First(x => x.Id == GuidCollection.TimeTrackerService.CommandOutputCharacteristic);

            var result = await SendCommand(commandInputCharacteristic, CommandCodes.Status).ConfigureAwait(false);
            if (!result) throw new TestException("Fail to send command");

            var output = await commandOutputCharacteristic.ReadAsync().ConfigureAwait(false);
            if (output == null) throw new TestException("Fail to read status");

            try
            {
                var t = new DeviceStatus
                {
                    AutopauseTimeout = BitConverter.ToUInt16(output, 2),
                    LockStatus = (OutputAnswers) output[0] == OutputAnswers.Enabled,
                    PauseStatus = (OutputAnswers) output[1] == OutputAnswers.Enabled
                };
            }
            catch (Exception)
            {
                throw new TestException("Status flags is invalid");
            }
        }

        private static async Task<bool> SendCommand(ICharacteristic commandInputChar, CommandCodes command, byte[] data = null)
        {
            var payloadLength = 1 + (data?.Length).GetValueOrDefault();
            var payload = new List<byte>(payloadLength) {(byte) command};
            if (data != null)
                payload.AddRange(data);
            return await commandInputChar.WriteAsync(payload.ToArray()).ConfigureAwait(false);
        }

        private static async Task<byte[]> ReadRawHistory(ICharacteristic commandOutputCharacteristic)
        {
            var parcelLength = 21;
            var bytesPerInterval = 3;
            var maxBufferLength = 50000 * bytesPerInterval;

            var buffer = new List<byte>(parcelLength * 2);
            byte[] currentParcel = null;
            while (true)
            {
                currentParcel = await commandOutputCharacteristic.ReadAsync().ConfigureAwait(false);
                if (currentParcel == null)
                    return null;
                if (currentParcel.All(x => x == 0))
                    break;
                buffer.AddRange(currentParcel);
                if (buffer.Count > maxBufferLength - parcelLength)
                    return null;
            }

            if (buffer.Count == 0 || buffer.Count < parcelLength * 2)
                return null;
#if DEBUG
            Console.WriteLine("got from device: [{0}]", string.Join(", ", buffer));
#endif

            var infoIndex = buffer.Count - parcelLength;
            var infoBytes = new byte[2];
            infoBytes[0] = buffer[infoIndex];
            infoBytes[1] = buffer[infoIndex + 1];
            short intervalCount = 0;
            intervalCount = BitConverter.ToInt16(infoBytes, 0);
            if (intervalCount <= 0 || buffer.Count < intervalCount * bytesPerInterval)
                return null;
            return buffer.Take(intervalCount * 3).ToArray();
        }

        private static async Task InsureHistory(IEnumerable<ICharacteristic> timeTrackerServiceChars)
        {
            var commandInputCharacteristic = timeTrackerServiceChars.First(x => x.Id == GuidCollection.TimeTrackerService.CommandInputCharacteristic);
            var commandOutputCharacteristic = timeTrackerServiceChars.First(x => x.Id == GuidCollection.TimeTrackerService.CommandOutputCharacteristic);
            var result = await SendCommand(commandInputCharacteristic, CommandCodes.InitHistory).ConfigureAwait(false);
            if (!result)
                throw new TestException("Fail to send command");

            var historyBytes = await ReadRawHistory(commandOutputCharacteristic).ConfigureAwait(false);
            if (historyBytes == null)
                throw new TestException("Fail to read raw history");
#if DEBUG
            Console.WriteLine("got history: [{0}]", string.Join(", ", historyBytes));
#endif

            var parsedHistory = new History();
            parsedHistory.AcquireTime = DateTimeOffset.UtcNow;
            parsedHistory.Intervals = History.ParseBytes(historyBytes);
            if (parsedHistory.Intervals != null)
            {
                var builder = new StringBuilder();
                builder.Append("history is parsed:");
                foreach (var interval in parsedHistory.Intervals)
                    builder.Append($"{interval.SideNumber}:{interval.Time.TotalSeconds}s, ");
                Console.WriteLine(builder.ToString());
            }
            else
            {
                throw new TestException("Fail to parse history");
            }
        }

        private static async Task InsureCalibrationVersion(ICharacteristic calibrationCharacteristic)
        {
            var data = await calibrationCharacteristic.ReadAsync().ConfigureAwait(false);
            if (data == null && data.Length != 4) throw new TestException("Fail to read CalibrationVersionCharachteristic");
        }

        private static void InsureVersion(IDevice device)
        {
            var manufacturerRecord = device.AdvertisementRecords.FirstOrDefault(x => x.Type == AdvertisementRecordType.ManufacturerSpecificData);
            if (manufacturerRecord == null)
                throw new TestException("ManufacturerSpecificData not found");
            var bytes = manufacturerRecord.Data;
            var skipLength = _manufacturerIDData.Length;
            if (bytes == null || bytes.Length < skipLength)
                throw new TestException("ManufacturerSpecificData is invalid");
            var manufacturerIdBytes = bytes.Take(skipLength).ToArray();
            if (manufacturerIdBytes[0] != _manufacturerIDData[0] ||
                manufacturerIdBytes[1] != _manufacturerIDData[1])
            {
                throw new TestException("ManufacturerSpecificData is invalid");
            }
            var versionBytes = bytes.Skip(skipLength).ToArray();
            var version = string.Empty;
            try
            {
                version = Encoding.ASCII.GetString(versionBytes, 0, versionBytes.Length);
            }
            catch (Exception)
            {
            }

            if (version == string.Empty) throw new TestException("ManufacturerSpecificData is invalid");
        }

        private static void InsureTimeTrackerService(IList<ICharacteristic> chars)
        {
            try
            {
                var t = chars.First(x => x.Id == GuidCollection.TimeTrackerService.AccelerometerCharacteristic);
                t = chars.First(x => x.Id == GuidCollection.TimeTrackerService.CurrentSideCharacteristic);
                t = chars.First(x => x.Id == GuidCollection.TimeTrackerService.CommandOutputCharacteristic);
                t = chars.First(x => x.Id == GuidCollection.TimeTrackerService.CommandInputCharacteristic);
                t = chars.First(x => x.Id == GuidCollection.TimeTrackerService.TapCharacteristic);
                t = chars.First(x => x.Id == GuidCollection.TimeTrackerService.UniqueIDCharacteristic);
                t = chars.First(x => x.Id == GuidCollection.TimeTrackerService.PasswordCharacteristic);
                t = chars.First(x => x.Id == GuidCollection.TimeTrackerService.CalibrationVersionCharachteristic);
            }
            catch (Exception ex)
            {
                throw new TestException("One of TimeTrackerService chars not found", ex);
            }
        }

        private static void InsureVersionService(IList<ICharacteristic> chars)
        {
            try
            {
                var t = chars.First(x => x.Id == GuidCollection.VersionService.VersionCharacteristic);
            }
            catch (Exception ex)
            {
                throw new TestException("One of VersionService chars not found", ex);
            }
        }

        private static void InsureServices(IList<IService> services)
        {
            var timeTrackerService = services.FirstOrDefault(x => x.Id == GuidCollection.TimeTrackerService.ServiceId);
            if (timeTrackerService == default(IService))
                throw new TestException($"There is no {nameof(GuidCollection.TimeTrackerService)} service");
            var versionService = services.FirstOrDefault(x => x.Id == GuidCollection.VersionService.ServiceId);
            if (versionService == default(IService))
                throw new TestException($"There is no {nameof(GuidCollection.VersionService)} service");
        }

        private static class GuidCollection //v3.0
        {
            public static class TimeTrackerService
            {
                public static readonly Guid ServiceId = Guid.Parse("F1196F50-71A4-11E6-BDF4-0800200C9A66");
                public static readonly Guid AccelerometerCharacteristic = Guid.Parse("F1196F51-71A4-11E6-BDF4-0800200C9A66");
                public static readonly Guid CurrentSideCharacteristic = Guid.Parse("F1196F52-71A4-11E6-BDF4-0800200C9A66");
                public static readonly Guid CommandOutputCharacteristic = Guid.Parse("F1196F53-71A4-11E6-BDF4-0800200C9A66");
                public static readonly Guid CommandInputCharacteristic = Guid.Parse("F1196F54-71A4-11E6-BDF4-0800200C9A66");
                public static readonly Guid TapCharacteristic = Guid.Parse("F1196F55-71A4-11E6-BDF4-0800200C9A66");
                public static readonly Guid UniqueIDCharacteristic = Guid.Parse("F1196F56-71A4-11E6-BDF4-0800200C9A66");
                public static readonly Guid PasswordCharacteristic = Guid.Parse("F1196F57-71A4-11E6-BDF4-0800200C9A66");
                public static readonly Guid CalibrationVersionCharachteristic = Guid.Parse("F1196F56-71A4-11E6-BDF4-0800200C9A66");
            }

            public static class VersionService
            {
                public static readonly Guid ServiceId = Guid.Parse("0000180a-0000-1000-8000-00805f9b34fb");
                public static readonly Guid VersionCharacteristic = Guid.Parse("00002a26-0000-1000-8000-00805f9b34fb");
            }
        }
    }
}