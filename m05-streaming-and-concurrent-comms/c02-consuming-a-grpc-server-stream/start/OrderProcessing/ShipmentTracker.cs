using OrderProcessing.Shipping;
using static OrderProcessing.Shipping.Shipping;

namespace OrderProcessing;

public class ShipmentTracker
{
    private readonly ShippingClient _client;

    public ShipmentTracker(ShippingClient client)
    {
        _client = client;
    }

    public string Status { get; private set; } = "UNKNOWN";

    public Task TrackAsync(string shipmentId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
