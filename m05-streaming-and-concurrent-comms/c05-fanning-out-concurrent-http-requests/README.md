# Fanning out concurrent HTTP requests

Module 5, clip 5: Streaming and Concurrent Communications

Four demo beats, all whole-file pastes of `FanOutSummary.cs` (class `OrderSummaryFanOut`): the slow sequential `BuildAsync` (run it, read the timer), the fan-out rewrite with `Task.WhenAll` (run it again), and a bounded `PriceManyAsync` built on `Parallel.ForEachAsync` (editor only).

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/FanOutSummary.cs`

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

Menu options the clip runs, in order: `20`, `20`, `20`. Type `q` to quit.
