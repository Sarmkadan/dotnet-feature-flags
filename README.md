# dotnet-feature-flags

A production-grade feature flag engine for .NET with support for percentage rollouts, user targeting, A/B testing, real-time toggles, and comprehensive audit logging.

## Features

### Core Capabilities
- **Percentage-Based Rollouts**: Gradually roll out features to a percentage of users with consistent hashing
- **User Targeting**: Define targeting rules with complex conditions (equals, contains, in-list, numeric comparisons)
- **A/B Testing**: Run A/B tests with multiple variants and allocation percentages
- **Real-Time Toggle**: Enable/disable feature flags instantly without redeployment
- **Audit Logging**: Complete audit trail of all feature flag changes for compliance

### Advanced Features
- **Rule-Based Evaluation**: Support for AND/OR logic across multiple conditions
- **Gradual Rollout**: Time-based gradual rollout with daily percentage increments
- **User Context**: Rich user context with standard and custom attributes
- **Search & Filtering**: Comprehensive search and filtering capabilities
- **Pagination**: Efficient paging for large result sets
- **Performance Metrics**: Track user assignments and conversion rates for A/B tests

## Architecture

### Core Components

#### Models
- **FeatureFlag**: Main feature flag entity with rollout configuration
- **Rule**: Targeting rules with conditions
- **Condition**: Individual condition for targeting logic
- **UserContext**: User attributes for evaluation
- **AuditLog**: Change history and audit trail
- **RolloutStrategy**: Rollout configuration and scheduling
- **ABTestVariant**: A/B test variant definition

#### Services
- **IFeatureFlagService**: Core feature flag operations and evaluation
- **IRuleEvaluationService**: Complex rule evaluation with AND/OR logic
- **IPercentageRolloutService**: Percentage-based rollout with consistent hashing
- **IAuditLogService**: Audit trail and change history

#### Repository Layer
- **IFeatureFlagRepository**: Feature flag persistence with advanced queries
- **IAuditLogRepository**: Audit log storage and retrieval
- **IRepository<T>**: Generic repository base interface

#### Data Access
- **FeatureFlagDbContext**: Entity Framework Core database context
- **EF Core**: SQL Server integration with automatic migrations

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or full edition)
- Visual Studio 2024 or VS Code

### Installation

1. Clone the repository
```bash
git clone https://github.com/yourusername/dotnet-feature-flags.git
cd dotnet-feature-flags
```

2. Install dependencies
```bash
dotnet restore
```

3. Configure database connection in `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FeatureFlagEngine;User Id=sa;Password=your_password;"
  }
}
```

4. Apply migrations
```bash
dotnet ef database update
```

5. Run the application
```bash
dotnet run
```

## API Usage

### Evaluate Feature Flag
```http
POST /api/featureflag/evaluate
Content-Type: application/json

{
  "featureFlagKey": "new-checkout-flow",
  "userId": "user123",
  "email": "user@example.com",
  "country": "US",
  "tier": "premium",
  "region": "north-america"
}
```

### Get A/B Test Variant
```http
POST /api/featureflag/variant
Content-Type: application/json

{
  "featureFlagKey": "checkout-redesign",
  "userId": "user123",
  "email": "user@example.com"
}
```

### Get Feature Flag by Key
```http
GET /api/featureflag/new-checkout-flow
```

### Create Feature Flag
```http
POST /api/featureflag
Content-Type: application/json

{
  "key": "new-checkout-flow",
  "displayName": "New Checkout Flow",
  "description": "Redesigned checkout experience",
  "isEnabled": false,
  "rolloutType": 0,
  "percentageRollout": 25
}
```

### Enable/Disable Feature Flag
```http
POST /api/featureflag/{id}/enable
POST /api/featureflag/{id}/disable
```

### Get Audit Logs
```http
GET /api/featureflag/{id}/audit
```

## Configuration

### appsettings.json
```json
{
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

## Rollout Types

- **Percentage**: Gradual rollout to a percentage of users
- **RulesBased**: Rollout based on targeting rules
- **ABTest**: A/B testing with variants
- **Full**: 100% rollout to all users
- **None**: 0% rollout (disabled)

## Condition Operators

- **Equals**: Exact string match (case-insensitive)
- **NotEquals**: Not equal comparison
- **Contains**: String contains check
- **StartsWith**: String starts with
- **EndsWith**: String ends with
- **GreaterThan**: Numeric comparison
- **LessThan**: Numeric comparison
- **In**: Value in comma-separated list

## Audit Actions

All changes to feature flags are logged:
- Created
- Enabled/Disabled
- Updated
- RolloutChanged
- RuleAdded/Removed
- VariantUpdated
- Deleted

## Performance Considerations

- **Consistent Hashing**: Uses deterministic hashing for stable percentage rollouts
- **Eager Loading**: Includes related entities to reduce database queries
- **Pagination**: Supports efficient paging for large datasets
- **Caching**: Optional caching layer for frequently accessed flags

## Development

### Running Tests
```bash
dotnet test
```

### Building
```bash
dotnet build
```

### Publishing
```bash
dotnet publish -c Release
```

## Roadmap

- Redis caching integration
- Multi-tenant support
- Webhook notifications for flag changes
- Advanced analytics and insights dashboard
- Native SDKs for popular frameworks

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Vladyslav Zaiets**
- Portfolio: https://sarmkadan.com
- Email: rutova2@gmail.com

## Support

For issues, questions, or suggestions, please open an issue on GitHub or contact the author directly.
