namespace InitiateNorth.ThrottlingService
{
    using System.Security.Cryptography;
    using System.Runtime.Caching;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class ThrottlingService : IThrottlingService, IDisposable
    {
        private const int SingleRequest = 1;

        private static readonly MemoryCache Cache = MemoryCache.Default;

        private readonly AutoResetEvent _cacheLock = new AutoResetEvent(true);

        private bool _isDisposed;

        public int RequestsPerPeriod { get; }

        public int PeriodInMinutes { get; }

        public ThrottlingService(int requestsPerPeriod, int periodInMinutes)
        {
            RequestsPerPeriod = requestsPerPeriod;
            PeriodInMinutes = periodInMinutes;
        }

        public bool IsThrottled(string identity)
        {
            _cacheLock?.WaitOne();

            var isThrottled = true;

            try
            {
                if (GetSumOfRequests(identity) < RequestsPerPeriod)
                {
                    var currentKey = GetCurrentKey(GetHashedIdentity(identity));
                    var newNumOfRequests = Cache.Contains(currentKey) ? (int)Cache.Get(currentKey) + SingleRequest : SingleRequest;
                    Cache.Set(currentKey, newNumOfRequests, new DateTimeOffset(DateTime.UtcNow.AddMinutes(PeriodInMinutes)));
                    isThrottled = false;
                }
            }
            finally
            {
                _cacheLock?.Set();
            }

            return isThrottled;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                Cache?.Dispose();
                _cacheLock?.Close();
            }

            _isDisposed = true;
        }

        private static string GetCurrentKey(string hashedIdentity) => $"{hashedIdentity}-{DateTime.UtcNow:HHmm}";

        private static string GetHashedIdentity(string identity)
        {
            using (var md5 = MD5.Create())
            {
                var hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(identity));
                var result = new StringBuilder(hashedBytes.Length * 2);

                foreach (var hashedByte in hashedBytes)
                {
                    result.Append(hashedByte.ToString("x2"));
                }

                return result.ToString();
            }
        }

        private int GetSumOfRequests(string identity) => GetKeysToCheck(GetHashedIdentity(identity)).Where(k => Cache.Contains(k)).Select(k => (int)Cache.Get(k)).Sum();

        private IEnumerable<string> GetKeysToCheck(string hashedIdentity) => Enumerable.Range(0, PeriodInMinutes).Select(minute => DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(minute)).ToString("HHmm")).Select(d => $"{hashedIdentity}-{d}");
    }
}
