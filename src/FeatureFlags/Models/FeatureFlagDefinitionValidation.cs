#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Exceptions;
using FeatureFlags.Utilities;

namespace FeatureFlags.Models;

/// <summary>
/// Validates feature flag definitions at creation time: key format, percentage rollout range,
/// and A/B variant weights. Every rule is checked so callers receive all violations at once.
/// </summary>
public static class FeatureFlagDefinitionValidation
{
    private const int MaxKeyLength = 100;
    private const int MaxPercentage = 100;

    /// <summary>
    /// Validates the given feature flag definition and returns every rule violation found.
    /// </summary>
    /// <param name="value">The feature flag to validate.</param>
    /// <returns>List of violations; empty if the definition is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this FeatureFlag? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // The key is also the bucketing input for percentage rollouts, so it must be non-empty and well-formed.
        if (string.IsNullOrWhiteSpace(value.Key))
        {
            errors.Add("Key is required and cannot be empty or whitespace.");
        }
        else
        {
            if (value.Key.Length > MaxKeyLength)
            {
                errors.Add($"Key must be {MaxKeyLength} characters or less.");
            }

            if (!value.Key.IsValidKeyFormat())
            {
                errors.Add($"Key '{value.Key}' must match ^[a-zA-Z0-9_-]+$ (letters, digits, underscores, and dashes only).");
            }
        }

        // PercentageRollout is required for Percentage flags and must be within 0-100 whenever it is set.
        if (value.PercentageRollout is null)
        {
            if (value.RolloutType == RolloutType.Percentage)
            {
                errors.Add("PercentageRollout is required when RolloutType is Percentage.");
            }
        }
        else if (value.PercentageRollout < 0 || value.PercentageRollout > MaxPercentage)
        {
            errors.Add($"PercentageRollout must be between 0 and {MaxPercentage} inclusive (was {value.PercentageRollout}).");
        }

        var variants = value.Variants ?? Array.Empty<ABTestVariant>();

        if (value.RolloutType == RolloutType.ABTest && variants.Count == 0)
        {
            errors.Add("ABTest requires at least one variant.");
        }

        // Variation weights must each be 0-100 and total exactly 100 when variants are defined.
        if (variants.Count > 0)
        {
            foreach (var variant in variants)
            {
                if (variant.AllocationPercentage < 0 || variant.AllocationPercentage > MaxPercentage)
                {
                    errors.Add($"Variant '{variant.VariantKey}' AllocationPercentage must be between 0 and {MaxPercentage} inclusive (was {variant.AllocationPercentage}).");
                }
            }

            var totalWeight = variants.Sum(v => (long)v.AllocationPercentage);
            if (totalWeight != MaxPercentage)
            {
                errors.Add($"Variation weights must sum to {MaxPercentage} (current total: {totalWeight}).");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Ensures the given feature flag definition is valid, throwing <see cref="InvalidFlagDefinitionException"/>
    /// that lists every violation if it is not.
    /// </summary>
    /// <param name="value">The feature flag to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidFlagDefinitionException">Thrown if the definition violates one or more rules.</exception>
    public static void EnsureValid(this FeatureFlag? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new InvalidFlagDefinitionException(value.Key, errors);
        }
    }
}
