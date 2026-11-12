# Why new HttpClient() causes outages

Module 1, clip 3: Communicating Resiliently in Distributed Systems

Two demo beats: run the textbook `using var client = new HttpClient()` payment call once and watch it work, then fire 400 of them in a tight loop and watch the process run out of sockets. This clip only RUNS code; it never edits it, so the c04 starter is byte-identical to this one.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- No source file changes. The clip runs or reads the starter as it is.

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

Menu options the clip runs, in order: `1`, `2`. Type `q` to quit.
