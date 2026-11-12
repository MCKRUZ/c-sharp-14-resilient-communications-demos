# Propagating W3C trace headers end to end

Module 6, clip 6: Controlling and Correlating Traffic

Two demo beats, no code edits: an `HttpClient` call made inside an activity shows the `traceparent` header .NET attached by itself, with the same trace id as `Activity.Current`; then one order runs through every service, gRPC included, and each service reports what it logged under that one trace id. Three slides (what traveling looks like, inside `traceparent`, who decides the format) are interleaved.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- No source file changes. The clip runs or reads the starter as it is.

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

Menu options the clip runs, in order: `29`, `30`. Type `q` to quit.
