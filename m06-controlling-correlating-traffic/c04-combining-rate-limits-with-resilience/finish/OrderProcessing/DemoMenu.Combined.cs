using System.Diagnostics;

namespace OrderProcessing;

// m06-c04: a failing, slow gateway and a burst of whole orders, each going through
// PaymentGatewayClient the way checkout does. Prints what happened to every order, then
// the gateway's own count of the calls it received.
public partial class DemoMenu
{
    private async Task ArmDownedThenOrderBurstAsync(int count)
    {
        await PostAdminAsync("/admin/payments/reset", new { });
        await PostAdminAsync("/admin/payments/mode", new { mode = "down" });
        await PostAdminAsync("/admin/payments/latency", new { delayMs = 300 });
        Console.WriteLine($"Gateway armed: failing, and slow (300 ms per call). Firing {count} payment orders at once...");

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(1, count).Select(async i =>
        {
            try
            {
                await _payments.ChargeAsync(9.99m);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  order {i}: {ex.GetBaseException().GetType().Name} at {stopwatch.Elapsed.TotalSeconds:F1} s");
            }
        }));

        Console.WriteLine($"All {count} orders settled after {stopwatch.Elapsed.TotalSeconds:F1} s");
        await PrintGatewayLoadAsync();
        await PostAdminAsync("/admin/payments/reset", new { });
    }
}
