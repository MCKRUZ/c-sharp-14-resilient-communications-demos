namespace OrderProcessing;

public class PaymentGatewayOptions
{
    public string BaseAddress { get; init; } = "";

    public int TimeoutSeconds { get; init; } = 10;
}
