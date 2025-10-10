# 🔐 Authorization - Enterprise Security Framework

Arch's Authorization module provides **enterprise-grade authentication and authorization** capabilities with support for multiple identity providers, role-based access control (RBAC), and fine-grained permissions.

## 🏗️ Architecture

### Core Components

```
Authorization.Abstractions/
├── IAuthorizationMiddleware.cs      # Authorization pipeline interface
├── AuthorizationExecutionOptions.cs # Request processing options
├── AuthorizationOptions.cs         # Configuration options

Authorization.Kundera/
├── KunderaAuthorizationMiddleware.cs     # JWT validation middleware
├── RequestInfoExtensions.cs             # Request context helpers
├── EndpointDefinitionExtensions.cs      # Route authorization helpers
├── AllowAnonymous/                      # Anonymous access decorators
├── Permission/                          # Permission-based decorators
```

### Key Interfaces

```csharp
public interface IAuthorizationMiddleware
{
    ValueTask<AuthorizationResult> AuthorizeAsync(
        HttpContext context,
        AuthorizationExecutionOptions options,
        CancellationToken cancellationToken = default);
}

public class AuthorizationResult
{
    public bool IsAuthorized { get; set; }
    public string? FailureReason { get; set; }
    public ClaimsPrincipal? User { get; set; }
    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
}
```

## 🎯 Authentication Methods

### JWT Bearer Tokens

```csharp
// Configuration
builder.Services.AddArch(options =>
{
    options.AddAuthorization(authorizationOptions =>
        authorizationOptions.UseKundera(kunderaOptions => {
            kunderaOptions.BaseUrl = "https://auth.company.com";
            kunderaOptions.ServiceSecret = "jwt-secret-key";
            kunderaOptions.TokenValidationParameters = new() {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "https://auth.company.com",
                ValidAudience = "arch-gateway"
            };
        }));
});
```

### External Identity Providers

```csharp
// OAuth2/OpenID Connect support
options.AddAuthorization(authorizationOptions =>
    authorizationOptions.UseExternalProvider(externalOptions => {
        externalOptions.Provider = IdentityProvider.AzureAD;
        externalOptions.ClientId = "azure-client-id";
        externalOptions.TenantId = "azure-tenant-id";
        externalOptions.Scopes = new[] { "openid", "profile", "email" };
    }));
```

## 🛡️ Authorization Patterns

### Role-Based Access Control (RBAC)

```csharp
// Decorate endpoints with roles
[Authorize(Roles = "Admin,Manager")]
public class AdminController : ControllerBase
{
    [HttpGet("users")]
    [Authorize(Roles = "UserManager")]
    public IActionResult GetUsers() { /* ... */ }

    [HttpPost("users")]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateUser() { /* ... */ }
}
```

### Permission-Based Authorization

```csharp
// Fine-grained permissions
[Permission("user.create", "user.read")]
public class UserController : ControllerBase
{
    [HttpGet("{id}")]
    [Permission("user.read")]
    public IActionResult GetUser(int id) { /* ... */ }

    [HttpPost]
    [Permission("user.create")]
    public IActionResult CreateUser(CreateUserRequest request) { /* ... */ }
}
```

### Policy-Based Authorization

```csharp
// Custom authorization policies
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrOwner", policy =>
        policy.RequireAssertion(context =>
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            var resourceOwnerId = context.Request.RouteValues["userId"]?.ToString();

            return context.User.IsInRole("Admin") || userId == resourceOwnerId;
        }));
});

[Authorize(Policy = "AdminOrOwner")]
public IActionResult UpdateUser(int userId) { /* ... */ }
```

## 🔄 Request Processing Pipeline

### Authorization Middleware Flow

```
1. Extract Token ──► Validate Token ──► Enrich Context ──► Check Permissions
       │                     │                    │                    │
       └─ JWT Bearer         └─ Signature         └─ Claims           └─ RBAC/ABAC
       └─ API Key           └─ Expiration        └─ Roles           └─ Custom Policies
       └─ Certificate      └─ Issuer/Audience  └─ Permissions
```

### Middleware Implementation

```csharp
public class KunderaAuthorizationMiddleware : IAuthorizationMiddleware
{
    private readonly KunderaOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public async ValueTask<AuthorizationResult> AuthorizeAsync(
        HttpContext context,
        AuthorizationExecutionOptions options,
        CancellationToken cancellationToken)
    {
        // Extract token from header
        var token = ExtractToken(context.Request);

        if (string.IsNullOrEmpty(token))
            return AuthorizationResult.Unauthorized("Missing token");

        // Validate with identity provider
        var validationResult = await ValidateTokenAsync(token, cancellationToken);

        if (!validationResult.IsValid)
            return AuthorizationResult.Unauthorized(validationResult.Error);

        // Enrich context with user information
        context.User = validationResult.ClaimsPrincipal;

        // Check permissions
        var permissions = await GetUserPermissionsAsync(validationResult.UserId, cancellationToken);
        var requiredPermissions = GetRequiredPermissions(context.Request);

        if (!HasRequiredPermissions(permissions, requiredPermissions))
            return AuthorizationResult.Forbidden("Insufficient permissions");

        return AuthorizationResult.Success(validationResult.ClaimsPrincipal, permissions);
    }
}
```

## 🔑 Token Management

### JWT Token Validation

```csharp
private async Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken)
{
    var handler = new JwtSecurityTokenHandler();
    var validationParameters = _options.TokenValidationParameters;

    try
    {
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        return new TokenValidationResult
        {
            IsValid = true,
            ClaimsPrincipal = principal,
            UserId = principal.FindFirst("sub")?.Value
        };
    }
    catch (SecurityTokenException ex)
    {
        return new TokenValidationResult
        {
            IsValid = false,
            Error = ex.Message
        };
    }
}
```

### Token Refresh

```csharp
public async Task<string> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
{
    var refreshRequest = new RefreshTokenRequest
    {
        RefreshToken = refreshToken,
        ClientId = _options.ClientId,
        ClientSecret = _options.ClientSecret
    };

    var response = await _httpClient.PostAsJsonAsync(
        $"{_options.BaseUrl}/oauth/token",
        refreshRequest,
        cancellationToken);

    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

    return tokenResponse.AccessToken;
}
```

## 🛡️ Security Features

### Threat Protection

- **JWT Replay Attack Prevention**: Token blacklisting and nonce validation
- **Rate Limiting Integration**: Automatic rate limiting for failed authentication attempts
- **Brute Force Protection**: Account lockout after consecutive failures
- **Suspicious Activity Detection**: Anomaly detection for unusual authentication patterns

### Audit Logging

```csharp
public class AuthorizationAuditLogger
{
    private readonly ILogger _logger;

    public void LogAuthenticationAttempt(string userId, bool success, string ipAddress, string userAgent)
    {
        var logLevel = success ? LogLevel.Information : LogLevel.Warning;
        var eventType = success ? "AUTH_SUCCESS" : "AUTH_FAILURE";

        _logger.Log(logLevel, "Authentication {EventType} - User: {UserId}, IP: {IPAddress}, UA: {UserAgent}",
            eventType, userId, ipAddress, userAgent);
    }

    public void LogAuthorizationCheck(string userId, string resource, string action, bool granted)
    {
        var eventType = granted ? "AUTHZ_GRANTED" : "AUTHZ_DENIED";

        _logger.LogInformation("Authorization {EventType} - User: {UserId}, Resource: {Resource}, Action: {Action}",
            eventType, userId, resource, action);
    }
}
```

## 🔧 Configuration Options

### Basic Configuration

```json
{
  "Kundera": {
    "BaseUrl": "https://auth.company.com",
    "Kundera_Service_Secret": "your-jwt-secret",
    "Arch_Service_Secret": "gateway-specific-secret",
    "TokenExpirationHours": 8,
    "RefreshTokenExpirationDays": 30
  }
}
```

### Advanced Configuration

```csharp
options.AddAuthorization(authorizationOptions =>
    authorizationOptions.UseKundera(kunderaOptions => {
        // Token validation
        kunderaOptions.TokenValidationParameters = new() {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://auth.company.com",
            ValidAudience = "arch-gateway"
        };

        // Caching
        kunderaOptions.EnableTokenCaching = true;
        kunderaOptions.CacheExpiration = TimeSpan.FromMinutes(30);

        // Security
        kunderaOptions.EnableReplayAttackPrevention = true;
        kunderaOptions.MaxFailedAttempts = 5;
        kunderaOptions.LockoutDuration = TimeSpan.FromMinutes(15);

        // Monitoring
        kunderaOptions.EnableMetrics = true;
        kunderaOptions.MetricsPrefix = "arch_auth";
    }));
```

## 📊 Monitoring & Metrics

### Prometheus Metrics

```csharp
// Authentication metrics
private readonly Counter _authAttempts = Metrics.CreateCounter(
    "arch_auth_attempts_total",
    "Total authentication attempts",
    new[] { "result" });

private readonly Histogram _authDuration = Metrics.CreateHistogram(
    "arch_auth_duration_seconds",
    "Authentication duration in seconds");

private readonly Gauge _activeSessions = Metrics.CreateGauge(
    "arch_auth_active_sessions",
    "Number of active authenticated sessions");
```

### Health Checks

```csharp
public class AuthorizationHealthCheck : IHealthCheck
{
    private readonly IAuthorizationService _authService;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // Test token validation
            var testToken = GenerateTestToken();
            var result = await _authService.ValidateTokenAsync(testToken, cancellationToken);

            return result.IsValid
                ? HealthCheckResult.Healthy("Authorization service is healthy")
                : HealthCheckResult.Unhealthy("Token validation failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Authorization service error", ex);
        }
    }
}
```

## 🧪 Testing

### Unit Tests

```csharp
public class AuthorizationMiddlewareTests
{
    [Fact]
    public async Task AuthorizeAsync_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var middleware = new KunderaAuthorizationMiddleware(options, httpClientFactory);
        var context = CreateHttpContextWithValidToken();

        // Act
        var result = await middleware.AuthorizeAsync(context, new(), CancellationToken.None);

        // Assert
        Assert.True(result.IsAuthorized);
        Assert.NotNull(result.User);
    }

    [Fact]
    public async Task AuthorizeAsync_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var middleware = new KunderaAuthorizationMiddleware(options, httpClientFactory);
        var context = CreateHttpContextWithInvalidToken();

        // Act
        var result = await middleware.AuthorizeAsync(context, new(), CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Contains("invalid", result.FailureReason);
    }
}
```

### Integration Tests

```csharp
public class AuthorizationIntegrationTests
{
    [Fact]
    public async Task EndToEnd_AuthenticatedRequest_Succeeds()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetValidTokenAsync();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/protected");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## 🚀 Performance Optimization

### Token Caching

```csharp
public class TokenCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenCache> _logger;

    public async ValueTask<TokenValidationResult> GetOrAddAsync(
        string token,
        Func<Task<TokenValidationResult>> factory,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"token:{token.GetHashCode()}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

            var result = await factory();

            if (!result.IsValid)
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            }

            return result;
        });
    }
}
```

### Concurrent Request Handling

```csharp
public class ConcurrentAuthorizationHandler
{
    private readonly SemaphoreSlim _semaphore = new(100); // Max 100 concurrent auth requests

    public async Task<AuthorizationResult> HandleAsync(AuthorizationRequest request)
    {
        await _semaphore.WaitAsync();

        try
        {
            return await ProcessAuthorizationAsync(request);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

## 🔄 Multi-Tenant Support

### Tenant-Aware Authorization

```csharp
public class MultiTenantAuthorizationMiddleware : IAuthorizationMiddleware
{
    public async ValueTask<AuthorizationResult> AuthorizeAsync(
        HttpContext context,
        AuthorizationExecutionOptions options,
        CancellationToken cancellationToken)
    {
        // Extract tenant from request
        var tenantId = ExtractTenantId(context.Request);

        // Load tenant-specific authorization rules
        var tenantRules = await LoadTenantAuthorizationRulesAsync(tenantId, cancellationToken);

        // Apply tenant-specific authorization
        return await ApplyTenantAuthorizationAsync(context, tenantRules, cancellationToken);
    }
}
```

## 📚 API Reference

### Authorization Decorators

```csharp
// Allow anonymous access
[AllowAnonymous]
public IActionResult PublicEndpoint() { }

// Require specific roles
[Authorize(Roles = "Admin,Manager")]
public IActionResult AdminEndpoint() { }

// Require permissions
[Permission("user.create", "user.read")]
public IActionResult UserEndpoint() { }

// Custom authorization policy
[Authorize(Policy = "Over18Only")]
public IActionResult AdultContent() { }
```

### Middleware Integration

```csharp
// Add to request pipeline
app.UseArch(options =>
{
    options.BeforeDispatching(dispatchingOptions =>
    {
        dispatchingOptions.UseAuthorization(executionOptions =>
            executionOptions.UseKundera(builder.Configuration));
    });
});
```

## 🛠️ Extending Authorization

### Custom Authorization Providers

```csharp
public class CustomAuthorizationMiddleware : IAuthorizationMiddleware
{
    public async ValueTask<AuthorizationResult> AuthorizeAsync(
        HttpContext context,
        AuthorizationExecutionOptions options,
        CancellationToken cancellationToken)
    {
        // Custom authorization logic
        var apiKey = context.Request.Headers["X-API-Key"];

        if (string.IsNullOrEmpty(apiKey))
            return AuthorizationResult.Unauthorized("Missing API key");

        var isValid = await ValidateApiKeyAsync(apiKey.ToString(), cancellationToken);

        return isValid
            ? AuthorizationResult.Success(CreatePrincipal(apiKey.ToString()))
            : AuthorizationResult.Unauthorized("Invalid API key");
    }
}
```

### Custom Permission Evaluators

```csharp
public class CustomPermissionEvaluator : IPermissionEvaluator
{
    public async ValueTask<bool> HasPermissionAsync(
        ClaimsPrincipal user,
        string permission,
        CancellationToken cancellationToken)
    {
        // Custom permission evaluation logic
        var userPermissions = await GetUserPermissionsAsync(user, cancellationToken);

        return EvaluatePermission(permission, userPermissions);
    }
}
```

## 📈 Scaling Considerations

### High Availability

- **Token Validation**: Distribute token validation across multiple instances
- **Session Management**: Use distributed cache for session storage
- **Audit Logging**: Async logging to prevent blocking operations

### Performance Tuning

```csharp
// Optimize for high throughput
options.AddAuthorization(authorizationOptions =>
    authorizationOptions.UseKundera(kunderaOptions => {
        kunderaOptions.EnableTokenCaching = true;
        kunderaOptions.CacheExpiration = TimeSpan.FromMinutes(30);
        kunderaOptions.MaxConcurrentRequests = 1000;
        kunderaOptions.EnableMetrics = false; // Disable in high-throughput scenarios
    }));
```

## 🎯 Best Practices

### Security

1. **Always validate tokens**: Never trust client-provided tokens
2. **Use HTTPS**: Encrypt all authentication communications
3. **Implement rate limiting**: Protect against brute force attacks
4. **Log security events**: Maintain comprehensive audit trails
5. **Rotate secrets**: Regularly update cryptographic keys

### Performance

1. **Cache token validation**: Reduce external API calls
2. **Use async operations**: Prevent blocking threads
3. **Implement circuit breakers**: Handle identity provider failures gracefully
4. **Monitor performance**: Track authorization latency and success rates

### Maintainability

1. **Separate concerns**: Keep authentication and authorization logic separate
2. **Use dependency injection**: Make components testable and replaceable
3. **Implement health checks**: Monitor authorization service health
4. **Document policies**: Clearly document authorization rules and policies

## 📚 Related Documentation

- [Setup Guide](setup.md) - Initial setup and configuration
- [Encryption](encryption.md) - Data encryption and security
- [Rate Limiting](rate-limiting.md) - Request throttling and protection
- [Monitoring](monitoring.md) - Observability and alerting

## 🤝 Contributing

See [Contributing Guide](../../CONTRIBUTING.md) for authorization module development.

---

<div align="center">
  <p>🔐 Enterprise-grade security for modern applications</p>
  <p>
    <a href="endpoint-graph.md">← Endpoint Graph</a> |
    <a href="../README.md">README</a> |
    <a href="encryption.md">Encryption →</a>
  </p>
</div>
