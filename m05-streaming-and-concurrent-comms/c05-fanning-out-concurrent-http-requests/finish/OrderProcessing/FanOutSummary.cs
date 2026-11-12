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
        Task<decimal> pricingTask = _pricingClient.GetPriceAsync(id);
        Task<Product> catalogTask = _catalogClient.GetProductAsync(id);
        Task<decimal> discountTask = _discountClient.GetDiscountAsync(id);
        Task<int> shippingTask = _shippingClient.GetEstimateAsync(id);

        await Task.WhenAll(pricingTask, catalogTask, discountTask, shippingTask);

        return new OrderSummary(
            await catalogTask,
            await pricingTask,
            await discountTask,
            await shippingTask);
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
}
