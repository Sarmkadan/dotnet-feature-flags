// existing content ...

## FeatureFlag

Represents the core feature flag entity, containing configuration, rollout strategy, and targeting rules. Use FeatureFlag to define feature status, manage gradual rollouts, or configure A/B testing variants for controlled feature releases.

Example usage:
```csharp
using FeatureFlags.Models;
using FeatureFlags.Enums;

var flag = new FeatureFlag
{
    Id = 1,
    Key = "new_checkout_flow",
    DisplayName = "New Checkout Flow",
    Description = "Enables the redesigned checkout process",
    IsEnabled = true,
    RolloutType = RolloutType.Percentage,
    PercentageRollout = 50,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    CreatedBy = "admin@example.com",
    UpdatedBy = "admin@example.com"
};

// Add a targeting rule
flag.Rules.Add(new Rule 
{ 
    Name = "Premium Users Only", 
    Priority = 10,
    IsActive = true
});

// Validate the flag configuration
bool isValid = flag.IsValid(); // Returns true
Console.WriteLine($"Flag is valid: {isValid}");

// Get a summary snapshot of the flag state
string snapshot = flag.GetSnapshot();
Console.WriteLine($"Snapshot: {snapshot}");
```

## Condition

Represents a single condition within a rule that evaluates context attributes against expected values using various comparison operators. Conditions are used to define targeting rules for feature flags by matching user context properties like country, tier, or custom attributes.

Example usage:
```csharp
var condition = new Condition
{
    AttributeName = "country",
    Operator = ConditionOperator.Equals,
    ExpectedValue = "US"
};

// Evaluate against user context
bool isMatch = condition.Evaluate("US"); // Returns true

// Check if condition is valid
bool isValid = condition.IsValid(); // Returns true

// Using different operators
var percentageCondition = new Condition
{
    AttributeName = "userId",
    Operator = ConditionOperator.In,
    ExpectedValue = "user1,user2,user3,user4,user5"
};

bool isInList = percentageCondition.Evaluate("user3"); // Returns true
```

## UserContext

Represents a user's context for feature flag evaluation, containing identity attributes and metadata for targeting rules. Provides methods for validating user data and generating consistent hash values for percentage-based rollouts.

Example usage:
```csharp
var userContext = new UserContext
{
    UserId = "user123",
    Email = "user@example.com",
    Country = "US",
    Tier = "Premium",
    Region = "North America"
};

userContext.SetCustomAttribute("userType", "PowerUser");

if (userContext.IsValid())
{
    var hash = userContext.GetConsistentHash("feature.new_ui");
    Console.WriteLine($"Consistent hash for feature: {hash}");
}
```

## Result

A generic result wrapper class that represents the outcome of an operation. The `Result<T>` class provides a consistent way to return success/failure with data or error messages, making it ideal for error handling in feature flag operations and other business logic.

## ABTestVariant

Represents a variant in an A/B test for a feature flag. Tracks allocation percentage and metrics for statistical analysis. Use ABTestVariant to implement feature flag variants with controlled rollout and conversion tracking.

Example usage:
```csharp
var variant = new ABTestVariant
{
    VariantKey = "new_ui_variant",
    DisplayName = "New UI Variant",
    Description = "Variant with redesigned user interface",
    AllocationPercentage = 30,
    IsControl = false
};

// Record user assignment to track participation
variant.RecordUserAssignment();

// Record conversion when user completes desired action
if (userCompletedAction)
{
    variant.RecordConversion();
}

// Calculate conversion rate
double conversionRate = variant.GetConversionRate();
Console.WriteLine($"Conversion rate: {conversionRate:P2}");

// Check if variant configuration is valid
if (variant.IsValid())
{
    Console.WriteLine("Variant configuration is valid");
}

// Get statistical confidence level based on user count
string confidence = variant.GetStatisticalConfidence();
Console.WriteLine($"Statistical confidence: {confidence}");
```

## ApiResponse

Generic API response wrapper classes that provide consistent response structure across all endpoints. `ApiResponse<T>` is used for operations returning data, while the non-generic `ApiResponse` is used for operations without return values. Both include success status, optional messages/errors, metadata, and timestamps for standardized API communication.

Example usage:
```csharp
using FeatureFlags.Models;

// Successful response with data
var successResponse = ApiResponse<FeatureFlag>.Ok(new FeatureFlag
{
    Key = "new_ui",
    IsEnabled = true,
    Description = "Enables the new user interface"
}, "Feature flag created successfully");

Console.WriteLine($"Success: {successResponse.Success}");
Console.WriteLine($"Data: {successResponse.Data?.Key}");
Console.WriteLine($"Message: {successResponse.Message}");

// Failed response with error
var errorResponse = ApiResponse<FeatureFlag>.Fail("Feature flag not found with key: missing_flag");

Console.WriteLine($"Success: {errorResponse.Success}");
Console.WriteLine($"Error: {errorResponse.Error}");

// Non-generic response for operations without data
var operationResponse = ApiResponse.Ok("Feature flag updated successfully");

Console.WriteLine($"Success: {operationResponse.Success}");
Console.WriteLine($"Message: {operationResponse.Message}");

// Response with metadata
var metadataResponse = ApiResponse<FeatureFlag>.Ok(
    new FeatureFlag { Key = "beta_feature", IsEnabled = false },
    "Beta feature retrieved"
);
metadataResponse.Metadata = new ApiMetadata
{
    RequestId = Guid.NewGuid().ToString(),
    ExecutionTimeMs = 42,
    PageNumber = 1,
    PageSize = 10,
    TotalCount = 1
};

Console.WriteLine($"Request ID: {metadataResponse.Metadata?.RequestId}");
Console.WriteLine($"Execution time: {metadataResponse.Metadata?.ExecutionTimeMs}ms");
```

Example usage:
```csharp
// Successful operation with data
Result<FeatureFlag> result = Result<FeatureFlag>.Success(new FeatureFlag
{
    Key = "new_ui",
    IsEnabled = true,
    Description = "Enables the new user interface"
});

if (result.IsSuccess)
{
    FeatureFlag flag = result.Data!;
    Console.WriteLine($"Flag enabled: {flag.IsEnabled}");
}

// Failed operation with error
Result<bool> failureResult = Result<bool>.Failure("Feature flag not found", 404);

// Using Try for exception handling
Result<int> countResult = await Result<int>.Try(async () =>
{
    // Simulate database operation
    await Task.Delay(100);
    return 42;
});

// Chaining operations with Map
Result<string> nameResult = result.Map(f => f.Key.ToUpper());

// Chaining async operations with BindAsync
Result<FeatureFlag> updatedFlag = await result.BindAsync(async flag =>
{
    // Simulate updating flag in database
    await Task.Delay(50);
    return Result<FeatureFlag>.Success(flag with { Description = "Updated description" });
});

// Handling success/failure with callbacks
result.OnSuccess(flag => Console.WriteLine($"Success: {flag.Key}"))
     .OnFailure(error => Console.WriteLine($"Error: {error}"));

// Getting data with fallback
FeatureFlag flag = result.GetOrDefault(new FeatureFlag { Key = "default", IsEnabled = false });

// Non-generic Result for operations without return values
Result operationResult = Result.Success();
if (!operationResult.IsSuccess)
{
    Console.WriteLine($"Operation failed: {operationResult.Error}");
}
```

## AuditLog

Records all changes to feature flags for compliance, debugging, and audit trail requirements. It tracks who made what changes and when, enabling rollback analysis and change history review.

Example usage:
```csharp
using FeatureFlags.Models;
using FeatureFlags.Enums;

var auditLog = new AuditLog
{
    FeatureFlagId = 1,
    Action = AuditAction.Update,
    ChangedBy = "admin@example.com",
    ChangedAt = DateTime.UtcNow,
    OldValue = "true",
    NewValue = "false",
    Description = "Disabled feature flag new_ui",
    IpAddress = "192.168.1.1"
};

// Summary of the change
Console.WriteLine(auditLog.GetSummary());

// Check if valid
if (auditLog.IsValid())
{
    // Check if this log is a rollback
    var previousLog = new AuditLog { OldValue = "false", NewValue = "true" };
    bool isRollback = auditLog.IsRollbackOf(previousLog);
    Console.WriteLine($"Is rollback: {isRollback}");
}

// Get raw state changes
var (oldState, newState) = auditLog.GetChangeDetails();
Console.WriteLine($"Changed from {oldState} to {newState}");
```

## FeatureFlagRepository

Provides database persistence operations for managing feature flag entities, supporting CRUD operations and complex queries with eager loading of related configuration and audit logs. It serves as the primary interface for accessing feature flags within the application's data layer.

Example usage:
```csharp
using FeatureFlags.Repository;
using FeatureFlags.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
// Configure DbContext and Logging as needed...
var serviceProvider = services.BuildServiceProvider();

// Create repository instance
var repository = new FeatureFlagRepository(
    serviceProvider.GetRequiredService<FeatureFlagDbContext>(),
    serviceProvider.GetRequiredService<ILogger<FeatureFlagRepository>>()
);

// Create a new feature flag
var newFlag = new FeatureFlag { Key = "new_checkout", IsEnabled = false, DisplayName = "New Checkout" };
var addedFlag = await repository.AddAsync(newFlag);

// Retrieve and update a flag
var flag = await repository.GetByKeyAsync("new_checkout");
if (flag != null)
{
    flag.IsEnabled = true;
    await repository.UpdateAsync(flag);
}
```

## WebhookRepository

Manages webhook persistence and retrieval for feature flag event notifications. The `WebhookRepository` handles CRUD operations for webhooks and provides specialized queries to find active webhooks, webhooks by event type, and recently failed deliveries for monitoring and retry operations.

Example usage:
```csharp
using FeatureFlags.Repository;
using FeatureFlags.Integration;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddDbContext<FeatureFlagDbContext>(options => 
    options.UseSqlite("Data Source=featureflags.db"));
services.AddLogging();
var serviceProvider = services.BuildServiceProvider();

// Create repository
var repository = new WebhookRepository(
    serviceProvider.GetRequiredService<FeatureFlagDbContext>(),
    serviceProvider.GetRequiredService<ILogger<WebhookRepository>>()
);

// Create a new webhook for feature flag update events
var webhook = new Webhook
{
    Url = "https://api.example.com/webhooks/feature-flags",
    Description = "Feature flag update notifications",
    IsActive = true,
    EventTypes = WebhookEventType.FeatureFlagUpdated | WebhookEventType.FeatureFlagCreated,
    FeatureFlagKey = "new_checkout_flow",
    CreatedBy = "admin@example.com",
    MaxRetries = 3,
    RetryDelaySeconds = 60,
    AuthorizationHeader = "Bearer your-secret-token",
    Secret = "your-webhook-secret"
};

// Create the webhook in database
var createdWebhook = await repository.CreateAsync(webhook);
Console.WriteLine($"Created webhook with ID: {createdWebhook.Id}");

// Get webhook by ID
var retrievedWebhook = await repository.GetByIdAsync(createdWebhook.Id);
if (retrievedWebhook != null)
{
    Console.WriteLine($"Retrieved webhook: {retrievedWebhook.Url}");
}

// Get all active webhooks
var activeWebhooks = await repository.GetActiveAsync();
Console.WriteLine($"Active webhooks count: {activeWebhooks.Count}");

// Get webhooks that handle specific event types
var updateWebhooks = await repository.GetByEventTypeAsync(WebhookEventType.FeatureFlagUpdated);
Console.WriteLine($"Webhooks for update events: {updateWebhooks.Count}");

// Get webhooks with recent failures for retry processing
var failedWebhooks = await repository.GetRecentFailuresAsync();
Console.WriteLine($"Webhooks with recent failures: {failedWebhooks.Count}");

// Update webhook configuration
retrievedWebhook!.IsActive = false;
var updateSuccess = await repository.UpdateAsync(retrievedWebhook);
Console.WriteLine($"Update successful: {updateSuccess}");

// Get statistics
var totalCount = await repository.GetCountAsync();
var activeCount = await repository.GetActiveCountAsync();
Console.WriteLine($"Total webhooks: {totalCount}, Active: {activeCount}");

// Delete webhook when no longer needed
var deleteSuccess = await repository.DeleteAsync(createdWebhook.Id);
Console.WriteLine($"Delete successful: {deleteSuccess}");
```

## WebhookDeliveryRepository

Tracks webhook delivery attempts and their outcomes. The `WebhookDeliveryRepository` stores delivery history, supports retry operations, and provides statistics for monitoring webhook performance and reliability.

Example usage:
```csharp
using FeatureFlags.Repository;
using FeatureFlags.Integration;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddDbContext<FeatureFlagDbContext>(options => 
    options.UseSqlite("Data Source=featureflags.db"));
services.AddLogging();
var serviceProvider = services.BuildServiceProvider();

// Create repository
var deliveryRepository = new WebhookDeliveryRepository(
    serviceProvider.GetRequiredService<FeatureFlagDbContext>(),
    serviceProvider.GetRequiredService<ILogger<WebhookDeliveryRepository>>()
);

// Create a webhook first
var webhookRepository = new WebhookRepository(
    serviceProvider.GetRequiredService<FeatureFlagDbContext>(),
    serviceProvider.GetRequiredService<ILogger<WebhookRepository>>()
);
var webhook = new Webhook
{
    Url = "https://api.example.com/webhooks/feature-flags",
    Description = "Feature flag notifications",
    IsActive = true
};
await webhookRepository.CreateAsync(webhook);

// Create a delivery record
var delivery = new WebhookDelivery
{
    WebhookId = webhook.Id,
    Payload = "{\"eventType\":\"FeatureFlagUpdated\",\"flagKey\":\"new_ui\"}",
    TriggeredAt = DateTime.UtcNow,
    IsSuccess = true,
    ResponseStatusCode = 200,
    ResponseBody = "{\"status\":\"received\"}"
};

var createdDelivery = await deliveryRepository.CreateAsync(delivery);
Console.WriteLine($"Created delivery with ID: {createdDelivery.Id}");

// Get delivery by ID
var retrievedDelivery = await deliveryRepository.GetByIdAsync(createdDelivery.Id);
if (retrievedDelivery != null)
{
    Console.WriteLine($"Retrieved delivery for webhook: {retrievedDelivery.WebhookId}");
}

// Get all deliveries for a specific webhook
var webhookDeliveries = await deliveryRepository.GetByWebhookIdAsync(webhook.Id);
Console.WriteLine($"Deliveries for webhook {webhook.Id}: {webhookDeliveries.Count}");

// Get pending retries (deliveries that need to be retried)
var pendingRetries = await deliveryRepository.GetPendingRetriesAsync();
Console.WriteLine($"Pending retries: {pendingRetries.Count}");

// Get recent deliveries for a webhook (last 7 days)
var recentDeliveries = await deliveryRepository.GetRecentDeliveriesAsync(webhook.Id, 7);
Console.WriteLine($"Recent deliveries: {recentDeliveries.Count}");

// Get delivery statistics (successful vs failed)
var (successful, failed) = await deliveryRepository.GetDeliveryStatsAsync(webhook.Id, 7);
Console.WriteLine($"Delivery stats - Successful: {successful}, Failed: {failed}");

// Update delivery status
retrievedDelivery!.MarkFailed("Connection timeout", 3, 60);
var updateSuccess = await deliveryRepository.UpdateAsync(retrievedDelivery);
Console.WriteLine($"Update delivery status: {updateSuccess}");
```

## FlagEvaluationLog

Records a single feature flag evaluation event, capturing the flag name, user identity, outcome, and reasoning for debugging "why did user X see feature Y". Use `FlagEvaluationLog` to audit feature flag evaluations and track which users received which feature states.

Example usage:
```csharp
using FeatureFlags.Models;

// Log a successful feature flag evaluation
var evaluationLog = new FlagEvaluationLog
{
    FlagName = "new_ui",
    UserId = "user123",
    Result = true,
    Timestamp = DateTime.UtcNow,
    Reason = "RulesBased"
};

Console.WriteLine($"Flag '{evaluationLog.FlagName}' evaluated to {evaluationLog.Result} for user {evaluationLog.UserId}");
Console.WriteLine($"Reason: {evaluationLog.Reason} at {evaluationLog.Timestamp:u}");

// Log a failed evaluation
var failedLog = new FlagEvaluationLog
{
    FlagName = "beta_feature",
    UserId = "user456",
    Result = false,
    Reason = "PercentageRollout"
};

if (!failedLog.Result)
{
    Console.WriteLine($"User {failedLog.UserId} did not receive feature '{failedLog.FlagName}' due to {failedLog.Reason}");
}
```

## Rule

Represents a targeting rule that groups one or more `Condition` objects and determines whether a feature flag should be enabled based on the rule's priority, activation state, and logical combination of its conditions. Rules are evaluated in order of `Priority` (higher values first) and can be toggled on or off with `IsActive`.

Example usage:
```csharp
using FeatureFlags.Models;

var rule = new Rule
{
    Id = 1,
    FeatureFlagId = 42,
    Name = "US Premium Users",
    Description = "Enable feature for premium users in the US",
    Priority = 10,
    IsActive = true,
    ConditionLogic = "AND",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Add a condition that matches the user's country
rule.Conditions.Add(new Condition
{
    AttributeName = "country",
    Operator = ConditionOperator.Equals,
    ExpectedValue = "US",
    IsActive = true
});

// Add a condition that matches the user's tier
rule.Conditions.Add(new Condition
{
    AttributeName = "tier",
    Operator = ConditionOperator.Equals,
    ExpectedValue = "Premium",
    IsActive = true
});

bool valid = rule.IsValid();                     // true if the rule is well‑formed
int activeCount = rule.GetActiveConditionCount(); // 2
int evalPriority = rule.GetEvaluationPriority(); // 10
```

## RolloutStrategy

Defines the strategy for rolling out a feature to users. Supports percentage-based, rule-based, and A/B test rollout strategies with configurable start/end percentages, date ranges, and gradual rollout increments.

Example usage:
```csharp
using FeatureFlags.Models;
using FeatureFlags.Enums;

// Create a gradual percentage-based rollout strategy
var strategy = new RolloutStrategy
{
    FeatureFlagId = 1,
    Type = RolloutType.Percentage,
    StartPercentage = 0,
    EndPercentage = 100,
    IsGradual = true,
    DailyIncrement = 10,
    StartDate = DateTime.UtcNow.AddDays(-5), // Started 5 days ago
    EndDate = DateTime.UtcNow.AddDays(5),  // Ends in 5 days
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Check if the rollout is currently active
bool isActive = strategy.IsActive();
Console.WriteLine($"Rollout is active: {isActive}");

// Get the current percentage based on time elapsed and daily increment
int currentPercentage = strategy.GetCurrentPercentage();
Console.WriteLine($"Current rollout percentage: {currentPercentage}%");

// Validate the strategy configuration
bool isValid = strategy.IsValid();
Console.WriteLine($"Strategy is valid: {isValid}");

// Get remaining days until rollout ends
int remainingDays = strategy.GetRemainingDays();
Console.WriteLine($"Days remaining: {remainingDays}");

// Example: Percentage-based rollout without gradual increment
var instantStrategy = new RolloutStrategy
{
    FeatureFlagId = 2,
    Type = RolloutType.Percentage,
    StartPercentage = 50,
    EndPercentage = 50,
    IsGradual = false,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

Console.WriteLine($"Instant rollout at {instantStrategy.GetCurrentPercentage()}%");
```
