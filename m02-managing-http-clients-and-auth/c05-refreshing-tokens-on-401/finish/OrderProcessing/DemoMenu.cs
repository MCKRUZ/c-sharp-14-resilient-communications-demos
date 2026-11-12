using System.Diagnostics;

namespace OrderProcessing;

public class DemoMenu
{
    private readonly PaymentGatewayClient _payments;
    private readonly PricingClient _pricing;
    private readonly CatalogClient _catalog;
    private readonly OrderSummaryService _orderSummary;
    private readonly IHttpClientFactory _factory;

    public DemoMenu(
        PaymentGatewayClient payments,
        PricingClient pricing,
        CatalogClient catalog,
        OrderSummaryService orderSummary,
        IHttpClientFactory factory)
    {
        _payments = payments;
        _pricing = pricing;
        _catalog = catalog;
        _orderSummary = orderSummary;
        _factory = factory;
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
        Console.WriteLine("  [6] Payment and catalog calls");
        Console.WriteLine("  [7] Burst: 12 calls across payments, pricing, catalog");
        Console.WriteLine("  [8] Rotate the auth keys, then charge with the cached token");
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
            case "6":
                await PaymentAndCatalogCallsAsync();
                break;
            case "7":
                await BurstAsync();
                break;
            case "8":
                await RotateKeysThenChargeAsync();
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

    private async Task PaymentAndCatalogCallsAsync()
    {
        Console.WriteLine("Two payments and two catalog lookups...");

        await ReportAsync("charge 19.99", () => _payments.ChargeAsync(19.99m));
        await ReportAsync("catalog SKU-1001", async () => (await _catalog.GetProductAsync("SKU-1001")).Name);
        await ReportAsync("charge 49.99", () => _payments.ChargeAsync(49.99m));
        await ReportAsync("catalog SKU-1003", async () => (await _catalog.GetProductAsync("SKU-1003")).Name);
    }

    private async Task BurstAsync()
    {
        Console.WriteLine("Firing 12 calls, four through each client, the way a checkout would...");

        foreach (var sku in new[] { "SKU-1001", "SKU-1002", "SKU-1003", "SKU-1001" })
        {
            await ReportAsync("charge 9.99", () => _payments.ChargeAsync(9.99m));
            await ReportAsync($"pricing {sku}", async () => $"{await _pricing.GetPriceAsync(sku):C}");
            await ReportAsync($"catalog {sku}", async () => (await _catalog.GetProductAsync(sku)).Name);
        }
    }

    private async Task RotateKeysThenChargeAsync()
    {
        Console.WriteLine("Warming up: one payment, so a token is cached...");
        await ReportAsync("charge 49.99", () => _payments.ChargeAsync(49.99m));

        // Plays the auth service's key rotation: every token issued so far,
        // including the one cached above, stops validating.
        Console.WriteLine("Rotating the auth service's keys behind the scenes...");
        using var admin = _factory.CreateClient();
        var rotated = await admin.PostAsync("http://127.0.0.1:5200/admin/rotate", null);
        rotated.EnsureSuccessStatusCode();

        Console.WriteLine("Charging again with the cached token...");
        await ReportAsync("charge 49.99", () => _payments.ChargeAsync(49.99m));
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
