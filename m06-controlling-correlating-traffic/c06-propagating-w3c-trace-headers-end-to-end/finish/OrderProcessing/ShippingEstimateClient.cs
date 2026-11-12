using System.Net.Http.Json;

namespace OrderProcessing;

public class ShippingEstimateClient
{
    private readonly HttpClient _client;

    public ShippingEstimateClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<int> GetEstimateAsync(string productId)
    {
        var estimate = await _client.GetFromJsonAsync<ShippingEstimate>($"/shipping/estimate/{productId}");
        return estimate!.Days;
    }
}

public record ShippingEstimate(string Sku, int Days);
