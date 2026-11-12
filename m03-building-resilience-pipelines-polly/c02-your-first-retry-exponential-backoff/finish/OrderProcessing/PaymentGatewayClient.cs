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
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response =>
                        response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
            })
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
