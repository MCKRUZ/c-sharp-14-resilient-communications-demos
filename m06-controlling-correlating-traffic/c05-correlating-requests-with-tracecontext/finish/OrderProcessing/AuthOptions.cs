namespace OrderProcessing;

public class AuthOptions
{
    public string TokenEndpoint { get; init; } = "";

    public int CacheBufferSeconds { get; init; } = 60;
}
