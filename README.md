# dotnet-feature-flags

A feature flag engine for .NET: percentage rollouts with consistent hashing, rule-based targeting, A/B variants, and a full audit trail, exposed as an ASP.NET Core REST API backed by EF Core / SQL Server.

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the solution layout, evaluation data flow, DI composition, design decisions and known limitations. The sections below are per-type reference docs.

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

## RuleEvaluationService

Evaluates targeting rules for feature flags with support for AND/OR logic between conditions. This service determines whether a feature should be enabled for a specific user based on their context attributes and the flag's configured rules. It supports both synchronous condition evaluation and asynchronous flag evaluation with database-backed rules.

Example usage:

```csharp
using FeatureFlags.Services;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());
services.AddDbContext<FeatureFlagDbContext>(options =>
    options.UseSqlite("Data Source=featureflags.db"));

// Register repositories and services
services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
services.AddScoped<IRuleEvaluationService, RuleEvaluationService>();

var serviceProvider = services.BuildServiceProvider();

// Create service instance
var evaluationService = serviceProvider.GetRequiredService<IRuleEvaluationService>();

// Create a feature flag with rules
var featureFlag = new FeatureFlag
{
    Key = "new_checkout_flow",
    DisplayName = "New Checkout Flow",
    IsEnabled = true,
    Description = "Enables the redesigned checkout process"
};

// Add a rule with conditions
featureFlag.Rules.Add(new Rule
{
    Name = "Premium US Users",
    Priority = 10,
    IsActive = true,
    ConditionLogic = "AND"
});

featureFlag.Rules[0].Conditions.Add(new Condition
{
    AttributeName = "country",
    Operator = ConditionOperator.Equals,
    ExpectedValue = "US",
    IsActive = true
});

featureFlag.Rules[0].Conditions.Add(new Condition
{
    AttributeName = "tier",
    Operator = ConditionOperator.Equals,
    ExpectedValue = "Premium",
    IsActive = true
});

// Create user context
var userContext = new UserContext
{
    UserId = "user123",
    Country = "US",
    Tier = "Premium"
};

// Evaluate the feature flag for the user
bool isEnabled = await evaluationService.EvaluateAsync(featureFlag, userContext);
Console.WriteLine($"Feature enabled for user: {isEnabled}");

// Evaluate a specific rule
bool ruleMatches = await evaluationService.EvaluateRuleAsync(featureFlag.Rules[0], userContext);
Console.WriteLine($"Rule matches: {ruleMatches}");

// Get all applicable rules for the user
var applicableRules = await evaluationService.GetApplicableRulesAsync(featureFlag, userContext);
Console.WriteLine($"Applicable rules count: {applicableRules.Count()}");

// Evaluate individual conditions
bool countryMatches = evaluationService.EvaluateCondition(
    featureFlag.Rules[0].Conditions[0],
    userContext
);
Console.WriteLine($"Country condition matches: {countryMatches}");
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

## CliArgumentParser

Parses command-line arguments and converts them to structured command objects. Provides help text generation and validation for CLI arguments, supporting all available commands like `evaluate`, `create`, `update`, `list`, `get`, `enable`, `disable`, `audit`, `export`, `import`, and `webhook`.

Example usage:
```csharp
using FeatureFlags.CLI;

// Parse command line arguments
string[] args = new[] { "evaluate", "--key", "new_checkout_flow", "--user", "user123@example.com" };
var command = CliArgumentParser.Parse(args);

// Check if help was requested
if (command.ShowHelp)
{
    CliArgumentParser.PrintHelp();
    return;
}

// Get command name
Console.WriteLine($"Command: {command.Command}"); // "evaluate"

// Access arguments
if (command.HasArgument("key"))
{
    string key = command.GetArgument("key");
    Console.WriteLine($"Key: {key}"); // "new_checkout_flow"
}

if (command.HasArgument("user"))
{
    string user = command.GetArgument("user");
    Console.WriteLine($"User: {user}"); // "user123@example.com"
}

// Check if an argument exists
bool hasContext = command.HasArgument("context");
Console.WriteLine($"Has context: {hasContext}");

// Get all arguments
foreach (var arg in command.Arguments)
{
    Console.WriteLine($"{arg.Key}: {arg.Value}");
}

// Print help message
CliArgumentParser.PrintHelp();
```

## AuditLogService

Manages retrieval and cleanup of audit trails for compliance and debugging purposes. The `AuditLogService` provides methods to query audit logs by feature flag, user, date range, or recency, and includes functionality for maintaining log retention through scheduled cleanup operations.

Example usage:

```csharp
using FeatureFlags.Services;
using FeatureFlags.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());
services.AddDbContext<FeatureFlagDbContext>(options =>
    options.UseSqlite("Data Source=featureflags.db"));

// Register repositories and services
services.AddScoped<IAuditLogRepository, AuditLogRepository>();
services.AddScoped<IAuditLogService, AuditLogService>();

var serviceProvider = services.BuildServiceProvider();

// Create service instance
var auditLogService = serviceProvider.GetRequiredService<IAuditLogService>();

// Get all audit logs for a specific feature flag
var logs = await auditLogService.GetAuditLogsAsync(1);
Console.WriteLine($"Found {logs.Count()} audit logs for flag ID 1");

// Get paged audit logs for a feature flag
var pagedLogs = await auditLogService.GetAuditLogsPagedAsync(1, 1, 10);
Console.WriteLine($"Page 1 of audit logs: {pagedLogs.Count()} entries");

// Get audit logs by user who made the changes
var userLogs = await auditLogService.GetAuditLogsByUserAsync("admin@example.com");
Console.WriteLine($"User 'admin@example.com' made {userLogs.Count()} changes");

// Get recent audit logs (last 10 changes)
var recentLogs = await auditLogService.GetRecentAuditLogsAsync(10);
Console.WriteLine($"Most recent changes: {recentLogs.Count()} entries");

// Get the last change for a specific feature flag
var lastChange = await auditLogService.GetLastChangeAsync(1);
if (lastChange != null)
{
    Console.WriteLine($"Last change by {lastChange.ChangedBy} at {lastChange.ChangedAt}");
}

// Get change history for a specific date range
var history = await auditLogService.GetChangeHistoryAsync(
    DateTime.UtcNow.AddDays(-30),
    DateTime.UtcNow
);
Console.WriteLine($"Changes in last 30 days: {history.Count()} entries");

// Clean up old logs (older than 90 days)
await auditLogService.CleanupOldLogsAsync(90);
Console.WriteLine("Old audit logs cleaned up");
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

## FeatureFlagService

Central service that coordinates all feature flag operations including evaluation, creation, updates, deletion, and audit logging. The `FeatureFlagService` implements `IFeatureFlagService` and provides methods for checking feature availability, managing flag states, and retrieving flag configurations.

Example usage:

```csharp
using FeatureFlags.Services;
using FeatureFlags.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());
services.AddDbContext<FeatureFlagDbContext>(options =>
    options.UseSqlite("Data Source=featureflags.db"));

// Register repositories and services
services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
services.AddScoped<IAuditLogRepository, AuditLogRepository>();
services.AddScoped<IRuleEvaluationService, RuleEvaluationService>();
services.AddScoped<IPercentageRolloutService, PercentageRolloutService>();
services.AddScoped<IFlagEvaluationLogService, FlagEvaluationLogService>();
services.AddScoped<IFeatureFlagService, FeatureFlagService>();
services.Configure<FeatureFlagOptions>(options => 
{
    options.EnableAuditLog = true;
});

var serviceProvider = services.BuildServiceProvider();

// Create service instance
var featureFlagService = serviceProvider.GetRequiredService<IFeatureFlagService>();

// Create a user context for evaluation
var userContext = new UserContext
{
    UserId = "user123",
    Email = "user@example.com",
    Country = "US",
    Tier = "Premium"
};

// Check if a feature is enabled for a user
bool isNewCheckoutEnabled = await featureFlagService.IsEnabledAsync("new_checkout", userContext);
Console.WriteLine($"New checkout feature enabled: {isNewCheckoutEnabled}");

// Create a new feature flag
var newFlag = new FeatureFlag
{
    Key = "new_checkout_flow",
    DisplayName = "New Checkout Flow",
    Description = "Enables the redesigned checkout process",
    IsEnabled = true,
    RolloutType = RolloutType.Percentage,
    PercentageRollout = 50
};

var createdFlag = await featureFlagService.CreateFeatureFlagAsync(newFlag, "admin@example.com");
Console.WriteLine($"Created feature flag: {createdFlag.Key}");

// Get a feature flag by ID
var retrievedFlag = await featureFlagService.GetFeatureFlagAsync(createdFlag.Id);
if (retrievedFlag != null)
{
    Console.WriteLine($"Retrieved flag: {retrievedFlag.Key}");
}

// Get a feature flag by key
var flagByKey = await featureFlagService.GetFeatureFlagByKeyAsync("new_checkout_flow");
Console.WriteLine($"Flag by key: {flagByKey?.DisplayName}");

// Get all feature flags
var allFlags = await featureFlagService.GetAllFeatureFlagsAsync();
Console.WriteLine($"Total flags: {allFlags.Count()}");

// Get only enabled feature flags
var enabledFlags = await featureFlagService.GetEnabledFeatureFlagsAsync();
Console.WriteLine($"Enabled flags: {enabledFlags.Count()}");

// Enable or disable a feature flag
await featureFlagService.EnableFeatureFlagAsync(createdFlag.Id, "admin@example.com");
await featureFlagService.DisableFeatureFlagAsync(createdFlag.Id, "admin@example.com");

// Update a feature flag
retrievedFlag!.IsEnabled = true;
retrievedFlag.Description = "Updated description";
await featureFlagService.UpdateFeatureFlagAsync(retrievedFlag, "admin@example.com");

// Delete a feature flag
await featureFlagService.DeleteFeatureFlagAsync(createdFlag.Id, "admin@example.com");

// Search for feature flags
var searchResults = await featureFlagService.SearchFeatureFlagsAsync("checkout");
Console.WriteLine($"Search results: {searchResults.Count()}");

// Get A/B test variant for a user
var variantKey = await featureFlagService.GetVariantAsync("ab_test_feature", userContext);
Console.WriteLine($"User assigned variant: {variantKey ?? "None"}");
```

## ErrorHandlingMiddleware

Global exception handler middleware that catches all unhandled exceptions during HTTP request processing and returns standardized error responses. The middleware logs exceptions at appropriate levels (Error for unexpected exceptions, Warning for validation errors, etc.) and ensures consistent error response format across the API. It handles specific exception types like `FeatureFlagException`, `KeyNotFoundException`, and `ArgumentException` with appropriate HTTP status codes.

Example usage:
```csharp
using FeatureFlags.Middleware;
using FeatureFlags.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());

// Configure HTTP context accessor
services.AddHttpContextAccessor();

var serviceProvider = services.BuildServiceProvider();

// Create middleware instance (typically registered in Startup/Program.cs)
// app.UseMiddleware<ErrorHandlingMiddleware>();

// Example of how the middleware handles exceptions
try
{
    // Simulate an operation that might throw
    throw new FeatureFlagException("Feature flag 'new_ui' not found");
}
catch (Exception ex)
{
    // The middleware would catch this and return:
    // {
    //   "StatusCode": 400,
    //   "Message": "Feature flag 'new_ui' not found",
    //   "ErrorCode": "FeatureFlagException",
    //   "Timestamp": "2024-01-15T10:30:00Z"
    // }
    
    var errorResponse = new
    {
        StatusCode = 400,
        Message = ex.Message,
        ErrorCode = ex.GetType().Name,
        Timestamp = DateTime.UtcNow
    };
    
    Console.WriteLine($"Error Response: {System.Text.Json.JsonSerializer.Serialize(errorResponse)}");
}
```

## RateLimitingMiddleware

Rate limiting middleware that restricts the number of requests per client within a configurable time window. The `RateLimitingMiddleware` uses a sliding window approach to track request timestamps and prevent API abuse by enforcing rate limits based on client identifiers (user ID or IP address). It automatically cleans up expired entries and provides standard HTTP headers for rate limit information.

Example usage:
```csharp
using FeatureFlags.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());
services.AddHttpContextAccessor();

var serviceProvider = services.BuildServiceProvider();

// Configure rate limiting options
var rateLimitOptions = new RateLimitOptions
{
    MaxRequests = 100,      // Maximum 100 requests
    WindowSeconds = 60        // per 60-second window
};

// Create middleware instance
var middleware = new RateLimitingMiddleware(
    next: async (context) => await Task.CompletedTask,
    options: rateLimitOptions
);

// Example: Simulate request handling
var httpContext = new DefaultHttpContext();
httpContext.Request.Path = "/api/feature-flags";
httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");

// Simulate user context
httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user123") }));

// Invoke the middleware
await middleware.InvokeAsync(httpContext);

// Check rate limit headers
var remaining = httpContext.Response.Headers["X-RateLimit-Remaining"];
var reset = httpContext.Response.Headers["X-RateLimit-Reset"];
Console.WriteLine($"Remaining requests: {remaining}, Reset in: {reset} seconds");
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

## PercentageRolloutService

The `PercentageRolloutService` provides percentage-based rollout evaluation for feature flags using consistent hashing to ensure stable, reproducible decisions across application restarts. It determines whether a user should receive a feature based on a percentage threshold and the user's consistent hash bucket, making it ideal for gradual feature rollouts and A/B testing scenarios.

Example usage:

```csharp
using FeatureFlags.Services;
using FeatureFlags.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());

// Register the service
services.AddScoped<IPercentageRolloutService, PercentageRolloutService>();

var serviceProvider = services.BuildServiceProvider();

// Create service instance
var percentageService = serviceProvider.GetRequiredService<IPercentageRolloutService>();

// Create a feature flag with percentage rollout configuration
var featureFlag = new FeatureFlag
{
    Key = "new_checkout_flow",
    DisplayName = "New Checkout Flow",
    Description = "Enables the redesigned checkout process",
    IsEnabled = true,
    RolloutType = RolloutType.Percentage,
    PercentageRollout = 50 // Enable for 50% of users
};

// Create a user context for evaluation
var userContext = new UserContext
{
    UserId = "user123",
    Email = "user@example.com",
    Country = "US",
    Tier = "Premium"
};

// Evaluate the feature flag for the user (async)
bool isEnabled = await percentageService.EvaluateAsync(featureFlag, userContext);
Console.WriteLine($"Feature enabled for user: {isEnabled}");

// Get the user's bucket for consistent hashing (0-99)
int userBucket = percentageService.GetUserBucket(userContext, featureFlag.Key);
Console.WriteLine($"User bucket: {userBucket}");

// Check if user is in rollout directly
bool isInRollout = percentageService.IsUserInRollout(userContext, featureFlag.Key, featureFlag.PercentageRollout!.Value);
Console.WriteLine($"User in rollout: {isInRollout}");

// Example: 100% rollout (all users get the feature)
var fullRolloutFlag = new FeatureFlag
{
    Key = "full_feature",
    PercentageRollout = 100
};
bool fullRolloutResult = await percentageService.EvaluateAsync(fullRolloutFlag, userContext);
Console.WriteLine($"100% rollout result: {fullRolloutResult}"); // true

// Example: 0% rollout (no users get the feature)
var noRolloutFlag = new FeatureFlag
{
    Key = "disabled_feature",
    PercentageRollout = 0
};
bool noRolloutResult = await percentageService.EvaluateAsync(noRolloutFlag, userContext);
Console.WriteLine($"0% rollout result: {noRolloutResult}"); // false
```

## FeatureFlagOptions

Configuration options for the feature flag engine that control caching, audit logging, performance limits, and evaluation behavior. These options are loaded from `appsettings.json` under the `FeatureFlags` section and can be configured via dependency injection.

Example usage:

```csharp
using FeatureFlags.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure FeatureFlagOptions in your service setup
var services = new ServiceCollection();

services.Configure<FeatureFlagOptions>(options =>
{
    options.EnableCache = true;
    options.CacheDurationMinutes = 10;
    options.AuditLogRetentionDays = 90;
    options.EnableAuditLogging = true;
    options.MaxRulesPerFlag = 50;
    options.MaxConditionsPerRule = 25;
    options.MaxVariantsPerFlag = 5;
    options.LogEvaluationDetails = false;
    options.EnableAuditLog = true;
    options.DefaultRolloutPercentage = 75
});

var serviceProvider = services.BuildServiceProvider();

// Access the configured options
var featureFlagOptions = serviceProvider.GetRequiredService<IOptions<FeatureFlagOptions>>().Value;

if (featureFlagOptions.IsValid())
{
    Console.WriteLine($"Cache enabled: {featureFlagOptions.EnableCache}");
    Console.WriteLine($"Cache duration: {featureFlagOptions.CacheDurationMinutes} minutes");
    Console.WriteLine($"Audit retention: {featureFlagOptions.AuditLogRetentionDays} days");
}
```

## ICacheService

The `ICacheService` interface provides a unified abstraction for caching feature flag evaluations, configurations, and other frequently accessed data. It supports both in-memory and distributed caching implementations, making it suitable for single-server deployments (using `InMemoryCacheService`) or multi-server deployments (using `DistributedCacheService`).

The cache service helps improve performance by reducing database queries and computation for repeated feature flag evaluations, while supporting time-based cache invalidation through TTL (Time To Live) values.

Example usage:

```csharp
using FeatureFlags.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection for in-memory cache
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());

// Register the in-memory cache service with default 5-minute TTL
services.AddSingleton<ICacheService, InMemoryCacheService>();

var serviceProvider = services.BuildServiceProvider();

// Create cache service instance
var cacheService = serviceProvider.GetRequiredService<ICacheService>();

// Store feature flag evaluation result in cache
var featureFlagKey = "new_checkout_flow";
var userContextKey = "user123_context";
var cacheKey = $"{featureFlagKey}:{userContextKey}";

// Set a feature flag evaluation result with 10-minute TTL
cacheService.Set(cacheKey, true, TimeSpan.FromMinutes(10));

// Retrieve cached evaluation result
bool isEnabled = cacheService.Get<bool>(cacheKey);
Console.WriteLine($"Cached result: {isEnabled}");

// Update the cache with new value
cacheService.Set(cacheKey, false, TimeSpan.FromMinutes(15));

// Remove specific cache entry
cacheService.Remove(cacheKey);

// Clear all cache entries (use with caution in production)
cacheService.Clear();

// Async operations are also supported
await cacheService.SetAsync(cacheKey, true, TimeSpan.FromMinutes(5));
bool cachedValue = await cacheService.GetAsync<bool>(cacheKey);
```

## IGradualRolloutSchedulerService

The `IGradualRolloutSchedulerService` interface manages the scheduling and advancement of gradual feature flag rollouts. It supports time-based percentage advancement with configurable daily increment steps, start dates, and end dates. The service automatically processes scheduled rollouts to advance percentage allocations based on elapsed time, and provides methods to check rollout status and manually advance specific rollouts.

This service is typically invoked from a background worker or hosted service to ensure gradual rollouts progress according to schedule without manual intervention.

Example usage:

```csharp
using FeatureFlags.Services;
using FeatureFlags.Models;
using FeatureFlags.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());
services.AddDbContext<FeatureFlagDbContext>(options =>
    options.UseSqlite("Data Source=featureflags.db"));

// Register repositories and services
services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
services.AddScoped<IAuditLogRepository, AuditLogRepository>();
services.AddScoped<IGradualRolloutSchedulerService, GradualRolloutSchedulerService>();

var serviceProvider = services.BuildServiceProvider();

// Create service instance
var schedulerService = serviceProvider.GetRequiredService<IGradualRolloutSchedulerService>();

// Create a feature flag with gradual rollout strategy
var featureFlag = new FeatureFlag
{
    Key = "new_checkout_flow",
    DisplayName = "New Checkout Flow",
    Description = "Enables the redesigned checkout process",
    IsEnabled = true,
    RolloutType = RolloutType.Percentage,
    PercentageRollout = 0 // Start at 0%
};

// Create a gradual rollout strategy
var rolloutStrategy = new RolloutStrategy
{
    FeatureFlagId = featureFlag.Id,
    Type = RolloutType.Percentage,
    StartPercentage = 0,
    EndPercentage = 100,
    IsGradual = true,
    DailyIncrement = 10,
    StartDate = DateTime.UtcNow.AddDays(-5), // Started 5 days ago
    EndDate = DateTime.UtcNow.AddDays(15) // Ends in 15 days
};

// Process all scheduled rollouts (typically called from a background service)
int updatedFlags = await schedulerService.ProcessScheduledRolloutsAsync();
Console.WriteLine($"Processed {updatedFlags} feature flags");

// Check rollout status for a specific feature flag
var status = await schedulerService.GetScheduleStatusAsync(featureFlag.Id);
if (status != null)
{
    Console.WriteLine($"Feature flag '{status.FeatureFlagKey}' rollout status:");
    Console.WriteLine($"    FeatureFlagId: {status.FeatureFlagId}");
    Console.WriteLine($"    CurrentPercentage: {status.CurrentPercentage}%");
    Console.WriteLine($"    TargetPercentage: {status.TargetPercentage}%");
    Console.WriteLine($"    DailyIncrement: {status.DailyIncrement}%");
    Console.WriteLine($"    IsActive: {status.IsActive}");
    Console.WriteLine($"    IsComplete: {status.IsComplete}");
    Console.WriteLine($"  EstimatedDaysRemaining: {status.EstimatedDaysRemaining}");
}

// Manually advance a specific rollout
bool advanced = await schedulerService.AdvanceRolloutAsync(featureFlag.Id, "admin@example.com");
Console.WriteLine($"Manual rollout advance: {(advanced ? "Success" : "Failed")}");
```

## Phase2DependencyInjectionExtensions

The `Phase2DependencyInjectionExtensions` class provides dependency injection configuration for Phase 2 components including middleware, caching, webhooks, event system, background workers, and rate limiting. It registers all services, repositories, HTTP clients, and hosted services required for the enhanced feature flag system.

This extension class provides methods to configure services (`AddPhase2Services`), register middleware (`UsePhase2Middleware`), and initialize event subscribers (`InitializeEventSubscribers`) in your ASP.NET Core application.

Example usage:

```csharp
using FeatureFlags.Configuration;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());

// Load configuration (typically from appsettings.json)
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Register Phase 2 services
services.AddPhase2Services(configuration);

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Configure the application pipeline
var app = new ApplicationBuilder(serviceProvider);

// Add Phase 2 middleware
app.UsePhase2Middleware();

// Initialize event subscribers (webhooks, logging, etc.)
app.InitializeEventSubscribers();
```

## DatabaseSeeder

The `DatabaseSeeder` class provides methods for seeding and clearing test data in the feature flag database. It supports creating sample feature flags, rules, conditions, variants, and audit logs with realistic configurations for development, testing, and performance benchmarking scenarios.

Example usage:

```csharp
using FeatureFlags.Data;
using FeatureFlags.Models;
using FeatureFlags.Enums;

// Seed the database with sample data for development/testing
await DatabaseSeeder.SeedSampleDataAsync();

// Seed minimal data required for basic functionality
await DatabaseSeeder.SeedMinimalDataAsync();

// Clear all seeded data from the database
await DatabaseSeeder.ClearDatabaseAsync();

// Seed performance test data with many flags and rules
await DatabaseSeeder.SeedPerformanceTestDataAsync();

// Get statistics about the seeded data
var stats = await DatabaseSeeder.GetStatisticsAsync();
Console.WriteLine($"Total flags: {stats.TotalFeatureFlags}");
Console.WriteLine($"Enabled flags: {stats.EnabledFlags}");
Console.WriteLine($"Disabled flags: {stats.DisabledFlags}");
Console.WriteLine($"Total rules: {stats.TotalRules}");
Console.WriteLine($"Total conditions: {stats.TotalConditions}");
Console.WriteLine($"Total variants: {stats.TotalVariants}");
Console.WriteLine($"Total audit logs: {stats.TotalAuditLogs}");
Console.WriteLine($"Percentage rollout count: {stats.PercentageRolloutCount}");
Console.WriteLine($"Rules-based count: {stats.RulesBasedCount}");
Console.WriteLine($"A/B test count: {stats.ABTestCount}");
```

## FlagEvaluationLogService

Provides thread-safe logging and retrieval of feature flag evaluation events. The `FlagEvaluationLogService` records when and how feature flags are evaluated for users, enabling debugging of "why did user X see feature Y" scenarios and providing an audit trail for feature flag evaluations. It supports filtering logs by user ID, flag name, and provides methods for bulk retrieval and log cleanup.

Example usage:


```csharp
using FeatureFlags.Services;
using FeatureFlags.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());

// Register the service
services.AddScoped<IFlagEvaluationLogService, FlagEvaluationLogService>();

var serviceProvider = services.BuildServiceProvider();

// Create service instance
var evaluationLogService = serviceProvider.GetRequiredService<IFlagEvaluationLogService>();

// Create a feature flag and user context
var featureFlag = new FeatureFlag
{
    Key = "new_ui",
    DisplayName = "New User Interface",
    IsEnabled = true,
    Description = "Enables the redesigned user interface"
};

var userContext = new UserContext
{
    UserId = "user123",
    Email = "user@example.com",
    Country = "US",
    Tier = "Premium"
};

// Log a feature flag evaluation
// Method 1: Using the convenience method
evaluationLogService.LogEvaluation(featureFlag, userContext, true);

// Method 2: Using the direct Log method with a pre-created log entry
var evaluationLog = new FlagEvaluationLog
{
    FlagName = "beta_feature",
    UserId = "user456",
    Result = false,
    Reason = "PercentageRollout",
    Timestamp = DateTime.UtcNow
};
evaluationLogService.Log(evaluationLog);

// Retrieve all evaluation logs
var allLogs = evaluationLogService.GetAll();
Console.WriteLine($"Total evaluation logs: {allLogs.Count}");

// Retrieve logs for a specific user
var userLogs = evaluationLogService.GetByUserId("user123");
Console.WriteLine($"Logs for user123: {userLogs.Count}");

// Retrieve logs for a specific flag
var flagLogs = evaluationLogService.GetByFlagName("new_ui");
Console.WriteLine($"Logs for new_ui flag: {flagLogs.Count}");

// Use convenience aliases
var evaluationLogs = evaluationLogService.GetEvaluationLogs();
var userEvaluationLogs = evaluationLogService.GetEvaluationLogsForUser("user456");
var flagEvaluationLogs = evaluationLogService.GetEvaluationLogsForFlag("beta_feature");

// Clear all logs when needed (e.g., for testing or cleanup)
evaluationLogService.ClearLogs();
```
