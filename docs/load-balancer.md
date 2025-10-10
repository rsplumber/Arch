# ⚖️ Load Balancer - Intelligent Traffic Distribution

Arch's Load Balancer provides **high-performance traffic distribution** with multiple algorithms, health monitoring, and automatic failover capabilities for optimal service reliability.

## 🏗️ Architecture

### Core Components

```
Load Balancer Layer
├── Algorithm Engine                 # Distribution algorithms
├── Health Monitor                   # Service health checking
├── Endpoint Registry               # Service endpoint management
├── Circuit Breaker                 # Failure handling
└── Metrics Collector               # Performance monitoring
```

### Load Balancing Algorithms

#### Round Robin
```csharp
public class RoundRobinBalancer : ILoadBalancer
{
    private int _currentIndex = 0;

    public EndpointInfo SelectEndpoint(IReadOnlyList<EndpointInfo> endpoints)
    {
        if (endpoints.Count == 0) throw new NoAvailableEndpointsException();

        var endpoint = endpoints[_currentIndex];
        _currentIndex = (_currentIndex + 1) % endpoints.Count;

        return endpoint;
    }
}
```

#### Weighted Round Robin
```csharp
public class WeightedRoundRobinBalancer : ILoadBalancer
{
    private readonly Dictionary<string, int> _currentWeights = new();

    public EndpointInfo SelectEndpoint(IReadOnlyList<EndpointInfo> endpoints)
    {
        var maxWeight = endpoints.Max(e => e.Weight);
        var selectedEndpoint = endpoints.First();

        foreach (var endpoint in endpoints)
        {
            var currentWeight = _currentWeights.GetValueOrDefault(endpoint.Url, 0) + endpoint.Weight;

            if (currentWeight >= maxWeight)
            {
                selectedEndpoint = endpoint;
                _currentWeights[endpoint.Url] = currentWeight - maxWeight;
            }
            else
            {
                _currentWeights[endpoint.Url] = currentWeight;
            }
        }

        return selectedEndpoint;
    }
}
```

#### Least Connections
```csharp
public class LeastConnectionsBalancer : ILoadBalancer
{
    public EndpointInfo SelectEndpoint(IReadOnlyList<EndpointInfo> endpoints)
    {
        return endpoints
            .Where(e => e.IsHealthy)
            .OrderBy(e => e.ActiveConnections)
            .FirstOrDefault()
            ?? throw new NoAvailableEndpointsException();
    }
}
```

## ⚙️ Configuration Options

### Basic Setup

```csharp
builder.Services.AddArch(options =>
{
    options.ConfigureLoadBalancer(balancerOptions =>
        balancerOptions.UseBasic(basicOptions => {
            basicOptions.Algorithm = LoadBalancingAlgorithm.WeightedRoundRobin;
            basicOptions.EnableHealthMonitoring = true;
        }));
});
```

### Advanced Configuration

```csharp
builder.Services.AddArch(options =>
{
    options.ConfigureLoadBalancer(balancerOptions =>
        balancerOptions.UseBasic(basicOptions => {
            // Algorithm selection
            basicOptions.Algorithm = LoadBalancingAlgorithm.LeastConnections;

            // Health monitoring
            basicOptions.EnableHealthMonitoring = true;
            basicOptions.HealthCheckEndpoint = "/health";
            basicOptions.HealthCheckTimeout = TimeSpan.FromSeconds(5);
            basicOptions.HealthCheckInterval = TimeSpan.FromSeconds(30);
            basicOptions.UnhealthyThreshold = 3;
            basicOptions.HealthyThreshold = 2;

            // Connection management
            basicOptions.MaxConcurrentConnections = 1000;
            basicOptions.ConnectionTimeout = TimeSpan.FromSeconds(30);
            basicOptions.RetryPolicy = new RetryPolicy {
                MaxRetries = 3,
                InitialDelay = TimeSpan.FromMilliseconds(100),
                BackoffMultiplier = 2.0
            };

            // Circuit breaker
            basicOptions.EnableCircuitBreaker = true;
            basicOptions.CircuitBreakerThreshold = 50; // 50% failure rate
            basicOptions.CircuitBreakerTimeout = TimeSpan.FromMinutes(1);
        }));
});
```

## 🏥 Health Monitoring

### Health Check Implementation

```csharp
public class EndpointHealthChecker
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EndpointHealthChecker> _logger;

    public async Task<HealthStatus> CheckHealthAsync(EndpointInfo endpoint)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint.HealthUrl);
            request.Headers.Add("User-Agent", "Arch-LoadBalancer/1.0");

            using var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                endpoint.HealthTimeout);

            return response.IsSuccessStatusCode
                ? HealthStatus.Healthy
                : HealthStatus.Unhealthy;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for endpoint {Endpoint}", endpoint.Url);
            return HealthStatus.Unhealthy;
        }
    }
}
```

### Health Status Management

```csharp
public class EndpointHealthManager
{
    private readonly ConcurrentDictionary<string, EndpointHealth> _endpointHealth = new();

    public async Task UpdateHealthAsync(string endpointUrl, HealthStatus status)
    {
        var health = _endpointHealth.GetOrAdd(endpointUrl, _ => new EndpointHealth());

        health.LastCheck = DateTime.UtcNow;
        health.Status = status;

        // Update consecutive failure/success counts
        if (status == HealthStatus.Healthy)
        {
            health.ConsecutiveFailures = 0;
            health.ConsecutiveSuccesses++;
        }
        else
        {
            health.ConsecutiveFailures++;
            health.ConsecutiveSuccesses = 0;
        }

        // Determine overall health status
        health.IsHealthy = DetermineOverallHealth(health);
    }

    private bool DetermineOverallHealth(EndpointHealth health)
    {
        if (health.ConsecutiveFailures >= 3) return false;
        if (health.ConsecutiveSuccesses >= 2) return true;
        return health.Status == HealthStatus.Healthy;
    }
}
```

## 🔄 Circuit Breaker Pattern

### Circuit Breaker Implementation

```csharp
public class CircuitBreaker
{
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private DateTime _lastFailureTime;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                _state = CircuitState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }

        try
        {
            var result = await action();
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure();
            throw;
        }
    }

    private void OnSuccess()
    {
        _failureCount = 0;
        _state = CircuitState.Closed;
    }

    private void OnFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;

        if (_failureCount >= _failureThreshold)
        {
            _state = CircuitState.Open;
        }
    }
}
```

## 📊 Load Balancing Strategies

### Service-Aware Load Balancing

```csharp
public class ServiceAwareLoadBalancer : ILoadBalancer
{
    private readonly IServiceDiscovery _serviceDiscovery;
    private readonly ILoadBalancer _innerBalancer;

    public async ValueTask<EndpointInfo> SelectEndpointAsync(string serviceName)
    {
        var endpoints = await _serviceDiscovery.GetHealthyEndpointsAsync(serviceName);

        if (!endpoints.Any())
            throw new NoHealthyEndpointsException(serviceName);

        return _innerBalancer.SelectEndpoint(endpoints);
    }
}
```

### Geographic Load Balancing

```csharp
public class GeographicLoadBalancer : ILoadBalancer
{
    private readonly IGeoLocationService _geoService;

    public EndpointInfo SelectEndpoint(IReadOnlyList<EndpointInfo> endpoints, string clientIp)
    {
        var clientLocation = _geoService.GetLocation(clientIp);

        return endpoints
            .Where(e => e.IsHealthy)
            .OrderBy(e => CalculateDistance(clientLocation, e.Location))
            .First();
    }

    private double CalculateDistance(GeoLocation client, GeoLocation endpoint)
    {
        // Haversine distance calculation
        const double R = 6371; // Earth's radius in km
        var dLat = ToRadians(endpoint.Latitude - client.Latitude);
        var dLon = ToRadians(endpoint.Longitude - client.Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(client.Latitude)) * Math.Cos(ToRadians(endpoint.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}
```

## 📈 Performance Monitoring

### Metrics Collection

```csharp
// Prometheus metrics
private readonly Counter _requestsTotal = Metrics.CreateCounter(
    "arch_loadbalancer_requests_total",
    "Total requests processed",
    new[] { "endpoint", "status" });

private readonly Gauge _activeConnections = Metrics.CreateGauge(
    "arch_loadbalancer_active_connections",
    "Active connections per endpoint",
    new[] { "endpoint" });

private readonly Histogram _requestDuration = Metrics.CreateHistogram(
    "arch_loadbalancer_request_duration_seconds",
    "Request duration by endpoint",
    new[] { "endpoint" });

private readonly Gauge _endpointHealth = Metrics.CreateGauge(
    "arch_loadbalancer_endpoint_health",
    "Endpoint health status (1=healthy, 0=unhealthy)",
    new[] { "endpoint" });
```

### Performance Dashboards

```csharp
public class LoadBalancerMetricsCollector
{
    public void RecordRequest(string endpoint, TimeSpan duration, bool success)
    {
        _requestsTotal.WithLabels(endpoint, success ? "success" : "failure").Inc();
        _requestDuration.WithLabels(endpoint).Observe(duration.TotalSeconds);
    }

    public void UpdateConnectionCount(string endpoint, int count)
    {
        _activeConnections.WithLabels(endpoint).Set(count);
    }

    public void UpdateHealthStatus(string endpoint, bool isHealthy)
    {
        _endpointHealth.WithLabels(endpoint).Set(isHealthy ? 1 : 0);
    }
}
```

## 🧪 Testing

### Unit Tests

```csharp
public class RoundRobinBalancerTests
{
    [Fact]
    public void SelectEndpoint_RoundRobinDistribution()
    {
        // Arrange
        var balancer = new RoundRobinBalancer();
        var endpoints = new[] {
            new EndpointInfo { Url = "http://service1" },
            new EndpointInfo { Url = "http://service2" },
            new EndpointInfo { Url = "http://service3" }
        };

        // Act
        var selections = new List<string>();
        for (int i = 0; i < 6; i++)
        {
            selections.Add(balancer.SelectEndpoint(endpoints).Url);
        }

        // Assert
        Assert.Equal(new[] {
            "http://service1", "http://service2", "http://service3",
            "http://service1", "http://service2", "http://service3"
        }, selections);
    }
}
```

### Integration Tests

```csharp
public class LoadBalancerIntegrationTests
{
    [Fact]
    public async Task LoadBalancer_DistributesTrafficEvenly()
    {
        // Arrange
        var endpoints = new[] {
            new EndpointInfo { Url = "http://service1", IsHealthy = true },
            new EndpointInfo { Url = "http://service2", IsHealthy = true },
            new EndpointInfo { Url = "http://service3", IsHealthy = true }
        };

        var balancer = new RoundRobinBalancer();
        var distribution = new Dictionary<string, int>();

        // Act - Simulate 300 requests
        for (int i = 0; i < 300; i++)
        {
            var endpoint = balancer.SelectEndpoint(endpoints);
            distribution[endpoint.Url] = distribution.GetValueOrDefault(endpoint.Url, 0) + 1;
        }

        // Assert - Each endpoint should get ~100 requests
        foreach (var count in distribution.Values)
        {
            Assert.InRange(count, 95, 105); // Allow 5% variance
        }
    }
}
```

## 🚀 Advanced Features

### Session Affinity

```csharp
public class StickySessionBalancer : ILoadBalancer
{
    private readonly ConcurrentDictionary<string, string> _sessionEndpoints = new();
    private readonly ILoadBalancer _fallbackBalancer;

    public EndpointInfo SelectEndpoint(IReadOnlyList<EndpointInfo> endpoints, HttpContext context)
    {
        var sessionId = context.Request.Cookies["session-id"];

        if (!string.IsNullOrEmpty(sessionId) &&
            _sessionEndpoints.TryGetValue(sessionId, out var endpointUrl))
        {
            var endpoint = endpoints.FirstOrDefault(e => e.Url == endpointUrl && e.IsHealthy);
            if (endpoint != null) return endpoint;
        }

        // Fallback to normal load balancing
        var selectedEndpoint = _fallbackBalancer.SelectEndpoint(endpoints);

        if (!string.IsNullOrEmpty(sessionId))
        {
            _sessionEndpoints[sessionId] = selectedEndpoint.Url;
        }

        return selectedEndpoint;
    }
}
```

### Dynamic Weight Adjustment

```csharp
public class AdaptiveLoadBalancer : ILoadBalancer
{
    private readonly IPerformanceMonitor _performanceMonitor;

    public EndpointInfo SelectEndpoint(IReadOnlyList<EndpointInfo> endpoints)
    {
        // Adjust weights based on performance metrics
        foreach (var endpoint in endpoints)
        {
            var metrics = _performanceMonitor.GetMetrics(endpoint.Url);
            endpoint.Weight = CalculateWeight(metrics);
        }

        return new WeightedRoundRobinBalancer().SelectEndpoint(endpoints);
    }

    private int CalculateWeight(EndpointMetrics metrics)
    {
        var baseWeight = 100;

        // Reduce weight for slow endpoints
        if (metrics.AverageResponseTime > TimeSpan.FromSeconds(2))
            baseWeight /= 2;

        // Reduce weight for high error rates
        if (metrics.ErrorRate > 0.05) // 5%
            baseWeight /= 2;

        // Increase weight for high throughput endpoints
        if (metrics.RequestsPerSecond > 1000)
            baseWeight *= 2;

        return Math.Max(baseWeight, 1);
    }
}
```

## 🔧 Operations

### Endpoint Management

```csharp
public class EndpointManager
{
    private readonly ConcurrentDictionary<string, List<EndpointInfo>> _serviceEndpoints = new();

    public void RegisterEndpoint(string serviceName, EndpointInfo endpoint)
    {
        var endpoints = _serviceEndpoints.GetOrAdd(serviceName, _ => new List<EndpointInfo>());
        endpoints.Add(endpoint);

        _logger.LogInformation("Registered endpoint {Endpoint} for service {Service}",
            endpoint.Url, serviceName);
    }

    public void UnregisterEndpoint(string serviceName, string endpointUrl)
    {
        if (_serviceEndpoints.TryGetValue(serviceName, out var endpoints))
        {
            endpoints.RemoveAll(e => e.Url == endpointUrl);
        }
    }

    public IReadOnlyList<EndpointInfo> GetHealthyEndpoints(string serviceName)
    {
        return _serviceEndpoints.GetValueOrDefault(serviceName, new List<EndpointInfo>())
            .Where(e => e.IsHealthy)
            .ToList();
    }
}
```

### Configuration Hot Reload

```csharp
public class LoadBalancerConfigurationReloader
{
    private readonly IConfiguration _configuration;
    private readonly ILoadBalancer _loadBalancer;

    public LoadBalancerConfigurationReloader(
        IConfiguration configuration,
        ILoadBalancer loadBalancer)
    {
        _configuration = configuration;
        _loadBalancer = loadBalancer;

        ChangeToken.OnChange(
            () => _configuration.GetReloadToken(),
            () => ReloadConfiguration());
    }

    private void ReloadConfiguration()
    {
        var newConfig = _configuration.GetSection("LoadBalancer").Get<LoadBalancerConfig>();
        _loadBalancer.UpdateConfiguration(newConfig);

        _logger.LogInformation("Load balancer configuration reloaded");
    }
}
```

## 🎯 Use Cases

### E-commerce Platform

```csharp
// Product catalog - read-heavy, use round robin
options.ConfigureLoadBalancer(balancerOptions =>
    balancerOptions.UseBasic(basicOptions => {
        basicOptions.Algorithm = LoadBalancingAlgorithm.RoundRobin;
        basicOptions.EnableHealthMonitoring = true;
    }));

// Checkout service - session affinity required
options.AddLoadBalancer("checkout", new StickySessionBalancer());
```

### API Gateway

```csharp
// Microservices behind gateway
options.ConfigureLoadBalancer(balancerOptions => {
    balancerOptions.UseBasic(basicOptions => {
        basicOptions.Algorithm = LoadBalancingAlgorithm.LeastConnections;

        // Different strategies for different services
        balancerOptions.ServiceBalancers.Add("auth-service",
            new WeightedRoundRobinBalancer());

        balancerOptions.ServiceBalancers.Add("search-service",
            new LeastConnectionsBalancer());
    });
});
```

### Global Deployment

```csharp
// Geographic load balancing for global users
options.ConfigureLoadBalancer(balancerOptions =>
    balancerOptions.UseGeographic(geoOptions => {
        geoOptions.Regions.Add("us-east", new[] { "us-east-1", "us-east-2" });
        geoOptions.Regions.Add("eu-west", new[] { "eu-west-1", "eu-west-2" });
        geoOptions.FallbackRegion = "us-east";
    }));
```

## 📊 Monitoring & Troubleshooting

### Common Issues

#### Uneven Load Distribution
```csharp
// Check endpoint weights and health status
var endpoints = _endpointManager.GetAllEndpoints();
foreach (var endpoint in endpoints)
{
    _logger.LogInformation("Endpoint {Url}: Weight={Weight}, Healthy={IsHealthy}, Connections={ActiveConnections}",
        endpoint.Url, endpoint.Weight, endpoint.IsHealthy, endpoint.ActiveConnections);
}
```

#### Health Check Failures
```csharp
// Debug health check issues
try
{
    var response = await _httpClient.GetAsync(endpoint.HealthUrl);
    _logger.LogInformation("Health check response: {StatusCode}", response.StatusCode);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Health check failed for {Endpoint}", endpoint.Url);
}
```

### Performance Tuning

```csharp
// Optimize for high throughput
options.ConfigureLoadBalancer(balancerOptions =>
    balancerOptions.UseBasic(basicOptions => {
        basicOptions.HealthCheckInterval = TimeSpan.FromMinutes(1); // Reduce health check frequency
        basicOptions.ConnectionPooling = true;
        basicOptions.MaxConnectionsPerEndpoint = 100;
        basicOptions.EnableMetrics = false; // Disable in production for better performance
    }));
```

## 📚 Related Documentation

- [Setup Guide](setup.md) - Load balancer configuration
- [Performance Tuning](performance.md) - Load balancing optimization
- [Monitoring](monitoring.md) - Load balancer metrics
- [Troubleshooting](troubleshooting.md) - Common load balancer issues

## 🤝 Contributing

See [Contributing Guide](../../CONTRIBUTING.md) for load balancer development.

---

<div align="center">
  <p>⚖️ Intelligent traffic distribution for optimal performance</p>
  <p>
    <a href="event-bus.md">← Event Bus</a> |
    <a href="../README.md">README</a> |
    <a href="logging.md">Logging →</a>
  </p>
</div>
