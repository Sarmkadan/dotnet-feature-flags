# FeatureFlagServiceTestExampleExtensions

The `FeatureFlagServiceTestExampleExtensions` type serves as a static utility container within the `dotnet-feature-flags` testing ecosystem, providing helper members for validating complex feature flag scenarios such as percentage rollouts, rule-based evaluations, and A/B test variant assignments. It aggregates test data definitions, execution results, and performance metrics to facilitate comprehensive verification of the feature flag evaluation engine under various load and configuration conditions.

## API

### Static Members

#### `TestPercentageRolloutComprehensive`
```csharp
public static Dictionary<string, bool> TestPercentageRolloutComprehensive
```
Provides a pre-configured dictionary mapping feature flag keys to their expected boolean states for comprehensive percentage rollout testing. This member is used to define the ground truth for validating that users are enabled or disabled according to specific rollout percentages. It does not accept parameters and returns a `Dictionary<string, bool>`. It does not throw exceptions during access.

#### `TestRuleBasedEvaluation`
```csharp
public static List<RuleEvaluationResult> TestRuleBasedEvaluation
```
Returns a list of `RuleEvaluationResult` objects representing the outcomes of various rule-based evaluation scenarios. This collection is utilized to verify that complex filter chains and context-based rules resolve correctly. It does not accept parameters and returns a `List<RuleEvaluationResult>`. Accessing this member does not throw exceptions.

#### `TestABTestVariantAssignment`
```csharp
public static Dictionary<string, string> TestABTestVariantAssignment
```
Supplies a dictionary mapping user identifiers or session keys to their assigned A/B test variant strings. This is essential for validating consistent variant allocation across multiple evaluations. It returns a `Dictionary<string, string>` and requires no parameters. No exceptions are thrown during retrieval.

#### `MonitorEvaluationPerformanceAsync`
```csharp
public static async Task<FlagPerformanceMetrics> MonitorEvaluationPerformanceAsync
```
Asynchronously executes a performance benchmark on the feature flag evaluation logic and returns a `FlagPerformanceMetrics` object containing statistical data. This method measures latency, throughput, and error rates over a defined set of iterations. It does not require explicit parameters in this signature (relying on internal test configurations) and returns a `Task<FlagPerformanceMetrics>`. It may throw asynchronous exceptions if the underlying evaluation infrastructure fails or if the test execution is interrupted.

### Instance Members (Context/Data Properties)

*Note: The following members appear to be properties of a result context or test state object associated with the extension logic.*

#### `FlagKey`
```csharp
public string FlagKey
```
Gets the identifier of the feature flag currently being tested or evaluated. Returns a `string`.

#### `MatchingUserEnabled`
```csharp
public bool MatchingUserEnabled
```
Indicates whether a user matching the specific test criteria was successfully enabled by the flag evaluation. Returns `true` if enabled, `false` otherwise.

#### `NonMatchingUserEnabled`
```csharp
public bool NonMatchingUserEnabled
```
Indicates whether a user who does not match the test criteria was inadvertently enabled. This is typically expected to be `false` in valid configurations.

#### `ExpectedMatching`
```csharp
public bool ExpectedMatching
```
Defines the expected boolean outcome for a user matching the test conditions. Used for assertion comparisons.

#### `ExpectedNonMatching`
```csharp
public bool ExpectedNonMatching
```
Defines the expected boolean outcome for a user not matching the test conditions.

#### `Iterations`
```csharp
public int Iterations
```
Represents the total number of evaluation cycles performed during a performance test or simulation.

#### `SuccessCount`
```csharp
public int SuccessCount
```
Counts the number of successful flag evaluations recorded during the test run.

#### `ErrorCount`
```csharp
public int ErrorCount
```
Counts the number of failed evaluations or exceptions caught during the test run.

#### `TotalTimeMs`
```csharp
public long TotalTimeMs
```
The aggregate time in milliseconds consumed by all evaluation iterations.

#### `AverageMs`
```csharp
public double AverageMs
```
The arithmetic mean of the evaluation latency per iteration in milliseconds.

#### `MaxMs`
```csharp
public long MaxMs
```
The maximum latency observed for a single evaluation operation in milliseconds.

#### `MinMs`
```csharp
public long MinMs
```
The minimum latency observed for a single evaluation operation in milliseconds.

#### `P95Ms`
```csharp
public double P95Ms
```
The 95th percentile latency value in milliseconds, indicating the threshold below which 95% of evaluations completed.

#### `P99Ms`
```csharp
public double P99Ms
```
The 99th percentile latency value in milliseconds, useful for identifying tail latency issues.

#### `ThroughputPerSecond`
```csharp
public double ThroughputPerSecond
```
Calculates the number of evaluations processed per second based on the total iterations and total time.

## Usage

### Example 1: Validating Rule-Based Evaluation Results
This example demonstrates how to retrieve the pre-defined rule evaluation results and assert that the actual service behavior matches the expected outcomes stored in `TestRuleBasedEvaluation`.

```csharp
using System.Collections.Generic;
using System.Linq;
using Microsoft.FeatureFlags.Testing; // Hypothetical namespace

public class RuleValidationTest
{
    public void ValidateRules()
    {
        // Retrieve the static test data
        List<RuleEvaluationResult> testCases = FeatureFlagServiceTestExampleExtensions.TestRuleBasedEvaluation;

        foreach (var testCase in testCases)
        {
            // Simulate or call actual evaluation logic
            bool actualResult = EvaluateFlag(testCase.Context);

            // Assert against the expected result contained in the test case
            if (actualResult != testCase.ExpectedResult)
            {
                throw new AssertionException(
                    $"Rule evaluation failed for {testCase.RuleId}. Expected {testCase.ExpectedResult}, got {actualResult}");
            }
        }
    }

    private bool EvaluateFlag(dynamic context) 
    {
        // Placeholder for actual feature flag service call
        return true; 
    }
}
```

### Example 2: Monitoring Performance Metrics
This example illustrates invoking the asynchronous performance monitor and analyzing the resulting metrics to ensure the system meets throughput requirements.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.FeatureFlags.Testing; // Hypothetical namespace

public class PerformanceBenchmark
{
    public async Task RunBenchmarkAsync()
    {
        // Execute the performance monitoring task
        var metrics = await FeatureFlagServiceTestExampleExtensions.MonitorEvaluationPerformanceAsync();

        Console.WriteLine($"Performance Report:");
        Console.WriteLine($"Iterations: {metrics.Iterations}");
        Console.WriteLine($"Success Count: {metrics.SuccessCount}");
        Console.WriteLine($"Error Count: {metrics.ErrorCount}");
        Console.WriteLine($"Average Latency: {metrics.AverageMs:F2} ms");
        Console.WriteLine($"P99 Latency: {metrics.P99Ms:F2} ms");
        Console.WriteLine($"Throughput: {metrics.ThroughputPerSecond:F0} ops/sec");

        if (metrics.ErrorCount > 0)
        {
            throw new InvalidOperationException("Performance test completed with errors.");
        }

        if (metrics.P99Ms > 100.0)
        {
            throw new InvalidOperationException("P99 latency exceeded acceptable threshold of 100ms.");
        }
    }
}
```

## Notes

### Thread Safety
The static members `TestPercentageRolloutComprehensive`, `TestRuleBasedEvaluation`, and `TestABTestVariantAssignment` return mutable collection types (`Dictionary` and `List`). While the retrieval of these references is thread-safe, modifying the contents of these collections from multiple threads concurrently is not safe and may lead to data corruption. Consumers should treat these collections as read-only or apply external synchronization if modification is required during test setup. The `MonitorEvaluationPerformanceAsync` method is designed to be called concurrently; however, running multiple instances simultaneously may contended for system resources, potentially skewing performance metrics.

### Edge Cases
- **Empty Collections**: If the internal test data initialization fails or is empty, `TestRuleBasedEvaluation` may return an empty list. Consumers should verify the count before iterating to avoid logical gaps in test coverage.
- **Division by Zero**: The `ThroughputPerSecond` and `AverageMs` properties depend on `Iterations` and `TotalTimeMs`. If `Iterations` is zero or `TotalTimeMs` is zero (e.g., in an instantaneous mock run), calculations for throughput or averages should be handled carefully by the consumer to avoid `NaN` or `Infinity` results, although the implementation typically guards against dividing by zero time by returning 0 or a minimal epsilon.
- **Asynchronous Timeouts**: `MonitorEvaluationPerformanceAsync` relies on underlying evaluation services. If those services hang or deadlock, the task may not complete without an external cancellation token, though the signature provided does not expose one directly.
