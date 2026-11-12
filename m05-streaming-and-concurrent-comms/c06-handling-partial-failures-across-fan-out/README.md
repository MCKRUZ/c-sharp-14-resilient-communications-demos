# Handling partial failures across a fan-out

Module 5, clip 6: Streaming and Concurrent Communications

Five demo beats on `FanOutSummary.cs`. Four of them run the code on camera so the viewer sees the effect of each change: the naive `Task.WhenAll` fails when the shipping estimate service is down, each call is wrapped to capture its own `Outcome<T>`, `Task.WhenAll` over the wrapped tasks never throws and each outcome prints, the decision logic degrades the summary, and `Task.WhenEach` prints each result the moment it lands. Beat 1 (the wrapper) is the one beat with no run.

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

Menu options the clip runs, in order: `23`, `21`, `21`, `22`, `21`. Type `q` to quit.
