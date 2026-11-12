using System.Net.Http.Json;

namespace OrderProcessing;

public class CatalogClient
{
    private readonly HttpClient _client;

    public CatalogClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<Product> GetProductAsync(string id)
    {
        var product = await _client.GetFromJsonAsync<Product>(
            $"/catalog/products/{id}");

        return product!;
    }
}
