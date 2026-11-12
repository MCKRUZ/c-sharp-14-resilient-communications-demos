# Named vs typed clients

Module 1, clip 5: Communicating Resiliently in Distributed Systems

Five demo beats: register a named client (`AddHttpClient("payments", ...)`) in Program.cs, switch PaymentGatewayClient to `CreateClient("payments")` with a relative URL and prove it with one charge, tour the pre-written PricingClient class, point at its `AddHttpClient<PricingClient>` registration, then wire PricingClient into DemoMenu and look up a price through the typed client. Section 0 pastes the COMPLETE final Program.cs once (both registrations land together); the section that narrates the typed registration is a navigate-and-point beat, not a second paste. PricingClient.cs is pre-written in the starter and only toured, never created on camera.

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

Menu options the clip runs, in order: `1`, `3`. Type `q` to quit.
