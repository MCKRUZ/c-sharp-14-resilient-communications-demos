using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderProcessing;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

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
    .AddHttpMessageHandler<RequestTimingHandler>();

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalog"]!);
})
    .AddHttpMessageHandler<AuthTokenHandler>()
    .AddHttpMessageHandler<RequestTimingHandler>();

builder.Services.AddSingleton<PaymentGatewayClient>();
builder.Services.AddTransient<OrderSummaryService>();
builder.Services.AddTransient<DemoMenu>();

var host = builder.Build();

await host.Services.GetRequiredService<DemoMenu>().RunAsync();
