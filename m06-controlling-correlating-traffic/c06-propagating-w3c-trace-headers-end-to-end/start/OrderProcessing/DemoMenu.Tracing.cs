using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderProcessing;

// m06-c06: what actually left the building. Both options start their own activity on the
// "OrderProcessing" source, make real calls, and then ask the receiving side what it got.
public partial class DemoMenu
{
    private static readonly ActivitySource MenuActivitySource = new("OrderProcessing");

    // One ordinary call through a default HttpClient to an endpoint that reflects the
    // request headers. Nothing in this method sets a traceparent header.
    private async Task ShowOutgoingTraceparentAsync()
    {
        using var activity = MenuActivitySource.StartActivity("ProcessOrder");
        Console.WriteLine($"Activity.Current.TraceId: {Activity.Current?.TraceId}");

        using var client = _factory.CreateClient();
        var headers = await client.GetFromJsonAsync<Dictionary<string, string>>("http://127.0.0.1:5200/echo/headers");
        var traceparent = headers!.GetValueOrDefault("traceparent", "(none)");

        Console.WriteLine($"Header the service received: traceparent: {traceparent}");
        Console.WriteLine(traceparent.Split('-').ElementAtOrDefault(1) == Activity.Current?.TraceId.ToString()
            ? "The trace id in the header matches Activity.Current.TraceId"
            : "MISMATCH between the header and Activity.Current.TraceId");
    }

    // One order through every service, then each service's own record of the calls that
    // carried this trace id.
    private async Task ShowEveryServiceForOneTraceAsync()
    {
        await PostAdminAsync("/admin/trace/reset", new { });

        string traceId;
        using (var checkout = MenuActivitySource.StartActivity("Checkout"))
        {
            traceId = Activity.Current!.TraceId.ToString();
            await _workflow.ProcessOrderAsync("4471");
        }

        using var admin = _factory.CreateClient();
        var entries = await admin.GetFromJsonAsync<JsonElement>($"http://127.0.0.1:5200/admin/trace/{traceId}");

        Console.WriteLine();
        Console.WriteLine($"Every service's log, filtered by trace id {traceId}:");
        foreach (var entry in entries.EnumerateArray())
        {
            Console.WriteLine($"  {entry.GetProperty("service").GetString(),-12} {entry.GetProperty("method").GetString(),-5} {entry.GetProperty("path").GetString()}");
        }
    }
}
