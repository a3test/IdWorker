using System;

namespace SnowflakeTest
{
    public class IdWorker
    {
        private static object _lockkey = new object();
        private static DateTime dt = new DateTime();

        private long _twepoch = 1451606400000L; /*2016-01-01*/

        private const int _workerIdBits = 5;
        private const int _datacenterIdBits = 5;
        private const int _sequenceBits = 12;

        private long _workerId;
        private long _datacenterId;
        private long _sequence;

        private long _maxWorkerId = -1L ^ (-1L << _workerIdBits);
        private long _maxDatacenterId = -1L ^ (-1L << _datacenterIdBits);
        private long _sequenceMask = -1L ^ (-1L << _sequenceBits);

        private int _workerIdShift = _sequenceBits;
        private int _datacenterIdShift = _sequenceBits + _workerIdBits;
        private int _timestampLeftShift = _sequenceBits + _workerIdBits + _datacenterIdBits;

        private long _lastTimestamp = -1L;

        public IdWorker(long workerId, long datacenterId)
        {
            if (workerId > _maxWorkerId || workerId < 0)
            {
                throw new Exception(string.Format("worker Id can't be greater than {0} or less than 0", _maxWorkerId));
            }
            if (datacenterId > _maxDatacenterId || datacenterId < 0)
            {
                throw new Exception(string.Format("datacenter Id can't be greater than {0} or less than 0", _maxWorkerId));
            }
            _workerId = workerId;
            _datacenterId = datacenterId;
        }

        public long NextId()
        {
            lock (_lockkey)
            {
                long timestamp = TimeGen();
                if (timestamp < _lastTimestamp)
                {
                    throw new Exception(string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds",
                        _lastTimestamp - timestamp));
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & _sequenceMask;
                    if (_sequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0L;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - _twepoch) << _timestampLeftShift) | (_datacenterId << _datacenterIdShift) |
                       (_workerId << _workerIdShift) | _sequence;
            }
        }

        protected long TimeGen()
        {
            return dt.CurrentTimeMillis();
        }

        protected long TilNextMillis(long lastTimestamp)
        {
            long timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }
    }

    public static class DateTimeExtensions
    {
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// java currentTimeMillis
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long CurrentTimeMillis(this DateTime dt)
        {
            return (long) ((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
        }
    }
}