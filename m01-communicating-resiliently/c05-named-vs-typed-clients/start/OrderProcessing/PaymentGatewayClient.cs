using System.Net.Http.Json;

namespace OrderProcessing;

public class PaymentGatewayClient
{
    private readonly IHttpClientFactory _factory;

    public PaymentGatewayClient(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<string> ChargeAsync(decimal amount)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "http://127.0.0.1:5201/payments/charge",
            new { Amount = amount, Currency = "USD" });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
