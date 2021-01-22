# PercentageRolloutServiceExtensions

Static extension methods that provide percentage‑based rollout logic for feature flags. These helpers compute bucket assignments and evaluate whether a user or bucket falls within a configured rollout percentage, enabling consistent, deterministic feature flagging across services.

## API

### EvaluateMultipleAsync
- **Purpose:** Asynchronously evaluates a collection of feature flags for a given context, returning whether each flag is enabled for the user or request.
- **Parameters:** Implementation‑defined; see the source code for the exact signature.
- **Return Value:** `Task<Dictionary<string, bool>>` where the key is the feature flag name and the value indicates whether the flag indicates the user is in the rollout for that flag.
- **flag is enabled** (`true`) or **disabled** (`false`).
- **Exceptions:** May throw `ArgumentNullException` if required arguments are `null`, or any exception propagated from the underlying feature‑flag service.

### IsBucketInRollout
- **Purpose:** Determines whether a supplied bucket identifier lies within the rollout range for a feature.
- **Parameters:** Implementation‑defined; see the source code for the exact signature.
- **Return Value:** `bool` – `true` if the bucket is included in the rollout, otherwise `false`.
- **Exceptions:** May throw `ArgumentOutOfRangeException` if the bucket value is outside the expected range (typically 0‑99), or `ArgumentNullException` for null inputs.

### GetUserBucket
- **Purpose:** Computes a deterministic bucket number (0‑99) for a user based on identifying information such as a user ID or tenant ID.
- **Parameters:** Implementation‑defined; see the source code for the exact signature.
- **Return Value:** `int` representing the bucket assigned to the user.
- **Exceptions:** May throw `ArgumentNullException` if the user identifier is `null`, or `FormatException` if the identifier cannot be parsed.

### IsUserInRollout
- **Purpose:** Checks whether a specific user is included in the rollout for a particular feature flag.
- **Parameters:** Implementation‑defined; see the source code for the exact signature.
- **Return Value:** `bool` – `true` if the user is within the rollout percentage for the flag, otherwise `false`.
- **Exceptions:** May throw `ArgumentNullException` if the user or flag identifier is `null`, or other exceptions from the underlying bucket calculation.

## Usage

```csharp
using Microsoft.FeatureManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

// Example 1: Evaluate multiple flags at once
public async Task<Dictionary<string, bool>> GetFeatureFlagsAsync(string userId, IEnumerable<string> featureNames)
{
    // Assuming an IPercentageRolloutService is registered via DI
    var rolloutService = serviceProvider.GetRequiredService<IPercentageRolloutService>();
    var results = await PercentageRolloutServiceExtensions.EvaluateMultipleAsync(
        rolloutService, userId, featureNames);
    return results;
}
```

```csharp
using Microsoft.FeatureManagement;

// Example 2: Check a single flag for a user
public bool IsFeatureEnabledForUser(string userId, string featureName)
{
    var rolloutService = serviceProvider.GetRequiredService<IPercentageRolloutService>();
    return PercentageRolloutServiceExtensions.IsUserInRollout(
        rolloutService, userId, featureName);
}
```

## Notes
- The methods are **pure** with respect to their inputs; they do not modify any internal state and therefore are thread‑safe when called concurrently with different arguments.
- Bucket values are expected to be in the range **0‑99** inclusive; values outside this range will cause `IsBucketInRollout` to return `false` and may throw an exception depending on the implementation.
- Passing `null` for any required argument (e.g., user identifier, feature name, or service instance) will result in an `ArgumentNullException`.
- Because the extensions rely on an underlying `IPercentageRolloutService`, any thread‑safety guarantees of that service apply to the overall operation. The extension methods themselves introduce no additional synchronization requirements.
