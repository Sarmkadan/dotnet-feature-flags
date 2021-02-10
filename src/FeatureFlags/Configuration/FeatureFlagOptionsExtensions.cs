#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FeatureFlags.Configuration;

/// <summary>
/// Extension methods for <see cref="FeatureFlagOptions"/> configuration.
/// </summary>
public static class FeatureFlagOptionsExtensions
{
    /// <summary>
    /// Validates the feature flag options and throws if invalid.
    /// </summary>
    /// <param name="options">The feature flag options to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when options are invalid.</exception>
    public static void Validate(this FeatureFlagOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IsValid())
        {
            throw new InvalidOperationException(
                "FeatureFlagOptions validation failed. Ensure all values are positive and DefaultRolloutPercentage is between 0 and 100.");
        }
    }

    /// <summary>
    /// Creates a deep copy of the feature flag options.
    /// </summary>
    /// <param name="options">The options to clone.</param>
    /// <returns>A new instance with the same property values.</returns>
    public static FeatureFlagOptions Clone(this FeatureFlagOptions options)
    {
        return new FeatureFlagOptions
        {
            EnableCache = options.EnableCache,
            CacheDurationMinutes = options.CacheDurationMinutes,
            AuditLogRetentionDays = options.AuditLogRetentionDays,
            EnableAuditLogging = options.EnableAuditLogging,
            MaxRulesPerFlag = options.MaxRulesPerFlag,
            MaxConditionsPerRule = options.MaxConditionsPerRule,
            MaxVariantsPerFlag = options.MaxVariantsPerFlag,
            LogEvaluationDetails = options.LogEvaluationDetails,
            EnableAuditLog = options.EnableAuditLog,
            DefaultRolloutPercentage = options.DefaultRolloutPercentage
        };
    }

    /// <summary>
    /// Merges the current options with another instance, prioritizing the current instance's values.
    /// </summary>
    /// <param name="options">The target options.</param>
    /// <param name="overrideOptions">The options to merge in.</param>
    /// <returns>A new merged instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> or <paramref name="overrideOptions"/> is <see langword="null"/>.</exception>
    public static FeatureFlagOptions MergeWith(this FeatureFlagOptions options, FeatureFlagOptions overrideOptions)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(overrideOptions);

        return new FeatureFlagOptions
        {
            EnableCache = overrideOptions.EnableCache,
            CacheDurationMinutes = overrideOptions.CacheDurationMinutes != default
                ? overrideOptions.CacheDurationMinutes
                : options.CacheDurationMinutes,
            AuditLogRetentionDays = overrideOptions.AuditLogRetentionDays != default
                ? overrideOptions.AuditLogRetentionDays
                : options.AuditLogRetentionDays,
            EnableAuditLogging = overrideOptions.EnableAuditLogging,
            MaxRulesPerFlag = overrideOptions.MaxRulesPerFlag != default
                ? overrideOptions.MaxRulesPerFlag
                : options.MaxRulesPerFlag,
            MaxConditionsPerRule = overrideOptions.MaxConditionsPerRule != default
                ? overrideOptions.MaxConditionsPerRule
                : options.MaxConditionsPerRule,
            MaxVariantsPerFlag = overrideOptions.MaxVariantsPerFlag != default
                ? overrideOptions.MaxVariantsPerFlag
                : options.MaxVariantsPerFlag,
            LogEvaluationDetails = overrideOptions.LogEvaluationDetails,
            EnableAuditLog = overrideOptions.EnableAuditLog,
            DefaultRolloutPercentage = overrideOptions.DefaultRolloutPercentage != default
                ? overrideOptions.DefaultRolloutPercentage
                : options.DefaultRolloutPercentage
        };
    }

    /// <summary>
    /// Determines whether audit logging is enabled and configured for retention.
    /// </summary>
    /// <param name="options">The feature flag options.</param>
    /// <returns>True if audit logging is enabled and retention is positive; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsAuditLoggingConfigured(this FeatureFlagOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.EnableAuditLogging && options.EnableAuditLog && options.AuditLogRetentionDays > 0;
    }

    /// <summary>
    /// Gets the effective cache duration in seconds.
    /// </summary>
    /// <param name="options">The feature flag options.</param>
    /// <returns>The cache duration in seconds, or 0 if caching is disabled.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static int GetCacheDurationSeconds(this FeatureFlagOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.EnableCache && options.CacheDurationMinutes > 0
            ? options.CacheDurationMinutes * 60
            : 0;
    }
}