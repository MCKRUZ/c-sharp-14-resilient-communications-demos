# Attaching auth tokens automatically

Module 2, clip 3: Managing HTTP Clients and Authentication

Six demo beats: build `AuthTokenHandler` in three full-file pastes (a `DelegatingHandler` with an `ITokenProvider` constructor, a `SendAsync` that asks the provider for a token, then the `Authorization: Bearer` assignment), register it and chain it onto the payment and catalog clients ahead of the timing handler, run two payments and two catalog lookups that now return 200, then charge once more and read the `Authorization: Bearer` line the request log shows for it.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/AuthTokenHandler.cs`
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

Menu options the clip runs, in order: `6`, `1`. Type `q` to quit.
