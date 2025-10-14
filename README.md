# Arch — Library-based API Gateway for .NET

<div align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-blue.svg" alt=".NET 9">
  <img src="https://img.shields.io/badge/Docker-Ready-blue.svg" alt="Docker Ready">
  <img src="https://img.shields.io/badge/License-MIT-green.svg" alt="License">
</div>

Arch is a modular, high‑performance API gateway for .NET. It assembles capabilities as independent libraries (authorization, encryption, endpoint graph, load balancing, rate limiting, logging, event bus) so you can include only what you need and extend what you use.

### Features
- Modular, library‑based architecture with clean abstractions
- High‑performance ASP.NET Core (.NET 9)
- Event‑first design via CAP (RabbitMQ + PostgreSQL support)
- Authorization (JWT/OAuth2), encryption (AES‑256‑GCM)
- Rate limiting, load balancing, service discovery
- Structured logging and APM integration

## Prerequisites
- .NET 9 SDK — see the official download page: [Get .NET 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Optional: Docker and Docker Compose for local infrastructure

## Quick start
```bash
# Start local infrastructure (optional)
docker compose up -d

# Run the gateway (development)
cd Application
dotnet run

# Health check
curl http://localhost:5229/health
```
Note: If your Docker version uses the legacy syntax, use `docker-compose` instead of `docker compose`.

## Minimal usage example
Add minimal Arch configuration to `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddArch(options =>
{
    options.EnableHealthCheck();

    options.ConfigureEndpointGraph(graph => graph.UseInMemory());
    options.ConfigureLoadBalancer(lb => lb.UseBasic());

    options.ConfigureData(data =>
    {
        data.UseEntityFramework(db =>
            db.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        data.AddCaching(cache => cache.UseInMemory());
    });

    options.AddAuthorization(auth => auth.UseKundera(builder.Configuration));
    options.UseRateLimit(rl => rl.AddCage());
    options.AddEncryption(enc => enc.UseTesSecurityEncryption(builder.Configuration));
    options.AddLogging(log => log.UseLogstash());
});

var app = builder.Build();

app.UseArch(pipeline =>
{
    pipeline.BeforeDispatching(d =>
    {
        d.UseAuthorization(e => e.UseKundera(builder.Configuration));
        d.UseRateLimit(e => e.UseCage(builder.Configuration));
    });

    pipeline.AfterDispatching(d =>
    {
        d.UseLogging();
    });
});

app.MapHealthChecks("/health");
await app.RunAsync();
```

## Configuration
Minimal `appsettings.json` example:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=arch;Username=arch;Password=arch"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  },
  "RateLimitDefault": {
    "MaxAllowedRequestInWindow": "1000",
    "WindowsSize": "00:01:00"
  }
}
```

For advanced configuration (environment variables, encryption and authorization options, CAP, logging, and production guidance), see `docs/SETUP.md`.

## Libraries ecosystem

| Library | Purpose | Key features |
|---------|---------|--------------|
| [Authorization](docs/authorization.md) | Authentication & access control | JWT, OAuth2, RBAC |
| [Encryption](docs/encryption.md) | Data security & privacy | AES‑256‑GCM, key rotation |
| [Endpoint Graph](docs/endpoint-graph.md) | URL routing | Trie‑based routing, O(1) lookup |
| [Event Bus](docs/event-bus.md) | Distributed messaging | CAP integration |
| [Load Balancer](docs/load-balancer.md) | Traffic distribution | Round‑robin, weighted, health checks |
| [Logging](docs/logging.md) | Structured observability | Console, Logstash, JSON |
| [Rate Limiting](docs/rate-limiting.md) | Request throttling | Sliding windows, distributed counters |

## Comparison (summary)
- Library‑based (not plugin‑based): include only what you need with low coupling
- Built for performance on .NET 9 with low memory overhead
- Event‑first design with CAP integration

More details and comparisons are available in `docs/TECHNICAL.md`.

## Performance (summary)
- High routing throughput and low lookup latency with in‑memory endpoint graph
- Efficient resource usage with a small memory baseline
- See benchmark details in `docs/TECHNICAL.md`

## Deployment
- Container‑friendly (Docker) and Kubernetes‑ready
- See production notes and examples in `docs/SETUP.md`

## Contributing
Contributions are welcome.
- Fork the repository and create a feature branch
- Add tests for changes and run `dotnet test`
- Open a pull request with a clear description

## License
MIT — see `LICENSE`.
