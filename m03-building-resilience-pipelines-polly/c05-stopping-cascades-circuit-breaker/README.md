# Stopping cascades with a circuit breaker

Module 3, clip 5: Building Resilience Pipelines with Polly

Three demo beats: add `AddCircuitBreaker` between the retry and the per-attempt timeout (`FailureRatio` 0.5, `SamplingDuration` 10s, `MinimumThroughput` 10, `BreakDuration` 30s), fire a 14-call concurrent burst against a downed gateway and watch the breaker trip mid-burst so later calls fail instantly with `BrokenCircuitException` instead of a real network call, then wait out the 30-second break duration and watch a Half-Open trial call succeed and the breaker close back to normal on its own.

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

Menu options the clip runs, in order: `14`, `15`. Type `q` to quit.
