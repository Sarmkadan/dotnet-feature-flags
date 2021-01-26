#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace FeatureFlags.Data;

/// <summary>
/// Provides validation helpers for <see cref="DatabaseStatistics"/> class.
/// Validates null values, empty strings, out-of-range numbers, and default dates.
/// </summary>
public static class DatabaseSeederValidation
{
    /// <summary>
    /// Validates a <see cref="DatabaseStatistics"/> instance.
    /// </summary>
    /// <param name="value">The database statistics instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this DatabaseStatistics? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate counts - should not be negative
        if (value.TotalFeatureFlags < 0)
        {
            problems.Add($"TotalFeatureFlags must be non-negative, but was {value.TotalFeatureFlags}.");
        }

        if (value.EnabledFlags < 0)
        {
            problems.Add($"EnabledFlags must be non-negative, but was {value.EnabledFlags}.");
        }

        if (value.DisabledFlags < 0)
        {
            problems.Add($"DisabledFlags must be non-negative, but was {value.DisabledFlags}.");
        }

        if (value.TotalRules < 0)
        {
            problems.Add($"TotalRules must be non-negative, but was {value.TotalRules}.");
        }

        if (value.TotalConditions < 0)
        {
            problems.Add($"TotalConditions must be non-negative, but was {value.TotalConditions}.");
        }

        if (value.TotalVariants < 0)
        {
            problems.Add($"TotalVariants must be non-negative, but was {value.TotalVariants}.");
        }

        if (value.TotalAuditLogs < 0)
        {
            problems.Add($"TotalAuditLogs must be non-negative, but was {value.TotalAuditLogs}.");
        }

        if (value.PercentageRolloutCount < 0)
        {
            problems.Add($"PercentageRolloutCount must be non-negative, but was {value.PercentageRolloutCount}.");
        }

        if (value.RulesBasedCount < 0)
        {
            problems.Add($"RulesBasedCount must be non-negative, but was {value.RulesBasedCount}.");
        }

        if (value.ABTestCount < 0)
        {
            problems.Add($"ABTestCount must be non-negative, but was {value.ABTestCount}.");
        }

        // Validate that enabled + disabled equals total
        if (value.TotalFeatureFlags > 0 && value.EnabledFlags + value.DisabledFlags != value.TotalFeatureFlags)
        {
            problems.Add(
                $"EnabledFlags ({value.EnabledFlags}) + DisabledFlags ({value.DisabledFlags}) " +
                $"should equal TotalFeatureFlags ({value.TotalFeatureFlags}).");
        }

        // Validate that rollout type counts don't exceed total flags
        var totalRolloutTypes = value.PercentageRolloutCount + value.RulesBasedCount + value.ABTestCount;
        if (value.TotalFeatureFlags > 0 && totalRolloutTypes > value.TotalFeatureFlags)
        {
            problems.Add(
                $"PercentageRolloutCount ({value.PercentageRolloutCount}) + RulesBasedCount ({value.RulesBasedCount}) + " +
                $"ABTestCount ({value.ABTestCount}) = {totalRolloutTypes} exceeds TotalFeatureFlags ({value.TotalFeatureFlags}).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="DatabaseStatistics"/> instance is valid.
    /// </summary>
    /// <param name="value">The database statistics instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this DatabaseStatistics? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="DatabaseStatistics"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The database statistics instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
    public static void EnsureValid(this DatabaseStatistics? value)
    {
        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DatabaseStatistics validation failed:{Environment.NewLine}- " +
                string.Join($"{Environment.NewLine}- ", problems));
        }
    }
}