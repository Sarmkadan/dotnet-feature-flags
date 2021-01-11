# PercentageRolloutServiceTests

This test class contains unit tests for the `PercentageRolloutService` implementation in the `dotnet-feature-flags` library. It validates argument checking, correct rollout evaluation for boundary percentages, deterministic hashing of user contexts, and proper distribution of users across the rollout range.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `EvaluateAsync_WithNullFeatureFlag_ThrowsArgumentNullException` | Verifies that calling `EvaluateAsync` with a `null` feature flag results in an `ArgumentNullException`. | none | `Task` (completed when the assertion finishes) | The test fails if the method does not throw `ArgumentNullException`. |
| `EvaluateAsync_WithNullUserContext_ThrowsArgumentNullException` | Verifies that calling `EvaluateAsync` with a `null` user context results in an `ArgumentNullException`. | none | `Task` | The test fails if the method does not throw `ArgumentNullException`. |
| `EvaluateAsync_With100Percent_ReturnsTrue` | Confirms that when a feature flag is configured for 100 % rollout, `EvaluateAsync` returns `true` for any valid user context. | none | `Task` | The test fails if the result is not `true`. |
| `EvaluateAsync_With0Percent_ReturnsFalse` | Confirms that when a feature flag is configured for 0 % rollout, `EvaluateAsync` returns `false` for any valid user context. | none | `Task` | The test fails if the result is not `false`. |
| `IsUserInRollout_SameUserConsistentHash_ReturnsSameResult` | Ensures that repeated calls to `IsUserInRollout` with the same user context and flag produce identical results, confirming hash stability. | none | `void` | The test fails if results differ across calls. |
| `GetUserBucket_ReturnsValueBetween0And99` | Checks that the internal bucketing function returns an integer in the inclusive range `[0, 99]`. | none | `void` | The test fails if the returned value lies outside the range. |
| `IsUserInRollout_WithNullUserContext_ThrowsArgumentNullException` | Validates that passing a `null` user context to `IsUserInRollout` throws `ArgumentNullException`. | none | `void` | The test fails if no exception is thrown. |
| `IsUserInRollout_WithNullFlagKey_ThrowsArgumentException` | Validates that passing a `null` or empty flag key to `IsUserInRollout` throws `ArgumentException`. | none | `void` | The test fails if no exception is thrown. |
| `IsUserInRollout_WithInvalidPercentage_ThrowsArgumentException` | Ensures that configuring a rollout percentage outside the `[0, 100]` range causes `IsUserInRollout` to throw `ArgumentException`. | none | `void` | The test fails if no exception is thrown. |
| `IsUserInRollout_DistributionTest` | Performs a statistical check that, over many random user contexts, the proportion of users flagged as “in rollout” approximates the configured percentage (within a reasonable tolerance). | none | `void` | The test fails if the observed distribution deviates beyond the tolerance. |

## Usage

The following examples illustrate how to interact with the `PercentageRolloutService` in production code. The test class itself is exercised by a test runner (e.g., xUnit, NUnit, or MSTest) and is not instantiated directly in application logic.

```csharp
// Example 1: Evaluating a feature flag with a 50 % rollout.
var service = new PercentageRolloutService();
var flag = new FeatureFlag { Key = "NewUi", RolloutPercentage = 50 };
var user = new UserContext { Id = "user-123" };

bool isEnabled = await service.EvaluateAsync(flag, user);
// isEnabled will be true for roughly half of all distinct user IDs.
```

```csharp
// Example 2: Determining a user's bucket for manual experimentation.
var service = new PercentageRolloutService();
var user = new UserContext { Id = "user-456" };

int bucket = service.GetUserBucket(user.Key); // Returns a value between 0 and 99.
// You can compare the bucket against a feature's rollout percentage to decide visibility.
```

## Notes

- **Argument validation**: All public methods of `PercentageRolloutService` throw `ArgumentNullException` when a required reference argument is `null`. The `IsUserInRollout` overload also throws `ArgumentException` for an empty or `null` flag key and for percentages outside the inclusive range `[0, 100]`.
- **Thread safety**: The service does not maintain mutable state; all operations depend only on their input parameters. Consequently, it is safe to call its methods from multiple threads concurrently without external synchronization.
- **Hash consistency**: The hashing algorithm used to map a user identifier to a bucket is deterministic. Repeated calls with the same user context and flag key will always yield the same bucket, ensuring stable feature flag evaluation across requests.
- **Distribution**: Over a sufficiently large set of distinct user identifiers, the distribution of bucket values approximates a uniform distribution. This property enables the rollout percentage to be interpreted as the probability that a random user will be considered “in the rollout.”
- **Floating‑point considerations**: The service uses integer bucket arithmetic (`0–99`) to avoid rounding errors; therefore, a rollout percentage of `0` never yields `true`, and a percentage of `100` always yields `true`. Intermediate percentages are applied by checking whether the user's bucket is less than the configured percentage.
