# GradualRolloutSchedulerServiceTests

Test suite for the `GradualRolloutSchedulerService` class, covering validation of input arguments, correct handling of rollout strategies, and proper interaction with cancellation tokens.

## API

### `public GradualRolloutSchedulerServiceTests()`
Constructor for the test class. No parameters; creates an instance used by the test runner to execute the test methods.

### `public async Task GetScheduleStatusAsync_WithNegativeId_ThrowsArgumentException`
Verifies that calling `GetScheduleStatusAsync` with a negative feature flag identifier throws an `ArgumentException`.  
- **Parameters:** None (the test supplies a negative ID internally).  
- **Return Value:** A completed `Task` when the assertion passes; the test fails if no exception is thrown or if a different exception type is raised.  
- **Throws:** The test itself throws if the method under test does not throw an `ArgumentException` for a negative ID.

### `public async Task AdvanceRolloutAsync_WithInvalidId_ThrowsArgumentException`
Ensures that `AdvanceRolloutAsync` throws an `ArgumentException` when provided with an identifier that does not correspond to any known feature flag.  
- **Parameters:** None (invalid ID is supplied inside the test).  
- **Return Value:** Completed `Task` on successful assertion.  
- **Throws:** Test fails if the method does not throw an `ArgumentException` for an invalid ID.

### `public async Task AdvanceRolloutAsync_WithEmptyAdvancedBy_ThrowsArgumentException`
Confirms that supplying an empty or whitespace‑only string for the `advancedBy` parameter to `AdvanceRolloutAsync` results in an `ArgumentException`.  
- **Parameters:** None (empty string is used internally).  
- **Return Value:** Completed `Task` when the expected exception is observed.  
- **Throws:** Test fails if no `ArgumentException` is thrown.

### `public async Task ProcessScheduledRolloutsAsync_WithNoStrategies_ReturnsZero`
Checks that when the service has no rollout strategies configured, `ProcessScheduledRolloutsAsync` returns zero, indicating no flags were processed.  
- **Parameters:** None.  
- **Return Value:** Completed `Task`; the test asserts that the returned integer equals `0`.  
- **Throws:** Test fails if the returned count is not zero.

### `public async Task ProcessScheduledRolloutsAsync_WithInactiveStrategy_SkipsIt`
Validates that an inactive rollout strategy is ignored during processing, leaving the associated feature flag unchanged.  
- **Parameters:** None.  
- **Return Value:** Completed `Task`; the test asserts that the flag’s rollout percentage remains as expected.  
- **Throws:** Test fails if the flag is modified despite the strategy being inactive.

### `public async Task ProcessScheduledRolloutsAsync_WithCancellation_StopsProcessing`
Ensures that supplying a cancellation token that is triggered causes `ProcessScheduledRolloutsAsync` to halt further processing and exit promptly.  
- **Parameters:** None (cancellation token is created and cancelled inside the test).  
- **Return Value:** Completed `Task`; the test asserts that processing stopped before completing all scheduled items.  
- **Throws:** Test fails if processing continues after cancellation.

### `public async Task ProcessScheduledRolloutsAsync_CalculatesCorrectCount`
Confirms that the method correctly counts and processes the number of scheduled rollouts that are due, returning the accurate total.  
- **Parameters:** None.  
- **Return Value:** Completed `Task`; the test asserts that the returned count matches the expected number of due rollouts.  
- **Throws:** Test fails if the count is incorrect.

### `public async Task AdvanceRolloutAsync_WithValidStrategy_UpdatesFlag`
Verifies that when a valid rollout strategy is supplied, `AdvanceRolloutAsync` updates the associated feature flag’s rollout percentage accordingly.  
- **Parameters:** None (valid strategy and flag are set up inside the test).  
- **Return Value:** Completed `Task`; the test asserts the flag’s new rollout percentage.  
- **Throws:** Test fails if the flag is not updated as expected.

### `public async Task AdvanceRolloutAsync_WithoutStrategy_ReturnsFalse`
Ensures that attempting to advance a rollout for a feature flag that lacks an associated strategy causes `AdvanceRolloutAsync` to return `false`, indicating no change was made.  
- **Parameters:** None.  
- **Return Value:** Completed `Task`; the test asserts the method returns `false`.  
- **Throws:** Test fails if the method returns `true` or throws an exception.

## Usage

The test class is intended to be executed by a unit‑test runner (e.g., xUnit, NUnit, or MSTest). Below are two examples showing how a developer might interact with the tests in practice.

### Example 1: Running a specific test via the dotnet CLI
```bash
# Assuming the project is built and the test assembly is available
dotnet test dotnet-feature-flags.Tests --filter "FullyQualifiedName~GradualRolloutSchedulerServiceTests.GetScheduleStatusAsync_WithNegativeId_ThrowsArgumentException"
```
This command invokes the test runner and executes only the test that validates the exception for a negative identifier.

### Example 2: Invoking a test method directly in code (for debugging)
```csharp
using System.Threading.Tasks;
using Xunit;

public class TestRunner
{
    [Fact]
    public async Task RunAdvanceRolloutWithValidStrategyTest()
    {
        var testInstance = new GradualRolloutSchedulerServiceTests();
        await testInstance.AdvanceRolloutAsync_WithValidStrategy_UpdatesFlag();
        // If the method completes without throwing, the test passed.
    }
}
```
Here we instantiate the test class and call a test method directly; the method will throw if the assertion fails, allowing the caller to catch the exception and treat it as a test failure.

## Notes

- **Argument validation:** All tests that verify `ArgumentException` assume the service validates inputs at the method boundary. Supplying a negative ID, an unknown ID, or an empty `advancedBy` string should never proceed to internal logic.
- **Cancellation:** The cancellation test relies on a `CancellationTokenSource` that is triggered before the processing loop begins. If the service does not periodically check the token, the test will fail, indicating a missing cancellation check.
- **Thread‑safety:** The test class itself does not maintain static state; each test method operates on its own instance of `GradualRolloutSchedulerService` (or mocks). Consequently, the tests are safe to run in parallel. However, the production service is **not** guaranteed to be thread‑safe; concurrent calls to `ProcessScheduledRolloutsAsync` or `AdvanceRolloutAsync` on the same service instance may lead to race conditions and should be synchronized by the caller if needed.
- **Edge cases:**  
  - Negative identifiers are caught before any lookup.  
  - Empty or whitespace‑only `advancedBy` values are treated as invalid input.  
  - Inactive strategies are short‑circuited to avoid unnecessary work.  
  - When no strategies are registered, the service returns zero without attempting to access any flag store.  
  - The service correctly accumulates the count of processed rollouts only for those whose scheduled time has elapsed.  

These observations follow directly from the method signatures and the documented behavior of the corresponding production methods.
