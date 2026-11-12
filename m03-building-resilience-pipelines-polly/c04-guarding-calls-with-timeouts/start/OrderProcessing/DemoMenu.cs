using System.Diagnostics;
using System.Net.Http.Json;

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
        Console.WriteLine("  [9] Arm the gateway to fail intermittently (~50%), then charge");
        Console.WriteLine("  [10] Arm the gateway to fail once then heal, then charge");
        Console.WriteLine("  [11] Arm intermittent failures, fire 5 clients at once (jitter)");
        Console.WriteLine("  [12] Arm a 429 with Retry-After: 3, then charge");
        Console.WriteLine("  [13] Arm a stalled gateway (accepts, never answers), then charge");
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
            case "9":
                await ArmIntermittentThenChargeAsync();
                break;
            case "10":
                await ArmFailOnceThenChargeAsync();
                break;
            case "11":
                await ArmIntermittentThenParallelChargesAsync();
                break;
            case "12":
                await ArmThrottledThenChargeAsync();
                break;
            case "13":
                await ArmStalledThenChargeAsync();
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

    // m03 fault plane: every option below arms the backend stub off camera
    // (POST to its :5200 admin endpoints, same idiom as RotateKeysThenChargeAsync
    // above) before running the on-camera call. See content/demo/shared/README.md.
    private async Task ArmIntermittentThenChargeAsync()
    {
        await PostAdminAsync("/admin/payments/mode", new { mode = "intermittent", failureRate = 0.5 });
        Console.WriteLine("Gateway armed: fails about half the time.");
        await ChargeOneAsync();
    }

    private async Task ArmFailOnceThenChargeAsync()
    {
        await PostAdminAsync("/admin/payments/queue-failures", new { status = 503, count = 1 });
        Console.WriteLine("Gateway armed: the next charge fails once, then heals.");
        await ChargeOneAsync();
    }

    private async Task PostAdminAsync(string path, object body)
    {
        using var admin = _factory.CreateClient();
        var response = await admin.PostAsJsonAsync(new Uri(new Uri("http://127.0.0.1:5200"), path), body);
        response.EnsureSuccessStatusCode();
    }

    // Fires 5 concurrent charges against an intermittently-failing gateway so
    // jittered retry delays land at visibly different times instead of a
    // synchronized spike.
    private async Task ArmIntermittentThenParallelChargesAsync()
    {
        await PostAdminAsync("/admin/payments/mode", new { mode = "intermittent", failureRate = 0.6 });
        Console.WriteLine("Gateway armed: intermittent failures. Firing 5 clients at once...");

        var stopwatch = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, 5).Select(async i =>
        {
            try
            {
                await _payments.ChargeAsync(9.99m + i);
                Console.WriteLine($"  client {i}: approved at {stopwatch.Elapsed.TotalSeconds:F2}s");
            }
            catch (Exception ex)
            {
                var root = ex.GetBaseException();
                Console.WriteLine($"  client {i}: FAILED at {stopwatch.Elapsed.TotalSeconds:F2}s ({root.GetType().Name})");
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task ArmThrottledThenChargeAsync()
    {
        await PostAdminAsync("/admin/payments/queue-failures", new { status = 429, count = 1, retryAfterSeconds = 3 });
        Console.WriteLine("Gateway armed: the next charge gets a 429 with Retry-After: 3.");
        await ChargeOneAsync();
    }

    // Used twice in this clip's recording: once before AddTimeout exists (the
    // call just hangs, no exception, ChargeOneAsync never returns -- nothing to
    // catch), once after (Polly's per-attempt timeout cuts it at 2s, well inside
    // the 8s stall, and DOES throw). The second run's payoff line names the
    // exception on screen, so it must be caught here, not left to crash the
    // process: an unhandled exception's dump goes to stderr, which DemoLog only
    // ever mirrors from stdout, so demo-output.log would never show it -- no
    // retake could ever satisfy that postcondition without this catch.
    //
    // Deliberately prints ex.GetType().Name, NOT ex.GetBaseException() the way
    // the other Arm*ThenCharge handlers do. Measured directly against a real
    // stalled gateway (2026-09-18): Polly's own TimeoutRejectedException is
    // what ChargeOneAsync actually throws, but it carries the underlying
    // aborted-socket exception as its inner exception, and GetBaseException()
    // walks straight past the type this clip's whole narration is about
    // ("Polly's own TimeoutRejectedException, not the framework's
    // TimeoutException") down to a bare SocketException instead.
    private async Task ArmStalledThenChargeAsync()
    {
        await PostAdminAsync("/admin/payments/mode", new { mode = "stall", stallMilliseconds = 8000 });
        Console.WriteLine("Gateway armed: accepts the request, then stalls for 8s.");
        try
        {
            await ChargeOneAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
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
