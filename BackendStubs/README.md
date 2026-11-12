# BackendStubs

One process that plays every downstream service the demo app calls. Start it before running any clip's code and leave it running.

```powershell
# modules 1, 3 and 4: no token required
dotnet run --project BackendStubs

# modules 2, 5 and 6: every service demands a bearer token
dotnet run --project BackendStubs -- --require-auth
```

Run these from the repository root. From inside a clip folder use `../../BackendStubs`.

| Service | Address | Endpoint | Notes |
| --- | --- | --- | --- |
| Auth service | `http://127.0.0.1:5200` | `POST /connect/token`, `POST /admin/rotate` | auth mode only |
| Payment gateway | `http://127.0.0.1:5201` | `POST /payments/charge` | 401 without a token in auth mode |
| Pricing service | `http://127.0.0.1:5202` | `GET /pricing/price/{productId}` | 401 without a token in auth mode |
| Catalog service | `http://127.0.0.1:5203` | `GET /catalog/products/{id}` | 401 without a token in auth mode |
| Discounts service | `http://127.0.0.1:5204` | `GET /discounts/{sku}` | module 5 |
| Shipping-estimate service | `http://127.0.0.1:5205` | `GET /shipping/estimate/{sku}` | module 5 |
| Payment gateway, sandbox | `http://127.0.0.1:5211` | `POST /payments/charge` | auth mode only |
| gRPC Inventory and Shipping | `https://localhost:5001` | `Inventory.CheckStock`, `Shipping.TrackShipment` | modules 4 and 5 |

Product data is fixed: SKU-1001 Wireless Mouse 24.99, SKU-1002 Mechanical Keyboard 89.99, SKU-1003 USB-C Dock 149.99.

The HTTP URLs use `127.0.0.1` rather than `localhost` on purpose. The module 1 socket-exhaustion demo needs the IPv4 loopback so a constrained IPv4 dynamic port range actually applies.

## gRPC needs a trusted development certificate

The gRPC port uses the ASP.NET Core development certificate. Run this once before module 4 or 5:

```powershell
dotnet dev-certs https --trust
```

The Inventory service knows one SKU, `ABC-123` (in stock, 40 available). Any other SKU returns `NotFound`.

## Auth mode

In auth mode `POST /connect/token` issues an opaque bearer token (`tok_` plus 16 hex characters, valid for 300 seconds). A pre-issued development token, `tok_dev_2026`, exists at startup. `POST /admin/rotate` revokes every token, the development token included, so restart the stubs afterwards. The gRPC port is not bearer-checked.

## Faults the demos arm

Every fault is off by default. The demo menu in the app calls these admin endpoints for you, so you normally never call them by hand.

- Payments: `POST /admin/payments/mode` (`normal`, `down`, `intermittent`, `stall`), `POST /admin/payments/queue-failures` (one-shot 503 or 429 with `Retry-After`), `POST /admin/payments/reset`. All on port 5200.
- Pricing: `POST /admin/pricing/slow-next` (one-shot delays), `POST /admin/pricing/reset`.
- Shipping stream: `POST /admin/shipping/drop-after` (cut the next stream after N events), `POST /admin/shipping/reset`.
- Fan-out: `POST /admin/fanout/latency`, `POST /admin/shipping-estimate/mode`, `POST /admin/fanout/reset`.
- Rate limiting (module 6): `POST /admin/payments/latency` adds a fixed delay to every charge. `GET /admin/payments/stats` reports what the gateway saw (total calls, peak in flight, arrival waves) and `POST /admin/payments/stats/reset` clears it.

The full request shapes are in the comment block at the top of `Program.cs`.

## Trace recording (module 6)

Every service records the W3C `traceparent` header of each request it receives, read off the wire. `GET http://127.0.0.1:5200/admin/trace/{traceId}` returns the calls that carried that trace id, oldest first, with the service, method, path and the caller's span id. `POST /admin/trace/reset` clears the log. `GET http://127.0.0.1:5200/echo/headers` reflects the request headers it received.

## Running while you edit

`dotnet run` locks `BackendStubs/bin`. To keep editing the stubs, build to another folder (`dotnet build -o <folder>`) and run the DLL from there.
