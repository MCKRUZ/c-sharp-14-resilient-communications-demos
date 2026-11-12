# Correlating requests with TraceContext

Module 6, clip 5: Controlling and Correlating Traffic

Three demo beats in `OrderWorkflow.cs`: declare a shared `ActivitySource`, wrap the order work in `StartActivity("ProcessOrder")` and tag it with the order id, then print `Activity.Current`'s `TraceId` and `SpanId` and run one order. Slides (the words trace, span, correlation id, TraceContext; Activity as .NET's span) come first.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/OrderWorkflow.cs`

## Run it

Start the backend stubs from this folder (this module runs them in auth mode):

```powershell
dotnet run --project ../../BackendStubs -- --require-auth
```

Then, in a second terminal:

```powershell
cd finish
dotnet run --project OrderProcessing
```

Menu options the clip runs, in order: `28`. Type `q` to quit.
