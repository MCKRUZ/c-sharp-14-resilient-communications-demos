# Keeping streams alive with keepalive and deadlines

Module 5, clip 3: Streaming and Concurrent Communications

One demo beat, editor-only. The rest of the clip is slides (the idle-connection trap, keepalive pings, why a unary deadline is wrong for a stream, what to use instead). In the demo, `ShippingChannel.Create` grows a `SocketsHttpHandler` with the four keepalive settings and hands it to the channel through `GrpcChannelOptions.HttpHandler`.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/ShippingChannel.cs`

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
