namespace OrderProcessing;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);

    Task<string> RefreshTokenAsync(CancellationToken cancellationToken);
}
