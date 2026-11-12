using System.Net.Http.Json;

namespace OrderProcessing;

public class DiscountClient
{
    private readonly HttpClient _client;

    public DiscountClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<decimal> GetDiscountAsync(string productId)
    {
        var quote = await _client.GetFromJsonAsync<DiscountQuote>($"/discounts/{productId}");
        return quote!.Percent;
    }
}

public record DiscountQuote(string Sku, decimal Percent);
