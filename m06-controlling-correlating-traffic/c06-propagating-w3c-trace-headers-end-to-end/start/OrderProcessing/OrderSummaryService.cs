namespace OrderProcessing;

public class OrderSummaryService
{
    private readonly CatalogClient _catalog;

    public OrderSummaryService(CatalogClient catalog)
    {
        _catalog = catalog;
    }

    public async Task<string> BuildSummaryAsync(string orderId)
    {
        // A real order would come from a database; this demo order is fixed.
        var product = await _catalog.GetProductAsync("SKU-1002");

        return $"Order {orderId}: 1 x {product.Name} at {product.Price:C}";
    }
}
