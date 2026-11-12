# Applying client-side rate limiting

Module 6, clip 2: Controlling and Correlating Traffic

Five demo beats that put a token-bucket rate limiter in front of the payment gateway: add the `System.Threading.RateLimiting` package, build a `RateLimitingHandler`, give it a ticket system (`AcquireAsync`, `RateLimitLease`, `IsAcquired`), configure the bucket, and fire 15 orders to watch the limiter turn a burst into a steady stream. A slide on what a token bucket is sits between beats 2 and 3.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/OrderProcessing.csproj`
- `OrderProcessing/PaymentRateLimiter.cs`
- `OrderProcessing/Program.cs`
- `OrderProcessing/RateLimitingHandler.cs`

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

Menu options the clip runs, in order: `24`. Type `q` to quit.
