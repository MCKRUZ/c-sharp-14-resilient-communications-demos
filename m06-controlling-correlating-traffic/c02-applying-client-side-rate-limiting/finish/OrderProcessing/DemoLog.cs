using System.Text;

namespace OrderProcessing;

// Mirrors console output to demo-output.log next to the exe so automated
// recording takes can assert on what a run actually printed (disk truth), and
// grows the console window so a whole burst of request-log lines stays on
// screen. Not part of the course material; never shown on camera.
internal sealed class DemoLog : TextWriter
{
    private const int PreferredWindowHeight = 45;

    private readonly TextWriter _console;
    private readonly StreamWriter _file;

    private DemoLog(TextWriter console, StreamWriter file)
    {
        _console = console;
        _file = file;
    }

    public static DemoLog Attach()
    {
        GrowConsoleWindow();

        var path = Path.Combine(AppContext.BaseDirectory, "demo-output.log");
        var file = new StreamWriter(path, append: false) { AutoFlush = true };
        var log = new DemoLog(Console.Out, file);
        Console.SetOut(log);
        return log;
    }

    public override Encoding Encoding => _console.Encoding;

    public override void Write(char value)
    {
        _console.Write(value);
        _file.Write(value);
    }

    public override void Write(string? value)
    {
        _console.Write(value);
        _file.Write(value);
    }

    public override void WriteLine(string? value)
    {
        _console.WriteLine(value);
        _file.WriteLine(value);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Console.SetOut(_console);
            _file.Dispose();
        }

        base.Dispose(disposing);
    }

    private static void GrowConsoleWindow()
    {
        if (!OperatingSystem.IsWindows() || Console.IsOutputRedirected)
        {
            return;
        }

        try
        {
            var height = Math.Min(PreferredWindowHeight, Console.LargestWindowHeight);
            if (height > Console.WindowHeight)
            {
                Console.SetWindowSize(Console.WindowWidth, height);
            }
        }
        catch (Exception ex) when (ex is IOException or ArgumentOutOfRangeException or PlatformNotSupportedException)
        {
            // A host that refuses to resize (Windows Terminal, a redirected
            // console) just keeps its own size; the demo still runs.
        }
    }
}
