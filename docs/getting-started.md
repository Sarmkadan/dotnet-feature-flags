# Getting Started Guide

Welcome to dotnet-feature-flags! This guide will help you get up and running with the feature flag engine in your .NET application.

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 10 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **SQL Server** (LocalDB, Express, or Standard)
  - For LocalDB: Typically installed with Visual Studio
  - For Express: [Download](https://www.microsoft.com/en-us/sql-server/sql-server-express)
- **Visual Studio 2024**, **VS Code**, or **JetBrains Rider**
- **git** for cloning the repository

## Installation Options

### Option 1: From GitHub (Recommended)

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/dotnet-feature-flags.git
cd dotnet-feature-flags

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build
```

### Option 2: NuGet Package (Coming Soon)

```bash
dotnet add package FeatureFlags
```

### Option 3: Docker

```bash
docker-compose up -d
```

The service will be available at `http://localhost:5000`.

## Configuration

### Database Setup

1. Update `appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FeatureFlagEngine;Integrated Security=true;"
  }
}
```

**Common connection strings:**

```json
// SQL Server LocalDB
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FeatureFlagEngine;Integrated Security=true;"

// SQL Server Express (local)
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=FeatureFlagEngine;Integrated Security=true;"

// SQL Server with authentication
"DefaultConnection": "Server=your-server;Database=FeatureFlagEngine;User Id=sa;Password=YourPassword;"

// Azure SQL Database
"DefaultConnection": "Server=your-server.database.windows.net;Database=FeatureFlagEngine;User Id=user@server;Password=YourPassword;"
```

2. Create the database:

```bash
dotnet ef database update
```

This will:
- Create the `FeatureFlagEngine` database
- Create all necessary tables
- Set up indexes and relationships

### Application Settings

Configuration is in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FeatureFlagEngine;..."
  },
  "FeatureFlags": {
    "EnableCache": true,
    "CacheDurationMinutes": 5,
    "AuditLogRetentionDays": 365,
    "EnableAuditLogging": true,
    "MaxRulesPerFlag": 100,
    "MaxConditionsPerRule": 50,
    "MaxVariantsPerFlag": 10,
    "LogEvaluationDetails": false,
    "DefaultRolloutPercentage": 50
  }
}
```

### Environment-Specific Configuration

Create `appsettings.{Environment}.json` files:

**appsettings.Development.json:**
```json
{
  "FeatureFlags": {
    "EnableCache": false,
    "LogEvaluationDetails": true
  }
}
```

**appsettings.Production.json:**
```json
{
  "FeatureFlags": {
    "CacheDurationMinutes": 10,
    "AuditLogRetentionDays": 730
  }
}
```

### Using Environment Variables

You can override settings with environment variables:

```bash
# Linux/Mac
export ConnectionStrings__DefaultConnection="Server=prod-db;..."
export FeatureFlags__CacheDurationMinutes=10

# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection="Server=prod-db;..."
$env:FeatureFlags__CacheDurationMinutes=10
```

## Running the Application

### Command Line

```bash
# Run with default settings
dotnet run

# Run in Development environment
dotnet run --launch-profile Development

# Run in Production environment
dotnet run --launch-profile Production
```

The API will be available at `http://localhost:5000`.

### Visual Studio

1. Open `dotnet-feature-flags.sln`
2. Right-click on the `FeatureFlags` project
3. Select "Set as Startup Project"
4. Press F5 or click "Run"

### VS Code

1. Install the C# extension
2. Open the project folder
3. Press Ctrl+F5 to run

## First Steps

### 1. Access the API

Open your browser or API client (Postman, Insomnia, curl):

```bash
curl http://localhost:5000/api/featureflag
```

You should see an empty array `[]` since no flags exist yet.

### 2. Create Your First Feature Flag

```bash
curl -X POST http://localhost:5000/api/featureflag \
  -H "Content-Type: application/json" \
  -d '{
    "key": "welcome-banner",
    "displayName": "Welcome Banner",
    "description": "New welcome banner for new users",
    "isEnabled": true,
    "rolloutType": 0,
    "percentageRollout": 100
  }'
```

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "key": "welcome-banner",
  "displayName": "Welcome Banner",
  "isEnabled": true,
  "createdDate": "2024-02-20T10:30:00Z"
}
```

### 3. Evaluate the Flag

```bash
curl -X POST http://localhost:5000/api/featureflag/evaluate \
  -H "Content-Type: application/json" \
  -d '{
    "featureFlagKey": "welcome-banner",
    "userId": "user123",
    "email": "user@example.com"
  }'
```

Response:
```json
{
  "success": true,
  "isEnabled": true,
  "evaluationTime": 2.5
}
```

## Integrating into Your Application

### 1. Register Services

In your `Program.cs`:

```csharp
using FeatureFlags.Configuration;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add FeatureFlags services
builder.Services.AddFeatureFlags(builder.Configuration);

// ... other services ...

var app = builder.Build();
app.Run();
```

### 2. Inject IFeatureFlagService

```csharp
public class CheckoutService
{
    private readonly IFeatureFlagService _featureFlagService;

    public CheckoutService(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public async Task<CheckoutResult> ProcessCheckoutAsync(Order order)
    {
        var userContext = new UserContext
        {
            UserId = order.UserId,
            Email = order.CustomerEmail,
            Tier = order.CustomerTier
        };

        // Check if new payment gateway is enabled
        bool useNewGateway = await _featureFlagService
            .IsEnabledAsync("new-payment-gateway", userContext);

        var processor = useNewGateway
            ? new NewPaymentProcessor()
            : new LegacyPaymentProcessor();

        return await processor.ProcessAsync(order);
    }
}
```

### 3. Use in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;

    public ProductController(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var userContext = new UserContext { UserId = User.Identity?.Name ?? "anonymous" };
        var useRecommendations = await _featureFlagService
            .IsEnabledAsync("product-recommendations", userContext);

        var product = await _productService.GetProductAsync(id);

        if (useRecommendations)
        {
            product.Recommendations = await _recommendationService
                .GetRecommendationsAsync(product.Id);
        }

        return Ok(product);
    }
}
```

## Testing with Postman

1. Download [Postman](https://www.postman.com/downloads/)
2. Import the collection from `docs/postman-collection.json`
3. Set the base URL: `http://localhost:5000`
4. Try different endpoints

Or create requests manually:

**Create Flag:**
- Method: POST
- URL: `http://localhost:5000/api/featureflag`
- Body (JSON):
```json
{
  "key": "test-feature",
  "displayName": "Test Feature",
  "isEnabled": true,
  "rolloutType": 0,
  "percentageRollout": 50
}
```

**Evaluate Flag:**
- Method: POST
- URL: `http://localhost:5000/api/featureflag/evaluate`
- Body (JSON):
```json
{
  "featureFlagKey": "test-feature",
  "userId": "test-user",
  "email": "test@example.com"
}
```

## Next Steps

- **[API Reference](./api-reference.md)** - Complete API documentation
- **[Deployment Guide](./deployment.md)** - Deploy to production
- **[FAQ](./faq.md)** - Common questions and answers
- **[Examples](../examples/)** - Code examples for different scenarios

## Troubleshooting

### Connection String Issues

**Error:** "Cannot connect to database"

```bash
# Test connection with sqlcmd (SQL Server tools)
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"
```

**Solution:** Verify SQL Server is running and connection string is correct.

### Database Migration Errors

**Error:** "Cannot migrate database"

```bash
# Remove LocalDB instance and recreate
sqllocaldb delete mssqllocaldb
sqllocaldb create mssqllocaldb
sqllocaldb start mssqllocaldb
```

### Port Already in Use

**Error:** "Address already in use"

Change the port in `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5001"
      }
    }
  }
}
```

## Support

- **Issues**: GitHub Issues on the repository
- **Discussions**: GitHub Discussions
- **Email**: See README.md for contact information
