# Modeling contracts with Protobuf

Module 4, clip 2: Consuming gRPC Services

Five demo beats, all editor-only: open a pre-seeded empty `inventory.proto`, then build it up in four cumulative pastes (syntax + namespace, the `Inventory` service with its one `CheckStock` rpc, the `CheckStockRequest` message, then `CheckStockResponse`), ending on a highlight of both messages' field numbers. No app runs in this clip; the proto file isn't wired into the build until m04-c03.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/Protos/inventory.proto`

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
