using System.Diagnostics;

namespace OrderProcessing;

public class DemoMenu
{
    private readonly PaymentGatewayClient _payments;

    public DemoMenu(PaymentGatewayClient payments)
    {
        _payments = payments;
    }

    public async Task RunAsync()
    {
        using var log = DemoLog.Attach();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Order processing demo");
            Console.WriteLine("  [1] Charge one payment");
            Console.WriteLine("  [2] Flood: 400 payments in a tight loop");
            Console.WriteLine("  [q] Quit");
            Console.Write("> ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    await ChargeOneAsync();
                    break;
                case "2":
                    await FloodAsync();
                    break;
                case "q":
                    return;
            }
        }
    }

    private async Task ChargeOneAsync()
    {
        var receipt = await _payments.ChargeAsync(49.99m);
        Console.WriteLine($"Payment approved: {receipt}");
    }

    private async Task FloodAsync()
    {
        Console.WriteLine("Firing 400 payment calls...");
        var stopwatch = Stopwatch.StartNew();

        for (var i = 1; i <= 400; i++)
        {
            try
            {
                await _payments.ChargeAsync(9.99m);
            }
            catch (Exception ex)
            {
                var root = ex.GetBaseException();
                Console.WriteLine();
                Console.WriteLine($"Call {i} FAILED after {stopwatch.Elapsed.TotalSeconds:F1}s:");
                Console.WriteLine($"  {root.GetType().Name}: {root.Message}");
                return;
            }

            if (i % 50 == 0)
            {
                Console.WriteLine($"  {i} calls completed...");
            }
        }

        Console.WriteLine($"All 400 calls completed in {stopwatch.Elapsed.TotalSeconds:F1}s with no errors.");
    }
}
