# `FeatureFlagService`

`FeatureFlags.Services.FeatureFlagService` is the application service that coordinates feature-flag reads, evaluation, lifecycle changes, audit records, evaluation logs, and bulk-evaluation events. It implements `IFeatureFlagService`; the concrete class also exposes overloads with optional cancellation tokens for several operations.

The service does not own feature-flag data or implement every rollout algorithm itself. It delegates persistence to `IFeatureFlagRepository`, rules-based and A/B-test eligibility to `IRuleEvaluationService`, percentage rollout to `IPercentageRolloutService`, and audit persistence to `IAuditLogRepository`.

## Construction and dependencies

```csharp
public FeatureFlagService(
    IFeatureFlagRepository featureFlagRepository,
    IAuditLogRepository auditLogRepository,
    IRuleEvaluationService ruleEvaluationService,
    IPercentageRolloutService percentageRolloutService,
    IFlagEvaluationLogService evaluationLogService,
    IOptions<FeatureFlagOptions> options,
    ILogger<FeatureFlagService> logger,
    IFeatureFlagCache? featureFlagCache = null,
    IEventBus? eventBus = null)
```

The repository, evaluation services, options, and logger are required by the implementation. `featureFlagCache` and `eventBus` are optional:

- When a cache is supplied, single-flag reads by key use it. Collection queries still read from the repository.
- When an event bus is supplied, `EvaluateAllAsync` publishes one `BulkFeatureFlagEvaluation` event after the batch completes.
- `FeatureFlagOptions.EnableAuditLog` controls the separate in-memory `FlagEvaluationLog` written by `IsEnabledAsync`. It does not suppress audit records stored through `IAuditLogRepository`.

The constructor itself does not perform null validation. A missing required dependency will fail when the corresponding operation uses it.

## Evaluation behavior

### `IsEnabledAsync`

```csharp
public Task<bool> IsEnabledAsync(
    string featureFlagKey,
    UserContext userContext,
    CancellationToken cancellationToken = default)
```

Evaluates one flag for a user. `featureFlagKey` must be nonblank, and `userContext` must be non-null and valid. A valid context currently requires both `UserId` and `Email`.

The flag is loaded through `IFeatureFlagCache` when available, otherwise through the repository. A missing or globally disabled flag returns `false`. An enabled flag is evaluated by rollout type:

| `RolloutType` | Result |
| --- | --- |
| `Percentage` | Delegates to `IPercentageRolloutService` |
| `RulesBased` | Delegates to `IRuleEvaluationService` |
| `ABTest` | Delegates eligibility to `IRuleEvaluationService` |
| `Full` | `true` |
| `None` or unknown value | `false` |

Every completed evaluation, including missing and disabled flags, creates an `AuditAction.Evaluated` repository record. If `EnableAuditLog` is true, it also sends a `FlagEvaluationLog` to `IFlagEvaluationLogService` with a reason such as `FlagNotFound`, `FlagDisabled`, `PercentageRollout`, or `RulesBased`.

Non-`FeatureFlagException` failures, including failures while writing the audit record, are logged and wrapped in `FeatureFlagDataException`. Existing `FeatureFlagException` instances propagate unchanged. The cancellation token is accepted but is not passed to the lookup, evaluators, or audit repository by this implementation.

### `GetVariantAsync`

```csharp
public Task<string?> GetVariantAsync(
    string featureFlagKey,
    UserContext userContext,
    CancellationToken cancellationToken = default)
```

Returns the stable variant key for an enabled `ABTest` flag. It validates the key and context, resolves the flag ID by key, and then reloads the flag with variants. Missing keys throw `FeatureFlagNotFoundException`; a disabled/nonexistent loaded flag or a non-A/B-test flag returns `null` after writing an evaluation audit record.

Variants are ordered by ID. Their allocation percentages form cumulative buckets, compared with `userContext.GetConsistentHash(featureFlagKey)`. A selected variant records a user assignment and triggers `SaveChangesAsync`. If no allocation bucket matches, the first variant is returned as a fallback; if there are no variants, the result is `null`. The method does not call `IsEnabledAsync`, and its cancellation token is not used by the implementation.

### `EvaluateAllAsync`

```csharp
public Task<Dictionary<string, BulkEvaluationResult>> EvaluateAllAsync(
    UserContext userContext,
    bool includeVariants = false,
    bool includeReasons = false,
    CancellationToken cancellationToken = default)
```

Evaluates every flag returned by `GetEnabledAsync` and keys the result dictionary by flag key. It applies the same rollout dispatch described for `IsEnabledAsync`. Disabled flags are absent because the initial repository query requests enabled flags only.

`includeVariants` adds a variant for eligible A/B-test flags. `includeReasons` adds `Reason`, and adds `Percentage` for percentage rollouts. A failure evaluating one flag is isolated: that flag receives `Enabled = false` and, when requested, `Reason = "EvaluationError"`; evaluation then continues.

The cancellation token is checked before each flag and passed to event publication. After evaluation, the optional event bus receives one aggregate event with event type `BulkFeatureFlagEvaluation`, flag ID `0`, key `AllFlags`, and metadata containing counts and per-flag results. Event publication failures are logged and suppressed. Other batch-level non-`FeatureFlagException` failures are wrapped in `FeatureFlagDataException`.

Unlike `IsEnabledAsync`, bulk evaluation does not create per-flag audit or evaluation-log entries, except that requesting an A/B variant invokes `GetVariantAsync`, which does write an audit record.

## Query API

### `GetFeatureFlagAsync`

```csharp
public Task<FeatureFlag?> GetFeatureFlagAsync(
    int id,
    CancellationToken cancellationToken = default)
```

Loads a flag by integer ID directly from the repository. IDs must be greater than zero; no match returns `null`. The cancellation token is not forwarded.

### `GetFeatureFlagByKeyAsync`

```csharp
public Task<FeatureFlag?> GetFeatureFlagByKeyAsync(
    string key,
    CancellationToken cancellationToken = default)
```

Loads a flag by nonblank key, using the optional cache when present. No match returns `null`. The cancellation token is not forwarded.

### `GetAllFeatureFlagsAsync` and `GetEnabledFeatureFlagsAsync`

```csharp
public Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync()
public Task<IEnumerable<FeatureFlag>> GetEnabledFeatureFlagsAsync()
```

Read all flags or only enabled flags from the repository. If a cache is configured, each returned flag is invalidated by both key and ID. Despite the source comments, these methods do not populate cache entries; the locally calculated cache TTL is unused.

### `SearchFeatureFlagsAsync`

```csharp
public Task<IEnumerable<FeatureFlag>> SearchFeatureFlagsAsync(string searchTerm)
```

Delegates a nonblank term to the repository search. A null, empty, or whitespace term returns all flags instead.

### `GetStaleFlagsAsync`

```csharp
public Task<IEnumerable<FeatureFlag>> GetStaleFlagsAsync(TimeSpan olderThan)
```

Returns the repository's stale flags for a non-negative age window. A negative value throws `ArgumentException`.

### `GetFeatureFlagsETagAsync`

```csharp
public Task<string> GetFeatureFlagsETagAsync(
    CancellationToken cancellationToken = default)
```

Builds a deterministic MD5 configuration hash after ordering all flags by key. The hashed representation includes each flag's key, enabled state, rollout type, percentage, and round-trip-formatted `UpdatedAt` value. It does not include rules, variants, display text, or other properties. Failures are logged and wrapped in `FeatureFlagDataException`; the cancellation token is not used.

## Lifecycle API

### `CreateFeatureFlagAsync`

```csharp
public Task<FeatureFlag> CreateFeatureFlagAsync(
    FeatureFlag featureFlag,
    string createdBy,
    CancellationToken cancellationToken = default)
```

Requires a non-null flag and a nonblank actor. Duplicate keys cause `InvalidOperationException`. Before adding the entity, the service sets `CreatedBy` and `UpdatedBy` to the actor and both timestamps to `DateTime.UtcNow`. It then writes an `AuditAction.Created` record containing the new snapshot and logs the operation. It does not explicitly validate the rest of the model or invalidate the optional cache.

### `UpdateFeatureFlagAsync`

```csharp
public Task UpdateFeatureFlagAsync(
    FeatureFlag featureFlag,
    string updatedBy,
    CancellationToken cancellationToken = default)
```

Requires a non-null flag and nonblank actor. The existing entity is loaded by ID to capture its snapshot; if absent, `FeatureFlagNotFoundException` is thrown using the supplied flag key. The service updates `UpdatedBy` and `UpdatedAt`, persists the caller-supplied entity, and writes an `AuditAction.Updated` record with old and new snapshots. It does not invalidate the optional cache.

### `DeleteFeatureFlagAsync`

```csharp
public Task DeleteFeatureFlagAsync(
    int id,
    string deletedBy,
    CancellationToken cancellationToken = default)
```

Requires an ID greater than zero and a nonblank actor. The service loads the existing entity for its snapshot, throws `FeatureFlagNotFoundException` when absent, deletes it, and writes an `AuditAction.Deleted` record. It does not invalidate the optional cache.

### `EnableFeatureFlagAsync` and `DisableFeatureFlagAsync`

```csharp
public Task EnableFeatureFlagAsync(
    int id,
    string modifiedBy,
    CancellationToken cancellationToken = default)

public Task DisableFeatureFlagAsync(
    int id,
    string modifiedBy,
    CancellationToken cancellationToken = default)
```

Both require an ID greater than zero and throw `FeatureFlagNotFoundException` when no entity exists. They are idempotent: enabling an enabled flag or disabling a disabled flag returns without updating timestamps, audit records, or logs. A state change sets `IsEnabled`, `UpdatedBy`, and `UpdatedAt`, persists the entity, and records the corresponding `Enabled` or `Disabled` audit action.

Unlike create, update, and delete, these two methods do not validate `modifiedBy`; a blank value is stored in `UpdatedBy`, while the audit helper records the actor as `Anonymous`. Neither method invalidates the optional cache, and neither uses its cancellation token.

## Error and side-effect notes

- Argument validation is method-specific. Invalid IDs and blank keys/actors generally produce `ArgumentException`; null contexts produce `ArgumentNullException` in `IsEnabledAsync` and `EvaluateAllAsync`, while `GetVariantAsync` reports a null or otherwise invalid context as `InvalidOperationException`.
- Audit writes are part of the main operation, not best-effort. An audit failure after a repository mutation can make the call fail even though the mutation has already occurred.
- Evaluation-event publishing is best-effort and cannot fail `EvaluateAllAsync`.
- Most optional cancellation tokens currently have no effect. `EvaluateAllAsync` is the exception, although cancellation raised inside its per-flag loop is caught as a per-flag evaluation error and processing can continue to the next flag.
- Explicit `IFeatureFlagService` implementations for methods whose interface signatures omit cancellation tokens forward to the concrete token-taking overloads with the default token.

## Example

```csharp
var context = new UserContext
{
    UserId = "user-123",
    Email = "user-123@example.com",
    Country = "GB"
};

bool showNewCheckout = await featureFlagService.IsEnabledAsync(
    "new-checkout",
    context,
    cancellationToken);

Dictionary<string, BulkEvaluationResult> flags =
    await featureFlagService.EvaluateAllAsync(
        context,
        includeVariants: true,
        includeReasons: true,
        cancellationToken);
```
