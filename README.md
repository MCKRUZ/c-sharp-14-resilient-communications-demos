# Resilient Communications in C# 14: demo code

The demo code for each clip of the Pluralsight course *Resilient Communications in C# 14*. Every clip has two folders:

- `start/`: the solution as the clip opens.
- `finish/`: the solution exactly as the clip leaves it.

Each folder is a complete, runnable solution. Jump into any clip by opening its `start/`, and compare against `finish/` to see what the clip changed. All six modules are here.

## Requirements

- .NET 10 SDK
- PowerShell
- For modules 4, 5 and 6, a trusted development certificate: `dotnet dev-certs https --trust`

## Run a clip

1. Start the backend stubs from the repository root. Modules 2, 5 and 6 use auth mode.

```powershell
dotnet run --project BackendStubs
dotnet run --project BackendStubs -- --require-auth
```

2. In a second terminal, run the clip's solution:

```powershell
cd m03-building-resilience-pipelines-polly/c06-cutting-tail-latency-hedging/finish
dotnet run --project OrderProcessing
```

3. The app shows a numbered menu. Each clip's README lists the options that clip runs. Type `q` to quit.

See [BackendStubs/README.md](BackendStubs/README.md) for the services, ports and the faults the demos arm.

## Two things that are not on screen

- `DemoMenu.cs` and `DemoLog.cs` are the demo harness: the numbered menu and its console logging. They are in every solution so the code runs, and the clips do not show them. Where a clip's `finish/` differs from `start/` only in `DemoMenu.cs`, the clip README says the change was made off camera.
- Some clips paste a file several times, building it up (v1, then v2, and so on). `finish/` holds the last version. The earlier versions appear on screen only while the clip builds up to it.

## Clips

| Module | Clip | Folder |
| --- | --- | --- |
| 1. Communicating Resiliently in Distributed Systems | Why new HttpClient() causes outages | [`m01-communicating-resiliently/c03-why-new-httpclient-causes-outages`](m01-communicating-resiliently/c03-why-new-httpclient-causes-outages) |
| 1. Communicating Resiliently in Distributed Systems | Configuring IHttpClientFactory | [`m01-communicating-resiliently/c04-configuring-ihttpclientfactory`](m01-communicating-resiliently/c04-configuring-ihttpclientfactory) |
| 1. Communicating Resiliently in Distributed Systems | Named vs typed clients | [`m01-communicating-resiliently/c05-named-vs-typed-clients`](m01-communicating-resiliently/c05-named-vs-typed-clients) |
| 1. Communicating Resiliently in Distributed Systems | Wiring a typed client into your app | [`m01-communicating-resiliently/c06-wiring-a-typed-client-into-your-app`](m01-communicating-resiliently/c06-wiring-a-typed-client-into-your-app) |
| 2. Managing HTTP Clients and Authentication | How the factory manages handler lifetimes | [`m02-managing-http-clients-and-auth/c01-how-factory-manages-handler-lifetimes`](m02-managing-http-clients-and-auth/c01-how-factory-manages-handler-lifetimes) |
| 2. Managing HTTP Clients and Authentication | Intercepting requests with DelegatingHandlers | [`m02-managing-http-clients-and-auth/c02-intercepting-requests-delegatinghandlers`](m02-managing-http-clients-and-auth/c02-intercepting-requests-delegatinghandlers) |
| 2. Managing HTTP Clients and Authentication | Attaching auth tokens automatically | [`m02-managing-http-clients-and-auth/c03-attaching-auth-tokens-automatically`](m02-managing-http-clients-and-auth/c03-attaching-auth-tokens-automatically) |
| 2. Managing HTTP Clients and Authentication | Acquiring and caching bearer tokens | [`m02-managing-http-clients-and-auth/c04-acquiring-and-caching-bearer-tokens`](m02-managing-http-clients-and-auth/c04-acquiring-and-caching-bearer-tokens) |
| 2. Managing HTTP Clients and Authentication | Refreshing tokens on 401 responses | [`m02-managing-http-clients-and-auth/c05-refreshing-tokens-on-401`](m02-managing-http-clients-and-auth/c05-refreshing-tokens-on-401) |
| 2. Managing HTTP Clients and Authentication | Binding client config with the options pattern | [`m02-managing-http-clients-and-auth/c06-binding-client-config-options-pattern`](m02-managing-http-clients-and-auth/c06-binding-client-config-options-pattern) |
| 3. Building Resilience Pipelines with Polly | Your first retry with exponential backoff | [`m03-building-resilience-pipelines-polly/c02-your-first-retry-exponential-backoff`](m03-building-resilience-pipelines-polly/c02-your-first-retry-exponential-backoff) |
| 3. Building Resilience Pipelines with Polly | Adding jitter and honoring Retry-After | [`m03-building-resilience-pipelines-polly/c03-adding-jitter-honoring-retry-after`](m03-building-resilience-pipelines-polly/c03-adding-jitter-honoring-retry-after) |
| 3. Building Resilience Pipelines with Polly | Guarding calls with timeouts | [`m03-building-resilience-pipelines-polly/c04-guarding-calls-with-timeouts`](m03-building-resilience-pipelines-polly/c04-guarding-calls-with-timeouts) |
| 3. Building Resilience Pipelines with Polly | Stopping cascades with a circuit breaker | [`m03-building-resilience-pipelines-polly/c05-stopping-cascades-circuit-breaker`](m03-building-resilience-pipelines-polly/c05-stopping-cascades-circuit-breaker) |
| 3. Building Resilience Pipelines with Polly | Cutting tail latency with hedging | [`m03-building-resilience-pipelines-polly/c06-cutting-tail-latency-hedging`](m03-building-resilience-pipelines-polly/c06-cutting-tail-latency-hedging) |
| 3. Building Resilience Pipelines with Polly | Composing the standard resilience handler | [`m03-building-resilience-pipelines-polly/c07-composing-standard-resilience-handler`](m03-building-resilience-pipelines-polly/c07-composing-standard-resilience-handler) |
| 4. Consuming gRPC Services | Modeling contracts with Protobuf | [`m04-consuming-grpc-services/c02-modeling-contracts-with-protobuf`](m04-consuming-grpc-services/c02-modeling-contracts-with-protobuf) |
| 4. Consuming gRPC Services | Generating a strongly typed client | [`m04-consuming-grpc-services/c03-generating-a-strongly-typed-client`](m04-consuming-grpc-services/c03-generating-a-strongly-typed-client) |
| 4. Consuming gRPC Services | Making unary calls with the gRPC client | [`m04-consuming-grpc-services/c04-making-unary-calls-grpc-client`](m04-consuming-grpc-services/c04-making-unary-calls-grpc-client) |
| 4. Consuming gRPC Services | Adding Polly resilience to gRPC | [`m04-consuming-grpc-services/c05-adding-polly-resilience-to-grpc`](m04-consuming-grpc-services/c05-adding-polly-resilience-to-grpc) |
| 4. Consuming gRPC Services | Handling deadlines, cancellation, and status codes | [`m04-consuming-grpc-services/c06-handling-deadlines-cancellation-status-codes`](m04-consuming-grpc-services/c06-handling-deadlines-cancellation-status-codes) |
| 5. Streaming and Concurrent Communications | Consuming a gRPC server stream | [`m05-streaming-and-concurrent-comms/c02-consuming-a-grpc-server-stream`](m05-streaming-and-concurrent-comms/c02-consuming-a-grpc-server-stream) |
| 5. Streaming and Concurrent Communications | Keeping streams alive with keepalive and deadlines | [`m05-streaming-and-concurrent-comms/c03-keeping-streams-alive-keepalive-deadlines`](m05-streaming-and-concurrent-comms/c03-keeping-streams-alive-keepalive-deadlines) |
| 5. Streaming and Concurrent Communications | Recovering a broken stream | [`m05-streaming-and-concurrent-comms/c04-recovering-a-broken-stream`](m05-streaming-and-concurrent-comms/c04-recovering-a-broken-stream) |
| 5. Streaming and Concurrent Communications | Fanning out concurrent HTTP requests | [`m05-streaming-and-concurrent-comms/c05-fanning-out-concurrent-http-requests`](m05-streaming-and-concurrent-comms/c05-fanning-out-concurrent-http-requests) |
| 5. Streaming and Concurrent Communications | Handling partial failures across a fan-out | [`m05-streaming-and-concurrent-comms/c06-handling-partial-failures-across-fan-out`](m05-streaming-and-concurrent-comms/c06-handling-partial-failures-across-fan-out) |
| 6. Controlling and Correlating Traffic | Applying client-side rate limiting | [`m06-controlling-correlating-traffic/c02-applying-client-side-rate-limiting`](m06-controlling-correlating-traffic/c02-applying-client-side-rate-limiting) |
| 6. Controlling and Correlating Traffic | Choosing a rate-limiting algorithm | [`m06-controlling-correlating-traffic/c03-choosing-a-rate-limiting-algorithm`](m06-controlling-correlating-traffic/c03-choosing-a-rate-limiting-algorithm) |
| 6. Controlling and Correlating Traffic | Combining rate limits with resilience pipelines | [`m06-controlling-correlating-traffic/c04-combining-rate-limits-with-resilience`](m06-controlling-correlating-traffic/c04-combining-rate-limits-with-resilience) |
| 6. Controlling and Correlating Traffic | Correlating requests with TraceContext | [`m06-controlling-correlating-traffic/c05-correlating-requests-with-tracecontext`](m06-controlling-correlating-traffic/c05-correlating-requests-with-tracecontext) |
| 6. Controlling and Correlating Traffic | Propagating W3C trace headers end to end | [`m06-controlling-correlating-traffic/c06-propagating-w3c-trace-headers-end-to-end`](m06-controlling-correlating-traffic/c06-propagating-w3c-trace-headers-end-to-end) |

Clips with no demo code (slides only) have no folder: 1.1, 1.2, 3.1, 4.1, 5.1, 6.1 and 6.7.
