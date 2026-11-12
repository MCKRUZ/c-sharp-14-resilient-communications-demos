using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderProcessing;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddHttpClient();

builder.Services.AddHttpClient("payments", client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:5201");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<PricingClient>(client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:5202");
});

builder.Services.AddSingleton<PaymentGatewayClient>();
builder.Services.AddTransient<DemoMenu>();

var host = builder.Build();

await host.Services.GetRequiredService<DemoMenu>().RunAsync();
