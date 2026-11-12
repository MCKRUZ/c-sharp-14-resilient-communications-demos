namespace OrderProcessing;

// Stand-in provider: hands back a pre-issued development token every time.
// Replaced by TokenProvider in this clip's registration.
public class FixedTokenProvider : ITokenProvider
{
    public Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult("tok_dev_2026");
    }

    public Task<string> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult("tok_dev_2026");
    }
}
