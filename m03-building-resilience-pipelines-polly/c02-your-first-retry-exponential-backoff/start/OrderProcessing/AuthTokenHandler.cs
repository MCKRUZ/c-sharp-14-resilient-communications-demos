using System.Net;
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

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Console.WriteLine("  401 Unauthorized: refreshing the token and retrying once");
            response.Dispose();

            var freshToken = await _tokenProvider.RefreshTokenAsync(cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", freshToken);
            Console.WriteLine($"  Authorization: Bearer {freshToken}");

            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }
}
