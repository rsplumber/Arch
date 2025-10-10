# 📝 Logging - Structured Observability Framework

Arch's Logging framework provides **enterprise-grade structured logging** with multiple sinks, correlation ID tracking, and performance-optimized logging pipelines for comprehensive application observability.

## 🏗️ Architecture

### Core Components

```
Logging Layer
├── Logger Abstractions             # Unified logging interfaces
├── Logger Implementations          # Concrete logger providers
├── Structured Logging              # JSON-formatted logs
├── Correlation Tracking           # Request tracing
├── Performance Monitoring         # Logging metrics
└── Sink Management               # Multiple output targets
```

### Logger Interface

```csharp
public interface IArchLogger
{
    void Log(LogLevel level, string message, params object[] args);
    void Log(LogLevel level, Exception exception, string message, params object[] args);
    void LogRequest(HttpContext context, TimeSpan duration, LogLevel level = LogLevel.Information);
    void LogResponse(HttpContext context, int statusCode, TimeSpan duration);
    void SetCorrelationId(string correlationId);
}
```

## 🎯 Logger Implementations

### Console Logger

```csharp
public class ConsoleLogger : IArchLogger
{
    private readonly ILogger<ConsoleLogger> _logger;
    private readonly ConsoleLoggerOptions _options;

    public void LogRequest(HttpContext context, TimeSpan duration, LogLevel level = LogLevel.Information)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Level = level.ToString(),
            CorrelationId = GetCorrelationId(context),
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            Duration = duration.TotalMilliseconds,
            StatusCode = context.Response.StatusCode,
            ClientIp = GetClientIp(context)
        };

        _logger.Log(level, "HTTP Request: {@Request}", logEntry);
    }
}
```

### Logstash Logger

```csharp
public class LogstashLogger : IArchLogger
{
    private readonly ITcpClient _tcpClient;
    private readonly LogstashOptions _options;

    public async ValueTask LogAsync(LogEntry entry)
    {
        var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await _tcpClient.SendAsync(Encoding.UTF8.GetBytes(json));
    }

    public void LogRequest(HttpContext context, TimeSpan duration, LogLevel level = LogLevel.Information)
    {
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = "HTTP Request",
            CorrelationId = GetCorrelationId(context),
            Request = new RequestInfo
            {
                Method = context.Request.Method,
                Path = context.Request.Path.ToString(),
                QueryString = context.Request.QueryString.ToString(),
                Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                BodySize = context.Request.ContentLength
            },
            Response = new ResponseInfo
            {
                StatusCode = context.Response.StatusCode,
                Duration = duration,
                ContentType = context.Response.ContentType
            },
            Client = new ClientInfo
            {
                IpAddress = GetClientIp(context),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                UserId = context.User?.Identity?.Name
            }
        };

        _ = LogAsync(logEntry); // Fire and forget
    }
}
```

## ⚙️ Configuration Options

### Basic Setup

```csharp
builder.Services.AddArch(options =>
{
    options.AddLogging(loggingOptions =>
        loggingOptions.UseConsole(consoleOptions => {
            consoleOptions.UseJsonFormat = true;
            consoleOptions.IncludeScopes = true;
        }));
});
```

### Advanced Configuration

```csharp
builder.Services.AddArch(options =>
{
    options.AddLogging(loggingOptions => {
        // Console logging for development
        loggingOptions.UseConsole(consoleOptions => {
            consoleOptions.UseJsonFormat = true;
            consoleOptions.IncludeScopes = true;
            consoleOptions.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
            consoleOptions.MinimumLevel = LogLevel.Debug;
        });

        // Logstash for production
        loggingOptions.UseLogstash(logstashOptions => {
            logstashOptions.Host = "logstash.company.com";
            logstashOptions.Port = 5044;
            logstashOptions.UseSSL = true;
            logstashOptions.IndexFormat = "arch-gateway-{0:yyyy.MM.dd}";
            logstashOptions.BufferSize = 1000;
            logstashOptions.FlushInterval = TimeSpan.FromSeconds(5);
        });

        // Global filters
        loggingOptions.Filters.Add(new LogFilter {
            Category = "Microsoft.AspNetCore.*",
            Level = LogLevel.Warning
        });

        loggingOptions.Filters.Add(new LogFilter {
            Message = "*password*",
            Action = FilterAction.Redact
        });
    });
});
```

## 🔗 Correlation ID Tracking

### Request Correlation

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Set in logger context
        using (_logger.BeginScope(new Dictionary<string, object> {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
```

### Distributed Tracing

```csharp
public class DistributedLogger : IArchLogger
{
    public void LogRequest(HttpContext context, TimeSpan duration, LogLevel level = LogLevel.Information)
    {
        var spanId = context.Request.Headers["X-B3-SpanId"].FirstOrDefault();
        var traceId = context.Request.Headers["X-B3-TraceId"].FirstOrDefault();

        var logEntry = new
        {
            TraceId = traceId,
            SpanId = spanId,
            ParentSpanId = context.Request.Headers["X-B3-ParentSpanId"].FirstOrDefault(),
            CorrelationId = GetCorrelationId(context),
            // ... other fields
        };

        _logger.Log(level, "Distributed trace: {@Trace}", logEntry);
    }
}
```

## 📊 Structured Logging

### Log Entry Format

```csharp
public class StructuredLogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; }
    public string CorrelationId { get; set; }
    public string Category { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public ExceptionInfo? Exception { get; set; }
    public RequestInfo? Request { get; set; }
    public PerformanceInfo? Performance { get; set; }
}

public class RequestInfo
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string QueryString { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public long? ContentLength { get; set; }
    public string ClientIp { get; set; }
    public string UserAgent { get; set; }
}

public class PerformanceInfo
{
    public TimeSpan Duration { get; set; }
    public long MemoryUsed { get; set; }
    public double CpuUsage { get; set; }
}
```

### Custom Properties

```csharp
public static class LoggerExtensions
{
    public static void LogBusinessEvent(
        this IArchLogger logger,
        string eventName,
        string userId,
        Dictionary<string, object> properties)
    {
        logger.Log(LogLevel.Information, "Business event: {EventName}", eventName,
            new StructuredLogEntry {
                CorrelationId = GetCurrentCorrelationId(),
                Category = "Business",
                Properties = new Dictionary<string, object> {
                    ["EventName"] = eventName,
                    ["UserId"] = userId,
                    ["Timestamp"] = DateTime.UtcNow
                }.Concat(properties).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            });
    }
}
```

## 🚀 Performance Optimization

### Async Logging

```csharp
public class AsyncLogger : IArchLogger
{
    private readonly Channel<LogEntry> _logChannel;
    private readonly Task _processingTask;

    public AsyncLogger()
    {
        _logChannel = Channel.CreateBounded<LogEntry>(1000);
        _processingTask = ProcessLogEntriesAsync();
    }

    public void Log(LogLevel level, string message, params object[] args)
    {
        var entry = new LogEntry {
            Level = level,
            Message = message,
            Args = args,
            Timestamp = DateTime.UtcNow
        };

        _logChannel.Writer.TryWrite(entry);
    }

    private async Task ProcessLogEntriesAsync()
    {
        await foreach (var entry in _logChannel.Reader.ReadAllAsync())
        {
            await WriteToSinkAsync(entry);
        }
    }
}
```

### Buffered Logging

```csharp
public class BufferedLogger : IArchLogger
{
    private readonly List<LogEntry> _buffer = new();
    private readonly Timer _flushTimer;
    private readonly object _lock = new();

    public BufferedLogger()
    {
        _flushTimer = new Timer(FlushBuffer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public void Log(LogLevel level, string message, params object[] args)
    {
        lock (_lock)
        {
            _buffer.Add(new LogEntry { /* ... */ });

            if (_buffer.Count >= 100) // Flush threshold
            {
                FlushBuffer(null);
            }
        }
    }

    private void FlushBuffer(object? state)
    {
        List<LogEntry> entriesToFlush;

        lock (_lock)
        {
            entriesToFlush = _buffer.ToList();
            _buffer.Clear();
        }

        _ = FlushToSinkAsync(entriesToFlush);
    }
}
```

## 🔒 Security & Privacy

### Log Sanitization

```csharp
public class LogSanitizer
{
    private static readonly Regex[] _sensitivePatterns = {
        new Regex(@"(password|pwd|pass)\s*[:=]\s*[""']?([^""'\s]+)", RegexOptions.IgnoreCase),
        new Regex(@"(secret|key|token)\s*[:=]\s*[""']?([^""'\s]+)", RegexOptions.IgnoreCase),
        new Regex(@"\b\d{4}[-]?\d{4}[-]?\d{4}[-]?\d{4}\b"), // Credit cards
        new Regex(@"\b\d{3}[-]?\d{3}[-]?\d{4}\b") // SSN
    };

    public string Sanitize(string message)
    {
        foreach (var pattern in _sensitivePatterns)
        {
            message = pattern.Replace(message, m => $"{m.Groups[1].Value}: [REDACTED]");
        }

        return message;
    }
}
```

### PII Filtering

```csharp
public class PIIFilter
{
    public string Filter(string input)
    {
        // Remove or mask PII data
        input = Regex.Replace(input, @"\b\d{3}-\d{2}-\d{4}\b", "***-**-****"); // SSN
        input = Regex.Replace(input, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", "[EMAIL]"); // Email

        return input;
    }
}
```

## 📊 Monitoring & Metrics

### Logging Performance Metrics

```csharp
// Prometheus metrics
private readonly Counter _logsTotal = Metrics.CreateCounter(
    "arch_logging_logs_total",
    "Total log entries generated",
    new[] { "level", "category" });

private readonly Histogram _logProcessingDuration = Metrics.CreateHistogram(
    "arch_logging_processing_duration_seconds",
    "Log processing duration");

private readonly Gauge _bufferSize = Metrics.CreateGauge(
    "arch_logging_buffer_size",
    "Current log buffer size");

private readonly Counter _logsDropped = Metrics.CreateCounter(
    "arch_logging_logs_dropped_total",
    "Total log entries dropped due to buffer overflow");
```

### Health Checks

```csharp
public class LoggingHealthCheck : IHealthCheck
{
    private readonly IArchLogger _logger;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // Test logging functionality
            await _logger.LogAsync(LogLevel.Information, "Health check test");

            return HealthCheckResult.Healthy("Logging service is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Logging service error", ex);
        }
    }
}
```

## 🔧 Log Aggregation & Analysis

### Elasticsearch Integration

```csharp
public class ElasticsearchSink : ILogSink
{
    private readonly ElasticClient _elasticClient;

    public async ValueTask WriteAsync(LogEntry entry)
    {
        var indexName = $"logs-{entry.Timestamp:yyyy.MM.dd}";

        await _elasticClient.IndexAsync(entry, idx => idx
            .Index(indexName)
            .Id(entry.Id)
            .Refresh(Refresh.False));
    }

    public async ValueTask WriteBatchAsync(IEnumerable<LogEntry> entries)
    {
        var bulkRequest = new BulkRequest("logs")
        {
            Operations = entries.Select(e => new BulkIndexOperation<LogEntry>(e)
            {
                Id = e.Id.ToString()
            }).Cast<IBulkOperation>().ToList()
        };

        await _elasticClient.BulkAsync(bulkRequest);
    }
}
```

### Log Queries

```sql
-- Find all errors in the last hour
GET /logs-*/_search
{
  "query": {
    "bool": {
      "must": [
        { "term": { "level": "Error" } },
        { "range": { "@timestamp": { "gte": "now-1h" } } }
      ]
    }
  },
  "size": 100,
  "sort": [{ "@timestamp": { "order": "desc" } }]
}

-- Find requests with high latency
GET /logs-*/_search
{
  "query": {
    "bool": {
      "must": [
        { "term": { "category": "Request" } },
        { "range": { "duration": { "gte": 5000 } } }
      ]
    }
  }
}
```

## 🧪 Testing

### Logger Testing

```csharp
public class LoggerTests
{
    [Fact]
    public void LogRequest_IncludesAllRequiredFields()
    {
        // Arrange
        var logger = new Mock<IArchLogger>();
        var context = CreateHttpContext();

        // Act
        logger.Object.LogRequest(context, TimeSpan.FromMilliseconds(150));

        // Assert
        logger.Verify(l => l.Log(
            LogLevel.Information,
            It.Is<string>(s => s.Contains("HTTP Request")),
            It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task StructuredLogging_IncludesCorrelationId()
    {
        // Arrange
        var logger = new StructuredLogger();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        logger.SetCorrelationId(correlationId);
        await logger.LogAsync(LogLevel.Information, "Test message");

        // Assert - Verify correlation ID is included in output
    }
}
```

### Integration Testing

```csharp
public class LoggingIntegrationTests
{
    [Fact]
    public async Task RequestLogging_EndToEnd_CapturesAllData()
    {
        // Arrange
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/test");

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify logs contain request details
        var logs = factory.GetLogs();
        Assert.Contains(logs, log =>
            log.Contains("GET") &&
            log.Contains("/api/test") &&
            log.Contains("correlation-id"));
    }
}
```

## 🎯 Advanced Features

### Contextual Logging

```csharp
public class ContextualLogger : IArchLogger
{
    private readonly AsyncLocal<Dictionary<string, object>> _context = new();

    public IDisposable BeginScope(string key, object value)
    {
        var context = _context.Value ?? new Dictionary<string, object>();
        context[key] = value;
        _context.Value = context;

        return new Scope(() => {
            if (_context.Value?.Remove(key) == true && _context.Value.Count == 0) {
                _context.Value = null;
            }
        });
    }

    public void Log(LogLevel level, string message, params object[] args)
    {
        var enrichedProperties = _context.Value ?? new Dictionary<string, object>();
        var logEntry = new LogEntry {
            Message = message,
            Properties = enrichedProperties,
            // ... other fields
        };

        _innerLogger.Log(level, message, args);
    }
}
```

### Log Sampling

```csharp
public class SamplingLogger : IArchLogger
{
    private readonly IArchLogger _innerLogger;
    private readonly double _sampleRate;

    public void Log(LogLevel level, string message, params object[] args)
    {
        if (level >= LogLevel.Warning || Random.Shared.NextDouble() < _sampleRate)
        {
            _innerLogger.Log(level, message, args);
        }
    }
}
```

## 📚 Use Cases

### API Gateway Logging

```csharp
// Log all API requests and responses
app.UseArch(options =>
{
    options.BeforeDispatching(dispatchingOptions =>
    {
        dispatchingOptions.UseLogging();
    });

    options.AfterDispatching(dispatchingOptions =>
    {
        dispatchingOptions.UseLogging();
    });
});
```

### Business Event Logging

```csharp
public class OrderService
{
    public async Task ProcessOrderAsync(Order order)
    {
        _logger.LogBusinessEvent("OrderCreated", order.CustomerId, new Dictionary<string, object> {
            ["OrderId"] = order.Id,
            ["TotalAmount"] = order.TotalAmount,
            ["Items"] = order.Items.Count
        });

        // Process order...

        if (order.TotalAmount > 1000)
        {
            _logger.LogBusinessEvent("HighValueOrder", order.CustomerId, new Dictionary<string, object> {
                ["OrderId"] = order.Id,
                ["Amount"] = order.TotalAmount
            });
        }
    }
}
```

### Audit Logging

```csharp
public class AuditLogger
{
    public void LogUserAction(string userId, string action, Dictionary<string, object> context)
    {
        _logger.Log(LogLevel.Information, "User action: {Action}", action,
            new AuditLogEntry {
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetClientIp(),
                UserAgent = GetUserAgent(),
                Context = context
            });
    }
}
```

## 📈 Scaling Considerations

### High-Volume Logging

```csharp
// Configure for high throughput
options.AddLogging(loggingOptions =>
    loggingOptions.UseLogstash(logstashOptions => {
        logstashOptions.BufferSize = 10000;
        logstashOptions.FlushInterval = TimeSpan.FromMilliseconds(100);
        logstashOptions.UseCompression = true;
        logstashOptions.MaxConcurrentConnections = 10;
    }));
```

### Distributed Logging

```csharp
// Multi-region log aggregation
public class DistributedLogger : IArchLogger
{
    private readonly IEnumerable<IRegionalLogger> _regionalLoggers;

    public void Log(LogLevel level, string message, params object[] args)
    {
        // Log to local region
        _localLogger.Log(level, message, args);

        // Forward critical logs to central aggregator
        if (level >= LogLevel.Warning)
        {
            _centralLogger.Log(level, message, args);
        }
    }
}
```

## 📚 Related Documentation

- [Setup Guide](setup.md) - Logging configuration
- [Monitoring](monitoring.md) - Log analysis and dashboards
- [Performance Tuning](performance.md) - Logging optimization
- [Security](security.md) - Log security and compliance

## 🤝 Contributing

See [Contributing Guide](../../CONTRIBUTING.md) for logging framework development.

---

<div align="center">
  <p>📝 Comprehensive observability for modern applications</p>
  <p>
    <a href="load-balancer.md">← Load Balancer</a> |
    <a href="../README.md">README</a> |
    <a href="rate-limiting.md">Rate Limiting →</a>
  </p>
</div>
