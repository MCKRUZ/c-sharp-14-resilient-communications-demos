using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderProcessing;

// m06 bursts: the app-side half of the rate-limiting demos. Every burst goes straight
// through the "payments" HttpClient, so the only thing between the app and the gateway is
// whatever handlers the clip has registered, and then reads the gateway's own count of
// what it actually received (GET :5200/admin/payments/stats).
public partial class DemoMenu
{
    private async Task BurstOfOrdersAsync(int count)
    {
        await PostAdminAsync("/admin/payments/reset", new { });
        Console.WriteLine($"Firing {count} payment orders at once...");

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(1, count).Select(_ => SendOrderAsync()));

        Console.WriteLine($"All {count} orders settled after {stopwatch.Elapsed.TotalSeconds:F1} s");
        await PrintGatewayWavesAsync();
    }

    private async Task<int> SendOrderAsync()
    {
        var client = _factory.CreateClient("payments");

        try
        {
            using var response = await client.PostAsJsonAsync("/payments/charge", new { Amount = 9.99m, Currency = "USD" });
            return (int)response.StatusCode;
        }
        catch (HttpRequestException)
        {
            return 0;
        }
    }

    // The gateway's own account of the traffic, so the claim "it never sees the spike" is
    // the gateway's, not the client's.
    private async Task<JsonElement> GetGatewayStatsAsync()
    {
        using var admin = _factory.CreateClient();
        using var stream = await admin.GetStreamAsync("http://127.0.0.1:5200/admin/payments/stats");
        using var stats = await JsonDocument.ParseAsync(stream);
        return stats.RootElement.Clone();
    }

    // "Gateway saw 15 calls: 5 at +0.0 s, 5 at +1.0 s, 5 at +2.0 s": bursts held back by a
    // limiter arrive as separate waves.
    private async Task PrintGatewayWavesAsync()
    {
        var stats = await GetGatewayStatsAsync();
        var waves = stats.GetProperty("waves").EnumerateArray()
            .Select(wave => $"{wave.GetProperty("count").GetInt32()} at +{wave.GetProperty("atMs").GetInt32() / 1000.0:F1} s");

        Console.WriteLine($"Gateway saw {stats.GetProperty("total").GetInt32()} calls: {string.Join(", ", waves)}");
    }

    private async Task PrintGatewayLoadAsync()
    {
        var stats = await GetGatewayStatsAsync();

        Console.WriteLine(
            $"Gateway saw {stats.GetProperty("total").GetInt32()} calls: " +
            $"at most {stats.GetProperty("peakInFlight").GetInt32()} at once");
    }
}
