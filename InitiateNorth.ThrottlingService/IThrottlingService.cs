namespace InitiateNorth.ThrottlingService
{
    public interface IThrottlingService
    {
        int PeriodInMinutes { get; }

        int RequestsPerPeriod { get; }

        bool IsThrottled(string identity);
    }
}