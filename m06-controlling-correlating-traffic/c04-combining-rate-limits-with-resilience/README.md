# Combining rate limits with resilience pipelines

Module 6, clip 4: Controlling and Correlating Traffic

Four demo beats: a hand-wired retry with nothing limiting what is in flight multiplies 15 failing orders into 60 gateway calls; the retry code comes out; `AddStandardResilienceHandler` with a rate limiter of five puts the limiter outermost and the same failing burst becomes 20 calls, never more than five at once. Slides (two controls on one stream, the standard handler's order) sit between the beats.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/PaymentGatewayClient.cs`
- `OrderProcessing/Program.cs`

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

Menu options the clip runs, in order: `27`, `27`. Type `q` to quit.
