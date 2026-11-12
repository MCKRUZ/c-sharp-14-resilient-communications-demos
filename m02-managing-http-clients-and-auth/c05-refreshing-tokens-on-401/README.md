# Refreshing tokens on 401 responses

Module 2, clip 5: Managing HTTP Clients and Authentication

Six demo beats: extend `AuthTokenHandler.SendAsync` in four full-file pastes (capture the response, check for `HttpStatusCode.Unauthorized`, ask the provider for a fresh token, set the header again and send a second time), walk the guard that keeps it to exactly one retry, then run a scenario that warms the cache, rotates the auth service's keys behind the scenes, and charges again: the first attempt logs 401, the retry logs 200, and the caller just sees an approved payment.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/AuthTokenHandler.cs`

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

Menu options the clip runs, in order: `8`. Type `q` to quit.
