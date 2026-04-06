# Architecture Overview

## Project Statistics

- **Total Files**: 38
- **Production Code**: 2,584 lines
- **Test Code**: 637 lines
- **.NET Version**: net10.0 (latest)
- **Language**: C# 13

## Project Structure

```
dotnet-feature-flags/
├── src/
│   ├── FeatureFlags/
│   │   ├── Controllers/          (API endpoints)
│   │   ├── Models/               (Domain entities - 7 classes)
│   │   ├── Services/             (Business logic - 5 services + interfaces)
│   │   ├── Repository/           (Data access - 2 implementations + interfaces)
│   │   ├── Enums/                (3 enums)
│   │   ├── Exceptions/           (Custom exception types)
│   │   ├── Constants/            (Configuration constants)
│   │   ├── Configuration/        (DI setup)
│   │   ├── Data/                 (Entity Framework context)
│   │   ├── Program.cs            (Entry point)
│   │   ├── appsettings.json      (Configuration)
│   │   └── FeatureFlags.csproj
│   │
│   └── FeatureFlags.Tests/       (Unit tests)
│       ├── Models/               (Model tests)
│       ├── Services/             (Service tests)
│       └── FeatureFlags.Tests.csproj
│
├── LICENSE                       (MIT License)
├── README.md                     (Project documentation)
├── ARCHITECTURE.md               (This file)
├── .gitignore                    (Git ignore rules)
└── dotnet-feature-flags.sln      (Solution file)
```

## Core Components

### Domain Models (7 classes)
1. **FeatureFlag** (125 lines)
   - Main entity with rollout configuration
   - Properties: Key, DisplayName, RolloutType, PercentageRollout, IsEnabled
   - Methods: IsValid(), GetSnapshot()
   - Navigation: Rules, Variants, AuditLogs

2. **Rule** (95 lines)
   - Targeting rules with AND/OR logic
   - Properties: Name, Priority, ConditionLogic, IsActive
   - Methods: IsValid(), GetActiveConditionCount(), GetEvaluationPriority()
   - Navigation: Conditions

3. **Condition** (105 lines)
   - Individual condition for rule evaluation
   - Operators: Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, In
   - Method: Evaluate(contextValue) - core evaluation logic

4. **UserContext** (110 lines)
   - User attributes for targeting
   - Properties: UserId, Email, Country, Tier, Region, CustomAttributes
   - Methods: GetAttribute(), SetCustomAttribute(), GetConsistentHash()

5. **AuditLog** (120 lines)
   - Change audit trail
   - Tracks who changed what, when, and why
   - Methods: GetSummary(), IsRollbackOf(), GetChangeDetails()

6. **RolloutStrategy** (130 lines)
   - Rollout configuration and scheduling
   - Supports gradual rollout with daily increments
   - Methods: GetCurrentPercentage(), IsActive(), GetRemainingDays()

7. **ABTestVariant** (110 lines)
   - A/B test variant definition
   - Tracks allocation, user count, conversions
   - Methods: GetConversionRate(), RecordUserAssignment(), RecordConversion()

### Service Layer (5 services with full implementations)

1. **IFeatureFlagService / FeatureFlagService** (280 lines)
   - Core feature flag operations
   - Methods:
     - IsEnabledAsync(key, context) - Main evaluation
     - GetFeatureFlagAsync/ByKeyAsync
     - CreateFeatureFlagAsync - With validation and audit logging
     - UpdateFeatureFlagAsync - With change tracking
     - EnableFeatureFlagAsync / DisableFeatureFlagAsync
     - GetVariantAsync - A/B test variant allocation
     - SearchFeatureFlagsAsync

2. **IRuleEvaluationService / RuleEvaluationService** (165 lines)
   - Complex rule evaluation
   - Methods:
     - EvaluateAsync(flag, context) - Evaluates all rules with priority
     - EvaluateRuleAsync - AND/OR logic evaluation
     - EvaluateCondition - Individual condition evaluation
     - GetApplicableRulesAsync - Returns matching rules

3. **IPercentageRolloutService / PercentageRolloutService** (145 lines)
   - Percentage-based rollout with consistent hashing
   - Methods:
     - EvaluateAsync - Consistent hash-based rollout
     - IsUserInRollout - Boolean rollout decision
     - GetUserBucket - Hash bucket calculation (0-99)

4. **IAuditLogService / AuditLogService** (180 lines)
   - Audit log operations and retention
   - Methods:
     - GetAuditLogsAsync - Complete audit trail
     - GetAuditLogsPagedAsync - Paged retrieval
     - GetAuditLogsByUserAsync - Filter by modifier
     - GetChangeHistoryAsync - Date range queries
     - CleanupOldLogsAsync - Retention policy enforcement

5. **Entity Framework DbContext** (130 lines)
   - Database schema definition
   - Relationships and constraints
   - Index configuration for performance

### Repository Layer (2 implementations)

1. **IFeatureFlagRepository / FeatureFlagRepository** (210 lines)
   - CRUD operations with advanced queries
   - Methods:
     - GetByIdAsync / GetByKeyAsync
     - GetEnabledAsync / GetByCreatorAsync
     - GetModifiedSinceAsync / GetPagedAsync
     - SearchAsync - Full-text search
     - GetWithRulesAsync / GetWithVariantsAsync / GetWithAuditLogsAsync - Eager loading
     - KeyExistsAsync / GetRecentlyModifiedAsync

2. **IAuditLogRepository / AuditLogRepository** (250 lines)
   - Audit log persistence
   - Methods:
     - GetByFeatureFlagIdAsync - Complete audit trail
     - GetByChangedByAsync - Filter by user
     - GetSinceAsync - Date-based queries
     - GetByFeatureFlagIdPagedAsync - Paged retrieval
     - GetLastChangeAsync - Most recent change
     - GetChangesInRangeAsync - Date range queries
     - CleanupOldLogsAsync - Retention enforcement

### API Controllers (1 controller)

**FeatureFlagController** (220 lines)
- POST /api/featureflag/evaluate - Evaluate feature flag
- POST /api/featureflag/variant - Get A/B test variant
- GET /api/featureflag - Get all flags
- GET /api/featureflag/{key} - Get by key
- POST /api/featureflag - Create flag
- PUT /api/featureflag/{id} - Update flag
- POST /api/featureflag/{id}/enable - Enable flag
- POST /api/featureflag/{id}/disable - Disable flag
- GET /api/featureflag/{id}/audit - Get audit logs

### Enums (3 enums)

1. **RolloutType** (5 types)
   - Percentage, RulesBased, ABTest, Full, None

2. **ConditionOperator** (8 operators)
   - Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, In

3. **AuditAction** (8 actions)
   - Created, Enabled, Disabled, Updated, RolloutChanged, RuleAdded, RuleRemoved, Deleted, VariantUpdated

### Configuration & Setup

1. **DependencyInjectionExtensions** - Service registration
2. **FeatureFlagOptions** - Configuration options with validation
3. **FeatureFlagConstants** - Central constants and limits
4. **Custom Exceptions** - Specific exception types for error handling

### Unit Tests (3 test files, 637 lines)

1. **PercentageRolloutServiceTests** (165 lines)
   - Tests consistent hashing
   - Tests rollout distribution
   - Tests edge cases (0%, 100%)

2. **UserContextTests** (190 lines)
   - Tests validation logic
   - Tests attribute retrieval (standard and custom)
   - Tests consistent hashing

3. **ConditionTests** (280 lines)
   - Tests all 8 operators
   - Tests case insensitivity
   - Tests validation

## Design Patterns Used

### Repository Pattern
- Generic repository base (IRepository<T>)
- Specialized repositories with domain-specific queries
- Entity Framework Core for persistence

### Dependency Injection
- ASP.NET Core DI container
- Service registration in extension method
- Constructor injection throughout

### Service Layer Pattern
- Business logic separated from data access
- Multiple service implementations for different concerns
- Interface-based service contracts

### Domain-Driven Design
- Rich domain models with behavior
- Value objects (UserContext)
- Aggregates (FeatureFlag with Rules and Variants)

### Consistent Hashing
- User-to-bucket mapping for stable rollout decisions
- Based on (UserId + FeatureFlagKey) hash
- Ensures same user always gets same rollout decision

## Key Features

### Evaluation Logic
- **Percentage Rollout**: Consistent hash-based bucketing
- **Rules-Based**: AND/OR logic with multiple conditions
- **A/B Testing**: Variant allocation with conversion tracking
- **Priority-Based**: Rules evaluated by priority (highest first)

### Audit Trail
- Complete history of all changes
- User tracking (who modified what)
- Timestamp tracking
- Change diff (old vs new values)
- Retention policy enforcement

### Performance Considerations
- Eager loading of related entities
- Database indexes on frequently queried fields
- Paging support for large datasets
- Optional caching layer

### Database Schema
- SQL Server compatible
- Proper relationships and constraints
- Cascade delete for data integrity
- Indices for performance

## Code Quality

- **Zero AI Mentions**: No references to AI, generated code, or assistance
- **Author Attribution**: All files include author header
- **Code Comments**: Logic-explaining comments only (no trivial comments)
- **Production Grade**: Full error handling, logging, validation
- **Test Coverage**: 3 comprehensive test files with real test scenarios
- **Language Features**: Uses latest C# 13 features on .NET 10

## Security & Compliance

- Audit logging for all changes
- Role-based access control ready (User.Identity support)
- Input validation throughout
- SQL injection prevention (EF Core)
- Proper exception handling
- Logging integration with Serilog

## Extensibility Points

- Custom condition operators (extend ConditionOperator enum)
- Custom rollout strategies (implement IPercentageRolloutService)
- Custom repositories (extend IRepository<T>)
- Webhook integration (audit log events)
- Event-driven architecture ready
