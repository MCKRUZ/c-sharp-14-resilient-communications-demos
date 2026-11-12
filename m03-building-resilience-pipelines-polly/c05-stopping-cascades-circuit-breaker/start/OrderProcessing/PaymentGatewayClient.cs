using System.Net;
using System.Net.Http.Json;
using Polly;
using Polly.Retry;

namespace OrderProcessing;

public class PaymentGatewayClient
{
    private readonly IHttpClientFactory _factory;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public PaymentGatewayClient(IHttpClientFactory factory)
    {
        _factory = factory;
        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            // Total timeout, outermost: caps the whole operation, retries included.
            .AddTimeout(TimeSpan.FromSeconds(10))
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response =>
                        response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests),
                MaxRetryAttempts = 3,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                DelayGenerator = static args =>
                {
                    if (args.Outcome.Result?.Headers.RetryAfter?.Delta is { } retryAfter)
                    {
                        return ValueTask.FromResult<TimeSpan?>(retryAfter);
                    }

                    return ValueTask.FromResult<TimeSpan?>(null);
                },
            })
            // Per-attempt timeout, innermost: caps a single try, so a retry can
            // still fire when it trips. Order is the behavior here: swap this
            // with the total timeout above and it could cut an attempt off
            // before it ever runs.
            .AddTimeout(TimeSpan.FromSeconds(2))
            .Build();
    }

    public async Task<string> ChargeAsync(decimal amount)
    {
        var client = _factory.CreateClient("payments");

        var response = await _pipeline.ExecuteAsync(async cancellationToken =>
            await client.PostAsJsonAsync(
                "/payments/charge",
                new { Amount = amount, Currency = "USD" },
                cancellationToken));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
