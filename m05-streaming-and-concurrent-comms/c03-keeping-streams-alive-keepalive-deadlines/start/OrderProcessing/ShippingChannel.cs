using Grpc.Net.Client;

namespace OrderProcessing;

public static class ShippingChannel
{
    public static GrpcChannel Create(Uri address)
    {
        return GrpcChannel.ForAddress(address);
    }
}
