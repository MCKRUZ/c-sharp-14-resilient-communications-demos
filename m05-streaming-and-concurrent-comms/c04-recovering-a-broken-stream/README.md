# Recovering a broken stream

Module 5, clip 4: Streaming and Concurrent Communications

Four demo beats. Run the tracking stream and watch the service drop it (an `RpcException` with `StatusCode.Unavailable`). Add a `lastSequence` cursor, wrap the stream in a retry loop that catches `Unavailable`, backs off and resumes from the cursor, then run it again and watch it reconnect from sequence 3 and finish at 4.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/ShipmentTracker.cs`

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

Menu options the clip runs, in order: `19`, `19`. Type `q` to quit.
