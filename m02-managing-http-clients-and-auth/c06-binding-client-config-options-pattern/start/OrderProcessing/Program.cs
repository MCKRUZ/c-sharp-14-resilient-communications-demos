using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderProcessing;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddTransient<RequestTimingHandler>();

builder.Services.AddHttpClient("payments", client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:5201");
    client.Timeout = TimeSpan.FromSeconds(10);
})
    .AddHttpMessageHandler<AuthTokenHandler>()
    .AddHttpMessageHandler<RequestTimingHandler>();

builder.Services.AddHttpClient<PricingClient>(client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:5202");
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
