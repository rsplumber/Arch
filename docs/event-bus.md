# 📨 Event Bus - Distributed Messaging & Event-Driven Architecture

Arch's Event Bus provides **enterprise-grade distributed messaging** capabilities using the CAP (Cloud Application Platform) framework, ensuring reliable event delivery and distributed transaction support across microservices.

## 🏗️ Architecture

### CAP Framework Integration

```
Event Bus Layer
├── CAP Publisher/Subscriber          # Message publishing & consumption
├── Message Persistence               # Durable message storage
├── Transaction Coordination          # Distributed transaction support
├── Message Monitoring                # Real-time message tracking
└── Dead Letter Queues               # Failed message handling
```

### Core Components

#### `IEventBus` Interface

```csharp
public interface IEventBus
{
    ValueTask PublishAsync<T>(string name, T content, CancellationToken cancellationToken = default);
    ValueTask PublishAsync(string name, object content, CancellationToken cancellationToken = default);
    ValueTask SubscribeAsync<T>(string name, Func<T, Task> handler, CancellationToken cancellationToken = default);
    ValueTask UnsubscribeAsync(string name, CancellationToken cancellationToken = default);
}
```

#### CAP Implementation

```csharp
public class CapEventBus : IEventBus
{
    private readonly ICapPublisher _capPublisher;
    private readonly IServiceProvider _serviceProvider;

    public async ValueTask PublishAsync<T>(string name, T content, CancellationToken cancellationToken)
    {
        await _capPublisher.PublishAsync(name, content, cancellationToken: cancellationToken);
    }

    public async ValueTask SubscribeAsync<T>(string name, Func<T, Task> handler, CancellationToken cancellationToken)
    {
        // CAP handles subscription automatically via attributes
        // [CapSubscribe("event.name")]
        // public async Task HandleEvent(T content) => await handler(content);
    }
}
```

## 📨 Message Publishing Patterns

### Fire-and-Forget Publishing

```csharp
// Simple event publishing
await eventBus.PublishAsync("user.created", new UserCreatedEvent
{
    UserId = user.Id,
    Email = user.Email,
    Timestamp = DateTime.UtcNow
});
```

### Transactional Publishing

```csharp
// Publish within database transaction
using var transaction = await _dbContext.Database.BeginTransactionAsync(eventBus);

try
{
    // Business logic
    var user = await CreateUserAsync(request);

    // Publish event within transaction
    await eventBus.PublishAsync("user.created", new UserCreatedEvent { UserId = user.Id });

    await transaction.CommitAsync();
}
catch (Exception)
{
    await transaction.RollbackAsync();
    throw;
}
```

### Delayed Publishing

```csharp
// Publish with delay
await eventBus.PublishAsync("notification.reminder",
    new ReminderEvent { UserId = userId, Message = "Don't forget!" },
    delay: TimeSpan.FromHours(24));
```

## 🎯 Message Consumption Patterns

### Attribute-Based Subscription

```csharp
public class OrderEventHandler
{
    [CapSubscribe("order.created")]
    public async Task HandleOrderCreated(OrderCreatedEvent @event)
    {
        _logger.LogInformation("Processing order {OrderId}", @event.OrderId);

        // Process order logic
        await ProcessOrderAsync(@event.OrderId);
    }

    [CapSubscribe("order.cancelled")]
    public async Task HandleOrderCancelled(OrderCancelledEvent @event)
    {
        _logger.LogInformation("Cancelling order {OrderId}", @event.OrderId);

        // Cancel order logic
        await CancelOrderAsync(@event.OrderId);
    }
}
```

### Group-Based Consumption

```csharp
// Multiple consumers in same group (competing consumers)
[CapSubscribe("payment.processed", Group = "payment-handlers")]
public class PaymentHandlerA { /* ... */ }

[CapSubscribe("payment.processed", Group = "payment-handlers")]
public class PaymentHandlerB { /* ... */ }

// Different groups (pub-sub pattern)
[CapSubscribe("inventory.updated", Group = "warehouse-a")]
public class WarehouseAHandler { /* ... */ }

[CapSubscribe("inventory.updated", Group = "warehouse-b")]
public class WarehouseBHandler { /* ... */ }
```

## 🔄 Distributed Transactions

### Saga Pattern Support

```csharp
// Order processing saga
public class OrderSaga : ICapSubscribe
{
    [CapSubscribe("order.created")]
    public async Task HandleOrderCreated(OrderCreatedEvent @event)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Reserve inventory
            await _inventoryService.ReserveAsync(@event.OrderId, @event.Items);

            // Process payment
            var paymentResult = await _paymentService.ChargeAsync(@event.OrderId, @event.Total);

            if (paymentResult.Success)
            {
                // Publish success event
                await _capPublisher.PublishAsync("order.payment.succeeded",
                    new PaymentSucceededEvent { OrderId = @event.OrderId });

                await transaction.CommitAsync();
            }
            else
            {
                // Publish failure event
                await _capPublisher.PublishAsync("order.payment.failed",
                    new PaymentFailedEvent { OrderId = @event.OrderId });

                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await _capPublisher.PublishAsync("order.processing.failed",
                new ProcessingFailedEvent { OrderId = @event.OrderId, Error = ex.Message });
        }
    }
}
```

## ⚙️ Configuration Options

### Basic CAP Configuration

```csharp
builder.Services.AddArch(options =>
{
    options.ConfigureEventBus(busOptions => busOptions.UseCap(capOptions =>
    {
        // Message reliability
        capOptions.FailedRetryCount = 3;
        capOptions.FailedRetryInterval = 60; // seconds

        // Message expiration
        capOptions.SucceedMessageExpiredAfter = TimeSpan.FromHours(24);
        capOptions.FailedMessageExpiredAfter = TimeSpan.FromDays(7);

        // JSON serialization
        capOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        capOptions.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    }));
});
```

### Database Storage Configuration

```csharp
capOptions.UsePostgreSql(sqlOptions =>
{
    sqlOptions.ConnectionString = builder.Configuration.GetConnectionString("Default")!;
    sqlOptions.Schema = "event_store";
    sqlOptions.TableNamePrefix = "cap_";
});

// Or use SQL Server
capOptions.UseSqlServer(sqlOptions =>
{
    sqlOptions.ConnectionString = builder.Configuration.GetConnectionString("Default")!;
});

// Or use MongoDB
capOptions.UseMongoDB(mongoOptions =>
{
    mongoOptions.DatabaseConnection = "mongodb://localhost:27017";
    mongoOptions.DatabaseName = "eventstore";
});
```

### Message Broker Configuration

```csharp
// RabbitMQ (default)
capOptions.UseRabbitMQ(rabbitOptions =>
{
    rabbitOptions.HostName = "localhost";
    rabbitOptions.Port = 5672;
    rabbitOptions.UserName = "guest";
    rabbitOptions.Password = "guest";
    rabbitOptions.ExchangeName = "arch.events";
    rabbitOptions.QueueArguments = new Dictionary<string, object> {
        { "x-max-retries", 3 },
        { "x-message-ttl", 86400000 } // 24 hours
    };
});

// Or use Kafka
capOptions.UseKafka(kafkaOptions =>
{
    kafkaOptions.Servers = "localhost:9092";
    kafkaOptions.MainConfig = new Dictionary<string, string> {
        { "group.id", "arch-gateway" },
        { "auto.offset.reset", "earliest" }
    };
});
```

## 📊 Monitoring & Observability

### Message Dashboard

Access CAP dashboard at: `http://localhost:5229/cap-dashboard`

Features:
- Real-time message monitoring
- Failed message retry interface
- Message statistics and metrics
- Queue status visualization

### Metrics Collection

```csharp
// Prometheus metrics
private readonly Counter _messagesPublished = Metrics.CreateCounter(
    "arch_eventbus_messages_published_total",
    "Total messages published",
    new[] { "event_type" });

private readonly Counter _messagesProcessed = Metrics.CreateCounter(
    "arch_eventbus_messages_processed_total",
    "Total messages processed",
    new[] { "event_type", "result" });

private readonly Histogram _processingDuration = Metrics.CreateHistogram(
    "arch_eventbus_processing_duration_seconds",
    "Message processing duration",
    new[] { "event_type" });
```

### Health Checks

```csharp
public class EventBusHealthCheck : IHealthCheck
{
    private readonly ICapPublisher _capPublisher;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // Test message publishing
            await _capPublisher.PublishAsync("health.check",
                new HealthCheckEvent { Timestamp = DateTime.UtcNow },
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Event bus is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Event bus error", ex);
        }
    }
}
```

## 🔄 Message Patterns

### Event Sourcing

```csharp
public class EventSourcedAggregate
{
    private readonly List<DomainEvent> _changes = new();

    public Guid Id { get; private set; }
    public int Version { get; private set; }

    public void Apply(DomainEvent @event)
    {
        _changes.Add(@event);
        ApplyEvent(@event);
        Version++;
    }

    public async Task SaveAsync(IEventBus eventBus)
    {
        foreach (var @event in _changes)
        {
            await eventBus.PublishAsync(@event.GetType().Name.ToLower(), @event);
        }
        _changes.Clear();
    }
}
```

### CQRS with Events

```csharp
// Command side
public class CreateOrderCommandHandler
{
    public async Task Handle(CreateOrderCommand command, IEventBus eventBus)
    {
        var order = Order.Create(command);
        await _repository.SaveAsync(order);

        await eventBus.PublishAsync("order.created", new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Items = order.Items
        });
    }
}

// Query side
public class OrderCreatedEventHandler
{
    [CapSubscribe("order.created")]
    public async Task Handle(OrderCreatedEvent @event)
    {
        await _readModelRepository.UpdateOrderSummaryAsync(
            @event.OrderId,
            @event.CustomerId,
            @event.Items.Sum(i => i.Quantity));
    }
}
```

## 🧪 Testing

### Unit Tests

```csharp
public class EventBusTests
{
    [Fact]
    public async Task PublishAsync_CallsCapPublisher()
    {
        // Arrange
        var capPublisher = new Mock<ICapPublisher>();
        var eventBus = new CapEventBus(capPublisher.Object, null);

        var @event = new UserCreatedEvent { UserId = Guid.NewGuid() };

        // Act
        await eventBus.PublishAsync("user.created", @event);

        // Assert
        capPublisher.Verify(p => p.PublishAsync("user.created", @event,
            It.IsAny<IDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Integration Tests

```csharp
public class EventBusIntegrationTests : IClassFixture<TestEventBusFixture>
{
    private readonly TestEventBusFixture _fixture;

    public EventBusIntegrationTests(TestEventBusFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PublishAndConsume_MessageDeliveredSuccessfully()
    {
        // Arrange
        var eventBus = _fixture.GetEventBus();
        var receivedEvents = new List<TestEvent>();
        var resetEvent = new ManualResetEvent(false);

        await eventBus.SubscribeAsync("test.event", (TestEvent @event) =>
        {
            receivedEvents.Add(@event);
            resetEvent.Set();
            return Task.CompletedTask;
        });

        // Act
        await eventBus.PublishAsync("test.event", new TestEvent { Data = "test" });

        // Assert
        Assert.True(resetEvent.WaitOne(TimeSpan.FromSeconds(10)));
        Assert.Single(receivedEvents);
        Assert.Equal("test", receivedEvents[0].Data);
    }
}
```

## 🚀 Performance Optimization

### Batch Publishing

```csharp
public class BatchEventPublisher
{
    private readonly List<(string name, object content)> _batch = new();
    private readonly IEventBus _eventBus;

    public void AddToBatch(string name, object content)
    {
        _batch.Add((name, content));
    }

    public async Task PublishBatchAsync()
    {
        var tasks = _batch.Select(item =>
            _eventBus.PublishAsync(item.name, item.content));

        await Task.WhenAll(tasks);
        _batch.Clear();
    }
}
```

### Message Compression

```csharp
public class CompressedEventBus : IEventBus
{
    public async ValueTask PublishAsync(string name, object content, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(content);
        var compressed = await CompressAsync(json);

        await _innerEventBus.PublishAsync(name, new CompressedMessage
        {
            CompressedData = compressed,
            OriginalSize = json.Length
        }, cancellationToken);
    }
}
```

## 🔄 Multi-Region Deployment

### Cross-Region Event Routing

```csharp
public class MultiRegionEventBus : IEventBus
{
    private readonly IEventBus _localBus;
    private readonly IEventBus _remoteBus;

    public async ValueTask PublishAsync(string name, object content, CancellationToken cancellationToken)
    {
        // Publish to local region
        await _localBus.PublishAsync(name, content, cancellationToken);

        // Route to other regions if needed
        if (IsGlobalEvent(name))
        {
            await _remoteBus.PublishAsync(name, content, cancellationToken);
        }
    }
}
```

## 🎯 Use Cases

### Order Processing System

```csharp
public class OrderService
{
    [CapSubscribe("payment.succeeded")]
    public async Task HandlePaymentSucceeded(PaymentSucceededEvent @event)
    {
        var order = await _orderRepository.GetByIdAsync(@event.OrderId);

        order.Status = OrderStatus.Confirmed;
        await _orderRepository.UpdateAsync(order);

        await _eventBus.PublishAsync("order.confirmed", new OrderConfirmedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });
    }
}
```

### Inventory Management

```csharp
public class InventoryService
{
    [CapSubscribe("order.confirmed")]
    public async Task HandleOrderConfirmed(OrderConfirmedEvent @event)
    {
        foreach (var item in @event.Items)
        {
            await _inventoryRepository.DecreaseStockAsync(item.ProductId, item.Quantity);
        }

        await _eventBus.PublishAsync("inventory.updated", new InventoryUpdatedEvent
        {
            Items = @event.Items
        });
    }
}
```

### Notification System

```csharp
public class NotificationService
{
    [CapSubscribe("order.confirmed")]
    public async Task HandleOrderConfirmed(OrderConfirmedEvent @event)
    {
        var customer = await _customerRepository.GetByIdAsync(@event.CustomerId);

        await _emailService.SendAsync(customer.Email,
            "Order Confirmed",
            $"Your order {@event.OrderId} has been confirmed.");

        await _smsService.SendAsync(customer.Phone,
            $"Order {@event.OrderId} confirmed. Track at: {GetTrackingUrl(@event.OrderId)}");
    }
}
```

## 📚 Related Documentation

- [Setup Guide](setup.md) - Event bus configuration
- [Monitoring](monitoring.md) - Event bus observability
- [Performance Tuning](performance.md) - Event bus optimization
- [Troubleshooting](troubleshooting.md) - Common event bus issues

## 🤝 Contributing

See [Contributing Guide](../../CONTRIBUTING.md) for event bus development.

---

<div align="center">
  <p>📨 Reliable distributed messaging for modern applications</p>
  <p>
    <a href="encryption.md">← Encryption</a> |
    <a href="../README.md">README</a> |
    <a href="load-balancer.md">Load Balancer →</a>
  </p>
</div>
