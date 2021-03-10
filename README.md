// existing content ...

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
