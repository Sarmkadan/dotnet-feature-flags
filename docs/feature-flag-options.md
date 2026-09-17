# Feature Flag Options

This document describes the configuration options available for the feature flag engine, which are loaded from `appsettings.json` under the "FeatureFlags" section.

## Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| EnableCache | `bool` | `true` | Enable caching of feature flags for performance optimization. |
| CacheDurationMinutes | `int` | `5` | Cache duration in minutes. |
| AuditLogRetentionDays | `int` | `365` | Number of days to retain audit logs. |
| EnableAuditLogging | `bool` | `true` | Enable audit logging for all feature flag changes. |
| MaxRulesPerFlag | `int` | `100` | Maximum number of rules per feature flag. |
| MaxConditionsPerRule | `int` | `50` | Maximum number of conditions per rule. |
| MaxVariantsPerFlag | `int` | `10` | Maximum number of variants per A/B test. |
| LogEvaluationDetails | `bool` | `false` | Log evaluation details for debugging. |
| EnableAuditLog | `bool` | `false` | Enable in-memory audit log for each flag evaluation. When true, every call to `IsEnabledAsync` records a `FlagEvaluationLog` entry that can be retrieved via `IFlagEvaluationLogService`. Opt-in to avoid overhead in high-throughput scenarios. |
| DefaultRolloutPercentage | `int` | `50` | Default percentage for new rollouts. |

## Validation

The `IsValid()` method validates the options configuration:
- `CacheDurationMinutes` must be greater than 0
- `AuditLogRetentionDays` must be greater than 0
- `MaxRulesPerFlag` must be greater than 0
- `MaxConditionsPerRule` must be greater than 0
- `MaxVariantsPerFlag` must be greater than 0
- `DefaultRolloutPercentage` must be between 0 and 100 (inclusive)

## Example appsettings.json

```json
{
  "FeatureFlags": {
    "EnableCache": true,
    "CacheDurationMinutes": 5,
    "AuditLogRetentionDays": 365,
    "EnableAuditLogging": true,
    "MaxRulesPerFlag": 100,
    "MaxConditionsPerRule": 50,
    "MaxVariantsPerFlag": 10,
    "LogEvaluationDetails": false,
    "EnableAuditLog": false,
    "DefaultRolloutPercentage": 50
  }
}
```