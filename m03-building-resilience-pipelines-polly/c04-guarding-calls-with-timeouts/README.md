# Guarding calls with timeouts

Module 3, clip 4: Building Resilience Pipelines with Polly

Four demo beats: run the existing retry pipeline against a gateway that accepts a request and then stalls, watch it hang with no error, add `AddTimeout(TimeSpan.FromSeconds(2))` after the retry (per-attempt position) and watch `TimeoutRejectedException` fire cleanly at 2 seconds, then add a second `AddTimeout(TimeSpan.FromSeconds(10))` before the retry (total position) to show the finished three-layer ordering: total timeout outermost, retry in the middle, per-attempt timeout innermost.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/PaymentGatewayClient.cs`

## Run it

Start the backend stubs from this folder (no auth flag for this module):

```powershell
dotnet run --project ../../BackendStubs
```

Then, in a second terminal:

```powershell
cd finish
dotnet run --project OrderProcessing
```

Menu options the clip runs, in order: `13`, `13`. Type `q` to quit.
