# FeatureFlagServiceTestExample

Provides a comprehensive test harness for validating feature flag evaluation logic, performance characteristics, and health monitoring capabilities. This class encapsulates scenario-based tests for percentage rollouts, rule-based targeting, and A/B variant assignment, along with integrated performance benchmarking, load testing, and health check utilities to verify production readiness of feature flag implementations.

## API

### Nested Types

#### `FeatureFlagPerformanceMonitor`
Encapsulates performance telemetry for feature flag evaluations. Captures latency distributions, throughput metrics, and comparative benchmarks between implementation variants.

#### `FeatureFlagHealthCheck`
Represents the health status of the feature flag subsystem, including database connectivity, performance thresholds, and overall operational readiness.

#### `FeatureFlagLoadTester`
Executes concurrent load simulations against the feature flag evaluation pipeline to measure scalability and identify contention bottlenecks.

### Methods

#### `void TestPercentageRollout()`
Validates that percentage-based rollout logic distributes users correctly across the configured split. Executes a statistically significant sample size and asserts that observed allocation falls within expected confidence intervals.

**Throws:** `InvalidOperationException` if the underlying flag configuration cannot be resolved; `AssertionException` (test framework) if distribution deviates beyond tolerance.

#### `void TestRuleBasedEvaluation()`
Verifies that targeting rules (user attributes, segments, custom predicates) evaluate to the correct variant for a matrix of test identities. Covers positive matches, negative matches, and fallback behavior.

**Throws:** `InvalidOperationException` if rule parsing fails; `AssertionException` on unexpected variant resolution.

#### `void TestABTestVariantAssignment()`
Confirms deterministic, sticky variant assignment for A/B experiments. Ensures the same user identity consistently resolves to the same variant across repeated evaluations and that assignment ratios honor configured weights.

**Throws:** `InvalidOperationException` if experiment configuration is invalid; `AssertionException` on assignment inconsistency or ratio drift.

#### `async Task MonitorEvaluationPerformanceAsync()`
Runs a timed evaluation loop against the current flag configuration, recording per-call latency. Populates `AverageMs`, `MaxMs`, `P95Ms`, and `ThroughputPerSecond` on completion.

**Returns:** `Task` completing when the measurement window elapses.

**Throws:** `OperationCanceledException` if cancellation is requested; `InvalidOperationException` if no flags are configured for testing.

#### `async Task CompareImplementationsAsync()`
Executes `MonitorEvaluationPerformanceAsync` against two or more flag evaluation implementations (e.g., in-memory vs. distributed cache) and records comparative metrics for each.

**Returns:** `Task` completing when all implementations have been benchmarked.

**Throws:** `ArgumentException` if fewer than two implementations are registered; `OperationCanceledException` on cancellation.

#### `void PrintMetricsReport()`
Writes a formatted summary of the most recent performance run to standard output, including latency percentiles, throughput, and implementation comparison deltas.

**Throws:** `InvalidOperationException` if no performance data has been collected.

#### `async Task<HealthStatus> CheckHealthAsync()`
Probes the feature flag backend (configuration store, database, remote service) and evaluates latency against SLA thresholds. Returns a `HealthStatus` indicating `Healthy`, `Degraded`, or `Unhealthy`. Populates `IsHealthy`, `DatabaseConnectivity`, and `PerformanceOk`.

**Returns:** `Task<HealthStatus>` representing the aggregate health state.

**Throws:** `TimeoutException` if health probes exceed the configured deadline; `InvalidOperationException` if health check dependencies are not initialized.

#### `async Task RunLoadTestAsync()`
Launches a configurable concurrent workload simulating production evaluation traffic. Measures sustained throughput, error rates, and tail latency under load. Results are aggregated into the performance monitor properties.

**Returns:** `Task` completing when the load test duration expires or the target iteration count is reached.

**Throws:** `ArgumentOutOfRangeException` if concurrency or duration parameters are invalid; `OperationCanceledException` on cancellation; `InvalidOperationException` if the flag service is not in a testable state.

### Properties

#### `string FlagKey { get; }`
The unique identifier of the feature flag under test. Set during test initialization and used by all evaluation scenarios.

#### `DateTime Timestamp { get; }`
UTC timestamp marking when the last test, benchmark, or health check execution completed.

#### `double AverageMs { get; }`
Mean evaluation latency in milliseconds from the most recent performance or load test run.

#### `long MaxMs { get; }`
Maximum observed evaluation latency in milliseconds from the most recent run.

#### `double P95Ms { get; }`
95th percentile evaluation latency in milliseconds from the most recent run.

#### `double ThroughputPerSecond { get; }`
Sustained evaluations per second achieved during the most recent load test.

#### `bool IsHealthy { get; }`
Aggregate health indicator: `true` when `DatabaseConnectivity` and `PerformanceOk` are both `true`.

#### `bool DatabaseConnectivity { get; }`
Indicates successful connection to the flag configuration store during the last health check.

#### `bool PerformanceOk { get; }`
Indicates that the last measured `P95Ms` falls within the configured SLA threshold.

## Usage

### Example 1: Running the Full Test Suite

```csharp
var testHarness = new FeatureFlagServiceTestExample
{
    FlagKey = "new-checkout-flow"
};

testHarness.TestPercentageRollout();
testHarness.TestRuleBasedEvaluation();
testHarness.TestABTestVariantAssignment();

await testHarness.MonitorEvaluationPerformanceAsync();
testHarness.PrintMetricsReport();

var health = await testHarness.CheckHealthAsync();
if (health != HealthStatus.Healthy)
{
    throw new InvalidOperationException($"Feature flag subsystem unhealthy: {health}");
}
```

### Example 2: Comparative Benchmark and Load Test

```csharp
var harness = new FeatureFlagServiceTestExample
{
    FlagKey = "recommendation-algorithm-v2"
};

await harness.CompareImplementationsAsync();
harness.PrintMetricsReport();

await harness.RunLoadTestAsync();

Console.WriteLine($"Sustained throughput: {harness.ThroughputPerSecond:F0} eval/s");
Console.WriteLine($"P95 latency: {harness.P95Ms:F2} ms");

if (!harness.PerformanceOk)
{
    Console.WriteLine("WARNING: P95 latency exceeds SLA threshold");
}
```

## Notes

- **Thread Safety**: The test harness instance is not thread-safe. Concurrent invocation of test methods, benchmarks, or load tests on the same instance will produce corrupted metrics and undefined behavior. Create separate instances per parallel test execution.
- **Statefulness**: Properties (`AverageMs`, `P95Ms`, `ThroughputPerSecond`, `IsHealthy`, etc.) reflect only the most recent operation. They are not cumulative across multiple runs.
- **Async Cancellation**: All `async Task` methods honor `CancellationToken` via the standard `OperationCanceledException` pattern. Callers should supply a token for long-running load tests.
- **Health Check Dependencies**: `CheckHealthAsync` requires a configured flag backend (database, config service, etc.). In unit test environments, ensure a test double or in-memory provider is registered; otherwise `DatabaseConnectivity` will be `false`.
- **Statistical Validity**: `TestPercentageRollout` and `TestABTestVariantAssignment` rely on sample sizes sufficient for statistical confidence. Overriding the default iteration count (if exposed via configuration) may invalidate assertions.
- **Resource Cleanup**: `RunLoadTestAsync` may spawn background threads or connection pools. Ensure the process lifetime allows completion, or cancel explicitly to release resources.
- **Metric Precision**: Latency measurements use `Stopwatch.GetTimestamp()` for high resolution. On systems with low timer frequency, `MaxMs` and `P95Ms` may exhibit quantization artifacts.
