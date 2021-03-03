// existing content ...

## RuleEvaluationServiceTestsExtensions

The `RuleEvaluationServiceTestsExtensions` class provides helper methods for creating test data and asserting rule/condition evaluation outcomes in unit tests. It simplifies the setup of `UserContext`, `Condition`, and `Rule` objects, and includes assertion utilities for validating evaluation logic.

Example usage:
```csharp
// Create a user context for testing
var user = RuleEvaluationServiceTestsExtensions.CreateUserContext(
    userId: "user123",
    email: "user@example.com",
    country: "US",
    tier: "premium");

// Create a rule with a single condition
var rule = RuleEvaluationServiceTestsExtensions.CreateSingleConditionRule(
    attributeName: "Country",
    @operator: ConditionOperator.Equals,
    expectedValue: "US",
    ruleName: "Country Check Rule");

// Evaluate the rule and assert the result
var evaluationResult = await ruleService.EvaluateRuleAsync(rule, user);
await RuleEvaluationServiceTestsExtensions.AssertRuleResultAsync(
    evaluationResult,
    expected: true,
    rule: rule,
    userContext: user);

## FeatureFlagServiceTestExampleExtensions

The `FeatureFlagServiceTestExampleExtensions` class provides extension methods for testing feature flag services. It includes methods for testing percentage rollout, rule-based evaluation, A/B test variant assignment, and performance monitoring.

Example usage:
```csharp
var example = new FeatureFlagServiceTestExample();
var percentageRolloutResults = example.TestPercentageRolloutComprehensive();
var ruleEvaluationResults = example.TestRuleBasedEvaluation();
var variantAssignments = example.TestABTestVariantAssignment();
var performanceMetrics = await example.MonitorEvaluationPerformanceAsync("test-flag");
```

## FeatureFlagException

The `FeatureFlagException` class is the base exception type for all feature flag-related errors in the system. It provides an `ErrorCode` property to categorize different types of failures that can occur during feature flag evaluation, configuration, or data processing.

Example usage:
```csharp
try
{
    var featureFlag = await featureFlagService.GetFeatureFlagAsync("my-flag");
    if (featureFlag == null)
    {
        throw new FeatureFlagNotFoundException("Feature flag 'my-flag' was not found in the configuration");
    }
}
catch (FeatureFlagException ex) when (ex.ErrorCode == "FF_NOT_FOUND")
{
    // Handle missing feature flag
    logger.LogWarning(ex, "Feature flag not found: {FlagName}", "my-flag");
}
catch (FeatureFlagException ex) when (ex.ErrorCode == "FF_INVALID_DATA")
{
    // Handle invalid feature flag data
    logger.LogError(ex, "Invalid feature flag data encountered");
}
catch (FeatureFlagException ex)
{
    // Handle other feature flag exceptions
    logger.LogError(ex, "Feature flag evaluation failed");
}
```

## ValidationException

The `ValidationException` class is thrown when input validation fails during feature flag operations. It extends `FeatureFlagException` with an `Errors` property that contains a dictionary of validation error messages, where keys represent the field names and values contain the corresponding error descriptions. This is particularly useful for webhook validation and other scenarios where multiple validation errors need to be reported.

Example usage:
```csharp
// Basic validation exception
throw new ValidationException("Invalid feature flag configuration detected");

// Validation exception with error details
var errors = new Dictionary<string, string>
{
  { "Name", "Name is required and must be between 3-50 characters" },
  { "Enabled", "Enabled flag must be a valid boolean value" },
  { "Percentage", "Percentage must be between 0 and 100" }
};
throw new ValidationException("Feature flag configuration validation failed", errors);

// Validation exception with inner exception
try
{
  var featureFlag = await featureFlagService.GetFeatureFlagAsync("my-flag");
}
catch (Exception ex)
{
  throw new ValidationException("Failed to validate feature flag configuration", ex);
}

// Webhook validation exception
var webhookErrors = new Dictionary<string, string>
{
  { "Url", "Webhook URL must be a valid HTTPS endpoint" },
  { "Secret", "Webhook secret must be at least 32 characters" }
};
throw new WebhookValidationException("Webhook configuration is invalid", webhookErrors);
```

## ConfigurationException

The `ConfigurationException` class is the base exception type for configuration-related errors in the feature flag system. It is thrown when there are issues with application configuration, such as missing required settings, invalid configuration values, or malformed configuration files.

Example usage:
```csharp
// Basic configuration exception
throw new ConfigurationException("Missing required configuration: FeatureFlags:Database:ConnectionString");

// Configuration exception with inner exception
try
{
  var configValue = configuration["FeatureFlags:Database:ConnectionString"];
  if (string.IsNullOrEmpty(configValue))
  {
    throw new ConfigurationException("Database connection string is not configured");
  }
}
catch (Exception ex)
{
  throw new ConfigurationException("Failed to read database configuration", ex);
}

// Database-specific configuration exception
throw new DatabaseConfigurationException("Invalid database connection string format: expected Server=...;Database=...;");

// HTTP client configuration exception
throw new HttpClientConfigurationException("Timeout configuration must be between 1 and 30 seconds");
```

## FeatureFlagEvent

The `FeatureFlagEvent` class represents an event that occurs in the feature flag system. It contains information about the event type, feature flag identifier, trigger details, and additional metadata. Events are published through the `IEventBus` and can be consumed by subscribers such as `EventLoggingSubscriber` for audit trails or `WebhookEventSubscriber` for webhook integrations.

Example usage:
```csharp
// Create and publish a feature flag event
var featureFlagEvent = new FeatureFlagEvent
{
    EventType = "FeatureFlagEnabled",
    FeatureFlagId = 42,
    FeatureFlagKey = "new-dashboard",
    TriggeredBy = "admin@company.com",
    OccurredAt = DateTime.UtcNow,
    Metadata = new Dictionary<string, object?>
    {
        { "Environment", "Production" },
        { "UserId", "user-123" },
        { "PreviousState", false },
        { "NewState", true }
    }
};

// Using the event bus to publish the event
var eventBus = serviceProvider.GetRequiredService<IEventBus>();
await eventBus.PublishAsync(featureFlagEvent);

// Publishing with convenience method
await eventBus.PublishAsync(
    eventType: "FeatureFlagUpdated",
    featureFlagId: 17,
    featureFlagKey: "experimental-feature",
    triggeredBy: "ci-cd-pipeline",
    metadata: new Dictionary<string, object?>
    {
        { "Version", "2.1.0" },
        { "Changes", new[] { "rule-added", "percentage-updated" } }
    }
);

// Subscribing to events (automatic with AddEventSystem)
// Events are automatically logged by EventLoggingSubscriber
// WebhookEventSubscriber can be configured for HTTP integrations
```

## FeatureFlagsBenchmarks

The `FeatureFlagsBenchmarks` class provides performance benchmarks for feature flag evaluation operations. It measures the execution time of various evaluation scenarios including percentage rollout, rule-based evaluation, A/B test variant assignment, and complex rule evaluations with caching.

Example usage:
```csharp
[MemoryDiagnoser]
public class FeatureFlagBenchmarksExample
{
    private FeatureFlagService _featureFlagService;
    private IRuleEvaluationService _ruleEvaluationService;
    private UserContext _userContext;

    [GlobalSetup]
    public void Setup()
    {
        _featureFlagService = new FeatureFlagService();
        _ruleEvaluationService = new RuleEvaluationService();
        _userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com",
            Country = "US",
            Tier = "premium"
        };
    }

    [Benchmark]
    public void PercentageRolloutEvaluation() => _featureFlagService.IsEnabled("percentage-flag", _userContext);

    [Benchmark]
    public void PercentageRolloutEvaluation_100() => _featureFlagService.IsEnabled("percentage-100-flag", _userContext);

    [Benchmark]
    public void PercentageRolloutEvaluation_0() => _featureFlagService.IsEnabled("percentage-0-flag", _userContext);

    [Benchmark]
    public void RuleBasedEvaluation_Match()
    {
        var rule = new Rule
        {
            Name = "Country Rule",
            Conditions = new List<Condition>
            {
                new Condition { AttributeName = "Country", Operator = ConditionOperator.Equals, ExpectedValue = "US" }
            }
        };
        _ruleEvaluationService.EvaluateRule(rule, _userContext);
    }

    [Benchmark]
    public void RuleBasedEvaluation_NoMatch()
    {
        var rule = new Rule
        {
            Name = "Country Rule",
            Conditions = new List<Condition>
            {
                new Condition { AttributeName = "Country", Operator = ConditionOperator.Equals, ExpectedValue = "UK" }
            }
        };
        _ruleEvaluationService.EvaluateRule(rule, _userContext);
    }

    [Benchmark]
    public void ABTestVariantAssignment() => _featureFlagService.GetVariant("ab-test-flag", _userContext);

    [Benchmark]
    public void FullFeatureFlagEvaluation_Percentage() => _featureFlagService.IsEnabled("complex-percentage-flag", _userContext);

    [Benchmark]
    public void FullFeatureFlagEvaluation_RuleBased() => _featureFlagService.IsEnabled("complex-rule-flag", _userContext);

    [Benchmark]
    public void FullFeatureFlagEvaluation_ABTest() => _featureFlagService.GetVariant("complex-abtest-flag", _userContext);
}
``` 
```