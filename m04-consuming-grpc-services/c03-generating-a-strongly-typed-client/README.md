# Generating a strongly typed client

Module 4, clip 3: Consuming gRPC Services

Four demo beats: wire `Grpc.Tools` and a `<Protobuf>` build item into `OrderProcessing.csproj`, build (Grpc.Tools generates the client from `inventory.proto` behind the scenes), then prove it by typing scratch code into `DemoMenu.cs` and watching IntelliSense surface `InventoryClient`, `CheckStockRequest`, and `CheckStockResponse` -- then undoing it all. Nothing is saved or committed in this clip; the real, working call is written in m04-c04.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/OrderProcessing.csproj`

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

## Typed live and not saved

The clip types three lines into `DemoMenu.cs` (around line 342) to show IntelliSense offering the generated types, then undoes them without saving. They are not in `finish/`. As typed:

```csharp
Inventory.InventoryClient? client = null;
var req = new Inventory.CheckStockRequest();
var resp = new Inventory.CheckStockResponse();
```
