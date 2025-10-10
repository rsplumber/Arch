# 🚀 Arch Setup Guide

This guide will walk you through setting up Arch from scratch, including prerequisites, installation, and basic configuration.

## 📋 Prerequisites

### System Requirements

- **Operating System**: Linux, macOS, or Windows 10+
- **CPU**: 2+ cores recommended (1 core minimum)
- **RAM**: 2GB+ recommended (512MB minimum)
- **Storage**: 1GB+ free space

### Required Software

#### Core Dependencies
- **.NET 9.0 SDK** - [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Git** - Version control system

#### Database Options (Choose One)
- **PostgreSQL 15+** - [Download](https://www.postgresql.org/download/)
- **MySQL 8.0+** - [Download](https://dev.mysql.com/downloads/mysql/)
- **SQL Server 2022+** - [Download](https://www.microsoft.com/sql-server)

#### Message Broker (Required)
- **RabbitMQ 3.12+** - [Download](https://www.rabbitmq.com/download.html)

#### Optional Monitoring
- **Elasticsearch 8.x** - [Download](https://www.elastic.co/downloads/elasticsearch)
- **Kibana** - [Download](https://www.elastic.co/downloads/kibana)

## 🛠️ Installation

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/arch.git
cd arch
```

### 2. Install .NET Dependencies

```bash
# Restore NuGet packages
dotnet restore

# Verify .NET installation
dotnet --version
# Should output: 9.0.x
```

### 3. Set Up Infrastructure

#### Option A: Docker Compose (Recommended)

```bash
# Start all dependencies
docker-compose up -d

# Verify services are running
docker ps
```

#### Option B: Manual Setup

**PostgreSQL:**
```bash
# Create database
createdb arch

# Set up user (Linux/macOS)
psql -c "CREATE USER arch_user WITH PASSWORD 'secure_password';"
psql -c "GRANT ALL PRIVILEGES ON DATABASE arch TO arch_user;"
```

**RabbitMQ:**
```bash
# Enable management plugin
rabbitmq-plugins enable rabbitmq_management

# Create user
rabbitmqctl add_user arch_gateway secure_password
rabbitmqctl set_permissions -p / arch_gateway ".*" ".*" ".*"
```

## ⚙️ Configuration

### Basic Configuration

Create `appsettings.json` in the `Application` directory:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=arch;Username=arch_user;Password=secure_password"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "arch_gateway",
    "Password": "secure_password",
    "ExchangeName": "arch.events"
  }
}
```

### Environment-Specific Configuration

Create `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Arch": "Debug"
    }
  }
}
```

## 🏃‍♂️ Running Arch

### Development Mode

```bash
# Navigate to application directory
cd Application

# Run with hot reload
dotnet run

# Or with specific environment
dotnet run --environment Development
```

### Production Build

```bash
# Build for production
dotnet publish -c Release -o ./publish

# Run published application
cd publish
./Application
```

## 🧪 Testing Setup

### Unit Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test Arch.Tests.Unit/
```

### Integration Tests

```bash
# Run integration tests (requires infrastructure)
dotnet test --filter Category=Integration
```

## 🔍 Health Checks

Once running, verify Arch is healthy:

```bash
# Basic health check
curl http://localhost:5229/health

# Detailed health check
curl http://localhost:5229/health/detailed
```

Expected response:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    {
      "name": "message-broker",
      "status": "Healthy",
      "duration": "00:00:00.0034567"
    }
  ]
}
```

## 📊 Monitoring Setup

### Elastic APM (Optional)

1. **Install Elasticsearch and Kibana**
```bash
# Using Docker
docker run -d --name elasticsearch -p 9200:9200 -p 9300:9300 elasticsearch:8.11.0
docker run -d --name kibana -p 5601:5601 kibana:8.11.0
```

2. **Configure APM in appsettings.json**
```json
{
  "ElasticApm": {
    "ServiceName": "Arch.Gateway",
    "ServerUrl": "http://localhost:8200",
    "TransactionSampleRate": 0.1
  }
}
```

3. **Access Kibana**
- URL: http://localhost:5601
- Create APM index pattern: `apm-*`

### CAP Dashboard

Access the CAP message dashboard at:
- URL: `http://localhost:5229/cap-dashboard`
- Monitor message processing, retries, and failures

## 🐳 Docker Deployment

### Production Docker Image

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5229

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Application.dll"]
```

### Docker Compose (Full Stack)

```yaml
version: '3.8'
services:
  arch-gateway:
    build: .
    ports:
      - "5229:5229"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - postgres
      - rabbitmq
    networks:
      - arch-network

  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: arch
      POSTGRES_USER: arch_user
      POSTGRES_PASSWORD: secure_password
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - arch-network

  rabbitmq:
    image: rabbitmq:3-management
    environment:
      RABBITMQ_DEFAULT_USER: arch_gateway
      RABBITMQ_DEFAULT_PASS: secure_password
    ports:
      - "15672:15672"
    networks:
      - arch-network

networks:
  arch-network:
    driver: bridge

volumes:
  postgres_data:
```

## 🚀 Scaling Configuration

### Horizontal Scaling

```json
{
  "Arch": {
    "InstanceId": "gateway-pod-01",
    "Clustering": {
      "Enabled": true,
      "DiscoveryUrl": "http://consul:8500",
      "HealthCheckInterval": "00:00:30"
    }
  }
}
```

### Load Balancing

```json
{
  "LoadBalancer": {
    "Algorithm": "WeightedRoundRobin",
    "HealthCheck": {
      "Enabled": true,
      "Interval": "00:00:05",
      "Timeout": "00:00:02",
      "UnhealthyThreshold": 3
    }
  }
}
```

## 🔧 Troubleshooting

### Common Issues

#### Database Connection Issues
```bash
# Test database connection
psql "Host=localhost;Database=arch;Username=arch_user;Password=password"
```

#### RabbitMQ Connection Issues
```bash
# Check RabbitMQ status
rabbitmqctl status

# View logs
docker logs rabbitmq
```

#### Port Conflicts
```bash
# Find process using port
lsof -i :5229

# Kill process
kill -9 <PID>
```

### Logs and Debugging

#### Enable Debug Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Arch": "Debug"
    },
    "Console": {
      "FormatterName": "json"
    }
  }
}
```

#### View Application Logs
```bash
# Docker logs
docker logs arch-gateway

# File logs (if configured)
tail -f /app/logs/arch.log
```

## 📞 Getting Help

- **Documentation**: [Arch Docs](https://arch-gateway.dev/docs)
- **Issues**: [GitHub Issues](https://github.com/your-org/arch/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/arch/discussions)
- **Slack**: [Arch Community](https://arch-gateway.slack.com)

## 🎯 Next Steps

Now that you have Arch running, explore:

1. **[API Gateway Configuration](docs/api-gateway.md)** - Set up service discovery and routing
2. **[Security Setup](docs/security.md)** - Configure authentication and encryption
3. **[Monitoring](docs/monitoring.md)** - Set up observability and alerting
4. **[Production Deployment](docs/production.md)** - Scale and deploy to production

---

<div align="center">
  <p>🎉 Happy coding with Arch!</p>
  <p>
    <a href="../README.md">← Back to README</a> |
    <a href="library-development.md">Library Development →</a>
  </p>
</div>
