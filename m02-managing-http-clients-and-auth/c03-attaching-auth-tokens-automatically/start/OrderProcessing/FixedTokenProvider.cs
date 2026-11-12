namespace OrderProcessing;

// Stand-in provider: hands back a pre-issued development token every time.
// The next clip replaces it with one that acquires and caches real tokens.
public class FixedTokenProvider : ITokenProvider
{
    public Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult("tok_dev_2026");
    }
}
