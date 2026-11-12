using System.Net;
using System.Threading.RateLimiting;

namespace OrderProcessing;

public class RateLimitingHandler : DelegatingHandler
{
    private readonly RateLimiter _limiter;

    public RateLimitingHandler(RateLimiter limiter)
    {
        _limiter = limiter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using RateLimitLease lease = await _limiter.AcquireAsync(1, cancellationToken);

        if (!lease.IsAcquired)
        {
            return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
