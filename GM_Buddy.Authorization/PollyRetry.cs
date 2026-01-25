using Polly;
using Polly.Retry;

namespace GM_Buddy.Authorization;

public class PollyRetry
{
    public readonly AsyncRetryPolicy _retryPolicy = Policy
    .Handle<Exception>() // You can narrow this to transient exceptions
    .WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
        });
}
