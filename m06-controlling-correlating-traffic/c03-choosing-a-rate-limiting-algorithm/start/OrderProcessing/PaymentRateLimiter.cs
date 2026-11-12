using System.Threading.RateLimiting;

namespace OrderProcessing;

public static class PaymentRateLimiter
{
    public static readonly RateLimiter Instance = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
    {
        TokenLimit = 5,
        TokensPerPeriod = 5,
        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
        QueueLimit = 10,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    });
}
