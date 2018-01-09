namespace InitiateNorth.ThrottlingService.Sample
{
    using System;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            var numberOfAllowedRequests = 10;
            var perNumberOfMinutes = 1;
            var callerIdentity = "our-friendly-client";

            using (var service = new ThrottlingService(numberOfAllowedRequests, perNumberOfMinutes))
            {
                // Quick loop to use up all the allowed requests.
                for (var i = 1; i <= numberOfAllowedRequests + 1; i++)
                {
                    var isThrottled = service.IsThrottled(callerIdentity);
                    Console.WriteLine($"Request #{i.ToString().PadLeft(numberOfAllowedRequests.ToString().Length, '0')} is throttled: {isThrottled}");
                }

                // Snooze while the window moves on...
                Thread.Sleep(TimeSpan.FromMinutes(perNumberOfMinutes));

                for (var i = numberOfAllowedRequests + 2; i <= numberOfAllowedRequests + numberOfAllowedRequests + 2; i++)
                {
                    var isThrottled = service.IsThrottled(callerIdentity);
                    Console.WriteLine($"Request #{i.ToString().PadLeft(numberOfAllowedRequests.ToString().Length, '0')} is throttled: {isThrottled}");
                }
            }

            Console.WriteLine("Test finished, press any key to exit");
            Console.ReadKey();
        }
    }
}