using System.Net.Http.Json;

namespace OrderProcessing;

public class PaymentGatewayClient
{
    public async Task<string> ChargeAsync(decimal amount)
    {
        using var client = new HttpClient();

        var response = await client.PostAsJsonAsync(
            "http://127.0.0.1:5201/payments/charge",
            new { Amount = amount, Currency = "USD" });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
