# Consuming a gRPC server stream

Module 5, clip 2: Streaming and Concurrent Communications

Five demo beats. Show the `shipping.proto` contract (the `stream` keyword on the response), call `TrackShipment` and look at the `AsyncServerStreamingCall` it returns, look at `call.ResponseStream`, replace it with `await foreach` over `ReadAllAsync`, then run it and watch four tracking events land one at a time.

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

Menu options the clip runs, in order: `18`. Type `q` to quit.
