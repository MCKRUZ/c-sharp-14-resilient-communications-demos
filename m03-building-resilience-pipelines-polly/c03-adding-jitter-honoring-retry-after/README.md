# Adding jitter and honoring Retry-After

Module 3, clip 3: Building Resilience Pipelines with Polly

Four demo beats: add `UseJitter = true` to the retry options and fire five concurrent charges against an intermittently failing gateway to show their retries land at different times instead of a synchronized spike, then add a `DelayGenerator` that reads a `Retry-After` header off the response and run against a gateway that throttles with `Retry-After: 3` to watch the retry wait exactly three seconds.

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

Menu options the clip runs, in order: `11`, `12`. Type `q` to quit.
