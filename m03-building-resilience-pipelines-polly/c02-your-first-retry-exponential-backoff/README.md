# Your first retry with exponential backoff

Module 3, clip 2: Building Resilience Pipelines with Polly

Five demo beats: run the unmodified payment client against a flaky gateway and watch a transient 503 kill the order, then build a Polly retry pipeline in `PaymentGatewayClient.cs` across three pastes (bare `AddRetry`, then `ShouldHandle` plus `MaxRetryAttempts`, then `Delay`/`BackoffType.Exponential` and the call wrapped in `pipeline.ExecuteAsync`), then run it again and watch attempt one fail and attempt two heal the order automatically.

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

Menu options the clip runs, in order: `10`, `9`, `9`, `9`, `10`. Type `q` to quit.
