using System.Net.Http.Json;

namespace OrderProcessing;

public class PricingClient
{
    private readonly HttpClient _client;

    public PricingClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<decimal> GetPriceAsync(string productId)
    {
        var quote = await _client.GetFromJsonAsync<PriceQuote>(
            $"/pricing/price/{productId}");

        return quote!.Price;
    }
}

public record PriceQuote(string ProductId, decimal Price);
