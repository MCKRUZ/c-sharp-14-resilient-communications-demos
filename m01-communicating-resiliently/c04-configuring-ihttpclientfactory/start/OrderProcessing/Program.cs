using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderProcessing;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddSingleton<PaymentGatewayClient>();
builder.Services.AddTransient<DemoMenu>();

var host = builder.Build();

await host.Services.GetRequiredService<DemoMenu>().RunAsync();
