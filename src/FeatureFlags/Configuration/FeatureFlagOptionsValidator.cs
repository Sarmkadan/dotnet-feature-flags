#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace FeatureFlags.Configuration;

/// <summary>
/// Validates <see cref="FeatureFlagOptions"/> when the host starts (see <c>ValidateOnStart</c>).
/// Every failure names the offending property and its value.
/// </summary>
public sealed class FeatureFlagOptionsValidator : IValidateOptions<FeatureFlagOptions>
{
    /// <summary>
    /// Validates the feature flag options.
    /// </summary>
    /// <param name="name">The options instance name (unused; options are unnamed).</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>Success, or a failure listing every invalid property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public ValidateOptionsResult Validate(string? name, FeatureFlagOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        // Default flag TTL (cache duration) must be positive.
        if (options.CacheDurationMinutes <= 0)
        {
            failures.Add(Fail(nameof(options.CacheDurationMinutes), options.CacheDurationMinutes, "must be greater than 0 (default flag TTL in minutes)"));
        }

        // Audit logging needs a retention-backed store to write to.
        if (options.EnableAuditLogging && options.AuditLogRetentionDays <= 0)
        {
            failures.Add(Fail(nameof(options.AuditLogRetentionDays), options.AuditLogRetentionDays, $"must be greater than 0 when {nameof(options.EnableAuditLogging)} is true"));
        }

        if (options.MaxRulesPerFlag <= 0)
        {
            failures.Add(Fail(nameof(options.MaxRulesPerFlag), options.MaxRulesPerFlag, "must be greater than 0"));
        }

        if (options.MaxConditionsPerRule <= 0)
        {
            failures.Add(Fail(nameof(options.MaxConditionsPerRule), options.MaxConditionsPerRule, "must be greater than 0"));
        }

        if (options.MaxVariantsPerFlag <= 0)
        {
            failures.Add(Fail(nameof(options.MaxVariantsPerFlag), options.MaxVariantsPerFlag, "must be greater than 0"));
        }

        if (options.DefaultRolloutPercentage is < 0 or > 100)
        {
            failures.Add(Fail(nameof(options.DefaultRolloutPercentage), options.DefaultRolloutPercentage, "must be between 0 and 100"));
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static string Fail(string property, object value, string requirement) =>
        $"{nameof(FeatureFlagOptions)}.{property} {requirement}, but was '{value}'.";
}
