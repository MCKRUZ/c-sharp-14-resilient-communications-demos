using System.Net.Http.Json;
using Polly;
using Polly.Hedging;

namespace OrderProcessing;

public class PricingClient
{
    private readonly HttpClient _client;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public PricingClient(HttpClient client)
    {
        _client = client;
        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
            {
                MaxHedgedAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(100),
            })
            .Build();
    }

    public async Task<decimal> GetPriceAsync(string productId)
    {
        var response = await _pipeline.ExecuteAsync(async cancellationToken =>
            await _client.GetAsync($"/pricing/price/{productId}", cancellationToken));

        response.EnsureSuccessStatusCode();
        var quote = await response.Content.ReadFromJsonAsync<PriceQuote>();
        return quote!.Price;
    }
}

public record PriceQuote(string ProductId, decimal Price);
