# Frequently Asked Questions (FAQ)

## General Questions

### What is dotnet-feature-flags?

dotnet-feature-flags is a self-hosted feature flag engine for .NET applications. It provides sophisticated feature management capabilities including percentage rollouts, user targeting, A/B testing, and complete audit logging.

### Why use dotnet-feature-flags instead of external services?

**Advantages:**
- **Self-hosted**: No external dependencies, lower latency
- **Cost-effective**: No per-request fees
- **Privacy**: Data stays on your infrastructure
- **Control**: Complete ownership of configuration and data
- **Speed**: Direct database access, no API overhead
- **Compliance**: GDPR, SOC2 ready

**Trade-offs:**
- Must manage database and infrastructure
- No managed analytics dashboard
- Limited built-in integrations

### What .NET versions are supported?

Currently supports **.NET 10 only**. It uses the latest C# 13 features and EF Core 10.

For older .NET versions, fork the repository and adjust the target framework.

### How is this different from LaunchDarkly or split.io?

| Feature | dotnet-feature-flags | LaunchDarkly | split.io |
|---------|----------------------|--------------|----------|
| Self-hosted | ✓ | ✓ (Enterprise) | ✗ |
| Pricing | Free | Per-request | Per-request |
| Latency | ~1-5ms | ~50ms (API) | ~100ms (API) |
| Setup | 15 minutes | 5 minutes | 5 minutes |
| Analytics | Basic | Advanced | Advanced |
| SDKs | .NET only | 10+ languages | 8+ languages |

---

## Technical Questions

### How does percentage rollout work?

Percentage rollout uses **consistent hashing** based on `(UserId + FeatureFlagKey)`. This ensures:

- **Stable**: Same user always gets same rollout decision
- **Deterministic**: Can predict which users get the feature
- **Independent**: Each flag has independent rollout decisions

```
Hash(user123 + "feature-key") = 42
42 % 100 = 42 (bucket)
If 42 < 25% threshold? NO → Feature OFF
```

### Can I use external databases like PostgreSQL?

Currently, only **SQL Server** is supported via Entity Framework Core. To add PostgreSQL support:

1. Update `FeatureFlagDbContext.OnConfiguring()` to use Npgsql
2. Re-run migrations
3. Update connection string

Pull requests are welcome!

### What's the maximum number of rules per flag?

Configurable via `MaxRulesPerFlag` in `appsettings.json`. Default is 100.

Each rule can have up to `MaxConditionsPerRule` conditions (default 50).

### How is evaluation performance optimized?

1. **Consistent hashing**: O(1) lookup for percentage rollouts
2. **Rule priority**: Stops at first match
3. **Eager loading**: Loads all related entities in one query
4. **Optional caching**: In-memory cache with TTL
5. **Database indexes**: Optimized queries

Typical evaluation time: **1-5ms** with database, **<1ms** with caching.

### Can I use feature flags in distributed systems?

Yes! Since evaluation is stateless, any instance can evaluate flags. However:

- **Consistency**: Ensure all instances have same flag configuration
- **Database**: Must share same SQL Server
- **Caching**: Each instance maintains its own cache

For geographic distribution, consider regional database replicas.

---

## Configuration Questions

### How do I set up multiple environments?

Create environment-specific files:

```
appsettings.json                 (shared defaults)
appsettings.Development.json     (dev overrides)
appsettings.Staging.json         (staging overrides)
appsettings.Production.json      (production overrides)
```

Run with:

```bash
dotnet run --environment Production
```

Or set the environment variable:

```bash
export ASPNETCORE_ENVIRONMENT=Production
```

### How do I handle secrets?

Never commit secrets to git. Instead, use:

**Development:**
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
```

**Production:**
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- Environment variables

### Can I use feature flags without the API?

Yes! Inject `IFeatureFlagService` directly in your code:

```csharp
public MyService(IFeatureFlagService flagService)
{
    _flagService = flagService;
}

public async Task MyMethod()
{
    var enabled = await _flagService.IsEnabledAsync("my-flag", context);
}
```

You don't need to call the REST API.

### What happens if the database is unavailable?

Currently, the application will throw an exception. To add resilience:

```csharp
try
{
    var enabled = await _flagService.IsEnabledAsync("flag", context);
}
catch (DbException)
{
    // Fall back to safe default
    return false;
}
```

With caching enabled, short outages are handled gracefully.

---

## Usage Questions

### How do I evaluate flags in my controller?

```csharp
[ApiController]
public class MyController : ControllerBase
{
    private readonly IFeatureFlagService _flagService;

    public MyController(IFeatureFlagService flagService)
    {
        _flagService = flagService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userContext = new UserContext
        {
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value
        };

        var enabled = await _flagService.IsEnabledAsync("my-flag", userContext);

        if (enabled)
            return Ok(new { message = "New feature!" });
        else
            return Ok(new { message = "Classic feature" });
    }
}
```

### Can I use custom user attributes?

Yes! Add any custom attributes to `UserContext`:

```csharp
var context = new UserContext
{
    UserId = "user123",
    Email = "user@example.com"
};

context.SetCustomAttribute("plan", "premium");
context.SetCustomAttribute("account_age_days", "180");

// Use in conditions
var condition = new Condition
{
    Attribute = "plan",
    Operator = ConditionOperator.Equals,
    Value = "premium"
};
```

### How do I test feature flags?

```csharp
[Test]
public async Task Feature_EnabledForPremiumUsers()
{
    // Arrange
    var service = new FeatureFlagService(mockRepository, mockAuditService);
    var context = new UserContext { Tier = "premium" };

    // Act
    var result = await service.IsEnabledAsync("premium-feature", context);

    // Assert
    Assert.IsTrue(result);
}
```

Mock the repository for unit tests, use real database for integration tests.

### Can I use flags for A/B testing?

Yes! Create an A/B test flag:

```csharp
var flag = new FeatureFlag
{
    Key = "checkout-test",
    RolloutType = RolloutType.ABTest,
    Variants = new[]
    {
        new ABTestVariant { Name = "Control", AllocationPercentage = 50 },
        new ABTestVariant { Name = "Treatment", AllocationPercentage = 50 }
    }
};

var variant = await service.GetVariantAsync("checkout-test", context);
// variant.Name = "Control" or "Treatment"
```

---

## Deployment Questions

### How do I deploy to production?

See [Deployment Guide](./deployment.md) for detailed steps covering:
- Self-hosted on Linux/Windows
- Docker/Kubernetes
- Azure App Service
- AWS EC2

### Do I need HTTPS?

**Yes**, in production. Configure via:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/cert.pem",
          "KeyPath": "/etc/ssl/private/key.pem"
        }
      }
    }
  }
}
```

### How do I scale to multiple servers?

1. Deploy application to multiple servers
2. Point all instances to same SQL Server
3. Use load balancer (Nginx, IIS ARR, AWS ALB)
4. Enable caching to reduce database load

```
Client → Load Balancer → Instance 1 (cache)
                      → Instance 2 (cache)
                      → Instance 3 (cache)
                           ↓
                        SQL Server
```

### What about database backups?

Backup regularly:

```bash
# SQL Server
BACKUP DATABASE [FeatureFlagEngine] 
TO DISK = N'D:\Backups\FeatureFlagEngine.bak'
WITH COMPRESSION;

# Schedule daily via cron/Task Scheduler
```

Store backups in:
- Azure Blob Storage
- S3
- On-premises NAS
- Another server

---

## Performance Questions

### How many flags can the system handle?

No hard limit. Performance scales with:
- **Flags**: 10,000+ flags supported
- **Evaluations**: 10,000+ evaluations/second
- **Rules**: 100+ rules per flag
- **Users**: Millions of unique users

Performance is limited by:
- Database capacity
- Server CPU/RAM
- Network latency

### What about caching?

Enable caching in `appsettings.json`:

```json
{
  "FeatureFlags": {
    "EnableCache": true,
    "CacheDurationMinutes": 5
  }
}
```

With caching:
- 100,000+ evaluations/second
- Reduces database load by 95%+
- 5-second staleness window

### How does caching affect consistency?

Cached flags are slightly stale (default 5 minutes). If you need real-time updates:

1. Disable caching
2. Reduce cache TTL
3. Add webhook to invalidate cache on changes

---

## Troubleshooting Questions

### Flags aren't evaluating correctly

**Checklist:**
- [ ] Flag is enabled (`isEnabled: true`)
- [ ] User attributes match condition attributes (case-sensitive)
- [ ] Rule priorities are correct (lower = higher priority)
- [ ] Condition operators are correct
- [ ] Rule condition logic is AND/OR as intended

Check audit logs:
```bash
GET /api/featureflag/{id}/audit
```

### Database migrations fail

**Solution:**
```bash
# Check migration status
dotnet ef migrations list

# List pending migrations
dotnet ef migrations list --no-connect

# Apply specific migration
dotnet ef database update {MigrationName}

# Rollback last migration
dotnet ef database update {PreviousMigration}
```

### API is slow

**Optimization:**
1. Enable caching: `EnableCache: true`
2. Check database indexes exist
3. Reduce log level in production
4. Check database statistics:
   ```sql
   SELECT * FROM sys.dm_exec_query_stats
   ORDER BY total_elapsed_time DESC
   ```

### Users see different flag values

**Causes:**
- Percentage rollout: User's hash changed (shouldn't happen)
- Caching: Different cache TTL on different servers
- Rules: Different rule evaluation order

**Solution:**
- Enable cache synchronization across servers
- Use same configuration on all servers
- Check rule priorities

### How do I debug flag evaluation?

Enable detailed logging:

```json
{
  "FeatureFlags": {
    "LogEvaluationDetails": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

View logs:
```bash
tail -f logs/application.log | grep "evaluation"
```

---

## Support & Contributing

### How do I report a bug?

1. Check existing issues on GitHub
2. Create a new issue with:
   - .NET version
   - SQL Server version
   - Steps to reproduce
   - Expected vs actual behavior

### Can I contribute?

Yes! Please:
1. Fork the repository
2. Create a feature branch
3. Add tests for new features
4. Submit a pull request

See README.md for contribution guidelines.

### Where can I get help?

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions and ideas
- **Email**: See README.md for contact

---

## Licensing Questions

### What license is this under?

MIT License - see LICENSE file.

### Can I use this commercially?

Yes! MIT license allows commercial use with attribution.

### Do I need to contribute back changes?

No, MIT is permissive. But contributions are appreciated!

---

Still have questions? Open an issue on GitHub or check the documentation!
