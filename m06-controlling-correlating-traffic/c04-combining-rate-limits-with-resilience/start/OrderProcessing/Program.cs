using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderProcessing;
using OrderProcessing.Inventory;
using OrderProcessing.Shipping;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);
// The resilience handlers log every retry attempt at Warning; keep the console on the
// request lines the demo narrates.
builder.Logging.AddFilter("Polly", LogLevel.None);

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<PaymentGatewayOptions>(builder.Configuration.GetSection("PaymentGateway"));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddTransient<RequestTimingHandler>();

builder.Services.AddHttpClient("payments", (provider, client) =>
{
    var options = provider.GetRequiredService<IOptions<PaymentGatewayOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
    .AddHttpMessageHandler<AuthTokenHandler>()
    .AddHttpMessageHandler<RequestTimingHandler>();

builder.Services.AddHttpClient<PricingClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Pricing"]!);
})
    .AddHttpMessageHandler<AuthTokenHandler>()
    .AddHttpMessageHandler<RequestTimingHandler>()
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.MaxRetryAttempts = 2;
        o.Retry.Delay = TimeSpan.FromMilliseconds(100);
        o.Retry.UseJitter = false;
    });

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalog"]!);
})
    .AddHttpMessageHandler<AuthTokenHandler>()
    .AddHttpMessageHandler<RequestTimingHandler>()
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.MaxRetryAttempts = 2;
        o.Retry.Delay = TimeSpan.FromMilliseconds(100);
        o.Retry.UseJitter = false;
    });

builder.Services.AddHttpClient<DiscountClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Discounts"]!);
})
    .AddHttpMessageHandler<AuthTokenHandler>()
    .AddHttpMessageHandler<RequestTimingHandler>()
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.MaxRetryAttempts = 2;
        o.Retry.Delay = TimeSpan.FromMilliseconds(100);
        o.Retry.UseJitter = false;
    });

builder.Services.AddHttpClient<ShippingEstimateClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ShippingEstimate"]!);
})
    .AddHttpMessageHandler<AuthTokenHandler>()
    .AddHttpMessageHandler<RequestTimingHandler>()
    .AddStandardResilienceHandler(o =>
    {
        o.Retry.MaxRetryAttempts = 2;
        o.Retry.Delay = TimeSpan.FromMilliseconds(100);
        o.Retry.UseJitter = false;
    });

// chain the standard resilience handler onto the
// same IHttpClientBuilder AddGrpcClient returned.
builder.Services.AddGrpcClient<Inventory.InventoryClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Services:Inventory"]!);
})
    .AddStandardResilienceHandler();

builder.Services.AddSingleton(_ => new Shipping.ShippingClient(
    ShippingChannel.Create(new Uri(builder.Configuration["Services:Shipping"]!))));
builder.Services.AddTransient<ShipmentTracker>();

builder.Services.AddSingleton<PaymentGatewayClient>();
builder.Services.AddTransient<OrderSummaryService>();
builder.Services.AddTransient<OrderSummaryFanOut>();
builder.Services.AddTransient<DemoMenu>();

var host = builder.Build();

await host.Services.GetRequiredService<DemoMenu>().RunAsync();
