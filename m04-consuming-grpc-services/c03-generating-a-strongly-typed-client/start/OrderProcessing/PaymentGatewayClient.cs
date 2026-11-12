using System.Net;
using System.Net.Http.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace OrderProcessing;

public class PaymentGatewayClient
{
    private readonly IHttpClientFactory _factory;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public PaymentGatewayClient(IHttpClientFactory factory)
    {
        _factory = factory;

        var shouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(response =>
                response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests);

        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            // Total timeout, outermost: caps the whole operation, retries included.
            .AddTimeout(TimeSpan.FromSeconds(10))
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = shouldHandle,
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
            // Circuit breaker, between retry and the per-attempt timeout: once
            // it trips, a retry's next attempt is rejected instantly with a
            // BrokenCircuitException instead of ever reaching the gateway.
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = shouldHandle,
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30),
            })
            // Per-attempt timeout, innermost: caps a single try, so a retry can
            // still fire when it trips.
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
