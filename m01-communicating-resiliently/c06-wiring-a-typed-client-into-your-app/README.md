# Wiring a typed client into your app

Module 1, clip 6: Communicating Resiliently in Distributed Systems

Five demo beats: tour the pre-built `CatalogClient`, glance at the `Product` record, make the clip's one edit (register `AddHttpClient<CatalogClient>` in `Program.cs` with the base address pulled from configuration), tour `OrderSummaryService`, then run menu option 4 end to end and watch a real product fold into the order summary. Everything except that registration pre-exists in the starter; the script tours the code rather than typing it.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

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

Menu options the clip runs, in order: `4`. Type `q` to quit.
