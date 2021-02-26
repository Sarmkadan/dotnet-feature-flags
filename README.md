## CliArgumentParserExtensions

The `CliArgumentParserExtensions` class provides a set of extension methods for the `CliArgumentParser` type that enhance CLI argument parsing functionality. These methods allow you to extract command names, parse and validate arguments, check for argument presence, retrieve argument values, and generate help documentation for commands.

Below is a realistic usage example demonstrating the most commonly used extension methods:

```csharp
// Initialize the argument parser
var parser = new CliArgumentParser();

// Parse command line arguments
string[] args = new[] { "evaluate", "--key", "payment-v2", "--user", "john@example.com" };

// Get the command name
string command = parser.GetCommand(args);
Console.WriteLine($"Command: {command}"); // Output: Command: evaluate

// Parse with validation for required arguments
var validatedCommand = parser.ParseWithValidation(args, "key", "user");
Console.WriteLine($"Validated command has {parser.GetArgumentCount(validatedCommand)} arguments");

// Check if any arguments are present
bool hasArgs = parser.HasAnyArguments(validatedCommand);
Console.WriteLine($"Has arguments: {hasArgs}"); // Output: Has arguments: True

// Get all values for a specific argument key
var keyValues = parser.GetAllArguments(validatedCommand, "key");
foreach (var value in keyValues)
{
    Console.WriteLine($"Key value: {value}");
}

// Get help text for a command
string helpText = parser.GetCommandHelp("evaluate");
Console.WriteLine(helpText);

// Parse with group validation (at least one of several required groups)
string[] groupArgs = new[] { "create", "--key", "new-feature", "--name", "New Feature" };
var groupCommand = parser.ParseWithGroupValidation(groupArgs, 
    new[] { "key", "name" },
    new[] { "percentage", "rollout-type" }
);
Console.WriteLine($"Group validation successful: {groupCommand.HasArgument("key")}");
```

## RolloutStrategyExtensions

The `RolloutStrategyExtensions` class provides a set of extension methods for the `RolloutStrategy` type that enable common operations for filtering, metadata access, and event manipulation. These methods allow you to easily check event types, access metadata values, create modified copies of events, and format events for logging purposes.

Below is a realistic usage example demonstrating the most commonly used extension methods:

```csharp
// Create a rollout strategy
var strategy = new RolloutStrategy
{
  StartPercentage = 5,
  EndPercentage = 100,
  DailyIncrementPercentage = 10,
  StartDate = DateTime.UtcNow,
  EndDate = DateTime.UtcNow.AddDays(10)
};

// Check if strategy is percentage-based
bool isPercentageBased = strategy.IsPercentageBased();

// Check if strategy is rules-based
bool isRulesBased = strategy.IsRulesBased();

// Check if strategy is an A/B test
bool isABTest = strategy.IsABTest();

// Check if strategy is a full rollout
bool isFullRollout = strategy.IsFullRollout();

// Check if strategy is no rollout
bool isNoRollout = strategy.IsNoRollout();

// Get the effective percentage for the strategy
int effectivePercentage = strategy.GetEffectivePercentage();

// Get the progress percentage for the strategy
int progressPercentage = strategy.GetProgressPercentage();

// Check if the strategy has reached its target percentage
bool hasReachedTarget = strategy.HasReachedTarget();

// Get a human-readable description of the rollout strategy type
string description = strategy.GetDescription();
```

## AuditLogExtensions

The `AuditLogExtensions` class provides a set of extension methods for the `AuditLog` type that enable common operations for analyzing audit trail entries. These methods allow you to determine the type of change, calculate time since changes, generate human-readable descriptions, and format actions for display purposes.

Below is a realistic usage example demonstrating the most commonly used extension methods:

```csharp
// Create an audit log entry
var log = new AuditLog
{
    Action = AuditAction.Updated,
    ChangedAt = DateTime.UtcNow.AddMinutes(-45),
    Summary = "Feature flag updated",
    ChangeDetails = "Enabled state changed from false to true"
};

// Check if this represents a state change
bool isStateChange = log.IsStateChange();

// Check if this represents a creation event
bool isCreation = log.IsCreation();

// Check if this represents a deletion event
bool isDeletion = log.IsDeletion();

// Get the time since this change was made
string timeSinceChange = log.GetTimeSinceChange();

// Get a detailed change description
string detailedDescription = log.GetDetailedChangeDescription();

// Check if this change is recent (within 30 minutes)
bool isRecent = log.IsRecent();

// Get a simplified action name for UI display
string actionDisplayName = log.GetActionDisplayName();
```

## FeatureFlagOptionsExtensions

The `FeatureFlagOptionsExtensions` class provides utility methods for working with `FeatureFlagOptions` configuration. It includes validation, cloning, merging, and auditing/cache configuration checks to ensure options are correctly configured and used consistently across the application.

```csharp
// Create base options
var options = new FeatureFlagOptions
{
    EnableCache = true,
    CacheDurationMinutes = 30,
    EnableAuditLogging = true,
    AuditLogRetentionDays = 30,
    MaxRulesPerFlag = 5
};

// Validate options
options.Validate(); // Throws if invalid

// Clone options
var clonedOptions = options.Clone();

// Create override options
var overrideOptions = new FeatureFlagOptions
{
    CacheDurationMinutes = 60,
    MaxRulesPerFlag = 10
};

// Merge options
var mergedOptions = options.MergeWith(overrideOptions);

// Check audit logging configuration
bool isAuditConfigured = mergedOptions.IsAuditLoggingConfigured();

// Get effective cache duration in seconds
int cacheSeconds = mergedOptions.GetCacheDurationSeconds();
Console.WriteLine($"Cache duration: {cacheSeconds} seconds");
```
