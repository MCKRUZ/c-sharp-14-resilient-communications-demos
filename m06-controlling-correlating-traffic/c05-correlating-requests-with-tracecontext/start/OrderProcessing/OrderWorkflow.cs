using Grpc.Core;
using OrderProcessing.Inventory;
using OrderProcessing.Shipping;
using static OrderProcessing.Inventory.Inventory;
using static OrderProcessing.Shipping.Shipping;

namespace OrderProcessing;

public class OrderWorkflow
{
    private readonly PaymentGatewayClient _payments;
    private readonly InventoryClient _inventory;
    private readonly PricingClient _pricing;
    private readonly CatalogClient _catalog;
    private readonly ShippingClient _shipping;

    public OrderWorkflow(
        PaymentGatewayClient payments,
        InventoryClient inventory,
        PricingClient pricing,
        CatalogClient catalog,
        ShippingClient shipping)
    {
        _payments = payments;
        _inventory = inventory;
        _pricing = pricing;
        _catalog = catalog;
        _shipping = shipping;
    }

    public async Task ProcessOrderAsync(string orderId)
    {
        Console.WriteLine($"Processing order {orderId}");

        await _payments.ChargeAsync(89.99m);

        var stock = await _inventory.CheckStockAsync(new CheckStockRequest { Sku = "ABC-123", Quantity = 1 });
        var price = await _pricing.GetPriceAsync("SKU-1002");
        var product = await _catalog.GetProductAsync("SKU-1002");

        using var tracking = _shipping.TrackShipment(new TrackRequest { ShipmentId = orderId, AfterSequence = 3 });
        var shipmentStatus = "UNKNOWN";
        await foreach (var update in tracking.ResponseStream.ReadAllAsync())
        {
            shipmentStatus = update.Status;
        }

        Console.WriteLine($"  payment:   approved");
        Console.WriteLine($"  inventory: in stock = {stock.InStock}, {stock.Available} available");
        Console.WriteLine($"  pricing:   {price:C}");
        Console.WriteLine($"  catalog:   {product.Name}");
        Console.WriteLine($"  shipping:  {shipmentStatus}");
    }
}
