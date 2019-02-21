# ThrottlingService
C# .NET Helper library for using in-memory throttling on a single machine. The service can be configured to inform the caller whether they have reached the maximum number of calls within the sliding window time-frame.

## Summary
This library was created in order to provide a basic in-memory throttle for a total number of  `requestsPerPeriod /  periodInMinutes`, throttled by unique caller identities (a supplied token for example). 

The expiration time window is sliding so that the total requests over the specified period are counted from the current minute and the previous `periodInMinutes`. 

Please see the [sample file](https://github.com/InitiateNorth/ThrottlingService/blob/master/InitateNorth.ThrottlingService.Sample/Program.cs) for a demo of it's basic usage.

It should be noted that making a call to `IsThrottled `when you have not reached the throttle limit, counts towards the current total of requests you have made. Therefore, the expected usage for the code would be something like:

```C#
if (!throttlingService.IsThrottled(identity))`
{
    // Perform the action which is to be protected by throttling.
}
```

When throttled, the service will not continue to increase the number of requests when calling `IsThrottled`, so you can retry as often as is sensible in your app.

Obviously, if the app crashes, is restarted (or the machine running it) the cache will be reset.
