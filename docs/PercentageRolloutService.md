# PercentageRolloutService

The `PercentageRolloutService` class provides the core logic for determining whether a specific user falls within a defined percentage rollout range for a feature flag. By implementing the `IPercentageRolloutService` interface, it facilitates consistent user bucketing based on a unique user identifier and a seed value, ensuring that a user's eligibility remains stable across multiple evaluations while adhering to the configured percentage threshold.

## API

### Constructors

#### `public PercentageRolloutService()`
Initializes a new instance of the `PercentageRolloutService` class. This default constructor prepares the service to perform rollout evaluations using standard hashing algorithms for user bucketing.

### Methods

#### `public async Task<bool> EvaluateAsync(string userId, string seed, int percentage)`
Asynchronously evaluates whether the specified user is included in the rollout based on the provided percentage.
*   **Parameters**:
    *   `userId`: A unique identifier for the user being evaluated.
    *   `seed`: A string value used as a salt to ensure consistent hashing across different contexts or feature flags.
    *   `percentage`: An integer between 0 and 100 representing the percentage of users to include in the rollout.
*   **Returns**: A `Task<bool>` that resolves to `true` if the user is within the rollout range, or `false` otherwise.
*   **Throws**: Throws an `ArgumentException` if the `percentage` is outside the valid range of 0 to 100, or if `userId` or `seed` is null or empty.

#### `public bool IsUserInRollout(string userId, string seed, int percentage)`
Synchronously evaluates whether the specified user is included in the rollout based on the provided percentage.
*   **Parameters**:
    *   `userId`: A unique identifier for the user being evaluated.
    *   `seed`: A string value used as a salt to ensure consistent hashing.
    *   `percentage`: An integer between 0 and 100 representing the target rollout percentage.
*   **Returns**: `true` if the calculated user bucket is less than or equal to the specified percentage; otherwise, `false`.
*   **Throws**: Throws an `ArgumentException` if the `percentage` is not between 0 and 100 (inclusive), or if required string arguments are null or empty.

#### `public int GetUserBucket(string userId, string seed)`
Calculates the specific bucket value assigned to a user based on their ID and the provided seed. This value is an integer between 0 and 99.
*   **Parameters**:
    *   `userId`: A unique identifier for the user.
    *   `seed`: A string value used to salt the hash calculation.
*   **Returns**: An integer representing the user's bucket (0–99). This value remains constant for a given `userId` and `seed` pair.
*   **Throws**: Throws an `ArgumentException` if `userId` or `seed` is null or empty.

## Usage

### Example 1: Synchronous Evaluation in a Request Pipeline
This example demonstrates how to use the synchronous method to quickly determine feature availability within a synchronous context, such as a middleware component or a simple helper method.

```csharp
using Microsoft.FeatureManagement;

public class FeatureGate
{
    private readonly PercentageRolloutService _rolloutService;

    public FeatureGate()
    {
        _rolloutService = new PercentageRolloutService();
    }

    public bool CanAccessNewDashboard(string userId)
    {
        const string seed = "NewDashboardFeature";
        int rolloutPercentage = 25; // 25% of users

        try
        {
            return _rolloutService.IsUserInRollout(userId, seed, rolloutPercentage);
        }
        catch (ArgumentException ex)
        {
            // Handle invalid input (e.g., log error, default to false)
            return false;
        }
    }
}
```

### Example 2: Asynchronous Evaluation with Dynamic Configuration
This example illustrates the asynchronous evaluation method, which is suitable for integration with asynchronous configuration providers or when performing non-blocking operations in high-throughput services.

```csharp
using Microsoft.FeatureManagement;
using System.Threading.Tasks;

public class FeatureEvaluator
{
    private readonly PercentageRolloutService _rolloutService;

    public FeatureEvaluator()
    {
        _rolloutService = new PercentageRolloutService();
    }

    public async Task<bool> CheckExperimentalFeatureAsync(string userId, string featureSeed, int percentage)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Asynchronously evaluate the rollout
        bool isInRollout = await _rolloutService.EvaluateAsync(userId, featureSeed, percentage);
        
        return isInRollout;
    }
}
```

## Notes

*   **Consistency**: The `GetUserBucket` method ensures that a specific `userId` and `seed` combination always yields the same bucket value (0–99). Consequently, `IsUserInRollout` and `EvaluateAsync` will return deterministic results for the same inputs, preventing users from flickering in and out of a feature rollout during a session.
*   **Input Validation**: All public members strictly validate input parameters. Passing a `percentage` value less than 0 or greater than 100 will result in an exception. Similarly, null or empty strings for `userId` or `seed` are not permitted and will throw `ArgumentException`.
*   **Thread Safety**: The `PercentageRolloutService` is stateless regarding user data; it does not maintain internal mutable state between calls. Therefore, a single instance of this class is thread-safe and can be safely registered as a singleton service and shared across concurrent requests.
*   **Bucket Distribution**: The hashing algorithm used internally distributes users uniformly across the 100 available buckets. A percentage of `N` effectively includes users whose bucket value is less than `N`.
