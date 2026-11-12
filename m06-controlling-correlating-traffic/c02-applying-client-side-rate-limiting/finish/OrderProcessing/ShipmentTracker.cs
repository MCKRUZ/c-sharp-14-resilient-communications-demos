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
        var lastSequence = 0;
        var backoff = TimeSpan.FromSeconds(1);

        while (true)
        {
            try
            {
                var request = new TrackRequest { ShipmentId = shipmentId, AfterSequence = lastSequence };

                using AsyncServerStreamingCall<TrackingEvent> call = _client.TrackShipment(request, cancellationToken: cancellationToken);

                await foreach (var evt in call.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    Status = evt.Status;
                    Console.WriteLine($"  #{evt.Sequence} {evt.Status} at {evt.Timestamp.ToDateTime().ToLocalTime():HH:mm:ss}");

                    if (evt.Status == "OUT_FOR_DELIVERY")
                    {
                        Console.WriteLine("  -> Notifying the customer: your package is out for delivery.");
                    }

                    lastSequence = evt.Sequence;
                    backoff = TimeSpan.FromSeconds(1);
                }

                return;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                Console.WriteLine($"  Unavailable, reconnecting from sequence {lastSequence} in {backoff.TotalSeconds:0}s...");
                await Task.Delay(backoff, cancellationToken);
                backoff *= 2;
            }
        }
    }
}
