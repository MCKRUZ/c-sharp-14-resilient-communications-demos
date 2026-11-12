using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using OrderProcessing;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<PaymentGatewayOptions>(builder.Configuration.GetSection("PaymentGateway"));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddTransient<RequestTimingHandler>();

var paymentsShouldHandle = new PredicateBuilder<HttpResponseMessage>()
    .Handle<HttpRequestException>()
    .HandleResult(response =>
        response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests);

// Honor a Retry-After header when the server sends one; fall back to
// the strategy's own backoff when it does not.
static ValueTask<TimeSpan?> HonorRetryAfter(RetryDelayGeneratorArguments<HttpResponseMessage> args) =>
    args.Outcome.Result?.Headers.RetryAfter?.Delta is { } retryAfter
        ? ValueTask.FromResult<TimeSpan?>(retryAfter)
        : ValueTask.FromResult<TimeSpan?>(null);

var paymentsClientBuilder = builder.Services.AddHttpClient("payments", (provider, client) =>
{
    var options = provider.GetRequiredService<IOptions<PaymentGatewayOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
paymentsClientBuilder.AddResilienceHandler("payments", pipeline => pipeline
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = paymentsShouldHandle,
        MaxRetryAttempts = 3,
        UseJitter = true,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        DelayGenerator = HonorRetryAfter,
    })
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = paymentsShouldHandle,
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(10),
        MinimumThroughput = 10,
        BreakDuration = TimeSpan.FromSeconds(30),
    })
    .AddTimeout(TimeSpan.FromSeconds(2)));
paymentsClientBuilder.AddHttpMessageHandler<AuthTokenHandler>();
paymentsClientBuilder.AddHttpMessageHandler<RequestTimingHandler>();

// Resilience wraps auth here too, same call-order rule as payments above.
// Pricing keeps the hedging built in m03-c06 -- AddStandardResilienceHandler
// doesn't include hedging, so pricing gets its sibling one-liner,
// AddStandardHedgingHandler, tuned with the same two knobs set by hand
// there. Catalog never had a hand-built pipeline, so it goes straight to
// the plain standard handler.
var pricingClientBuilder = builder.Services.AddHttpClient<PricingClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Pricing"]!);
});
pricingClientBuilder.AddStandardHedgingHandler().Configure(options =>
{
    options.Hedging.MaxHedgedAttempts = 2;
    options.Hedging.Delay = TimeSpan.FromMilliseconds(100);
});
pricingClientBuilder.AddHttpMessageHandler<AuthTokenHandler>();
pricingClientBuilder.AddHttpMessageHandler<RequestTimingHandler>();

var catalogClientBuilder = builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalog"]!);
});
catalogClientBuilder.AddStandardResilienceHandler();
catalogClientBuilder.AddHttpMessageHandler<AuthTokenHandler>();
catalogClientBuilder.AddHttpMessageHandler<RequestTimingHandler>();

builder.Services.AddSingleton<PaymentGatewayClient>();
builder.Services.AddTransient<OrderSummaryService>();
builder.Services.AddTransient<DemoMenu>();

var host = builder.Build();

await host.Services.GetRequiredService<DemoMenu>().RunAsync();
