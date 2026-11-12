using Grpc.Core;
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

    public async Task TrackAsync(string shipmentId, CancellationToken cancellationToken)
    {
        var request = new TrackRequest { ShipmentId = shipmentId };

        using AsyncServerStreamingCall<TrackingEvent> call = _client.TrackShipment(request, cancellationToken: cancellationToken);

        await foreach (var evt in call.ResponseStream.ReadAllAsync(cancellationToken))
        {
            Status = evt.Status;
            Console.WriteLine($"  #{evt.Sequence} {evt.Status} at {evt.Timestamp.ToDateTime().ToLocalTime():HH:mm:ss}");

            if (evt.Status == "OUT_FOR_DELIVERY")
            {
                Console.WriteLine("  -> Notifying the customer: your package is out for delivery.");
            }
        }
    }
}
