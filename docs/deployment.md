# Deployment Guide

This guide covers deploying dotnet-feature-flags to various environments (Development, Staging, Production).

## Prerequisites

- .NET 10 Runtime
- SQL Server instance (for database)
- Docker & Docker Compose (optional, for containerized deployment)
- IIS/Apache/Nginx (for web hosting, optional)

## Deployment Strategies

### Strategy 1: Self-Hosted (Recommended)

Deploy as a standalone .NET application on your own infrastructure.

#### Prerequisites

- Server with .NET 10 Runtime installed
- SQL Server instance (local or remote)
- Reverse proxy (Nginx/IIS) optional but recommended

#### Steps

1. **Build for production:**

```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

2. **Copy to server:**

```bash
scp -r ./publish user@server:/app/featureflags
```

3. **Configure on server:**

```bash
cd /app/featureflags
# Update appsettings.Production.json with production connection string
nano appsettings.Production.json
```

4. **Run the application:**

```bash
# Using systemd (Linux/macOS)
sudo systemctl start featureflags

# Or manually
dotnet FeatureFlags.dll --environment Production
```

#### Systemd Service (Linux)

Create `/etc/systemd/system/featureflags.service`:

```ini
[Unit]
Description=Feature Flags API
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/app/featureflags
ExecStart=/usr/bin/dotnet FeatureFlags.dll --environment Production
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable featureflags
sudo systemctl start featureflags
sudo systemctl status featureflags
```

---

### Strategy 2: Docker Deployment

#### Using Docker Compose (Recommended for Local/Dev)

1. **Build the image:**

```bash
docker build -t feature-flags:latest .
```

2. **Run with docker-compose:**

```bash
docker-compose up -d
```

The service will start with SQL Server on the same network.

#### Using Docker on Kubernetes

1. **Build and push image:**

```bash
docker build -t registry.example.com/feature-flags:v1.0.0 .
docker push registry.example.com/feature-flags:v1.0.0
```

2. **Deploy Kubernetes manifest:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: feature-flags
spec:
  replicas: 2
  selector:
    matchLabels:
      app: feature-flags
  template:
    metadata:
      labels:
        app: feature-flags
    spec:
      containers:
      - name: api
        image: registry.example.com/feature-flags:v1.0.0
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "500m"
          limits:
            memory: "512Mi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: feature-flags-service
spec:
  selector:
    app: feature-flags
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
```

Deploy:

```bash
kubectl apply -f deployment.yaml
```

---

### Strategy 3: Azure App Service

1. **Create App Service:**

```bash
az appservice plan create \
  --name featureflags-plan \
  --resource-group mygroup \
  --sku B2

az webapp create \
  --resource-group mygroup \
  --plan featureflags-plan \
  --name feature-flags-api
```

2. **Configure SQL Database:**

```bash
az sql server create \
  --resource-group mygroup \
  --name flagsserver \
  --admin-user sqladmin

az sql db create \
  --resource-group mygroup \
  --server flagsserver \
  --name FeatureFlagEngine
```

3. **Deploy from GitHub:**

```bash
az webapp deployment github-actions add \
  --resource-group mygroup \
  --name feature-flags-api \
  --repo Sarmkadan/dotnet-feature-flags
```

4. **Add Application Settings:**

```bash
az webapp config appsettings set \
  --resource-group mygroup \
  --name feature-flags-api \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    "ConnectionStrings__DefaultConnection=Server=tcp:flagsserver.database.windows.net,1433;Initial Catalog=FeatureFlagEngine;Persist Security Info=False;User ID=sqladmin;Password=YourPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

---

### Strategy 4: AWS EC2

1. **Launch EC2 Instance:**

```bash
aws ec2 run-instances \
  --image-id ami-0c55b159cbfafe1f0 \
  --instance-type t3.medium \
  --key-name your-key \
  --security-groups default
```

2. **Install .NET Runtime:**

```bash
ssh -i your-key.pem ec2-user@instance-ip

# Amazon Linux 2
sudo yum update
sudo yum install dotnet-runtime-10.0

# Install SQL Server client tools (optional)
sudo yum install mssql-tools
```

3. **Deploy application and configure SQL Server (RDS or self-hosted)**

---

## Configuration for Production

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql-server;Database=FeatureFlagEngine;User Id=sa;Password=StrongPassword123!;Encrypt=true;TrustServerCertificate=false;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "FeatureFlags": {
    "EnableCache": true,
    "CacheDurationMinutes": 10,
    "AuditLogRetentionDays": 730,
    "EnableAuditLogging": true,
    "MaxRulesPerFlag": 100,
    "MaxConditionsPerRule": 50,
    "MaxVariantsPerFlag": 10,
    "LogEvaluationDetails": false,
    "DefaultRolloutPercentage": 50
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:80"
      }
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Variables (Production)

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:80
ConnectionStrings__DefaultConnection="Server=prod-db;Database=FeatureFlagEngine;..."
FeatureFlags__CacheDurationMinutes=10
FeatureFlags__EnableCache=true
FeatureFlags__AuditLogRetentionDays=730
```

---

## Database Migration

### Pre-Deployment

Before deploying a new version, backup your database:

```bash
# SQL Server backup
BACKUP DATABASE [FeatureFlagEngine] 
TO DISK = N'D:\Backups\FeatureFlagEngine.bak'
WITH COMPRESSION;
```

### Migration Steps

1. **Test migrations locally:**

```bash
dotnet ef database update --configuration Release
```

2. **On production server:**

```bash
cd /app/featureflags
dotnet ef database update --environment Production
```

3. **Rollback if needed:**

```bash
# List migrations
dotnet ef migrations list

# Revert to previous migration
dotnet ef database update PreviousMigration
```

---

## Performance Optimization

### Database Optimization

1. **Enable indexes:**

```sql
-- Already created by Entity Framework migrations
-- Verify indexes exist:
SELECT * FROM sys.indexes 
WHERE object_id = OBJECT_ID('FeatureFlags')
```

2. **Monitor query performance:**

```sql
-- Find slow queries
SELECT * FROM sys.dm_exec_query_stats
WHERE total_elapsed_time > 1000000
ORDER BY total_elapsed_time DESC
```

### Application Optimization

1. **Enable caching:**

```json
{
  "FeatureFlags": {
    "EnableCache": true,
    "CacheDurationMinutes": 10
  }
}
```

2. **Configure connection pooling:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...;Max Pool Size=100;Min Pool Size=5;..."
  }
}
```

3. **Set appropriate timeouts:**

```json
{
  "Kestrel": {
    "Limits": {
      "RequestHeadersTimeout": "00:00:30",
      "KeepAliveTimeout": "00:00:10"
    }
  }
}
```

---

## Monitoring & Logging

### Application Insights (Azure)

```csharp
// In Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
```

### Serilog Integration

```csharp
// In Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/featureflags-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Health Checks

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FeatureFlagDbContext>();

app.MapHealthChecks("/health");
```

---

## Security Checklist

- [ ] Database connection uses SSL/TLS
- [ ] SQL Server authentication with strong password
- [ ] API authentication enabled (Bearer tokens/OAuth)
- [ ] CORS configured properly
- [ ] HTTPS enforced
- [ ] API rate limiting enabled
- [ ] Input validation on all endpoints
- [ ] Sensitive data not logged
- [ ] Regular security updates applied
- [ ] Secrets stored in Azure Key Vault / AWS Secrets Manager
- [ ] Database backups encrypted
- [ ] WAF (Web Application Firewall) enabled if using cloud

---

## Secrets Management

### Using Azure Key Vault

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("KEYVAULT_ENDPOINT")!);
var credentials = new DefaultAzureCredential();

builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, credentials);
```

### Using AWS Secrets Manager

```csharp
var secretsConfig = builder
    .Configuration
    .AddSecretsManager(region: "us-east-1");
```

### Using Environment-Specific Secrets

```bash
# Production
export DB_PASSWORD="secure-password-from-vault"
export API_KEY="secure-key-from-vault"

# The app will load these automatically
```

---

## Scaling Considerations

### Horizontal Scaling

With multiple instances:

1. Use a load balancer (ALB, NLB, Nginx)
2. Configure sticky sessions if needed
3. Use shared database (single instance or replication)
4. Enable caching to reduce database load

### Vertical Scaling

For a single instance:

1. Increase server resources (CPU, RAM)
2. Enable caching
3. Optimize database indexes
4. Connection pooling

---

## Rollback Strategy

If deployment fails:

```bash
# Using Blue-Green deployment
# Keep previous version running as "green"
# New version is "blue"
# If blue fails, traffic switches back to green

# Using Docker:
docker service rollback featureflags

# Using Kubernetes:
kubectl rollout undo deployment/feature-flags
```

---

## Troubleshooting Deployments

### Application won't start

```bash
# Check logs
dotnet FeatureFlags.dll --environment Production 2>&1 | tail -50

# Verify environment variables
env | grep -E "(ConnectionStrings|FeatureFlags)"

# Test database connection
sqlcmd -S server -U user -P password -Q "SELECT @@VERSION"
```

### Database migration fails

```bash
# Check migration status
dotnet ef migrations list --environment Production

# Apply specific migration
dotnet ef database update {MigrationName} --environment Production

# Reset (DANGER - destroys data)
dotnet ef database drop --force --environment Production
dotnet ef database update --environment Production
```

### High CPU/Memory usage

```bash
# Check running processes
ps aux | grep dotnet

# Monitor in real-time
watch -n 1 'ps aux | grep dotnet'

# Check log level (should not be Debug in production)
grep "LogLevel" appsettings.Production.json
```

---

## Maintenance Tasks

### Regular Backups

```bash
# Daily backup to object storage
BACKUP DATABASE [FeatureFlagEngine] 
TO URL = 'https://storage.example.com/backups/featureflags.bak'
WITH CREDENTIAL = 'storage-cred', COMPRESSION;
```

### Audit Log Cleanup

```bash
# Monthly cleanup
POST /admin/cleanup-audit-logs?retentionDays=365
```

### Update .NET Runtime

```bash
# Check current version
dotnet --version

# Update runtime
sudo apt-get install dotnet-runtime-10.0

# Restart application
sudo systemctl restart featureflags
```

---

For more information, see:
- [Getting Started](./getting-started.md)
- [API Reference](./api-reference.md)
- [README](../README.md)
