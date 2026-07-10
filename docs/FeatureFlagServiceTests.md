# FeatureFlagServiceTests

Unit test class for the `FeatureFlagService` that verifies the behavior of feature flag evaluation under various conditions such as missing keys, disabled flags, rollout configurations, and duplicate flag creation.

## API

### `FeatureFlagServiceTests`
- **Purpose**: Contains test methods that validate the public API of `FeatureFlagService`.
- **Parameters**: None (static test class).
- **Return Value**: N/A.
- **Throws**: Does not throw directly; individual test methods may throw exceptions to signal test failures.

### `IsEnabledAsync_WithEmptyKey_ThrowsArgumentException`
- **Purpose**: Confirms that calling `IsEnabledAsync` with an empty or whitespace‑only flag key results in an `ArgumentException`.
- **Parameters**: None.
- **Return Value**: `Task` representing the asynchronous operation.
- **Throws**: The test fails if the method under test does not throw an `ArgumentException`.

### `IsEnabledAsync_WithInvalidUserContext_ThrowsInvalidOperationException`
- **Purpose**: Verifies that supplying a user context that does not meet validation rules causes `IsEnabledAsync` to throw an `InvalidOperationException`.
- **Parameters**: None.
- **Return Value**: `Task`.
- **Throws**: The test fails if no `InvalidOperationException` is thrown.

### `IsEnabledAsync_WhenFlagNotFound_ReturnsFalse`
- **Purpose**: Ensures that querying a flag that has not been registered returns `false`.
- **Parameters**: None.
- **Return Value**: `Task<bool>` where the result is `false`.
- **Throws**: None expected; the test fails if an exception is thrown or the result is not `false`.

### `IsEnabledAsync_WhenFlagIsDisabled_ReturnsFalse`
- **Purpose**: Checks that a flag explicitly set to disabled evaluates to `false`.
- **Parameters**: None.
- **Return Value**: `Task<bool>` where the result is `false`.
- **Throws**: None expected.

### `IsEnabledAsync_WithFullRolloutAndEnabledFlag_ReturnsTrue`
- **Purpose**: Validates that a flag with a 100 % rollout percentage and an enabled state returns `true`.
- **Parameters**: None.
- **Return Value**: `Task<bool>` where the result is `true`.
- **Throws**: None expected.

### `IsEnabledAsync_WithNoneRolloutAndEnabledFlag_ReturnsFalse`
- **Purpose**: Confirms that a flag with a 0 % rollout percentage (none) returns `false` even when the flag is enabled.
- **Parameters**: None.
- **Return Value**: `Task<bool>` where the result is `false`.
- **Throws**: None expected.

### `IsEnabledAsync_WithPercentageRollout_DelegatesToPercentageService`
- **Purpose**: Asserts that when a flag uses a percentage‑based rollout (neither 0 % nor 100 %), the service delegates the decision to the injected percentage service.
- **Parameters**: None.
- **Return Value**: `Task`.
- **Throws**: None expected; the test fails if delegation does not occur.

### `CreateFeatureFlagAsync_WhenKeyAlreadyExists_ThrowsInvalidOperationException`
- **Purpose**: Ensures that attempting to create a feature flag with a key that already exists throws an `InvalidOperationException`.
- **Parameters**: None.
- **Return Value**: `Task`.
- **Throws**: The test fails if no `InvalidOperationException` is thrown.

## Usage

```csharp
// Example 1: Verifying that an empty key throws.
[Fact]
public async Task EmptyKey_Throws()
{
    var service = new FeatureFlagService(/* dependencies */);
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.IsEnabledAsync(string.Empty, TestUserContext.Create()));
}

// Example 2: Testing percentage rollout delegation.
[Fact]
public async Task PercentageRollout_Delegates()
{
    var percentageServiceMock = new Mock<IFlagPercentageService>();
    percentageServiceMock.Setup(m => m.Evaluate(It.IsAny<string>(), It.IsAny<UserContext>()))
                         .Returns(true);

    var service = new FeatureFlagService(percentageServiceMock.Object, /* other deps */);
    await service.CreateFeatureFlagAsync("percent-flag", new FlagDefinition { RolloutPercentage = 50 });

    var result = await service.IsEnabledAsync("percent-flag", TestUserContext.Create());
    Assert.True(result);
    percentageServiceMock.Verify(m => m.Evaluate("percent-flag", It.IsAny<UserContext>()), Times.Once);
}
```

## Notes

- The test class does not maintain any mutable state; each test method operates on fresh instances of `FeatureFlagService` and its dependencies, making the tests safe to run in parallel.
- Edge cases covered include:
  - Empty or whitespace flag keys (`ArgumentException`).
  - Invalid user context (`InvalidOperationException`).
  - Missing flags (returns `false`).
  - Disabled flags (returns `false`).
  - Full rollout (returns `true`).
  - No rollout (returns `false`).
  - Percentage rollout (delegates to `IFlagPercentageService`).
  - Duplicate flag creation (`InvalidOperationException`).
- No thread‑safety guarantees are required for the test class itself; however, the production `FeatureFlagService` should be thread‑safe if intended for concurrent access, which is implicitly validated by the isolation of these tests.
