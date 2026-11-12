# How the factory manages handler lifetimes

Module 2, clip 1: Managing HTTP Clients and Authentication

One demo beat, inserted between the "Why the lifetime is a balancing act" and "The handler pipeline" slides. It shows the handler lifetime being configured in code: open `Program.cs`, chain `.SetHandlerLifetime(TimeSpan.FromMinutes(5))` onto the existing `AddHttpClient<PricingClient>` registration, build, run, and look up one price. Before this demo, `SetHandlerLifetime` was one spoken sentence in the course and never appeared on screen. Added for objective O2 after external review (Filip E). It is deliberately NOT a socket-exhaustion demo: m01-c03 already shows that crash.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

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

Menu options the clip runs, in order: `3`. Type `q` to quit.
