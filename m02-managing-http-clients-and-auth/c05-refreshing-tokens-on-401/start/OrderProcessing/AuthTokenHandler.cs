using System.Net.Http.Headers;

namespace OrderProcessing;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    public AuthTokenHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Console.WriteLine($"  Authorization: Bearer {token}");

        return await base.SendAsync(request, cancellationToken);
    }
}
