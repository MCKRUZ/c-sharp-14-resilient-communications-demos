using System.Diagnostics;

namespace OrderProcessing;

public static class TracingSetup
{
    public static void Enable()
    {
        ActivitySource.AddActivityListener(new ActivityListener
        {
            ShouldListenTo = source => source.Name == "OrderProcessing",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        });
    }
}
