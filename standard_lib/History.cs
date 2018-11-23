using System;
using System.Collections.Generic;
using System.Linq;

namespace standard_lib
{
    public class History
    {
        private readonly TimeSpan MaxDiff = TimeSpan.FromSeconds(1.5);
        public List<Interval> Intervals { get; set; }
        public DateTimeOffset AcquireTime { get; set; }

        public static List<Interval> ParseBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length % 3 != 0)
                return null;
            var retList = new List<Interval>();
            for (var interval = 0; interval < bytes.Length; interval += 3)
            {
                var sideByte = bytes[interval + 2];
                var sideNumber = (sideByte >> 2) - 1;
                var intervalBytes = new byte[]
                    {bytes[interval], bytes[interval + 1], (byte) (bytes[interval + 2] & 0b0000_0011), 0};
                var timeInSeconds = BitConverter.ToInt32(intervalBytes, 0);
                var time = TimeSpan.FromSeconds(timeInSeconds);
                retList.Add(new Interval {SideNumber = sideNumber, Time = time});
            }

            retList.Reverse();
            return retList;
        }

        /// <summary>
        ///     removes allready merged history. history that is later then latest interval from db. Also move history to
        ///     compensate transmit time.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="latestInterval"></param>
        public void FixHistory(DateTimeOffset lastSyncTime)
        {
            if (AcquireTime < lastSyncTime) Intervals.Clear();

            var historyStartTime = AcquireTime;
            //remove all old intervals
            for (var i = 0; i < Intervals.Count; i++)
            {
                var historyInterval = Intervals[i];
                //go back untill we get later than latestInterval
                historyStartTime -= historyInterval.Time;
                if (historyStartTime < lastSyncTime)
                {
                    var startIndex = i + 1; //delete all but this
                    Intervals.RemoveRange(startIndex, Intervals.Count - startIndex);
                    break;
                }
            }

            var tickSumm = Intervals.Sum(x => x.Time.Ticks);
            var timeInHistory = TimeSpan.FromTicks(tickSumm);

            var timeToSync = AcquireTime - lastSyncTime;
            var diff = timeInHistory - timeToSync;

            if (diff > TimeSpan.Zero || //need to cut
                diff < TimeSpan.Zero && Math.Abs(diff.Ticks) < MaxDiff.Ticks) //or no need to cut but need to move
            {
                var lastInterval = Intervals.LastOrDefault();
                if (lastInterval != null)
                {
                    if (diff > MaxDiff)
                        lastInterval.Time -= diff; //cut
                    else
                        AcquireTime += diff; //move
                }
            }

            MergeIntervals();
        }

        private void MergeIntervals()
        {
            if (Intervals.Count == 0) return;

            var removeList = new List<Interval>(Intervals.Count);
            var lastUniqInterval = Intervals.First();
            //search dupes
            for (var i = 0; i < Intervals.Count - 1; i++)
            {
                var currentInterval = Intervals[i];
                var nextInterval = Intervals[i + 1];

                if (currentInterval.SideNumber == nextInterval.SideNumber)
                {
                    //save dupe time
                    lastUniqInterval.Time += nextInterval.Time;
                    removeList.Add(nextInterval);
                }
                else
                {
                    lastUniqInterval = currentInterval;
                }
            }

            //remove dupes
            foreach (var interval in removeList) Intervals.Remove(interval);
        }
    }
}