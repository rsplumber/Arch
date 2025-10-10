# 🚦 Rate Limiting - Request Throttling & Protection

Arch's Rate Limiting framework provides **enterprise-grade request throttling** with distributed counters, sliding windows, and customizable rules to protect your APIs from abuse and ensure fair resource allocation.

## 🏗️ Architecture

### Core Components

```
Rate Limiting Layer
├── Rate Limit Engine              # Core throttling logic
├── Counter Storage                # Distributed counters
├── Rule Engine                   # Rate limit policies
├── Request Classification        # API endpoint categorization
├── Metrics & Monitoring         # Usage analytics
└── Circuit Breaker Integration  # Failure handling
```

### Rate Limiting Algorithms

#### Sliding Window

```csharp
public class SlidingWindowRateLimiter : IRateLimiter
{
    private readonly TimeSpan _windowSize;
    private readonly int _maxRequests;
    private readonly IRateLimitStorage _storage;

    public async ValueTask<RateLimitResult> CheckLimitAsync(string key, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var windowStart = now - _windowSize;

        // Remove old entries outside the window
        await _storage.RemoveBeforeAsync(key, windowStart, cancellationToken);

        // Count requests in current window
        var requestCount = await _storage.CountAsync(key, windowStart, now, cancellationToken);

        if (requestCount >= _maxRequests)
        {
            return new RateLimitResult {
                IsAllowed = false,
                RetryAfter = CalculateRetryAfter(requestCount),
                Limit = _maxRequests,
                Remaining = 0
            };
        }

        // Add current request
        await _storage.AddAsync(key, now, cancellationToken);

        return new RateLimitResult {
            IsAllowed = true,
            Limit = _maxRequests,
            Remaining = _maxRequests - requestCount - 1
        };
    }
}
```

#### Token Bucket

```csharp
public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly int _capacity;
    private readonly double _refillRate;
    private readonly IRateLimitStorage _storage;

    public async ValueTask<RateLimitResult> CheckLimitAsync(string key, CancellationToken cancellationToken)
    {
        var bucket = await _storage.GetBucketAsync(key, cancellationToken) ??
            new TokenBucket { Tokens = _capacity, LastRefill = DateTime.UtcNow };

        // Refill tokens based on time elapsed
        var now = DateTime.UtcNow;
        var timeElapsed = now - bucket.LastRefill;
        var tokensToAdd = timeElapsed.TotalSeconds * _refillRate;

        bucket.Tokens = Math.Min(_capacity, bucket.Tokens + tokensToAdd);
        bucket.LastRefill = now;

        if (bucket.Tokens < 1)
        {
            return new RateLimitResult {
                IsAllowed = false,
                RetryAfter = TimeSpan.FromSeconds(1 / _refillRate)
            };
        }

        bucket.Tokens -= 1;
        await _storage.SaveBucketAsync(key, bucket, cancellationToken);

        return new RateLimitResult { IsAllowed = true };
    }
}
```

## 💾 Storage Backends

### In-Memory Storage

```csharp
public class InMemoryRateLimitStorage : IRateLimitStorage
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    public async ValueTask AddAsync(string key, DateTime timestamp, CancellationToken cancellationToken)
    {
        var requests = _requests.GetOrAdd(key, _ => new List<DateTime>());
        lock (requests)
        {
            requests.Add(timestamp);
            // Cleanup old entries periodically
            if (requests.Count > 10000)
            {
                requests.RemoveAll(r => r < DateTime.UtcNow.AddHours(-1));
            }
        }
    }

    public async ValueTask<int> CountAsync(string key, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return _requests.GetValueOrDefault(key, new List<DateTime>())
            .Count(r => r >= from && r <= to);
    }
}
```

### Redis Distributed Storage

```csharp
public class RedisRateLimitStorage : IRateLimitStorage
{
    private readonly IConnectionMultiplexer _redis;

    public async ValueTask AddAsync(string key, DateTime timestamp, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        await db.SortedSetAddAsync($"ratelimit:{key}", timestamp.ToUnixTimeMilliseconds(), timestamp.ToUnixTimeMilliseconds());
    }

    public async ValueTask<int> CountAsync(string key, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var result = await db.SortedSetLengthAsync($"ratelimit:{key}",
            from.ToUnixTimeMilliseconds(),
            to.ToUnixTimeMilliseconds());

        return (int)result;
    }

    public async ValueTask RemoveBeforeAsync(string key, DateTime before, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        await db.SortedSetRemoveRangeByScoreAsync($"ratelimit:{key}", 0, before.ToUnixTimeMilliseconds());
    }
}
```

## ⚙️ Configuration Options

### Basic Setup

```csharp
builder.Services.AddArch(options =>
{
    options.UseRateLimit(rateLimitOptions =>
        rateLimitOptions.AddCage());
});
```

### Advanced Configuration

```csharp
builder.Services.AddArch(options =>
{
    options.UseRateLimit(rateLimitOptions =>
        rateLimitOptions.AddCage(cageOptions => {
            // Default global limit
            cageOptions.DefaultRule = new RateLimitRule {
                MaxRequests = 1000,
                WindowSize = TimeSpan.FromMinutes(1),
                BreachAction = RateLimitAction.Throttle
            };

            // API-specific rules
            cageOptions.Rules.Add("/api/public/*", new RateLimitRule {
                MaxRequests = 5000,
                WindowSize = TimeSpan.FromMinutes(1)
            });

            cageOptions.Rules.Add("/api/admin/*", new RateLimitRule {
                MaxRequests = 100,
                WindowSize = TimeSpan.FromMinutes(1),
                BreachAction = RateLimitAction.Block
            });

            // User-specific limits
            cageOptions.Rules.Add("user:*", new RateLimitRule {
                MaxRequests = 100,
                WindowSize = TimeSpan.FromHours(1),
                KeyExtractor = context => context.User?.Identity?.Name ?? "anonymous"
            });

            // IP-based limits
            cageOptions.Rules.Add("ip:*", new RateLimitRule {
                MaxRequests = 50,
                WindowSize = TimeSpan.FromMinutes(1),
                KeyExtractor = context => GetClientIp(context)
            });
        }));
});
```

## 🎯 Rule Engine

### Rate Limit Rules

```csharp
public class RateLimitRule
{
    public int MaxRequests { get; set; }
    public TimeSpan WindowSize { get; set; }
    public RateLimitAlgorithm Algorithm { get; set; } = RateLimitAlgorithm.SlidingWindow;
    public RateLimitAction BreachAction { get; set; } = RateLimitAction.Throttle;
    public Func<HttpContext, string>? KeyExtractor { get; set; }
    public Func<HttpContext, bool>? Condition { get; set; }
    public CustomResponse? CustomResponse { get; set; }
}

public enum RateLimitAlgorithm
{
    FixedWindow,
    SlidingWindow,
    TokenBucket,
    LeakyBucket
}

public enum RateLimitAction
{
    Throttle,    // Return 429 with retry-after
    Block,      // Return 429 without retry-after
    LogOnly,    // Allow but log violation
    Custom      // Use custom handler
}
```

### Rule Matching

```csharp
public class RateLimitRuleMatcher
{
    public RateLimitRule? Match(HttpContext context, IEnumerable<RateLimitRule> rules)
    {
        // Most specific rules first (exact path matches)
        var exactMatch = rules.FirstOrDefault(r =>
            r.Pattern != null && MatchesPattern(context.Request.Path, r.Pattern));

        if (exactMatch != null) return exactMatch;

        // Wildcard matches
        var wildcardMatch = rules.FirstOrDefault(r =>
            r.Pattern != null && MatchesWildcard(context.Request.Path, r.Pattern));

        if (wildcardMatch != null) return wildcardMatch;

        // Default rule
        return rules.FirstOrDefault(r => r.IsDefault);
    }

    private bool MatchesPattern(PathString requestPath, string pattern)
    {
        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return Regex.IsMatch(requestPath.Value!, regexPattern, RegexOptions.IgnoreCase);
    }
}
```

## 📊 Monitoring & Analytics

### Rate Limit Metrics

```csharp
// Prometheus metrics
private readonly Counter _requestsTotal = Metrics.CreateCounter(
    "arch_ratelimit_requests_total",
    "Total requests processed",
    new[] { "endpoint", "status", "rule" });

private readonly Counter _rateLimitViolations = Metrics.CreateCounter(
    "arch_ratelimit_violations_total",
    "Rate limit violations",
    new[] { "endpoint", "rule", "action" });

private readonly Gauge _currentLimits = Metrics.CreateGauge(
    "arch_ratelimit_current_limits",
    "Current rate limits by endpoint",
    new[] { "endpoint" });

private readonly Histogram _rateLimitCheckDuration = Metrics.CreateHistogram(
    "arch_ratelimit_check_duration_seconds",
    "Rate limit check duration");
```

### Real-time Monitoring

```csharp
public class RateLimitMonitor
{
    private readonly ConcurrentDictionary<string, RateLimitStats> _stats = new();

    public void RecordRequest(string endpoint, string rule, bool allowed)
    {
        var stats = _stats.GetOrAdd(endpoint, _ => new RateLimitStats());

        Interlocked.Increment(ref stats.TotalRequests);

        if (!allowed)
        {
            Interlocked.Increment(ref stats.BlockedRequests);
            _rateLimitViolations.WithLabels(endpoint, rule, "blocked").Inc();
        }

        _requestsTotal.WithLabels(endpoint, allowed ? "allowed" : "blocked", rule).Inc();
    }

    public RateLimitStats GetStats(string endpoint)
    {
        return _stats.GetValueOrDefault(endpoint, new RateLimitStats());
    }
}
```

## 🚀 Advanced Features

### Distributed Rate Limiting

```csharp
public class DistributedRateLimiter : IRateLimiter
{
    private readonly IEnumerable<IRateLimiter> _limiters;
    private readonly DistributedCache _cache;

    public async ValueTask<RateLimitResult> CheckLimitAsync(string key, CancellationToken cancellationToken)
    {
        // Check local cache first
        var cached = await _cache.GetAsync<RateLimitResult>($"ratelimit:{key}");
        if (cached != null && cached.ExpiresAt > DateTime.UtcNow)
        {
            return cached;
        }

        // Check all limiters (sliding window, token bucket, etc.)
        var results = await Task.WhenAll(_limiters.Select(l =>
            l.CheckLimitAsync(key, cancellationToken)));

        // Combine results (most restrictive wins)
        var finalResult = results.Aggregate((acc, curr) =>
            acc.Remaining < curr.Remaining ? acc : curr);

        // Cache result for short time
        await _cache.SetAsync($"ratelimit:{key}", finalResult,
            TimeSpan.FromMilliseconds(100), cancellationToken);

        return finalResult;
    }
}
```

### Adaptive Rate Limiting

```csharp
public class AdaptiveRateLimiter : IRateLimiter
{
    private readonly IRateLimiter _baseLimiter;
    private readonly IPerformanceMonitor _performanceMonitor;

    public async ValueTask<RateLimitResult> CheckLimitAsync(string key, CancellationToken cancellationToken)
    {
        var baseResult = await _baseLimiter.CheckLimitAsync(key, cancellationToken);

        // Adjust limits based on system performance
        var systemLoad = await _performanceMonitor.GetSystemLoadAsync(cancellationToken);

        if (systemLoad > 0.8) // 80% load
        {
            // Reduce limits during high load
            return new RateLimitResult {
                IsAllowed = baseResult.Remaining > 5, // More restrictive
                Limit = baseResult.Limit,
                Remaining = Math.Max(0, baseResult.Remaining - 5),
                RetryAfter = baseResult.RetryAfter
            };
        }

        return baseResult;
    }
}
```

### Burst Handling

```csharp
public class BurstRateLimiter : IRateLimiter
{
    private readonly IRateLimiter _baseLimiter;
    private readonly int _burstCapacity;

    public async ValueTask<RateLimitResult> CheckLimitAsync(string key, CancellationToken cancellationToken)
    {
        // Allow burst up to capacity, then fall back to base limiter
        var burstKey = $"{key}:burst";
        var burstCount = await _storage.GetCounterAsync(burstKey, cancellationToken);

        if (burstCount < _burstCapacity)
        {
            await _storage.IncrementCounterAsync(burstKey, cancellationToken);
            return new RateLimitResult { IsAllowed = true };
        }

        // Burst exhausted, use base limiter
        return await _baseLimiter.CheckLimitAsync(key, cancellationToken);
    }
}
```

## 🛡️ Security & Abuse Prevention

### DDoS Protection

```csharp
// Configure for DDoS protection
options.UseRateLimit(rateLimitOptions =>
    rateLimitOptions.AddCage(cageOptions => {
        // Very aggressive limits for suspicious patterns
        cageOptions.Rules.Add("ddos:*", new RateLimitRule {
            MaxRequests = 10,
            WindowSize = TimeSpan.FromSeconds(1),
            BreachAction = RateLimitAction.Block,
            KeyExtractor = context => GetClientIp(context)
        });

        // Rate limit by user agent
        cageOptions.Rules.Add("bot:*", new RateLimitRule {
            MaxRequests = 5,
            WindowSize = TimeSpan.FromSeconds(10),
            Condition = context => IsSuspiciousUserAgent(context.Request.Headers["User-Agent"])
        });
    }));
```

### API Key Rate Limiting

```csharp
public class ApiKeyRateLimiter : IRateLimiter
{
    private readonly IApiKeyValidator _apiKeyValidator;

    public async ValueTask<RateLimitResult> CheckLimitAsync(string key, CancellationToken cancellationToken)
    {
        var apiKey = ExtractApiKey(key);
        var plan = await _apiKeyValidator.GetPlanAsync(apiKey, cancellationToken);

        // Different limits based on API plan
        var limits = GetLimitsForPlan(plan);

        return await _baseLimiter.CheckLimitAsync($"apikey:{apiKey}", limits, cancellationToken);
    }
}
```

## 🧪 Testing

### Unit Tests

```csharp
public class RateLimiterTests
{
    [Fact]
    public async Task SlidingWindowRateLimiter_AllowsRequestsWithinLimit()
    {
        // Arrange
        var storage = new Mock<IRateLimitStorage>();
        storage.Setup(s => s.CountAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(5); // 5 requests in window

        var limiter = new SlidingWindowRateLimiter(TimeSpan.FromMinutes(1), 10, storage.Object);

        // Act
        var result = await limiter.CheckLimitAsync("test-key");

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(4, result.Remaining); // 10 - 5 - 1 = 4
    }

    [Fact]
    public async Task SlidingWindowRateLimiter_BlocksRequestsOverLimit()
    {
        // Arrange
        var storage = new Mock<IRateLimitStorage>();
        storage.Setup(s => s.CountAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(10); // At limit

        var limiter = new SlidingWindowRateLimiter(TimeSpan.FromMinutes(1), 10, storage.Object);

        // Act
        var result = await limiter.CheckLimitAsync("test-key");

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal(0, result.Remaining);
    }
}
```

### Integration Tests

```csharp
public class RateLimitIntegrationTests
{
    [Fact]
    public async Task EndToEnd_RateLimit_Enforced()
    {
        // Arrange
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Act - Make requests up to limit
        var tasks = Enumerable.Range(0, 100).Select(_ =>
            client.GetAsync("/api/test"));

        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        var rateLimitedCount = responses.Count(r => (int)r.StatusCode == 429);

        Assert.Equal(60, successCount); // Assuming 60 requests/minute limit
        Assert.Equal(40, rateLimitedCount);
    }
}
```

## 📈 Scaling Considerations

### High-Throughput Scenarios

```csharp
// Optimize for high throughput
options.UseRateLimit(rateLimitOptions =>
    rateLimitOptions.AddCage(cageOptions => {
        cageOptions.UseRedisStorage(redisOptions => {
            redisOptions.Configuration = "redis-cluster:6379,password=secure";
            redisOptions.KeyPrefix = "ratelimit:";
        });

        cageOptions.EnableAsyncProcessing = true;
        cageOptions.MaxConcurrentChecks = 1000;
        cageOptions.UseMemoryCache = true;
    }));
```

### Multi-Region Deployment

```csharp
// Global rate limiting across regions
public class GlobalRateLimiter : IRateLimiter
{
    private readonly IEnumerable<IRegionalRateLimiter> _regionalLimiters;

    public async ValueTask<RateLimitResult> CheckLimitAsync(string key, CancellationToken cancellationToken)
    {
        // Check global limit first
        var globalResult = await _globalLimiter.CheckLimitAsync($"global:{key}", cancellationToken);
        if (!globalResult.IsAllowed) return globalResult;

        // Check regional limit
        var regionalResult = await _regionalLimiter.CheckLimitAsync($"regional:{key}", cancellationToken);
        if (!regionalResult.IsAllowed) return regionalResult;

        return new RateLimitResult { IsAllowed = true };
    }
}
```

## 🎯 Use Cases

### API Protection

```csharp
// Protect public APIs
options.UseRateLimit(rateLimitOptions =>
{
    rateLimitOptions.AddCage(cageOptions => {
        // General API limits
        cageOptions.DefaultRule = new RateLimitRule {
            MaxRequests = 1000,
            WindowSize = TimeSpan.FromMinutes(1)
        };

        // Stricter limits for expensive operations
        cageOptions.Rules.Add("/api/search", new RateLimitRule {
            MaxRequests = 100,
            WindowSize = TimeSpan.FromMinutes(1)
        });

        // Very strict limits for authentication
        cageOptions.Rules.Add("/api/auth/login", new RateLimitRule {
            MaxRequests = 5,
            WindowSize = TimeSpan.FromMinutes(1),
            BreachAction = RateLimitAction.Block
        });
    });
});
```

### User-Based Limits

```csharp
// Different limits per user tier
options.UseRateLimit(rateLimitOptions =>
{
    rateLimitOptions.AddCage(cageOptions => {
        // Free tier
        cageOptions.Rules.Add("user:free:*", new RateLimitRule {
            MaxRequests = 100,
            WindowSize = TimeSpan.FromHours(1)
        });

        // Premium tier
        cageOptions.Rules.Add("user:premium:*", new RateLimitRule {
            MaxRequests = 1000,
            WindowSize = TimeSpan.FromHours(1)
        });

        // Enterprise tier
        cageOptions.Rules.Add("user:enterprise:*", new RateLimitRule {
            MaxRequests = 10000,
            WindowSize = TimeSpan.FromHours(1)
        });
    });
});
```

### Microservices Protection

```csharp
// Protect downstream services
public class ServiceRateLimiter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRateLimiter _rateLimiter;

    public async Task<HttpResponseMessage> CallServiceAsync(string serviceUrl, HttpRequestMessage request)
    {
        var key = $"service:{serviceUrl.GetHashCode()}";

        var rateLimitResult = await _rateLimiter.CheckLimitAsync(key);
        if (!rateLimitResult.IsAllowed)
        {
            throw new RateLimitExceededException(rateLimitResult.RetryAfter);
        }

        var client = _httpClientFactory.CreateClient();
        return await client.SendAsync(request);
    }
}
```

## 📊 Dashboard & Reporting

### Rate Limit Dashboard

Access rate limiting metrics and controls at: `http://localhost:5229/rate-limit-dashboard`

Features:
- Real-time request monitoring
- Rate limit violations by endpoint
- Top rate-limited IPs/users
- Rule effectiveness analysis
- Historical trends

### Custom Reporting

```csharp
public class RateLimitReporter
{
    private readonly IRateLimitMonitor _monitor;

    public async Task<RateLimitReport> GenerateReportAsync(DateTime from, DateTime to)
    {
        var violations = await _monitor.GetViolationsAsync(from, to);
        var topEndpoints = violations
            .GroupBy(v => v.Endpoint)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new EndpointStats {
                Endpoint = g.Key,
                ViolationCount = g.Count(),
                AverageRequestsPerMinute = g.Average(v => v.RequestsPerMinute)
            });

        return new RateLimitReport {
            Period = new DateRange(from, to),
            TotalViolations = violations.Count,
            TopViolatedEndpoints = topEndpoints,
            PeakViolationHour = violations
                .GroupBy(v => v.Timestamp.Hour)
                .OrderByDescending(g => g.Count())
                .First().Key
        };
    }
}
```

## 📚 Related Documentation

- [Setup Guide](setup.md) - Rate limiting configuration
- [Security](security.md) - DDoS protection and abuse prevention
- [Monitoring](monitoring.md) - Rate limit dashboards and analytics
- [Performance Tuning](performance.md) - High-throughput rate limiting

## 🤝 Contributing

See [Contributing Guide](../../CONTRIBUTING.md) for rate limiting development.

---

<div align="center">
  <p>🚦 Intelligent request throttling for optimal API performance</p>
  <p>
    <a href="logging.md">← Logging</a> |
    <a href="../README.md">README</a>
  </p>
</div>
