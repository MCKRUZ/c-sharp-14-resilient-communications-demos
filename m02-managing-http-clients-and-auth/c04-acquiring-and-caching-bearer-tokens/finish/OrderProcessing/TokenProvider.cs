using System.Net.Http.Json;

namespace OrderProcessing;

public class TokenProvider : ITokenProvider
{
    private const string TokenEndpoint = "http://127.0.0.1:5200/connect/token";
    private static readonly TimeSpan RefreshBuffer = TimeSpan.FromSeconds(60);

    private readonly IHttpClientFactory _factory;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _token;
    private DateTimeOffset _expiresAt;

    public TokenProvider(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (IsStillValid())
        {
            Console.WriteLine($"  token: reusing cached, {SecondsLeft()} s left");
            return _token!;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (IsStillValid())
            {
                Console.WriteLine("  token: reusing cached, another caller just fetched it");
                return _token!;
            }

            return await AcquireAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<string> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await AcquireAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsStillValid()
    {
        return _token is not null && DateTimeOffset.UtcNow < _expiresAt - RefreshBuffer;
    }

    private int SecondsLeft()
    {
        return (int)(_expiresAt - DateTimeOffset.UtcNow).TotalSeconds;
    }

    private async Task<string> AcquireAsync(CancellationToken cancellationToken)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            TokenEndpoint, new { clientId = "order-processing" }, cancellationToken);

        response.EnsureSuccessStatusCode();
        var issued = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        _token = issued!.AccessToken;
        _expiresAt = DateTimeOffset.UtcNow.AddSeconds(issued.ExpiresIn);

        Console.WriteLine($"  token: fetched from the auth service, valid for {issued.ExpiresIn} s");
        return _token;
    }
}

public record TokenResponse(string AccessToken, int ExpiresIn);
