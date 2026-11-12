using System.Collections.Concurrent;

namespace OrderProcessing;

public class OrderSummaryFanOut
{
    private readonly PricingClient _pricingClient;
    private readonly CatalogClient _catalogClient;
    private readonly DiscountClient _discountClient;
    private readonly ShippingEstimateClient _shippingClient;

    public OrderSummaryFanOut(
        PricingClient pricingClient,
        CatalogClient catalogClient,
        DiscountClient discountClient,
        ShippingEstimateClient shippingClient)
    {
        _pricingClient = pricingClient;
        _catalogClient = catalogClient;
        _discountClient = discountClient;
        _shippingClient = shippingClient;
    }

    public async Task<OrderSummary> BuildAsync(string id)
    {
        var pricingTask = CaptureAsync(() => _pricingClient.GetPriceAsync(id));
        var catalogTask = CaptureAsync(() => _catalogClient.GetProductAsync(id));
        var discountTask = CaptureAsync(() => _discountClient.GetDiscountAsync(id));
        var shippingTask = CaptureAsync(() => _shippingClient.GetEstimateAsync(id));

        await Task.WhenAll(pricingTask, catalogTask, discountTask, shippingTask);

        var pricing = await pricingTask;
        var catalog = await catalogTask;
        var discount = await discountTask;
        var shipping = await shippingTask;

        Console.WriteLine($"  pricing:           {Describe(pricing)}");
        Console.WriteLine($"  catalog:           {Describe(catalog)}");
        Console.WriteLine($"  discount:          {Describe(discount)}");
        Console.WriteLine($"  shipping estimate: {Describe(shipping)}");

        // Required data fails the summary; optional data degrades it.
        if (!pricing.Succeeded || !catalog.Succeeded)
        {
            throw new InvalidOperationException(
                $"Cannot build the summary: pricing {Describe(pricing)}, catalog {Describe(catalog)}");
        }

        return new OrderSummary(
            catalog.Value!,
            pricing.Value,
            discount.Succeeded ? discount.Value : null,
            shipping.Succeeded ? shipping.Value : null);
    }

    public async Task<IReadOnlyDictionary<string, decimal>> PriceManyAsync(IEnumerable<string> productIds)
    {
        var prices = new ConcurrentDictionary<string, decimal>();

        await Parallel.ForEachAsync(
            productIds,
            new ParallelOptions { MaxDegreeOfParallelism = 8 },
            async (productId, cancellationToken) =>
            {
                prices[productId] = await _pricingClient.GetPriceAsync(productId);
            });

        return prices;
    }

    // Act on each outcome the moment it lands, in completion order.
    public async Task ShowAsTheyFinishAsync(string id)
    {
        var calls = new[]
        {
            LabelAsync("pricing", () => _pricingClient.GetPriceAsync(id)),
            LabelAsync("catalog", () => _catalogClient.GetProductAsync(id)),
            LabelAsync("discount", () => _discountClient.GetDiscountAsync(id)),
            LabelAsync("shipping estimate", () => _shippingClient.GetEstimateAsync(id)),
        };

        await foreach (var finished in Task.WhenEach(calls))
        {
            Console.WriteLine($"  {await finished}");
        }
    }

    private static async Task<string> LabelAsync<T>(string name, Func<Task<T>> call) =>
        $"{name}: {Describe(await CaptureAsync(call))}";

    private static string Describe<T>(Outcome<T> outcome) =>
        outcome.Succeeded ? "ok" : $"FAILED, {outcome.Error}";

    private static async Task<Outcome<T>> CaptureAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return Outcome<T>.Success(await call());
        }
        catch (Exception ex)
        {
            return Outcome<T>.Failure(ex.Message);
        }
    }
}

public record Outcome<T>(T? Value, string? Error)
{
    public bool Succeeded => Error is null;

    public static Outcome<T> Success(T value) => new(value, null);

    public static Outcome<T> Failure(string error) => new(default, error);
}
