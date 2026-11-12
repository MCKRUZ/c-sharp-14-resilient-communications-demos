using System.Diagnostics;

namespace OrderProcessing;

// m06-c03 bursts. Both options read the gateway's own count of what arrived.
public partial class DemoMenu
{
    // Every payment takes 500 ms at the gateway, so calls stay in flight long enough for
    // the concurrency limiter to be the thing that holds them back.
    private async Task BurstOfSlowOrdersAsync(int count)
    {
        Console.WriteLine("Warming up the token and the connection first...");
        await SendOrderAsync();

        await PostAdminAsync("/admin/payments/reset", new { });
        await PostAdminAsync("/admin/payments/latency", new { delayMs = 500 });
        Console.WriteLine($"Every payment now takes 500 ms. Firing {count} orders at once...");

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(1, count).Select(_ => SendOrderAsync()));

        Console.WriteLine($"All {count} orders settled after {stopwatch.Elapsed.TotalSeconds:F1} s");
        await PrintGatewayLoadAsync();
        await PostAdminAsync("/admin/payments/latency", new { delayMs = 0 });
    }

    // Five orders just before one second is up, five more just after. Lined up against the
    // limiter's own clock: one warm-up payment spends a permit, and the moment the limiter
    // hands it back is the start of a fresh second.
    private async Task BurstAcrossOneSecondBoundaryAsync()
    {
        Console.WriteLine("Warming up the token and the connection first...");
        await SendOrderAsync();

        var limiter = PaymentRateLimiter.Instance;
        while (limiter.GetStatistics()!.CurrentAvailablePermits < 5)
        {
            await Task.Delay(1);
        }

        await PostAdminAsync("/admin/payments/reset", new { });
        var clock = Stopwatch.StartNew();

        await Task.Delay(TimeSpan.FromMilliseconds(880) - clock.Elapsed);
        Console.WriteLine("Sending 5 orders just before one second is up...");
        var first = Enumerable.Range(1, 5).Select(_ => SendOrderAsync()).ToArray();

        await Task.Delay(TimeSpan.FromMilliseconds(1120) - clock.Elapsed);
        Console.WriteLine("Sending 5 more a moment after...");
        var second = Enumerable.Range(1, 5).Select(_ => SendOrderAsync()).ToArray();

        await Task.WhenAll(first.Concat(second));
        Console.WriteLine($"10 orders settled after {clock.Elapsed.TotalSeconds:F1} s");
        await PrintGatewayWavesAsync();
    }
}
