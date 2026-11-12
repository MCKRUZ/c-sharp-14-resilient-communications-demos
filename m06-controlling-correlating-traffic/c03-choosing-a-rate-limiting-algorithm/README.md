# Choosing a rate-limiting algorithm

Module 6, clip 3: Controlling and Correlating Traffic

Three demo beats, one per limiter, all on the single file `PaymentRateLimiter.cs`: the fixed window and its boundary burst, the sliding window that closes that gap, and the concurrency limiter that counts requests in flight instead of requests per second. Slides (the four limiters, and how to pick) bookend the demo.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- No net change. The clip edits `PaymentRateLimiter.cs` along the way and ends on the version it started with, so `finish/` equals `start/`.

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

Menu options the clip runs, in order: `25`, `25`, `26`. Type `q` to quit.
