# Making unary calls with the gRPC client

Module 4, clip 4: Consuming gRPC Services

Four demo beats: build a hand-wired `GrpcChannel` and `InventoryClient` inline in `DemoMenu.cs` (no DI yet -- that's m04-c05), make the first real `CheckStockAsync` call this course has made, then run it for real and read the typed response back. This is the first clip in the module where the app actually runs.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- No source file changes. The clip runs the starter as it is.

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

Menu options the clip runs, in order: `17`. Type `q` to quit.
