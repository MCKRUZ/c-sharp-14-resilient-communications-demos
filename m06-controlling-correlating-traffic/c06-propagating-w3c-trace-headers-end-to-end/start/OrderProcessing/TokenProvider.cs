using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace OrderProcessing;

public class TokenProvider : ITokenProvider
{
    private readonly string _tokenEndpoint;
    private readonly TimeSpan _refreshBuffer;

    private readonly IHttpClientFactory _factory;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _token;
    private DateTimeOffset _expiresAt;

    public TokenProvider(IHttpClientFactory factory, IOptions<AuthOptions> options)
    {
        _factory = factory;
        _tokenEndpoint = options.Value.TokenEndpoint;
        _refreshBuffer = TimeSpan.FromSeconds(options.Value.CacheBufferSeconds);
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (IsStillValid())
        {
            return _token!;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (IsStillValid())
            {
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
        return _token is not null && DateTimeOffset.UtcNow < _expiresAt - _refreshBuffer;
    }

    private int SecondsLeft()
    {
        return (int)(_expiresAt - DateTimeOffset.UtcNow).TotalSeconds;
    }

    private async Task<string> AcquireAsync(CancellationToken cancellationToken)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            _tokenEndpoint, new { clientId = "order-processing" }, cancellationToken);

        response.EnsureSuccessStatusCode();
        var issued = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        // Publish the expiry BEFORE the token: IsStillValid() reads both without the gate, and a
        // fan-out's concurrent callers can see the new token with the old (default) expiry,
        // which underflows DateTimeOffset (found in m05-c06 with a cold token).
        _expiresAt = DateTimeOffset.UtcNow.AddSeconds(issued!.ExpiresIn);
        _token = issued.AccessToken;

        return _token;
    }
}

public record TokenResponse(string AccessToken, int ExpiresIn);
