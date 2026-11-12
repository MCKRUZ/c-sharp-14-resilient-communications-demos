using System.Diagnostics;

namespace OrderProcessing;

public class RequestTimingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await base.SendAsync(request, cancellationToken);

        stopwatch.Stop();
        Console.WriteLine(
            $"{request.Method} {request.RequestUri} -> " +
            $"{(int)response.StatusCode} {response.StatusCode} " +
            $"in {stopwatch.ElapsedMilliseconds} ms");

        return response;
    }
}
