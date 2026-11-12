# Adding Polly resilience to gRPC

Module 4, clip 5: Consuming gRPC Services

Three demo beats: move the gRPC client into DI with `AddGrpcClient` (replacing m04-c04's hand-built channel), chain `.AddStandardResilienceHandler()` onto the same builder, then inject `InventoryClient` into `DemoMenu` and run it -- proving the hardened call still works exactly as before. The resilience handler's presence is invisible on a success path by design; this clip's payoff is "same code, hardened path", not a visible retry.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/Program.cs`

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
