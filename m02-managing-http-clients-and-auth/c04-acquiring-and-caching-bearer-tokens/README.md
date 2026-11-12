# Acquiring and caching bearer tokens

Module 2, clip 4: Managing HTTP Clients and Authentication

Six demo beats: tour the pre-written `TokenProvider` (the two cache fields, the cache check with its refresh buffer, the acquire call to the auth service's token endpoint, the `SemaphoreSlim` gate), register it as a singleton in `Program.cs` in place of the stand-in, then fire a 12-call burst across all three clients and watch one `fetched` line followed by eleven `reusing cached` lines.

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

Menu options the clip runs, in order: `7`. Type `q` to quit.
