# 🛠️ Arch Library Development Guide

This guide explains how to develop custom libraries for the Arch ecosystem, following best practices and architectural patterns.

## 📚 Understanding Arch's Library Architecture

Arch follows a **modular, plugin-based architecture** where each library is a self-contained module that can be mixed and matched. Libraries are organized in layers:

```
┌─────────────────────────────────────────────────┐
│                    Libraries                    │
├─────────────────┬─────────────────┬─────────────┤
│ Abstractions    │ Implementations │ Extensions  │
│ (Contracts)     │ (Concrete)      │ (Options)   │
└─────────────────┴─────────────────┴─────────────┘
```

### Key Principles

- **Zero Coupling**: Libraries don't depend on each other
- **Clean Abstractions**: Clear separation between contracts and implementations
- **Extensibility**: Easy to extend and customize
- **Testability**: High test coverage and mock-friendly designs

## 🚀 Creating a New Library

### Step 1: Define the Problem Space

Choose a specific concern:
- **Data Layer**: Database providers, caching, migrations
- **Communication**: Message brokers, event buses, API clients
- **Security**: Authentication, authorization, encryption
- **Infrastructure**: Logging, monitoring, configuration
- **Business Logic**: Domain-specific functionality

### Step 2: Create Library Structure

```
Libraries/
└── YourLibrary/
    ├── YourLibrary.Abstractions/     # Contracts and interfaces
    ├── YourLibrary.Implementation/   # Concrete implementations
    └── YourLibrary.csproj           # Main package (optional)
```

### Step 3: Implement Abstractions

#### Define Core Interfaces

```csharp
// YourLibrary.Abstractions/IYourService.cs
namespace Arch.YourLibrary.Abstractions;

public interface IYourService
{
    ValueTask<YourResult> ProcessAsync(YourRequest request, CancellationToken cancellationToken = default);
    ValueTask<bool> ValidateAsync(YourRequest request, CancellationToken cancellationToken = default);
}

// YourLibrary.Abstractions/YourOptions.cs
namespace Arch.YourLibrary.Abstractions;

public class YourOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
}
```

#### Create Options Pattern

```csharp
// YourLibrary.Abstractions/YourOptions.cs
using Microsoft.Extensions.Options;

namespace Arch.YourLibrary.Abstractions;

public class YourOptions : IOptions<YourOptions>
{
    public string ConnectionString { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;

    YourOptions IOptions<YourOptions>.Value => this;
}
```

### Step 4: Implement Concrete Services

#### Create Implementation Class

```csharp
// YourLibrary.Implementation/YourService.cs
using Arch.YourLibrary.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arch.YourLibrary.Implementation;

public class YourService : IYourService
{
    private readonly YourOptions _options;
    private readonly ILogger<YourService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public YourService(
        IOptions<YourOptions> options,
        ILogger<YourService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async ValueTask<YourResult> ProcessAsync(YourRequest request, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = _options.Timeout;

        var retryCount = 0;
        while (retryCount < _options.MaxRetries)
        {
            try
            {
                _logger.LogInformation("Processing request {RequestId}", request.Id);

                var response = await httpClient.PostAsJsonAsync(_options.ConnectionString, request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<YourResult>(cancellationToken: cancellationToken);
                return result!;
            }
            catch (Exception ex) when (retryCount < _options.MaxRetries - 1)
            {
                _logger.LogWarning(ex, "Request failed, retrying ({RetryCount}/{MaxRetries})", ++retryCount, _options.MaxRetries);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), cancellationToken);
            }
        }

        throw new YourServiceException("Failed to process request after all retries");
    }

    public ValueTask<bool> ValidateAsync(YourRequest request, CancellationToken cancellationToken = default)
    {
        // Validation logic
        var isValid = !string.IsNullOrEmpty(request.Data) && request.Id != Guid.Empty;
        return ValueTask.FromResult(isValid);
    }
}
```

### Step 5: Create Extension Methods

#### Service Collection Extensions

```csharp
// YourLibrary.Implementation/ServiceCollectionExtensions.cs
using Arch.YourLibrary.Abstractions;
using Arch.YourLibrary.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.YourLibrary.Implementation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYourLibrary(
        this IServiceCollection services,
        Action<YourOptions>? configureOptions = null)
    {
        // Register options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Register services
        services.AddHttpClient();
        services.AddSingleton<IYourService, YourService>();

        // Register health checks if applicable
        services.AddHealthChecks()
            .AddCheck<YourHealthCheck>("your-service");

        return services;
    }
}
```

### Step 6: Integrate with Arch Core

#### Create Arch Integration

```csharp
// YourLibrary.Implementation/ArchOptionsExtensions.cs
using Arch.Configurations;

namespace Arch.YourLibrary.Implementation;

public static class ArchOptionsExtensions
{
    public static ArchOptions AddYourLibrary(
        this ArchOptions options,
        Action<YourOptions> configureOptions)
    {
        options.Services.AddYourLibrary(configureOptions);
        return options;
    }
}
```

### Step 7: Create Project Files

#### Abstractions Project File

```xml
<!-- YourLibrary.Abstractions/YourLibrary.Abstractions.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>Arch.YourLibrary.Abstractions</RootNamespace>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>Arch.YourLibrary.Abstractions</PackageId>
    <Version>1.0.0</Version>
    <Title>Arch.YourLibrary.Abstractions</Title>
    <Authors>Your Name</Authors>
    <Description>Abstractions for YourLibrary in Arch ecosystem</Description>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Options" Version="9.0.9" />
  </ItemGroup>
</Project>
```

#### Implementation Project File

```xml
<!-- YourLibrary.Implementation/YourLibrary.Implementation.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>Arch.YourLibrary.Implementation</RootNamespace>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>Arch.YourLibrary.Implementation</PackageId>
    <Version>1.0.0</Version>
    <Title>Arch.YourLibrary.Implementation</Title>
    <Authors>Your Name</Authors>
    <Description>Implementation for YourLibrary in Arch ecosystem</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\YourLibrary.Abstractions\YourLibrary.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.9" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.9" />
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="9.0.9" />
  </ItemGroup>
</Project>
```

## 🧪 Testing Your Library

### Unit Tests

```csharp
// YourLibrary.Implementation.Tests/YourServiceTests.cs
using Arch.YourLibrary.Abstractions;
using Arch.YourLibrary.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Arch.YourLibrary.Implementation.Tests;

public class YourServiceTests
{
    [Fact]
    public async Task ProcessAsync_ValidRequest_ReturnsResult()
    {
        // Arrange
        var options = Options.Create(new YourOptions { ConnectionString = "http://api.example.com" });
        var logger = new Mock<ILogger<YourService>>();
        var httpClientFactory = new Mock<IHttpClientFactory>();

        var service = new YourService(options, logger.Object, httpClientFactory.Object);
        var request = new YourRequest { Id = Guid.NewGuid(), Data = "test" };

        // Act
        var result = await service.ProcessAsync(request);

        // Assert
        Assert.NotNull(result);
    }
}
```

### Integration Tests

```csharp
// YourLibrary.Implementation.Tests.Integration/YourServiceIntegrationTests.cs
using Arch.YourLibrary.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arch.YourLibrary.Implementation.Tests.Integration;

public class YourServiceIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public YourServiceIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EndToEndTest_ProcessesRequestSuccessfully()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/your-endpoint", new { data = "test" });

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<YourResult>();
        Assert.NotNull(result);
    }
}
```

## 📦 Packaging and Distribution

### NuGet Package Configuration

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <Authors>Your Organization</Authors>
    <Company>Your Company</Company>
    <Product>Arch</Product>
    <Copyright>Copyright © $([System.DateTime]::Now.Year) Your Company</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/your-org/arch</PackageProjectUrl>
    <RepositoryUrl>https://github.com/your-org/arch</RepositoryUrl>
    <PackageTags>arch api-gateway microservices dotnet</PackageTags>
    <PackageRequireLicenseAcceptance>false</PackageRequireLicenseAcceptance>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>
</Project>
```

### Publishing to NuGet

```bash
# Build packages
dotnet build --configuration Release

# Pack packages
dotnet pack --configuration Release --output ./packages

# Publish to NuGet (replace with your API key)
dotnet nuget push "./packages/*.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## 🎯 Best Practices

### Design Principles

1. **Single Responsibility**: Each class has one reason to change
2. **Dependency Inversion**: Depend on abstractions, not concretions
3. **Interface Segregation**: Keep interfaces small and focused
4. **Open/Closed**: Open for extension, closed for modification

### Performance Considerations

- Use `ValueTask` for async operations where possible
- Implement proper cancellation token support
- Use object pooling for expensive resources
- Implement health checks for external dependencies

### Error Handling

```csharp
public class YourServiceException : Exception
{
    public YourServiceException(string message) : base(message) { }
    public YourServiceException(string message, Exception innerException) : base(message, innerException) { }
}
```

### Logging Best Practices

```csharp
_logger.LogInformation("Processing request {RequestId} for user {UserId}", request.Id, userId);
_logger.LogWarning("Service {ServiceName} is degraded: {Reason}", serviceName, reason);
_logger.LogError(ex, "Failed to process request {RequestId} after {RetryCount} retries", requestId, retryCount);
```

## 🔗 Integration Examples

### Using Your Library in Arch

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddArch(options =>
{
    // Add your custom library
    options.AddYourLibrary(config =>
    {
        config.ConnectionString = builder.Configuration.GetConnectionString("YourService");
        config.Timeout = TimeSpan.FromSeconds(10);
        config.MaxRetries = 5;
    });
});

var app = builder.Build();

app.UseArch(options =>
{
    options.BeforeDispatching(dispatchingOptions =>
    {
        dispatchingOptions.UseYourLibrary();
    });
});
```

### Extending Existing Libraries

```csharp
// Custom implementation of an existing abstraction
public class CustomYourService : IYourService
{
    public async ValueTask<YourResult> ProcessAsync(YourRequest request, CancellationToken cancellationToken)
    {
        // Custom implementation
        return await ProcessWithCustomLogicAsync(request, cancellationToken);
    }

    public ValueTask<bool> ValidateAsync(YourRequest request, CancellationToken cancellationToken)
    {
        // Custom validation logic
        return ValueTask.FromResult(IsValidRequest(request));
    }
}
```

## 📚 Documentation

### README for Your Library

```markdown
# Arch.YourLibrary

[![NuGet](https://img.shields.io/nuget/v/Arch.YourLibrary.Abstractions.svg)](https://www.nuget.org/packages/Arch.YourLibrary.Abstractions/)
[![NuGet](https://img.shields.io/nuget/v/Arch.YourLibrary.Implementation.svg)](https://www.nuget.org/packages/Arch.YourLibrary.Implementation/)

A custom library for the Arch ecosystem that provides [describe functionality].

## Installation

```bash
dotnet add package Arch.YourLibrary.Abstractions
dotnet add package Arch.YourLibrary.Implementation
```

## Usage

```csharp
builder.Services.AddArch(options =>
{
    options.AddYourLibrary(config =>
    {
        config.ConnectionString = "your-connection-string";
    });
});
```

## Contributing

See [Arch Library Development Guide](../docs/library-development.md) for details on developing libraries for the Arch ecosystem.
```

## 🏆 Advanced Patterns

### Strategy Pattern for Multiple Implementations

```csharp
public interface IYourServiceStrategy
{
    string StrategyName { get; }
    bool CanHandle(YourRequest request);
    ValueTask<YourResult> ProcessAsync(YourRequest request, CancellationToken cancellationToken);
}

public class CompositeYourService : IYourService
{
    private readonly IEnumerable<IYourServiceStrategy> _strategies;

    public CompositeYourService(IEnumerable<IYourServiceStrategy> strategies)
    {
        _strategies = strategies;
    }

    public async ValueTask<YourResult> ProcessAsync(YourRequest request, CancellationToken cancellationToken)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(request))
            ?? throw new InvalidOperationException("No strategy found for request");

        return await strategy.ProcessAsync(request, cancellationToken);
    }
}
```

### Configuration Validation

```csharp
public class YourOptionsValidator : IValidateOptions<YourOptions>
{
    public ValidateOptionsResult Validate(string name, YourOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrEmpty(options.ConnectionString))
            failures.Add("ConnectionString is required");

        if (options.Timeout <= TimeSpan.Zero)
            failures.Add("Timeout must be positive");

        if (options.MaxRetries < 0)
            failures.Add("MaxRetries cannot be negative");

        return failures.Any()
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
```

## 🎯 Getting Help

- **Arch Documentation**: [https://arch.dev/docs](https://arch.dev/docs)
- **GitHub Issues**: [Report bugs](https://github.com/your-org/arch/issues)
- **Discussions**: [Ask questions](https://github.com/your-org/arch/discussions)
- **Slack**: [Join community](https://arch.slack.com)

## 📄 License

Licensed under the MIT License. See [LICENSE](../../LICENSE) for details.

---

<div align="center">
  <p>Happy coding! 🎉</p>
  <p>
    <a href="setup.md">← Setup Guide</a> |
    <a href="../README.md">README</a> |
    <a href="../docs/endpoint-graph.md">Endpoint Graph →</a>
  </p>
</div>
