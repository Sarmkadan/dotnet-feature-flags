# Percentage rollout service

`PercentageRolloutService` makes a deterministic percentage-rollout decision
for a `UserContext` and feature-flag key. It does not select users randomly on
each call. Instead, it assigns each user-and-flag pair to one of 100 buckets and
compares that bucket with the configured rollout percentage.

Source: `src/FeatureFlags/Services/PercentageRolloutService.cs`.

## Decision algorithm

For a user, flag key, and percentage `P`, evaluation follows these steps:

1. `GetUserBucket` delegates to
   `userContext.GetConsistentHash(featureFlagKey)`.
2. `UserContext` canonicalizes a numeric `UserId` by parsing it as a signed
   64-bit integer and converting it back to a string. Non-numeric IDs are used
   unchanged. For example, `"0042"` and `"42"` have the same canonical ID.
3. It constructs the UTF-8 hash input as
   `"{canonicalUserId}:{featureFlagKey}"`.
4. `HashingUtilities.ComputeHashBucket` computes the SHA-256 digest, converts
   its first four bytes to an unsigned 32-bit integer with
   `BitConverter.ToUInt32`, and calculates `hashValue % 100`.
5. `IsUserInRollout` enables the feature exactly when `bucket < P`.

In compact form:

```text
bucket = UInt32(first 4 bytes of SHA256(UTF8(canonicalUserId + ":" + flagKey))) % 100
enabled = bucket < rolloutPercentage
```

The bucket is always in the inclusive range `0..99`. Because the comparison is
strictly less than the percentage, the enabled buckets are:

| Percentage | Enabled buckets | Result |
| ---: | --- | --- |
| 0 | none | Every user is disabled |
| 1 | `0` | One bucket is enabled |
| 50 | `0..49` | Fifty buckets are enabled |
| 99 | `0..98` | Only bucket `99` is disabled |
| 100 | `0..99` | Every user is enabled |

Increasing a rollout from `P` to `P + 1` adds bucket `P`; users in the existing
buckets remain enabled. Decreasing it removes buckets from the upper end.

## Stability and isolation

The same canonical user ID and flag key produce the same bucket on repeated
evaluations, including across service instances and application restarts. The
flag key is part of the hash input, so the same user is bucketed independently
for different flags. The user's email, country, tier, region, custom attributes,
and other `UserContext` properties do not affect percentage bucketing.

The configured percentage represents a number of hash buckets, not a guarantee
that exactly that percentage of a finite user population will be enabled. With
a sufficiently large and varied population, SHA-256 should distribute inputs
approximately uniformly across the buckets.

`BitConverter.ToUInt32` uses the runtime platform's byte order. Bucket values
are therefore stable when deployments use the same endianness, as mainstream
.NET deployments normally do, but the implementation does not define a
platform-independent byte order.

## Public methods

### `EvaluateAsync`

```csharp
Task<bool> EvaluateAsync(
    FeatureFlag featureFlag,
    UserContext userContext,
    CancellationToken cancellationToken = default)
```

The concrete method reads `featureFlag.Key` and
`featureFlag.PercentageRollout`, then applies `IsUserInRollout`. Although it
returns a `Task<bool>`, evaluation is synchronous and the returned task is
already completed. The cancellation token is not inspected, so cancellation
has no effect. The interface exposes an overload without a cancellation token.

This method rejects:

- a null flag with `ArgumentNullException`;
- a null user context with `ArgumentNullException`; and
- a flag whose `PercentageRollout` is null with
  `InvalidFeatureFlagException`.

Errors raised while performing the bucket evaluation are logged and wrapped in
`FeatureFlagDataException`, unless the error already derives from
`FeatureFlagException`. Consequently, an invalid key or out-of-range percentage
supplied through a `FeatureFlag` is observed as a `FeatureFlagDataException`
whose inner exception describes the invalid argument.

### `IsUserInRollout`

```csharp
bool IsUserInRollout(
    UserContext userContext,
    string featureFlagKey,
    int rolloutPercentage)
```

This is the direct decision API. It requires a non-null user context, a
non-empty and non-whitespace flag key, and a percentage in the inclusive range
`0..100`. Invalid arguments produce `ArgumentNullException` or
`ArgumentException` directly.

### `GetUserBucket`

```csharp
int GetUserBucket(UserContext userContext, string featureFlagKey)
```

This returns the deterministic bucket without applying a percentage threshold.
It requires a non-null user context and a non-empty, non-whitespace flag key.
The service does not call `UserContext.IsValid`, so it does not require an email
address or reject an empty `UserId` itself.

## Example

```csharp
var flag = new FeatureFlag
{
    Key = "new-checkout",
    PercentageRollout = 25
};

var user = new UserContext
{
    UserId = "customer-123",
    Email = "customer-123@example.com"
};

bool enabled = await percentageRolloutService.EvaluateAsync(flag, user);
int bucket = percentageRolloutService.GetUserBucket(user, flag.Key);

// These are equivalent for a valid percentage rollout:
Debug.Assert(enabled == (bucket < flag.PercentageRollout.Value));
```

The service is registered as scoped by `AddFeatureFlagServices`. It keeps no
per-user assignment state; rollout continuity comes from preserving the user
ID, flag key, and hashing algorithm.
