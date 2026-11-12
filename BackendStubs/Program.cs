using System.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OrderProcessing.Inventory;
using OrderProcessing.Shipping;

// Off-camera backend stubs for the demos. One process plays every downstream
// service the order-processing app calls, each on its own loopback port so the
// demo app configures distinct base addresses:
//   auth service             http://127.0.0.1:5200   (m02 only, see below)
//   payment gateway          http://127.0.0.1:5201
//   pricing service          http://127.0.0.1:5202
//   catalog service          http://127.0.0.1:5203
//   payment gateway, sandbox http://127.0.0.1:5211   (m02-c06: a second target)
// 127.0.0.1 (not localhost) everywhere: the m01-c03 socket-exhaustion demo
// depends on the client using the IPv4 loopback, so the constrained dynamic
// port range set up for that clip actually applies to these connections.
//
// AUTH MODE (m02). Module 2 opens on every service answering 401 because the
// app sends no bearer token. Start the stubs with `--require-auth` (or the
// environment variable DEMO_STUBS_REQUIRE_AUTH=1) and:
//   - POST /connect/token on :5200 issues an opaque bearer token good for
//     TokenLifetime; POST /admin/rotate revokes every issued token (the "key
//     rotation" m02-c05 recovers from).
//   - payments, pricing, catalog and the sandbox gateway reject any request
//     without a valid bearer token with 401 + WWW-Authenticate: Bearer.
//   - one pre-issued development token (DevToken) is accepted until the first
//     rotation, so m02-c03's stand-in provider can return a constant.
//   - payment receipts carry a "gateway" field so the m02-c06 config swap from
//     :5201 to :5211 is visible in the app's output.
// Without the flag nothing above exists and every m01 take runs exactly as it
// always has.
//
// M03 FAULT INJECTION (Polly resilience module). Every fault below is OFF by
// default: with no admin call ever made, /payments/charge and
// /pricing/price/{id} behave exactly as they do for m01 and m02 today. A
// take turns a fault on with an admin POST (same idiom as m02-c05's
// /admin/rotate: an app-side menu option or a demo.yaml step calls it mid
// recording), always against port :5200 so it is exempt from the bearer
// check the same way /admin/rotate is.
//   - POST /admin/payments/mode        { "mode": "normal"|"down"|"intermittent"|"stall",
//                                         "failureRate": 0.5, "stallMilliseconds": 8000 }
//     Persistent until changed. "down" fails every call with 503. "intermittent"
//     fails a call with probability failureRate. "stall" never returns inside
//     the request (Task.Delay for stallMilliseconds, canceled cleanly if the
//     caller's own timeout gives up first).
//   - POST /admin/payments/queue-failures { "status": 503|429, "count": 1,
//                                            "retryAfterSeconds": 3 }
//     One-shot instructions, consumed FIFO, one per call to /payments/charge,
//     ahead of whatever "mode" is set to. This is what makes "fails once,
//     then succeeds" (m03-c02) and "429 with Retry-After: 3" (m03-c03)
//     deterministic instead of a coin flip.
//   - POST /admin/payments/reset        clears the queue and returns to "normal".
//   - POST /admin/pricing/slow-next     { "count": 1, "delayMs": 2000 }
//     One-shot per-call delays on GET /pricing/price/{id}, consumed FIFO
//     regardless of productId. Polly's hedging strategy (m03-c06) launches a
//     second attempt against the same endpoint while the first is still
//     asleep; because the delay queue is consumed one-per-call, attempt 1
//     draws the queued delay and attempt 2 draws nothing and returns at
//     normal speed, so the hedge deterministically wins the race.
//   - POST /admin/pricing/reset         clears the delay queue.
//
// M01-C02-INTO-C03 RESTRUCTURE (fallacies demo, no flag needed). Added
// 2026-09-05 so c03 can show, not just describe, four more fallacies:
//   - GET /catalog/products              -- whole 24-item catalog, one call.
//   - GET /catalog/products?page=&pageSize= -- one page of it instead.
//   - POST /pricing/prices { productIds: [...] } -- one round trip for many
//     SKUs' prices, vs looping GET /pricing/price/{id} once per SKU.
//   - The existing /admin/pricing/slow-next fault (above) doubles as the
//     "network is homogeneous" beat: inject a delay into pricing only, then
//     show it sharing one HttpClient timeout with the still-fast payments call.
//
// M04 INVENTORY GRPC SERVICE (Consuming gRPC services). A sixth endpoint,
// https://localhost:5001, hosts the Inventory service the module's gRPC
// clips call. It is "localhost" (not 127.0.0.1 like every other stub here)
// because the ASP.NET Core dev HTTPS certificate's SAN is localhost, and
// gRPC needs TLS/HTTP2 -- the recording VM must run
// `dotnet dev-certs https --trust` once. There is no fault-injection surface
// for it: nothing in m04's filmed [DEMO:] blocks runs a live fault-recovery
// scenario (c05 and c06 are editor-only beats), so unlike the payments and
// pricing fault planes above, building one would sit unused. See
// content/demo/shared/README.md, "m04 gRPC inventory service".

// M05 STREAMING AND FAN-OUT (Streaming and concurrent communication). Every
// surface below is OFF by default: with no admin call ever made, m01-m04 behave
// exactly as before (the one m05-time change to existing behavior is that auth
// mode no longer applies its bearer check to the gRPC port, :5001, see below).
//   - Shipping gRPC service on https://localhost:5001 (same host as Inventory):
//     TrackShipment is a server stream of four events (1 PICKED_UP,
//     2 IN_TRANSIT, 3 OUT_FOR_DELIVERY, 4 DELIVERED), the first about 1s after
//     the call and the rest 3s apart, skipping any event whose sequence is
//     <= TrackRequest.after_sequence. :5001 is exempt from the bearer check
//     even in auth mode (gRPC tracking is not auth-gated; m02-m04 never called
//     it with auth mode on, and before this change every gRPC call in auth
//     mode answered Unauthenticated).
//   - Discounts http://127.0.0.1:5204  GET /discounts/{sku}
//     and shipping estimates http://127.0.0.1:5205  GET /shipping/estimate/{sku},
//     both bearer-checked in auth mode like :5202/:5203.
//   - POST /admin/shipping/drop-after   { "count": 3, "mode": "connection"|"abort"|"status" }
//     One-shot: the NEXT TrackShipment stream is cut right after it has emitted
//     `count` events, then the fault disarms so the reconnect succeeds. "mode"
//     is optional (default "connection"); see ShippingFaultController for what
//     each one does on the wire and what the client sees (measured, not assumed).
//   - POST /admin/shipping/reset        disarms it.
//   - POST /admin/fanout/latency        { "delayMs": 200 }
//     Persistent fixed delay (not random) added to GET /pricing/price/{id},
//     GET /catalog/products/{id}, GET /discounts/{sku} and
//     GET /shipping/estimate/{sku}. Default 0.
//   - POST /admin/shipping-estimate/mode { "mode": "fail"|"normal" }
//     "fail" makes :5205 answer 503 to every call.
//   - POST /admin/fanout/reset          clears the latency and the fail mode.
// All admin routes live on :5200, exempt from the bearer check.
//
// M06 TRAFFIC CONTROL AND TRACING (Controlling and correlating traffic). Every
// surface below is OFF by default: with no admin call ever made, m01-m05 behave
// exactly as before (the recorders below only observe).
//   - The payment gateway (:5201 and :5211) now records what it SEES: how many
//     charge calls arrived, the most in flight at once, and the most that arrived
//     inside any one-second span. The demo prints these so the viewer sees the
//     gateway's own point of view, not the client's claim about itself.
//     GET  /admin/payments/stats          { total, peakInFlight, maxInOneSecond,
//                                           waves: [{ count, atMs }] }
//     POST /admin/payments/stats/reset    clears the counters
//     POST /admin/payments/latency        { "delayMs": 500 } a persistent fixed
//                                         delay on every charge (m06-c03's slow
//                                         calls); /admin/payments/reset clears it
//                                         and the counters too.
//   - Every service records the W3C traceparent header of each request it gets
//     (HTTP stubs by port, gRPC by service name), read straight off the wire and
//     not from any server-side Activity.
//     GET  /admin/trace/{traceId}         the calls that carried that trace id,
//                                         oldest first, with the caller's span id
//     POST /admin/trace/reset             clears the log
//   - GET /echo/headers on :5200 reflects the request headers it received
//     (m06-c06: shows the traceparent HttpClient attached on its own).

var requireAuth = args.Contains("--require-auth")
    || Environment.GetEnvironmentVariable("DEMO_STUBS_REQUIRE_AUTH") == "1";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(kestrel =>
{
    foreach (var port in new[] { 5200, 5201, 5202, 5203, 5204, 5205, 5211 })
    {
        kestrel.Listen(IPAddress.Loopback, port, listen => listen.Protocols = HttpProtocols.Http1);
    }

    kestrel.ListenLocalhost(5001, listen =>
    {
        listen.Protocols = HttpProtocols.Http2;
        listen.UseHttps();
    });
});
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Services.AddGrpc();
builder.Services.AddSingleton<ShippingFaultController>();

var app = builder.Build();

app.MapGrpcService<InventoryServiceImpl>();
app.MapGrpcService<ShippingServiceImpl>();

var products = new Dictionary<string, (string Name, decimal Price)>
{
    ["SKU-1001"] = ("Wireless Mouse", 24.99m),
    ["SKU-1002"] = ("Mechanical Keyboard", 89.99m),
    ["SKU-1003"] = ("USB-C Dock", 149.99m),
};

// m01-c02-into-c03 restructure: a larger, separate catalog list so the
// unpaged-vs-paged demo has enough items to show a real bandwidth difference
// (three products would not). Deliberately not the same dictionary as
// `products` above -- other clips' pricing calls depend on exactly those
// three SKUs staying unchanged.
var catalog = Enumerable.Range(1, 24)
    .Select(i => new CatalogItem($"SKU-2{i:000}", $"Warehouse Item {i}", 9.99m + i))
    .ToArray();

var tokens = new TokenStore(requireAuth);
var paymentFaults = new PaymentFaultController();
var pricingFaults = new PricingFaultController();
var fanout = new FanoutController();
var traffic = new PaymentTrafficRecorder();
var traceLog = new TraceLog();

// Port-scoped bearer check. The auth service itself (:5200) is always open.
app.Use(async (context, next) =>
{
    var port = context.Connection.LocalPort;
    if (!requireAuth || port == 5200 || port == 5001 || tokens.IsValid(context.Request.Headers.Authorization))
    {
        await next(context);
        return;
    }

    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    context.Response.Headers.WWWAuthenticate = "Bearer";
    await context.Response.WriteAsJsonAsync(new { error = "missing or invalid bearer token" });
});

// m06: remember which trace each request belonged to. Runs after the bearer check,
// so an unauthorized call is never recorded as served.
app.Use(async (context, next) =>
{
    var service = TraceLog.ServiceFor(context.Connection.LocalPort, context.Request.Path);
    if (service is not null)
    {
        traceLog.Record(service, context.Request.Method, context.Request.Path, context.Request.Headers["traceparent"].ToString());
    }

    await next(context);
});

app.MapPost("/connect/token", (TokenRequest request) =>
{
    var issued = tokens.Issue();
    return Results.Ok(new
    {
        accessToken = issued.Value,
        expiresIn = (int)TokenStore.TokenLifetime.TotalSeconds,
        clientId = request.ClientId,
    });
});

app.MapPost("/admin/rotate", () =>
{
    var revoked = tokens.RevokeAll();
    return Results.Ok(new { rotated = true, revoked });
});

app.MapPost("/admin/payments/mode", (PaymentModeRequest request) =>
{
    var mode = paymentFaults.SetMode(request.Mode, request.FailureRate, request.StallMilliseconds);
    return Results.Ok(new { mode = mode.ToString() });
});

app.MapPost("/admin/payments/queue-failures", (PaymentQueueRequest request) =>
{
    paymentFaults.QueueFailures(request.Status, request.Count, request.RetryAfterSeconds);
    return Results.Ok(new { queued = request.Count, status = request.Status });
});

app.MapPost("/admin/payments/reset", () =>
{
    paymentFaults.Reset();
    traffic.Reset();
    return Results.Ok(new { reset = true });
});

app.MapPost("/admin/payments/latency", (PaymentLatencyRequest request) =>
{
    traffic.SetDelay(request.DelayMs);
    return Results.Ok(new { delayMs = request.DelayMs });
});

app.MapGet("/admin/payments/stats", () => Results.Ok(traffic.Snapshot()));

app.MapPost("/admin/payments/stats/reset", () =>
{
    traffic.ResetCounters();
    return Results.Ok(new { reset = true });
});

app.MapGet("/admin/trace/{traceId}", (string traceId) => Results.Ok(traceLog.ForTrace(traceId)));

app.MapPost("/admin/trace/reset", () =>
{
    traceLog.Reset();
    return Results.Ok(new { reset = true });
});

app.MapGet("/echo/headers", (HttpContext context) =>
    Results.Ok(context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())));

app.MapPost("/admin/pricing/slow-next", (PricingSlowRequest request) =>
{
    pricingFaults.QueueDelay(request.Count, request.DelayMs);
    return Results.Ok(new { queued = request.Count, delayMs = request.DelayMs });
});

app.MapPost("/admin/pricing/reset", () =>
{
    pricingFaults.Reset();
    return Results.Ok(new { reset = true });
});

app.MapPost("/admin/shipping/drop-after", (DropAfterRequest request, ShippingFaultController shippingFaults) =>
{
    var mode = shippingFaults.ArmDrop(request.Count, request.Mode);
    return Results.Ok(new { armed = request.Count, mode });
});

app.MapPost("/admin/shipping/reset", (ShippingFaultController shippingFaults) =>
{
    shippingFaults.Reset();
    return Results.Ok(new { reset = true });
});

app.MapPost("/admin/fanout/latency", (FanoutLatencyRequest request) =>
{
    fanout.SetDelay(request.DelayMs);
    return Results.Ok(new { delayMs = request.DelayMs });
});

app.MapPost("/admin/shipping-estimate/mode", (ShippingEstimateModeRequest request) =>
{
    var failing = fanout.SetEstimateMode(request.Mode);
    return Results.Ok(new { mode = failing ? "fail" : "normal" });
});

app.MapPost("/admin/fanout/reset", () =>
{
    fanout.Reset();
    return Results.Ok(new { reset = true });
});

app.MapPost("/payments/charge", async (HttpContext context, ChargeRequest request, CancellationToken cancellationToken) =>
{
    var gateway = context.Connection.LocalPort == 5211 ? "sandbox" : "primary";

    using var visit = traffic.Enter();
    try
    {
        await traffic.DelayAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
        return Results.Empty;
    }

    var fault = paymentFaults.NextOutcome();
    if (fault is { StallMilliseconds: { } stallMs })
    {
        try
        {
            // The gateway "accepts the request then stalls": no status, no
            // body, until either the delay elapses (a very slow but not
            // permanently broken dependency) or the caller gives up first.
            await Task.Delay(stallMs, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Results.Empty;
        }
    }
    else if (fault is { Status: { } status })
    {
        if (fault.RetryAfterSeconds is { } retryAfter)
        {
            context.Response.Headers.RetryAfter = retryAfter.ToString();
        }

        return Results.StatusCode(status);
    }

    return requireAuth
        ? Results.Ok(new
        {
            status = "approved",
            transactionId = Guid.NewGuid().ToString("N")[..12],
            amount = request.Amount,
            gateway,
        })
        : Results.Ok(new
        {
            status = "approved",
            transactionId = Guid.NewGuid().ToString("N")[..12],
            amount = request.Amount,
        });
});

app.MapGet("/pricing/price/{productId}", async (string productId) =>
{
    // A real remote call is never actually instant. Without some baseline
    // latency here, the m01-c02-into-c03 transport-cost beat's "N round trips
    // vs 1 batched call" comparison is decided by loopback noise instead of
    // the real difference it's teaching -- measured directly: batched came
    // out slower than 5 separate calls on one run. 8ms is small enough not
    // to affect any other clip's pacing (m03's own fault injection adds
    // seconds, not milliseconds) but large enough that N calls reliably cost
    // more than 1.
    await Task.Delay(8);

    var delayMs = pricingFaults.NextDelayMilliseconds();
    if (delayMs > 0)
    {
        await Task.Delay(delayMs);
    }

    await fanout.DelayAsync();

    return products.TryGetValue(productId, out var product)
        ? Results.Ok(new { productId, price = product.Price })
        : Results.NotFound();
});

app.MapGet("/catalog/products/{id}", async (string id) =>
{
    await fanout.DelayAsync();

    return products.TryGetValue(id, out var product)
        ? Results.Ok(new { id, name = product.Name, price = product.Price })
        : Results.NotFound();
});

// m05 fan-out services. Same fixed product table as pricing and catalog, so a
// summary assembled from all four describes the same SKU. Discount percent and
// delivery days differ a little per SKU so the printed summary is not flat.
var discountPercent = new Dictionary<string, int> { ["SKU-1001"] = 10, ["SKU-1002"] = 15, ["SKU-1003"] = 5 };
var estimateDays = new Dictionary<string, int> { ["SKU-1001"] = 3, ["SKU-1002"] = 4, ["SKU-1003"] = 5 };

app.MapGet("/discounts/{sku}", async (string sku) =>
{
    await fanout.DelayAsync();

    return discountPercent.TryGetValue(sku, out var percent)
        ? Results.Ok(new { sku, percent })
        : Results.NotFound();
});

app.MapGet("/shipping/estimate/{sku}", async (string sku) =>
{
    await fanout.DelayAsync();

    if (fanout.EstimateFailing)
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    return estimateDays.TryGetValue(sku, out var days)
        ? Results.Ok(new { sku, days })
        : Results.NotFound();
});

// m01-c02-into-c03 restructure, bandwidth-fallacy beat: no `page`/`pageSize`
// returns the entire 24-item catalog in one response (the "assume the network
// is free" call); passing both returns one page, `hasMore` telling the
// caller whether to request the next one.
app.MapGet("/catalog/products", (int? page, int? pageSize) =>
{
    if (page is null || pageSize is null)
    {
        return Results.Ok(catalog);
    }

    var skip = (page.Value - 1) * pageSize.Value;
    var slice = catalog.Skip(skip).Take(pageSize.Value).ToArray();
    return Results.Ok(new { page = page.Value, pageSize = pageSize.Value, items = slice, hasMore = skip + slice.Length < catalog.Length });
});

// m01-c02-into-c03 restructure, transport-cost-fallacy beat: one round trip
// for every SKU asked for, versus the per-item loop over
// GET /pricing/price/{productId} the client can also make against the same
// three SKUs -- same data, one call instead of N.
app.MapPost("/pricing/prices", (PricesBatchRequest request) =>
{
    var results = request.ProductIds
        .Where(products.ContainsKey)
        .Select(id => new { productId = id, price = products[id].Price })
        .ToArray();
    return Results.Ok(results);
});

Console.WriteLine(
    "Backend stubs listening on 127.0.0.1:5200 (auth), :5201 (payments), :5202 (pricing), " +
    ":5203 (catalog), :5204 (discounts), :5205 (shipping estimates), :5211 (payments sandbox), " +
    "https://localhost:5001 (inventory + shipping, gRPC)");
Console.WriteLine(requireAuth
    ? "Auth mode ON: every service except the auth service demands a bearer token"
    : "Auth mode OFF: no token required (m01 behavior); pass --require-auth for m02");
app.Run();

// m04 inventory gRPC service. No fault-injection surface -- see the file
// header comment, "M04 INVENTORY GRPC SERVICE". SKU-9002 is deliberately
// absent from Stock so it exercises a real NotFound in m04-c06's status-code
// switch without needing an admin endpoint to arm one.
internal sealed class InventoryServiceImpl : Inventory.InventoryBase
{
    private static readonly Dictionary<string, (bool InStock, int Available)> Stock = new()
    {
        ["ABC-123"] = (true, 40),
    };

    public override Task<CheckStockResponse> CheckStock(CheckStockRequest request, ServerCallContext context)
    {
        if (!Stock.TryGetValue(request.Sku, out var stock))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"No stock record for SKU '{request.Sku}'."));
        }

        return Task.FromResult(new CheckStockResponse
        {
            InStock = stock.InStock,
            Available = stock.Available,
        });
    }
}

// m05 shipping gRPC service. Four events, the first about a second after the
// call and the rest 3s apart, skipping everything the client already has
// (after_sequence). A stream armed through /admin/shipping/drop-after is cut
// after N emitted events; see ShippingFaultController.
internal sealed class ShippingServiceImpl(ShippingFaultController faults) : Shipping.ShippingBase
{
    private static readonly string[] Statuses = ["PICKED_UP", "IN_TRANSIT", "OUT_FOR_DELIVERY", "DELIVERED"];

    public override async Task TrackShipment(
        TrackRequest request,
        IServerStreamWriter<TrackingEvent> responseStream,
        ServerCallContext context)
    {
        var drop = faults.TakeDrop();
        var cancellationToken = context.CancellationToken;
        var emitted = 0;

        for (var sequence = Math.Max(1, request.AfterSequence + 1); sequence <= Statuses.Length; sequence++)
        {
            await Task.Delay(emitted == 0 ? 1500 : 3000, cancellationToken);

            await responseStream.WriteAsync(new TrackingEvent
            {
                Sequence = sequence,
                Status = Statuses[sequence - 1],
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
            });
            emitted++;

            if (drop is { } armed && emitted >= armed.Count && sequence < Statuses.Length)
            {
                if (armed.Mode == DropMode.Status)
                {
                    throw new RpcException(new Status(StatusCode.Unavailable, "Shipping service dropped the stream."));
                }

                if (armed.Mode == DropMode.Connection)
                {
                    // Give the event just written time to reach the wire, then kill
                    // the TCP connection under the stream: what a crashed or
                    // redeployed server looks like to the client.
                    await Task.Delay(250, cancellationToken);
                    context.GetHttpContext().Features.Get<Microsoft.AspNetCore.Connections.Features.IConnectionLifetimeFeature>()?.Abort();
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }

                context.GetHttpContext().Abort();
                return;
            }
        }
    }
}

internal sealed record DropAfterRequest(int Count, string? Mode);

internal sealed record FanoutLatencyRequest(int DelayMs);

internal sealed record ShippingEstimateModeRequest(string Mode);

internal enum DropMode
{
    Connection,
    Abort,
    Status,
}

internal sealed record DropInstruction(int Count, DropMode Mode);

// m05-c04 drop plane: a one-shot "cut the next stream after N events". Three
// ways to cut it, MEASURED against grpc-dotnet 2.84 on .NET 10 (client sees):
//   connection (default) kill the TCP connection under the stream
//                        -> RpcException StatusCode.Unavailable
//                           ("Error reading next message. IOException ... forcibly closed")
//   abort                HttpContext.Abort(), an HTTP/2 RST_STREAM(INTERNAL_ERROR)
//                        -> StatusCode.Internal, NOT Unavailable
//   status               server throws RpcException(Unavailable)
//                        -> StatusCode.Unavailable, but it is a clean trailer, not a drop
// Only "connection" is both honest and Unavailable, so it is the default.
internal sealed class ShippingFaultController
{
    private readonly Lock _lock = new();
    private DropInstruction? _drop;

    public string ArmDrop(int count, string? mode)
    {
        var parsed = mode is null ? DropMode.Connection : Enum.Parse<DropMode>(mode, ignoreCase: true);

        lock (_lock)
        {
            _drop = new DropInstruction(count, parsed);
        }

        return parsed.ToString().ToLowerInvariant();
    }

    public void Reset()
    {
        lock (_lock)
        {
            _drop = null;
        }
    }

    public DropInstruction? TakeDrop()
    {
        lock (_lock)
        {
            var drop = _drop;
            _drop = null;
            return drop;
        }
    }
}

// m05-c05/c06 fan-out plane: one fixed latency applied to the four services a
// summary fans out to, and a switch that makes the shipping-estimate service
// answer 503. Both default to off.
internal sealed class FanoutController
{
    private int _delayMilliseconds;
    private volatile bool _estimateFailing;

    public bool EstimateFailing => _estimateFailing;

    public void SetDelay(int delayMs) => Volatile.Write(ref _delayMilliseconds, Math.Max(0, delayMs));

    public bool SetEstimateMode(string mode)
    {
        _estimateFailing = string.Equals(mode, "fail", StringComparison.OrdinalIgnoreCase);
        return _estimateFailing;
    }

    public void Reset()
    {
        Volatile.Write(ref _delayMilliseconds, 0);
        _estimateFailing = false;
    }

    public Task DelayAsync()
    {
        var delay = Volatile.Read(ref _delayMilliseconds);
        return delay > 0 ? Task.Delay(delay) : Task.CompletedTask;
    }
}

internal sealed record ChargeRequest(decimal Amount, string Currency);

internal sealed record PaymentModeRequest(string Mode, double? FailureRate, int? StallMilliseconds);

internal sealed record PaymentQueueRequest(int Status, int Count, int? RetryAfterSeconds);

internal sealed record PricingSlowRequest(int Count, int DelayMs);

internal sealed record CatalogItem(string Id, string Name, decimal Price);

internal sealed record PricesBatchRequest(string[] ProductIds);

internal enum PaymentFaultMode
{
    Normal,
    Down,
    Intermittent,
    Stall,
}

internal sealed record PaymentFaultOutcome(int? Status, int? RetryAfterSeconds, int? StallMilliseconds);

// m03 payments fault plane. A one-shot queue (queue-failures) always takes
// priority over the persistent mode, so a take can leave "intermittent" set
// for flavor while still forcing one deterministic failure for the camera.
// Every field defaults to the m01/m02 behavior: Mode.Normal, empty queue,
// meaning NextOutcome() returns null and /payments/charge always succeeds.
internal sealed class PaymentFaultController
{
    private readonly Queue<PaymentFaultOutcome> _queue = new();
    private readonly Random _random = new();
    private readonly Lock _lock = new();

    private PaymentFaultMode _mode = PaymentFaultMode.Normal;
    private double _failureRate = 0.5;
    private int _stallMilliseconds = 8000;

    public PaymentFaultMode SetMode(string mode, double? failureRate, int? stallMilliseconds)
    {
        var parsed = Enum.Parse<PaymentFaultMode>(mode, ignoreCase: true);

        lock (_lock)
        {
            _mode = parsed;
            if (failureRate is { } rate)
            {
                _failureRate = rate;
            }

            if (stallMilliseconds is { } stall)
            {
                _stallMilliseconds = stall;
            }

            return _mode;
        }
    }

    public void QueueFailures(int status, int count, int? retryAfterSeconds)
    {
        lock (_lock)
        {
            for (var i = 0; i < count; i++)
            {
                _queue.Enqueue(new PaymentFaultOutcome(status, retryAfterSeconds, null));
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _queue.Clear();
            _mode = PaymentFaultMode.Normal;
        }
    }

    public PaymentFaultOutcome? NextOutcome()
    {
        lock (_lock)
        {
            if (_queue.TryDequeue(out var queued))
            {
                return queued;
            }

            return _mode switch
            {
                PaymentFaultMode.Down => new PaymentFaultOutcome(StatusCodes.Status503ServiceUnavailable, null, null),
                PaymentFaultMode.Intermittent => _random.NextDouble() < _failureRate
                    ? new PaymentFaultOutcome(StatusCodes.Status503ServiceUnavailable, null, null)
                    : null,
                PaymentFaultMode.Stall => new PaymentFaultOutcome(null, null, _stallMilliseconds),
                _ => null,
            };
        }
    }
}

// m03-c06 pricing fault plane: a one-shot per-call delay queue. Consumed one
// entry per GET /pricing/price/{id} regardless of productId, which is what
// lets a hedged second attempt against the same endpoint win deterministically
// (see the file header comment). Empty by default, so /pricing/price/{id}
// answers instantly exactly as it does today.
internal sealed class PricingFaultController
{
    private readonly Queue<int> _delaysMilliseconds = new();
    private readonly Lock _lock = new();

    public void QueueDelay(int count, int delayMs)
    {
        lock (_lock)
        {
            for (var i = 0; i < count; i++)
            {
                _delaysMilliseconds.Enqueue(delayMs);
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _delaysMilliseconds.Clear();
        }
    }

    public int NextDelayMilliseconds()
    {
        lock (_lock)
        {
            return _delaysMilliseconds.TryDequeue(out var delay) ? delay : 0;
        }
    }
}

internal sealed record TokenRequest(string? ClientId);

internal sealed record IssuedToken(string Value, DateTimeOffset ExpiresAt);

// Opaque bearer tokens, validated by membership and expiry. RevokeAll plays the
// role of a signing-key rotation: every token issued so far stops validating,
// including the pre-issued development token.
internal sealed class TokenStore
{
    public const string DevToken = "tok_dev_2026";
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(5);

    private readonly Dictionary<string, DateTimeOffset> _issued = new();
    private readonly Lock _lock = new();

    public TokenStore(bool requireAuth)
    {
        if (requireAuth)
        {
            _issued[DevToken] = DateTimeOffset.MaxValue;
        }
    }

    public IssuedToken Issue()
    {
        var token = new IssuedToken(
            "tok_" + Guid.NewGuid().ToString("N")[..16],
            DateTimeOffset.UtcNow.Add(TokenLifetime));

        lock (_lock)
        {
            _issued[token.Value] = token.ExpiresAt;
        }

        return token;
    }

    public int RevokeAll()
    {
        lock (_lock)
        {
            var count = _issued.Count;
            _issued.Clear();
            return count;
        }
    }

    public bool IsValid(string? authorizationHeader)
    {
        const string scheme = "Bearer ";
        if (authorizationHeader is null || !authorizationHeader.StartsWith(scheme, StringComparison.Ordinal))
        {
            return false;
        }

        var token = authorizationHeader[scheme.Length..].Trim();

        lock (_lock)
        {
            return _issued.TryGetValue(token, out var expiresAt) && expiresAt > DateTimeOffset.UtcNow;
        }
    }
}

internal sealed record PaymentLatencyRequest(int DelayMs);

// m06 payment traffic recorder. Watches /payments/charge from the gateway's side:
// how many calls arrived, the most in flight at once, and the most that arrived
// inside any one-second span. Counters only observe; the only knob is the fixed
// per-call delay (default 0), so every earlier clip behaves as before. A fixed
// delay on every charge is what gives m06-c03's concurrency beat calls that stay
// in flight long enough to count.
internal sealed class PaymentTrafficRecorder
{
    private readonly List<long> _arrivals = new();
    private readonly Lock _lock = new();
    private int _inFlight;
    private int _peakInFlight;
    private int _delayMilliseconds;

    public IDisposable Enter()
    {
        lock (_lock)
        {
            _arrivals.Add(System.Diagnostics.Stopwatch.GetTimestamp());
            _inFlight++;
            _peakInFlight = Math.Max(_peakInFlight, _inFlight);
        }

        return new Visit(this);
    }

    public Task DelayAsync(CancellationToken cancellationToken)
    {
        var delay = Volatile.Read(ref _delayMilliseconds);
        return delay > 0 ? Task.Delay(delay, cancellationToken) : Task.CompletedTask;
    }

    public void SetDelay(int delayMs) => Volatile.Write(ref _delayMilliseconds, Math.Max(0, delayMs));

    public void Reset()
    {
        SetDelay(0);
        ResetCounters();
    }

    public void ResetCounters()
    {
        lock (_lock)
        {
            _arrivals.Clear();
            _peakInFlight = _inFlight;
        }
    }

    public object Snapshot()
    {
        lock (_lock)
        {
            var oneSecond = System.Diagnostics.Stopwatch.Frequency;
            var sorted = _arrivals.Order().ToArray();
            var maxInOneSecond = 0;
            var start = 0;

            for (var end = 0; end < sorted.Length; end++)
            {
                while (sorted[end] - sorted[start] >= oneSecond)
                {
                    start++;
                }

                maxInOneSecond = Math.Max(maxInOneSecond, end - start + 1);
            }

            // Waves: arrivals bunched together (gaps under 100 ms) count as one wave,
            // reported with its size and when it landed after the first arrival. This is
            // how a burst held back by a limiter reads: "5 at +0.0 s, 5 at +1.0 s".
            var waves = new List<object>();
            var waveStart = 0;
            for (var i = 1; i <= sorted.Length; i++)
            {
                if (i == sorted.Length || (sorted[i] - sorted[i - 1]) * 1000 / oneSecond >= 100)
                {
                    var atMs = (int)((sorted[waveStart] - sorted[0]) * 1000 / oneSecond);
                    waves.Add(new { count = i - waveStart, atMs });
                    waveStart = i;
                }
            }

            return new { total = sorted.Length, peakInFlight = _peakInFlight, maxInOneSecond, waves };
        }
    }

    private void Leave()
    {
        lock (_lock)
        {
            _inFlight--;
        }
    }

    private sealed class Visit(PaymentTrafficRecorder owner) : IDisposable
    {
        public void Dispose() => owner.Leave();
    }
}

internal sealed record TraceEntry(int Sequence, string Service, string Method, string Path, string TraceId, string ParentSpanId);

// m06 trace log. Remembers the W3C traceparent header ("00-<trace id>-<parent span id>-<flags>")
// of every request the services receive, read straight off the wire. It never creates or
// continues an Activity itself: what is recorded is exactly what the client sent, which is
// the point of m06-c06.
internal sealed class TraceLog
{
    private const int Capacity = 5000;

    private readonly Queue<TraceEntry> _entries = new();
    private readonly Lock _lock = new();
    private int _sequence;

    public static string? ServiceFor(int port, PathString path) => port switch
    {
        5201 => "payments",
        5211 => "payments-sandbox",
        5202 => "pricing",
        5203 => "catalog",
        5204 => "discounts",
        5205 => "shipping-estimate",
        5001 when path.Value?.Contains("Inventory", StringComparison.Ordinal) == true => "inventory",
        5001 when path.Value?.Contains("Shipping", StringComparison.Ordinal) == true => "shipping",
        _ => null,
    };

    public void Record(string service, string method, string path, string traceparent)
    {
        var parts = traceparent.Split('-');
        var traceId = parts.Length == 4 ? parts[1] : string.Empty;
        var parentSpanId = parts.Length == 4 ? parts[2] : string.Empty;

        lock (_lock)
        {
            _entries.Enqueue(new TraceEntry(++_sequence, service, method, path, traceId, parentSpanId));
            while (_entries.Count > Capacity)
            {
                _entries.Dequeue();
            }
        }
    }

    public TraceEntry[] ForTrace(string traceId)
    {
        lock (_lock)
        {
            return _entries
                .Where(entry => string.Equals(entry.TraceId, traceId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}
