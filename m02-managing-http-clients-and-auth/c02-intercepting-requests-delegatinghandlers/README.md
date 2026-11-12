# Intercepting requests with DelegatingHandlers

Module 2, clip 2: Managing HTTP Clients and Authentication

Six demo beats: build `RequestTimingHandler` in four full-file pastes (inherit `DelegatingHandler`, override `SendAsync`, wrap `base.SendAsync` in a `Stopwatch`, log method, URI, status, and elapsed milliseconds), register it transient and chain it onto the typed clients in `Program.cs`, then run six pricing and catalog calls and watch each one print a timing line. Every line reads `401 Unauthorized`, because the backend stubs now demand a bearer token and nothing attaches one yet. That is the hook m02-c01 opened on and the problem m02-c03 fixes.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/Program.cs`
- `OrderProcessing/RequestTimingHandler.cs`

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

Menu options the clip runs, in order: `5`. Type `q` to quit.
