using System.Net;
using System.Net.Http.Json;
using Polly;
using Polly.Retry;

namespace OrderProcessing;

public class PaymentGatewayClient
{
    private readonly IHttpClientFactory _factory;
    private readonly ResiliencePipeline<HttpResponseMessage> _retry;

    public PaymentGatewayClient(IHttpClientFactory factory)
    {
        _factory = factory;

        _retry = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response => response.StatusCode == HttpStatusCode.ServiceUnavailable),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Constant,
            })
            .Build();
    }

    public async Task<string> ChargeAsync(decimal amount)
    {
        var client = _factory.CreateClient("payments");

        var response = await _retry.ExecuteAsync(async cancellationToken =>
            await client.PostAsJsonAsync(
                "/payments/charge",
                new { Amount = amount, Currency = "USD" },
                cancellationToken));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
