# AuditLogServiceTests

Unit test class for the `AuditLogService` component, verifying the correctness of audit‑log retrieval methods under various input conditions and failure scenarios.

## API

### AuditLogServiceTests
- **Purpose**: Contains test methods that exercise the public API of `AuditLogService`.  
- **Parameters**: None (the class has a parameterless constructor).  
- **Return Value**: N/A.  
- **Throws**: Does not throw exceptions directly; test methods may propagate exceptions from the system under test or the test framework (e.g., `AssertFailedException`).

### GetAuditLogsAsync_WithValidId_ReturnsLogs
- **Purpose**: Verifies that `AuditLogService.GetAuditLogsAsync` returns the expected logs when supplied with a valid identifier.  
- **Parameters**: None (the test supplies a hard‑coded valid ID internally).  
- **Return Value**: `Task` representing the asynchronous test execution.  
- **Throws**: Propagates `ArgumentException` if the service incorrectly treats the valid ID as invalid; otherwise the test passes.

### GetAuditLogsAsync_WithInvalidId_ThrowsArgumentException
- **Purpose**: Confirms that an invalid identifier causes `AuditLogService.GetAuditLogsAsync` to throw an `ArgumentException`.  
- **Parameters**: None (the test supplies an invalid ID internally).  
- **Return Value**: `Task`.  
- **Throws**: The test fails if the method does not throw an `ArgumentException`; any other exception is considered unexpected.

### GetAuditLogsAsync_WhenRepositoryThrows_ThrowsFeatureFlagDataException
- **Purpose**: Ensures that when the underlying repository throws an exception, the service wraps it in a `FeatureFlagDataException`.  
- **Parameters**: None (the test mocks the repository to throw).  
- **Return Value**: `Task`.  
- **Throws**: The test fails if the service does not throw a `FeatureFlagDataException`.

### GetAuditLogsPagedAsync_WithValidParameters_ReturnsPaginatedLogs
- **Purpose**: Validates that paginated audit‑log retrieval works correctly with valid page number and size.  
- **Parameters**: None (the test supplies valid page number and size internally).  
- **Return Value**: `Task`.  
- **Throws**: Propagates unexpected exceptions; the test fails if the returned pagination does not match expectations.

### GetAuditLogsPagedAsync_WithInvalidPageNumber_ThrowsArgumentException
- **Purpose**: Checks that supplying a page number less than one results in an `ArgumentException`.  
- **Parameters**: None (the test supplies an invalid page number internally).  
- **Return Value**: `Task`.  
- **Throws**: The test fails if no `ArgumentException` is thrown.

### GetAuditLogsPagedAsync_WithInvalidPageSize_ThrowsArgumentException
- **Purpose**: Checks that supplying a page size less than one results in an `ArgumentException`.  
- **Parameters**: None (the test supplies an invalid page size internally).  
- **Return Value**: `Task`.  
- **Throws**: The test fails if no `ArgumentException` is thrown.

### GetAuditLogsByUserAsync_WithValidUser_ReturnsUserLogs
- **Purpose**: Verifies that retrieving audit logs for a valid user name returns the expected subset.  
- **Parameters**: None (the test supplies a valid user name internally).  
- **Return Value**: `Task`.  
- **Throws**: Propagates unexpected exceptions; the test fails if the result set is incorrect.

### GetAuditLogsByUserAsync_WithEmptyUser_ThrowsArgumentException
- **Purpose**: Ensures that an empty or whitespace‑only user name causes the method to throw an `ArgumentException`.  
- **Parameters**: None (the test supplies an empty user name internally).  
- **Return Value**: `Task`.  
- **Throws**: The test fails if no `ArgumentException` is thrown.

### GetRecentAuditLogsAsync_WithValidCount_ReturnsRecentLogs
- **Purpose**: Confirms that requesting a valid number of recent audit logs returns that many entries, ordered correctly.  
- **Parameters**: None (the test supplies a valid count internally).  
- **Return Value**: `Task`.  
- **Throws**: Propagates unexpected exceptions; the test fails if the count or ordering is wrong.

### GetRecentAuditLogsAsync_WithInvalidCount_ThrowsArgumentException
- **Purpose**: Ensures that a non‑positive count argument results in an `ArgumentException`.  
- **Parameters**: None (the test supplies an invalid count internally).  
- **Return Value**: `Task`.  
- **Throws**: The test fails if no `ArgumentException` is thrown.

### GetRecentAuditLogsAsync_WhenRepositoryThrows_ThrowsFeatureFlagDataException
- **Purpose**: Verifies that repository failures during recent‑log retrieval are propagated as `FeatureFlagDataException`.  
- **Parameters**: None (the test mocks the repository to throw).  
- **Return Value**: `Task`.  
- **Throws**: The test fails if the service does not wrap the repository exception in a `FeatureFlagDataException`.

## Usage

```csharp
// Example 1: Running a successful test case
var testSuite = new AuditLogServiceTests();
await testSuite.GetAuditLogsAsync_WithValidId_ReturnsLogs();
// If the test passes, execution continues; otherwise an exception is thrown.

// Example 2: Verifying exception handling
var testSuite = new AuditLogServiceTests();
try
{
    await testSuite.GetAuditLogsAsync_WithInvalidId_ThrowsArgumentException();
}
catch (ArgumentException)
{
    // Expected outcome – test succeeded
}
catch (Exception ex)
{
    // Unexpected exception type – test failed
    throw new InvalidOperationException("Test failed with unexpected exception.", ex);
}
```

## Notes

- The test class does not maintain mutable state; each method operates independently, making the class thread‑safe for concurrent execution by a test runner.  
- Edge cases covered include null or empty identifiers, negative or zero pagination values, and repository‑level failures.  
- All assertions are expressed through expected exception types; any deviation results in a test failure that propagates as an exception from the test method.  
- Because the methods are `async Task`, they must be awaited; failure to do so will result in the test not executing its logic and being marked as skipped or failed depending on the test framework.
