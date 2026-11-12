# Binding client config with the options pattern

Module 2, clip 6: Managing HTTP Clients and Authentication

Six demo beats: paste two options classes (`AuthOptions`, `PaymentGatewayOptions`), tour their mirror sections in `appsettings.json`, bind them with `Configure<T>(GetSection)` in `Program.cs`, move `TokenProvider` onto `IOptions<AuthOptions>`, move the payments client onto the `(provider, client)` overload reading `IOptions<PaymentGatewayOptions>`, then run once against the primary gateway, change one line of `appsettings.json`, and run again against the sandbox gateway with no recompile.

## Folders

- `start/`: the solution as it is when the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

## What changes in this clip

- `OrderProcessing/AuthOptions.cs`
- `OrderProcessing/PaymentGatewayOptions.cs`
- `OrderProcessing/Program.cs`
- `OrderProcessing/TokenProvider.cs`
- `OrderProcessing/appsettings.json`

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

Menu options the clip runs, in order: `1`, `1`. Type `q` to quit.
