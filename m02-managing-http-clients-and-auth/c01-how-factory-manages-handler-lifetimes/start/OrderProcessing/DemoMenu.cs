using System.Diagnostics;

namespace OrderProcessing;

public class DemoMenu
{
    private readonly PaymentGatewayClient _payments;
    private readonly PricingClient _pricing;
    private readonly CatalogClient _catalog;
    private readonly OrderSummaryService _orderSummary;

    public DemoMenu(
        PaymentGatewayClient payments,
        PricingClient pricing,
        CatalogClient catalog,
        OrderSummaryService orderSummary)
    {
        _payments = payments;
        _pricing = pricing;
        _catalog = catalog;
        _orderSummary = orderSummary;
    }

    public async Task RunAsync()
    {
        using var log = DemoLog.Attach();

        Console.WriteLine("Order processing demo");
        Console.WriteLine("  [1] Charge one payment");
        Console.WriteLine("  [2] Flood: 400 payments in a tight loop");
        Console.WriteLine("  [3] Look up a price");
        Console.WriteLine("  [4] Build an order summary");
        Console.WriteLine("  [5] Pricing and catalog calls");
        Console.WriteLine("  [q] Quit");

        while (true)
        {
            Console.WriteLine();
            Console.Write("> ");

            var choice = Console.ReadLine()?.Trim();
            if (choice == "q")
            {
                return;
            }

            try
            {
                await RunOptionAsync(choice);
            }
            catch (Exception ex)
            {
                var root = ex.GetBaseException();
                Console.WriteLine($"FAILED: {root.GetType().Name}: {root.Message}");
            }
        }
    }

    private async Task RunOptionAsync(string? choice)
    {
        switch (choice)
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
            case "5":
                await PricingAndCatalogCallsAsync();
                break;
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

    private async Task PricingAndCatalogCallsAsync()
    {
        Console.WriteLine("Three pricing lookups and three catalog lookups...");

        foreach (var sku in new[] { "SKU-1001", "SKU-1002", "SKU-1003" })
        {
            await ReportAsync($"pricing {sku}", async () => $"{await _pricing.GetPriceAsync(sku):C}");
            await ReportAsync($"catalog {sku}", async () => (await _catalog.GetProductAsync(sku)).Name);
        }
    }

    // One call, one line: the result, or the HTTP failure that stopped it.
    private static async Task ReportAsync(string label, Func<Task<string>> call)
    {
        try
        {
            Console.WriteLine($"  {label}: {await call()}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode is { } status)
        {
            Console.WriteLine($"  {label}: FAILED, {(int)status} {status}");
        }
    }
}
