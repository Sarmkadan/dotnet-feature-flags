// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Configuration;

/// <summary>
/// Configuration options for the feature flag engine.
/// Loaded from appsettings.json under the "FeatureFlags" section.
/// </summary>
public class FeatureFlagOptions
{
    /// <summary>
    /// Enable caching of feature flags for performance optimization.
    /// </summary>
    public bool EnableCache { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 5;

    /// <summary>
    /// Number of days to retain audit logs.
    /// </summary>
    public int AuditLogRetentionDays { get; set; } = 365;

    /// <summary>
    /// Enable audit logging for all feature flag changes.
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Maximum number of rules per feature flag.
    /// </summary>
    public int MaxRulesPerFlag { get; set; } = 100;

    /// <summary>
    /// Maximum number of conditions per rule.
    /// </summary>
    public int MaxConditionsPerRule { get; set; } = 50;

    /// <summary>
    /// Maximum number of variants per A/B test.
    /// </summary>
    public int MaxVariantsPerFlag { get; set; } = 10;

    /// <summary>
    /// Log evaluation details for debugging.
    /// </summary>
    public bool LogEvaluationDetails { get; set; } = false;

    /// <summary>
    /// Default percentage for new rollouts.
    /// </summary>
    public int DefaultRolloutPercentage { get; set; } = 50;

    /// <summary>
    /// Validates the options configuration.
    /// </summary>
    public bool IsValid()
    {
        return CacheDurationMinutes > 0
            && AuditLogRetentionDays > 0
            && MaxRulesPerFlag > 0
            && MaxConditionsPerRule > 0
            && MaxVariantsPerFlag > 0
            && DefaultRolloutPercentage >= 0
            && DefaultRolloutPercentage <= 100;
    }
}
