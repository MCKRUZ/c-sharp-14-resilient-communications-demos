# Cutting tail latency with hedging

Module 3, clip 6: Building Resilience Pipelines with Polly

Two demo beats: wrap `PricingClient.GetPriceAsync` in a hedging pipeline (`MaxHedgedAttempts` 2, `Delay` 100ms), then run a price lookup against a pricing service rigged to stall its first responder for two seconds and watch the hedged second attempt win the race, finishing the whole call in well under two seconds.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/PricingClient.cs`

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

Menu options the clip runs, in order: `16`. Type `q` to quit.
