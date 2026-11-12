# Configuring IHttpClientFactory

Module 1, clip 4: Communicating Resiliently in Distributed Systems

Four demo beats: register the factory with one `builder.Services.AddHttpClient()` line in Program.cs, inject `IHttpClientFactory` into PaymentGatewayClient and swap `new HttpClient()` for `_factory.CreateClient()`, walk the unchanged call site, then re-run the exact 400-call flood that killed c03 and watch it finish clean. The edited files must end byte-identical to the c05 starter; c05 picks up where this take leaves off.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/PaymentGatewayClient.cs`
- `OrderProcessing/Program.cs`

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

Menu options the clip runs, in order: `2`. Type `q` to quit.
