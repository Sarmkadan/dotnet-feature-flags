# FlagEvaluationLogServiceTests

Test class for verifying the behavior of `FlagEvaluationLogService`. The class contains a suite of unit tests that validate logging of feature flag evaluations, retrieval and filtering of logs, statistics generation, and proper handling of edge cases such as null inputs.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `FlagEvaluationLogServiceTests()` | Constructor for the test class. | None | Instance of `FlagEvaluationLogServiceTests`. | None. |
| `LogEvaluation_WithValidFlag_RecordsLog` | Verifies that a valid flag evaluation creates a log entry. | None | `void`. | Throws `Xunit.Sdk.AssertException` if the log is not recorded as expected. |
| `LogEvaluation_MultipleCalls_RecordsAllEvaluations` | Ensures that successive calls to `LogEvaluation` each produce a distinct log entry. | None | `void`. | Throws `Xunit.Sdk.AssertException` if any evaluation is missing or duplicated incorrectly. |
| `LogEvaluation_WithNullFlag_HandlesGracefully` | Confirms that passing a `null` flag key does not cause an exception and is handled safely. | None | `void`. | Throws `Xunit.Sdk.AssertException` if an unexpected exception is thrown. |
| `LogEvaluation_WithNullUserContext_HandlesGracefully` | Confirms that passing a `null` user context does not cause an exception and is handled safely. | None | `void`. | Throws `Xunit.Sdk.AssertException` if an unexpected exception is thrown. |
| `GetEvaluationLogs_WithNoLogs_ReturnsEmptyList` | Checks that when no evaluations have been logged, the log retrieval returns an empty collection. | None | `void`. | Throws `Xunit.Sdk.AssertException` if a non‑empty list is returned. |
| `GetEvaluationLogs_ReturnsCopy_NotOriginalList` | Validates that the list returned by `GetEvaluationLogs` is a copy, so modifications to the returned list do not affect internal storage. | None | `void`. | Throws `Xunit.Sdk.AssertException` if the returned list is the same reference as the internal list. |
| `ClearLogs_RemovesAllEvaluations` | Asserts that invoking `ClearLogs` removes all previously stored evaluation logs. | None | `void`. | Throws `Xunit.Sdk.AssertException` if any logs remain after clearing. |
| `LogEvaluation_RecordsTimestamp` | Ensures that each logged evaluation includes a timestamp that falls within an acceptable range of the call time. | None | `void`. | Throws `Xunit.Sdk.AssertException` if the timestamp is missing or outside the allowed tolerance. |
| `GetEvaluationLogsForFlag_FiltersByFlagKey` | Confirms that filtering logs by a specific flag key returns only evaluations for that flag. | None | `void`. | Throws `Xunit.Sdk.AssertException` if the filtered list contains incorrect entries. |
| `GetEvaluationLogStats_ReturnsAccurateMetrics` | Checks that the statistics method returns correct counts, success rates, and other metrics based on the logged evaluations. | None | `void`. | Throws `Xunit.Sdk.AssertException` if any metric deviates from the expected value. |
| `GetEvaluationLogsForUser_FiltersByUserId` | Verifies that filtering logs by user ID returns only evaluations performed for that user. | None | `void`. | Throws `Xunit.Sdk.AssertException` if the filtered list contains entries for other users. |
| `LogEvaluation_RecordsCorrectResult` | Validates that the result value (true/false) supplied to `LogEvaluation` is stored accurately in the log entry. | None | `void`. | Throws `Xunit.Sdk.AssertException` if the stored result differs from the input. |

## Usage

### Example 1: Running the test suite with xUnit

```csharp
using Xunit;
using DotNetFeatureFlags.Tests;

public class FlagEvaluationLogServiceTestsRunner
{
    // This test demonstrates how to invoke each test method programmatically.
    // In practice, you would let the test runner discover and execute the methods.
    [Fact]
    public void ExecuteAllFlagEvaluationLogServiceTests()
    {
        var testClass = new FlagEvaluationLogServiceTests();

        testClass.LogEvaluation_WithValidFlag_RecordsLog();
        testClass.LogEvaluation_MultipleCalls_RecordsAllEvaluations();
        testClass.LogEvaluation_WithNullFlag_HandlesGracefully();
        testClass.LogEvaluation_WithNullUserContext_HandlesGracefully();
        testClass.GetEvaluationLogs_WithNoLogs_ReturnsEmptyList();
        testClass.GetEvaluationLogs_ReturnsCopy_NotOriginalList();
        testClass.ClearLogs_RemovesAllEvaluations();
        testClass.LogEvaluation_RecordsTimestamp();
        testClass.GetEvaluationLogsForFlag_FiltersByFlagKey();
        testClass.GetEvaluationLogStats_ReturnsAccurateMetrics();
        testClass.GetEvaluationLogsForUser_FiltersByUserId();
        testClass.LogEvaluation_RecordsCorrectResult();
    }
}
```

### Example 2: Using the production service that the tests cover

```csharp
using DotNetFeatureFlags.Services; // Namespace containing FlagEvaluationLogService
using System;

var logService = new FlagEvaluationLogService();

// Log a few evaluations
logService.LogEvaluation(flagKey: "beta-feature", userId: "alice", result: true);
logService.LogEvaluation(flagKey: "beta-feature", userId: "bob", result: false);
logService.LogEvaluation(flagKey: "new-ui", userId: "alice", result: true);

// Retrieve all logs
var allLogs = logService.GetEvaluationLogs();
// allLogs.Count == 3

// Get logs for a specific flag
var betaLogs = logService.GetEvaluationLogsForFlag("beta-feature");
// betaLogs.Count == 2

// Get statistics
var stats = logService.GetEvaluationLogStats();
// stats.TotalEvaluations == 3
// stats.SuccessRate == 0.666... (2 out of 3)

// Clear the log store
logService.ClearLogs();
var afterClear = logService.GetEvaluationLogs();
// afterClear.Count == 0
```

## Notes

- The test methods assume a single‑threaded execution context. Concurrent calls to `FlagEvaluationLogService` from multiple threads are not covered by these tests and may lead to race conditions; the production service is not guaranteed to be thread‑safe without external synchronization.
- Null flag keys and null user contexts are handled gracefully: the service does not throw `ArgumentNullException`; instead, it records the evaluation with placeholder values or skips logging, depending on its internal implementation. The tests verify that no exception is propagated.
- `GetEvaluationLogs` returns a shallow copy of the internal list. Modifying the returned list (e.g., adding or removing items) will not affect the state of `FlagEvaluationLogService`, but changes to the objects contained within the list will be reflected because the objects themselves are not cloned.
- Timestamp verification in `LogEvaluation_RecordsTimestamp` allows a small tolerance (typically a few hundred milliseconds) to account for execution delay between capturing the time and storing it.
- Statistics returned by `GetEvaluationLogStats` are calculated from the current set of logs; calling `ClearLogs` resets all metrics to zero.
- Filtering methods (`GetEvaluationLogsForFlag`, `GetEvaluationLogsForUser`) perform exact string matches; they are case‑sensitive and do not trim whitespace. Providing an empty string or whitespace‑only key will return an empty list.
