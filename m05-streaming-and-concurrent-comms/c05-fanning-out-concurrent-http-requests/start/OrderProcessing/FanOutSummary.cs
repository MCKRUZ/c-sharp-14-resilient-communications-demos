namespace OrderProcessing;

// Written on camera in m05-c05. The constructor is the only thing that exists
// when the clip opens.
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

    public Task<OrderSummary> BuildAsync(string id)
    {
        throw new NotImplementedException("Written on camera in m05-c05.");
    }
}
