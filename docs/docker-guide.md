# Docker Guide for dotnet-feature-flags

This guide provides comprehensive instructions for deploying and managing dotnet-feature-flags using Docker and Docker Compose.

## Table of Contents

- [Quick Start](#quick-start)
- [Docker Compose Usage](#docker-compose-usage)
- [Environment Variables Reference](#environment-variables-reference)
- [Production Deployment Checklist](#production-deployment-checklist)
- [Advanced Configuration](#advanced-configuration)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

## Quick Start

### Prerequisites

- Docker Engine 20.10+ or Docker Desktop
- Docker Compose v2+
- 2GB RAM minimum (4GB recommended)

### Run with Docker Compose (Recommended)

```bash
# Clone repository
git clone https://github.com/sarmkadan/dotnet-feature-flags.git
cd dotnet-feature-flags

# Start services
docker-compose up -d
```

### Access the Application

- **API**: http://localhost:8080
- **Health Check**: http://localhost:8080/health
- **Swagger UI**: http://localhost:8080/swagger
- **SQL Server**: localhost:1433 (credentials in docker-compose.yml)

### Stop Services

```bash
docker-compose down
```

## Docker Compose Usage

### Basic Commands

```bash
# Start in detached mode
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down

# Rebuild images
docker-compose build --no-cache

# Update images
docker-compose pull

# View running containers
docker-compose ps
```

### Development vs Production

**Development (with hot reload):**
```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

**Production:**
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Scaling

```bash
# Scale API instances
docker-compose up -d --scale api=3

# Verify
docker-compose ps
```

### Database Operations

```bash
# Connect to SQL Server
sqlcmd -S localhost,1433 -U sa -P YourStrong!Passw0rd -d FeatureFlagEngine

# List databases
SELECT name FROM sys.databases;
```

## Environment Variables Reference

### Core Configuration

| Variable | Default Value | Description |
|----------|---------------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Application environment |
| `ASPNETCORE_URLS` | `http://+:8080` | Server binding URL |
| `DOTNET_EnableDiagnostics` | `0` | Disable diagnostics for smaller footprint |

### Database Configuration

| Variable | Default Value | Description |
|----------|---------------|-------------|
| `SA_PASSWORD` | `YourStrong!Passw0rd` | SQL Server SA password |
| `ACCEPT_EULA` | `Y` | SQL Server EULA acceptance |
| `MSSQL_PID` | `Developer` | SQL Server edition |
| `MSSQL_MEMORY_LIMIT_MB` | `1024` | Memory limit for SQL Server |

### Feature Flags Configuration

| Variable | Default Value | Description |
|----------|---------------|-------------|
| `FeatureFlags__EnableCache` | `true` | Enable in-memory caching |
| `FeatureFlags__CacheDurationMinutes` | `5` | Cache duration in minutes |
| `FeatureFlags__EnableAuditLogging` | `true` | Enable audit logging |
| `FeatureFlags__AuditLogRetentionDays` | `365` | Audit log retention days |
| `FeatureFlags__EnableExperimentationMetrics` | `false` | Enable A/B test metrics |
| `FeatureFlags__MetricsRetentionDays` | `90` | Metrics retention days |
| `FeatureFlags__ConsistentHashSeed` | `42` | Consistent hashing seed |

### Connection Strings

| Variable | Default Value | Description |
|----------|---------------|-------------|
| `ConnectionStrings__DefaultConnection` | `Server=sql-server;Database=FeatureFlagEngine;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;` | Database connection string |

### Logging Configuration

| Variable | Default Value | Description |
|----------|---------------|-------------|
| `Logging__LogLevel__Default` | `Information` | Default log level |
| `Logging__LogLevel__Microsoft` | `Warning` | Microsoft namespace log level |

### Example: Production Environment

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://+:8080
export FeatureFlags__EnableCache=true
export FeatureFlags__CacheDurationMinutes=10
export FeatureFlags__AuditLogRetentionDays=730

export ConnectionStrings__DefaultConnection="Server=prod-sql;Database=FeatureFlagEngine;User Id=sa;Password=ProdPass123!;TrustServerCertificate=false;Encrypt=true;"

docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Production Deployment Checklist

### ✅ Before Deployment

- [ ] Set strong passwords for SQL Server SA account
- [ ] Configure proper volume mounts for persistent data
- [ ] Set appropriate resource limits (CPU/memory)
- [ ] Configure health checks and readiness probes
- [ ] Set up proper logging configuration
- [ ] Configure backup strategy
- [ ] Set up monitoring and alerting

### ✅ Security Configuration

- [ ] Change default SQL Server SA password
- [ ] Enable HTTPS/TLS
- [ ] Configure CORS properly
- [ ] Set up authentication (if needed)
- [ ] Enable rate limiting
- [ ] Configure firewall rules
- [ ] Set resource limits to prevent DoS

### ✅ Performance Configuration
- [ ] Enable caching (`FeatureFlags__EnableCache=true`)
- [ ] Set appropriate cache duration
- [ ] Configure connection pooling in SQL Server
- [ ] Set resource limits (CPU/memory)
- [ ] Consider horizontal scaling
- [ ] Enable compression

### ✅ Monitoring & Operations
- [ ] Set up health checks
- [ ] Configure logging to persistent storage
- [ ] Set up backup automation
- [ ] Configure alerting for failures
- [ ] Set up log rotation
- [ ] Configure metrics collection

## Advanced Configuration

### Custom docker-compose.yml

```yaml
version: '3.9'

services:
  api:
    image: feature-flags:latest
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - FeatureFlags__EnableCache=true
      - FeatureFlags__CacheDurationMinutes=10
      - ConnectionStrings__DefaultConnection=Server=sql-server;Database=FeatureFlagEngine;User Id=sa;Password=StrongPass123!;TrustServerCertificate=false;Encrypt=true;
    depends_on:
      sql-server:
        condition: service_healthy
    networks:
      - featureflags-network
    restart: unless-stopped
    mem_limit: 512m
    cpus: 0.5
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=StrongPass123!
      - ACCEPT_EULA=Y
      - MSSQL_PID=Express
      - MSSQL_MEMORY_LIMIT_MB=1024
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql
      - ./docker/sql-setup.sql:/docker-entrypoint-initdb.d/setup.sql
    networks:
      - featureflags-network
    restart: unless-stopped
    mem_limit: 2g
    cpus: 1.0
    healthcheck:
      test: ["CMD", "sqlcmd", "-S", "localhost", "-U", "sa", "-P", "StrongPass123!", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 30s

volumes:
  sql-data:
  logs:
  app-data:

networks:
  featureflags-network:
    driver: bridge
```

### Custom Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Create non-root user
RUN adduser -D appuser && chown -R appuser:appuser /app
USER appuser

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FeatureFlags.dll"]
```

### Multi-Stage Build with Optimizations

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app \
    /p:PublishReadyToRun=true \
    /p:PublishSingleFile=false \
    /p:PublishTrimmed=true \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Optimizations
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV DOTNET_EnableDiagnostics=0

# Security
RUN chmod 755 /app/FeatureFlags

# Non-root user
RUN adduser -D appuser && chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "FeatureFlags.dll"]
```

### Custom SQL Server Setup

Create `docker/sql-setup.sql`:

```sql
-- Create database
CREATE DATABASE [FeatureFlagEngine];
GO

-- Create schema
USE [FeatureFlagEngine];
GO

-- Create tables (simplified, EF Core will create these)
CREATE TABLE [FeatureFlags] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [Key] NVARCHAR(100) NOT NULL,
    [DisplayName] NVARCHAR(200),
    [Description] NVARCHAR(1000),
    [IsEnabled] BIT DEFAULT 0,
    [RolloutType] INT DEFAULT 0,
    [PercentageRollout] INT DEFAULT 0,
    [CreatedDate] DATETIME2 DEFAULT GETUTCDATE(),
    [ModifiedDate] DATETIME2 DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(255)
);
GO

-- Create indexes
CREATE INDEX IX_FeatureFlags_Key ON [FeatureFlags]([Key]);
GO
```

### Custom Configuration File

Create `docker/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql-server;Database=FeatureFlagEngine;User Id=sa;Password=StrongPass123!;TrustServerCertificate=false;Encrypt=true;"
  },
  "FeatureFlags": {
    "EnableCache": true,
    "CacheDurationMinutes": 10,
    "AuditLogRetentionDays": 730,
    "EnableAuditLogging": true,
    "EnableExperimentationMetrics": true,
    "MetricsRetentionDays": 90,
    "ConsistentHashSeed": 42
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

Then mount it in docker-compose:

```yaml
volumes:
  - ./docker/appsettings.Production.json:/app/appsettings.Production.json
```

## Troubleshooting

### Container Won't Start

**Error**: `standard_init_linux.go:228: exec user process caused: permission denied`

**Solution**: Fix volume permissions:

```bash
sudo chown -R 100:101 ./logs
sudo chown -R 100:101 ./app-data
```

### Database Connection Issues

**Error**: `Login failed for user 'sa'`

**Solution**:
1. Check SQL Server is running: `docker-compose ps`
2. Verify credentials match in both services
3. Check SQL Server health: `docker-compose logs sql-server`
4. Wait for SQL Server to be ready:

```yaml
# In docker-compose.yml
sql-server:
  healthcheck:
    test: ["CMD", "sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourPassword", "-Q", "SELECT 1"]
    interval: 10s
    timeout: 5s
    retries: 10
    start_period: 30s
```

### Application Crashes on Startup

**Error**: `System.Data.SqlClient.SqlException: Cannot open database`

**Solution**:
1. Wait for database to be ready
2. Check database initialization:

```bash
docker-compose logs sql-server | grep "Database"
```

3. Verify database exists:

```bash
docker exec -it featureflags-api-sql-server-1 sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT name FROM sys.databases"
```

### High Memory Usage

**Issue**: SQL Server using too much memory

**Solution**: Limit memory:

```yaml
sql-server:
  mem_limit: 2g
  environment:
    - MSSQL_MEMORY_LIMIT_MB=2048
```

### Slow Performance

**Issue**: API responses taking too long

**Solution**:
1. Enable caching:
```yaml
FeatureFlags__EnableCache: "true"
FeatureFlags__CacheDurationMinutes: "5"
```

2. Increase SQL Server resources:
```yaml
sql-server:
  mem_limit: 4g
  cpus: 2.0
```

3. Check database indexes:

```bash
docker exec -it featureflags-api-sql-server-1 sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('FeatureFlags')"
```

### Logs Not Persisting

**Issue**: Logs disappear after container restart

**Solution**: Mount log volume:

```yaml
volumes:
  - ./logs:/app/logs
```

Then configure Serilog in `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("/app/logs/featureflags-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();
```

### Port Already in Use

**Error**: `port is already allocated`

**Solution**: Change port mapping:

```yaml
ports:
  - "8081:8080"  # Host:Container
```

Then update:
- Reverse proxy configuration
- Health check URLs
- Application URLs

### SSL/TLS Configuration

**Issue**: Need HTTPS

**Solution**: Use reverse proxy (recommended) or configure Kestrel:

```yaml
api:
  environment:
    - ASPNETCORE_Kestrel__Certificates__Default__Path=/app/cert.pfx
    - ASPNETCORE_Kestrel__Certificates__Default__Password=YourPassword
  volumes:
    - ./cert.pfx:/app/cert.pfx
```

## Best Practices

### Security Best Practices

1. **Never use default passwords** in production
   ```yaml
   SA_PASSWORD: "ChangeThisToStrongPassword123!"
   ```

2. **Enable encryption** for database connections
   ```yaml
   ConnectionStrings__DefaultConnection: "...;Encrypt=true;TrustServerCertificate=false;"
   ```

3. **Use secrets management** for sensitive data
   - Azure Key Vault
   - AWS Secrets Manager
   - HashiCorp Vault

4. **Restrict network access**
   ```yaml
   networks:
     featureflags-network:
       driver: bridge
       internal: true  # No external access
   ```

5. **Set resource limits** to prevent DoS
   ```yaml
   mem_limit: 512m
   cpus: 0.5
   ```

### Performance Best Practices

1. **Enable caching** for production
   ```yaml
   FeatureFlags__EnableCache: "true"
   FeatureFlags__CacheDurationMinutes: "10"
   ```

2. **Configure connection pooling**
   ```yaml
   ConnectionStrings__DefaultConnection: "...;Max Pool Size=100;Min Pool Size=10;"
   ```

3. **Set appropriate timeouts**
   ```yaml
   Kestrel:
     Limits:
       RequestHeadersTimeout: "00:00:30"
       KeepAliveTimeout: "00:00:10"
   ```

4. **Use health checks** for load balancers
   ```yaml
   healthcheck:
     test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
     interval: 30s
     timeout: 10s
     retries: 3
   ```

5. **Enable diagnostics only when needed**
   ```yaml
   DOTNET_EnableDiagnostics: "0"
   ```

### Operational Best Practices

1. **Use volumes for persistent data**
   ```yaml
   volumes:
     - sql-data:/var/opt/mssql
     - ./logs:/app/logs
   ```

2. **Configure log rotation**
   ```yaml
   logging:
     driver: "json-file"
     options:
       max-size: "10m"
       max-file: "3"
   ```

3. **Set up monitoring**
   - Prometheus metrics endpoint
   - Health check endpoints
   - Log aggregation

4. **Plan for backups**
   ```bash
   # Daily backup script
   docker exec featureflags-api-sql-server-1 \
     /opt/mssql-tools/bin/sqlcmd \
     -S localhost -U sa -P YourPassword \
     -Q "BACKUP DATABASE [FeatureFlagEngine] TO DISK = '/var/opt/mssql/backup.bak'"
   ```

5. **Use health checks** for container orchestration
   ```yaml
   healthcheck:
     test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
     interval: 30s
     timeout: 10s
     retries: 3
     start_period: 40s
   ```

### Development Best Practices

1. **Use hot reload** for faster development
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
   ```

2. **Mount source code** for live reloading
   ```yaml
   volumes:
     - ./src:/app/src
   ```

3. **Enable detailed logging**
   ```yaml
   Logging__LogLevel__Default: "Debug"
   ```

4. **Use environment-specific configs**
   ```bash
   ASPNETCORE_ENVIRONMENT: "Development"
   ```

5. **Test with realistic data**
   ```yaml
   volumes:
     - ./docker/test-data.sql:/docker-entrypoint-initdb.d/test-data.sql
   ```

## Integration with Reverse Proxies

### Nginx Configuration

```nginx
upstream featureflags {
    server featureflags-api:8080;
}

server {
    listen 80;
    server_name feature-flags.example.com;

    location / {
        proxy_pass http://featureflags;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /health {
        proxy_pass http://featureflags/health;
    }
}
```

### Traefik Configuration

```yaml
labels:
  - "traefik.enable=true"
  - "traefik.http.routers.featureflags.rule=Host(`feature-flags.example.com`)"
  - "traefik.http.routers.featureflags.entrypoints=websecure"
  - "traefik.http.routers.featureflags.tls=true"
  - "traefik.http.services.featureflags.loadbalancer.server.port=8080"
  - "traefik.http.routers.featureflags.middlewares=featureflags-headers"
  - "traefik.http.middlewares.featureflags-headers.headers.accesscontrolallowmethods=GET,POST,PUT,DELETE"
```

### Caddy Configuration

```Caddyfile
feature-flags.example.com {
    reverse_proxy featureflags-api:8080 {
        health_uri /health
        health_interval 30s
    }
}
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Docker Build and Push

on:
  push:
    branches: [ main ]

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Login to Docker Hub
        uses: docker/login-action@v2
        with:
          username: ${{ secrets.DOCKER_HUB_USERNAME }}
          password: ${{ secrets.DOCKER_HUB_TOKEN }}

      - name: Build and push
        uses: docker/build-push-action@v3
        with:
          context: .
          push: true
          tags: yourusername/feature-flags:latest,yourusername/feature-flags:${{ github.sha }}

      - name: Deploy to server
        run: |
          ssh user@server "cd /app/feature-flags && docker-compose pull && docker-compose up -d"
```

### Azure DevOps Pipeline

```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: Docker@2
  displayName: Build and push
  inputs:
    containerRegistry: 'docker-hub-connection'
    repository: 'yourusername/feature-flags'
    command: 'buildAndPush'
    Dockerfile: '**/Dockerfile'
    tags: |
      latest
      $(Build.BuildId)

- task: SSH@0
  displayName: Deploy
  inputs:
    sshEndpoint: 'production-server'
    runOptions: 'commands'
    commands: |
      cd /app/feature-flags
      docker-compose down
      docker-compose pull
      docker-compose up -d
```

---

For more information, see:
- [Getting Started](../getting-started.md)
- [API Reference](../api-reference.md)
- [Deployment Guide](../deployment.md)
- [README](../../README.md)
