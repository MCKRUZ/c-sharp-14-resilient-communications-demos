# Composing the standard resilience handler

Module 3, clip 7: Building Resilience Pipelines with Polly

Two demo beats: chain `AddResilienceHandler("payments", ...)` onto the payments client's registration in `Program.cs` (carrying forward the exact retry, circuit breaker, and per-attempt timeout built by hand across c02-c05) and simplify `PaymentGatewayClient.cs` back down to a plain client, then replace the catalog client's registration with `AddStandardResilienceHandler()` and the pricing client's with `AddStandardHedgingHandler()` (keeps the hedging built in c06), simplify `PricingClient.cs` back down too, and run the app to show every call still succeeding with resilience (and hedging) now attached to the client instead of hand-wired inside it.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/PaymentGatewayClient.cs`
- `OrderProcessing/PricingClient.cs`
- `OrderProcessing/Program.cs`

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

Menu options the clip runs, in order: `56`, `1`, `5`. Type `q` to quit.
