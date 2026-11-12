using System.Diagnostics;

namespace OrderProcessing;

public class DemoMenu
{
    private readonly PaymentGatewayClient _payments;
    private readonly PricingClient _pricing;
    private readonly OrderSummaryService _orderSummary;

    public DemoMenu(
        PaymentGatewayClient payments,
        PricingClient pricing,
        OrderSummaryService orderSummary)
    {
        _payments = payments;
        _pricing = pricing;
        _orderSummary = orderSummary;
    }

    public async Task RunAsync()
    {
        using var log = DemoLog.Attach();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Order processing demo");
            Console.WriteLine("  [1] Charge one payment");
            Console.WriteLine("  [2] Flood: 400 payments in a tight loop");
            Console.WriteLine("  [3] Look up a price");
            Console.WriteLine("  [4] Build an order summary");
            Console.WriteLine("  [q] Quit");
            Console.Write("> ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    await ChargeOneAsync();
                    break;
                case "2":
                    await FloodAsync();
                    break;
                case "3":
                    await LookUpPriceAsync();
                    break;
                case "4":
                    await BuildOrderSummaryAsync();
                    break;
                case "q":
                    return;
            }
        }
    }

    private async Task ChargeOneAsync()
    {
        var receipt = await _payments.ChargeAsync(49.99m);
        Console.WriteLine($"Payment approved: {receipt}");
    }

    private async Task FloodAsync()
    {
        Console.WriteLine("Firing 400 payment calls...");
        var stopwatch = Stopwatch.StartNew();

        for (var i = 1; i <= 400; i++)
        {
            try
            {
                await _payments.ChargeAsync(9.99m);
            }
            catch (Exception ex)
            {
                var root = ex.GetBaseException();
                Console.WriteLine();
                Console.WriteLine($"Call {i} FAILED after {stopwatch.Elapsed.TotalSeconds:F1}s:");
                Console.WriteLine($"  {root.GetType().Name}: {root.Message}");
                return;
            }

            if (i % 50 == 0)
            {
                Console.WriteLine($"  {i} calls completed...");
            }
        }

        Console.WriteLine($"All 400 calls completed in {stopwatch.Elapsed.TotalSeconds:F1}s with no errors.");
    }

    private async Task LookUpPriceAsync()
    {
        var price = await _pricing.GetPriceAsync("SKU-1002");
        Console.WriteLine($"SKU-1002 price: {price:C}");
    }

    private async Task BuildOrderSummaryAsync()
    {
        var summary = await _orderSummary.BuildSummaryAsync("ORD-20260716-001");
        Console.WriteLine(summary);
    }
}
