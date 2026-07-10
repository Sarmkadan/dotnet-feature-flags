# FeatureFlagOptions

`FeatureFlagOptions` is a configuration class used to control the behavior and constraints of the feature flagging system in `dotnet-feature-flags`. It allows customization of caching, logging, rule limits, and evaluation settings to tailor the system to application needs.

## API

### `EnableCache`
- **Purpose**: Determines whether feature flag evaluations should use a caching layer to improve performance.
- **Type**: `bool`
- **Default**: `true`
- **Notes**: When enabled, the system caches flag evaluations for the duration specified by `CacheDurationMinutes`.

### `CacheDurationMinutes`
- **Purpose**: Specifies the duration (in minutes) for which feature flag evaluations are cached.
- **Type**: `int`
- **Default**: `5`
- **Constraints**: Must be a non-negative integer. Values less than `0` will be treated as `0`.
- **Notes**: Only applicable when `EnableCache` is `true`.

### `AuditLogRetentionDays`
- **Purpose**: Defines the number of days audit logs are retained before automatic cleanup.
- **Type**: `int`
- **Default**: `30`
- **Constraints**: Must be a non-negative integer. Values less than `0` will be treated as `0`.
- **Notes**: Only applicable when `EnableAuditLogging` or `EnableAuditLog` is `true`.

### `EnableAuditLogging`
- **Purpose**: Enables or disables detailed audit logging for feature flag evaluations and changes.
- **Type**: `bool`
- **Default**: `true`
- **Notes**: When enabled, logs include evaluation details, rule changes, and system events.

### `MaxRulesPerFlag`
- **Purpose**: Sets the maximum number of rules that can be defined for a single feature flag.
- **Type**: `int`
- **Default**: `100`
- **Constraints**: Must be a positive integer. Values less than `1` will be treated as `1`.
- **Notes**: Exceeding this limit during flag creation or update will result in an exception.

### `MaxConditionsPerRule`
- **Purpose**: Limits the number of conditions that can be specified within a single rule.
- **Type**: `int`
- **Default**: `10`
- **Constraints**: Must be a positive integer. Values less than `1` will be treated as `1`.
- **Notes**: Exceeding this limit during rule creation will result in an exception.

### `MaxVariantsPerFlag`
- **Purpose**: Restricts the number of variants (e.g., rollout percentages or discrete values) that can be defined for a feature flag.
- **Type**: `int`
- **Default**: `10`
- **Constraints**: Must be a positive integer. Values less than `1` will be treated as `1`.
- **Notes**: Exceeding this limit during flag creation will result in an exception.

### `LogEvaluationDetails`
- **Purpose**: Controls whether detailed evaluation logs (e.g., rule matches, variant assignments) are emitted during feature flag resolution.
- **Type**: `bool`
- **Default**: `false`
- **Notes**: When enabled, logs provide granular insight into how a flag’s value was determined.

### `EnableAuditLog`
- **Purpose**: Toggles the audit logging subsystem on or off.
- **Type**: `bool`
- **Default**: `true`
- **Notes**: When `false`, audit logging is disabled regardless of `EnableAuditLogging`.

### `DefaultRolloutPercentage`
- **Purpose**: Specifies the default percentage (0–100) used for rollout variants when no explicit variants are defined.
- **Type**: `int`
- **Default**: `100`
- **Constraints**: Must be an integer between `0` and `100` (inclusive). Out-of-range values will be clamped.
- **Notes**: Used as a fallback when a flag has no variants or during gradual rollouts.

### `IsValid`
- **Purpose**: Validates the current configuration for internal consistency and constraint compliance.
- **Type**: `bool`
- **Return Value**: `true` if all properties are within valid ranges; otherwise, `false`.
- **Notes**: Does not throw exceptions; intended for runtime validation checks.

## Usage

### Example 1: Basic Configuration
