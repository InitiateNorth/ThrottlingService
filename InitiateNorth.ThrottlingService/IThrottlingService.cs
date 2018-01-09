namespace InitiateNorth.ThrottlingService
{
    using System;

    public interface IThrottlingService : IDisposable
    {
        int PeriodInMinutes { get; }

        int RequestsPerPeriod { get; }

        bool IsThrottled(string identity);
    }
}