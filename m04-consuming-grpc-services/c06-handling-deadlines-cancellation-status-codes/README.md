# Handling deadlines, cancellation, and status codes

Module 4, clip 6: Consuming gRPC Services

Four demo beats, all editor-only: add a `deadline` to the call, add a `CancellationToken` alongside it, wrap the call in `try/catch (RpcException)`, then switch on `ex.StatusCode` for `DeadlineExceeded`, `Unavailable`, `NotFound`, and `Cancelled`. Unlike every other demo clip in this module, **the script never runs the app in this clip** -- every `[DEMO:]` block is a code edit, and the module recap that follows is a `[SLIDE:]`, not a demo run.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- No file shown on screen changes. Only the demo harness does (see below).

Changed off camera (demo harness, see the root README): `DemoMenu.cs`.

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
