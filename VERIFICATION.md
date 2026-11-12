# Verification: does the code match the delivered clips?

Each `finish/` folder was rebuilt from the clip's recorded demo script: the starter, plus every whole-file paste the script made, in order. The recording log confirms every paste step ran. All 74 pastes across modules 1 to 5 replace a whole file, so no editor behaviour had to be simulated.

That gives the code the clip *pasted*. To check it is what the clip *shows*, each delivered video was read by OCR (a frame every 2 seconds, editor pane only) and every line a clip adds or changes was searched for on screen.

| Check | Result |
| --- | --- |
| All 26 `start/` and 26 `finish/` solutions, plus BackendStubs, build with `dotnet build` | 0 errors |
| Changed lines checked against the video | 317 |
| Legible on screen, exact match by OCR | 252 |
| Not matched exactly, but the line's identifiers are on screen in one frame (OCR garbled it, or an editor hint such as `options:` was inserted mid-line) | 54 |
| No frame shows the line | 11 |

Lines in the last group are mostly `using` directives and lines near the top of a file that the video scrolls past after a whole-file paste. Their text comes from the paste in the script, not from a frame.

I also compared frames by eye for: m03-c06 (PricingClient), m03-c07 (Program.cs, line numbers 69 to 97 match the finish file), m04-c02 (inventory.proto, all 16 lines) and m04-c03 (the live-typed snippet).

## Per clip

| Clip | Changed lines | Exact on screen | Identifiers on screen | No frame shows it |
| --- | --- | --- | --- | --- |
| 1.4 Configuring IHttpClientFactory | 5 | 5 | 0 | 0 |
| 1.5 Named vs typed clients | 7 | 7 | 0 | 0 |
| 1.6 Wiring a typed client into your app | 2 | 1 | 1 | 0 |
| 2.1 How the factory manages handler lifetimes | 1 | 1 | 0 | 0 |
| 2.2 Intercepting requests with DelegatingHandlers | 15 | 15 | 0 | 0 |
| 2.3 Attaching auth tokens automatically | 15 | 15 | 0 | 0 |
| 2.4 Acquiring and caching bearer tokens | 1 | 1 | 0 | 0 |
| 2.5 Refreshing tokens on 401 responses | 10 | 10 | 0 | 0 |
| 2.6 Binding client config with the options pattern | 21 | 17 | 4 | 0 |
| 3.2 Your first retry with exponential backoff | 17 | 14 | 3 | 0 |
| 3.3 Adding jitter and honoring Retry-After | 5 | 3 | 2 | 0 |
| 3.4 Guarding calls with timeouts | 5 | 3 | 2 | 0 |
| 3.5 Stopping cascades with a circuit breaker | 10 | 9 | 1 | 0 |
| 3.6 Cutting tail latency with hedging | 11 | 9 | 2 | 0 |
| 3.7 Composing the standard resilience handler | 53 | 27 | 18 | 8 |
| 4.2 Modeling contracts with Protobuf | 10 | 8 | 0 | 2 |
| 4.3 Generating a strongly typed client | 2 | 2 | 0 | 0 |
| 4.4 Making unary calls with the gRPC client | 6 | 4 | 2 | 0 |
| 4.5 Adding Polly resilience to gRPC | 15 | 13 | 2 | 0 |
| 4.6 Handling deadlines, cancellation, and status codes | 19 | 15 | 4 | 0 |
| 5.2 Consuming a gRPC server stream | 9 | 6 | 3 | 0 |
| 5.3 Keeping streams alive with keepalive and deadlines | 7 | 7 | 0 | 0 |
| 5.4 Recovering a broken stream | 10 | 7 | 3 | 0 |
| 5.5 Fanning out concurrent HTTP requests | 20 | 19 | 0 | 1 |
| 5.6 Handling partial failures across a fan-out | 41 | 34 | 7 | 0 |

Clip 1.3 changes no source file, so it has nothing to compare. It only runs the starter.

## Lines no frame shows

These are the lines the OCR could not find anywhere in the clip:

- m03-c07 PricingClient.cs: `var quote = await _client.GetFromJsonAsync<PriceQuote>(`
- m03-c07 Program.cs: `using System.Net;`
- m03-c07 Program.cs: `using Microsoft.Extensions.Http.Resilience;`
- m03-c07 Program.cs: `response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests);`
- m03-c07 Program.cs: `// Honor a Retry-After header when the server sends one; fall back to`
- m03-c07 Program.cs: `args.Outcome.Result?.Headers.RetryAfter?.Delta is { } retryAfter`
- m03-c07 Program.cs: `: ValueTask.FromResult<TimeSpan?>(null);`
- m03-c07 Program.cs: `// there. Catalog never had a hand-built pipeline, so it goes straight to`
- m04-c02 inventory.proto: `syntax = "proto3";`
- m04-c02 inventory.proto: `option csharp_namespace = "OrderProcessing.Inventory";`
- m05-c05 FanOutSummary.cs: `using System.Collections.Concurrent;`

## Limits

- OCR reads a frame as text. It cannot prove a line is absent, only that it did not find it. A line it found is on screen; a line it missed may still be.
- Frames are sampled every 2 seconds, so a version visible for under 2 seconds can be missed.
- The check is against `delivery/` in the course repository. Where a clip was re-exported after delivery, a newer video can differ.
- Authoring comments such as `// m04-c05, paste v2, final: ...` are in the pasted code and on screen in the delivered videos. They are kept exactly.
