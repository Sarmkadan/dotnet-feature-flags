#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace FeatureFlags.Models;

/// <summary>
/// Provides extension methods for <see cref="ABTestVariant"/> to enhance A/B testing functionality.
/// </summary>
public static class ABTestVariantExtensions
{
    /// <summary>
    /// Determines if this variant has reached its allocation capacity based on user assignments.
    /// </summary>
    /// <param name="variant">The A/B test variant to check.</param>
    /// <returns>True if the variant has reached or exceeded its allocation percentage; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="variant"/> is null.</exception>
    public static bool HasReachedAllocation(this ABTestVariant variant)
    {
        ArgumentNullException.ThrowIfNull(variant);

        if (variant.AllocationPercentage <= 0)
            return false;

        if (variant.UserCount <= 0)
            return false;

        // Calculate the maximum users allowed based on allocation percentage
        // Use a reasonable default of 1000 total users if FeatureFlag is null
        long maxUsersAllowed = (long)Math.Ceiling((double)variant.AllocationPercentage / 100 * 1000);

        return variant.UserCount >= maxUsersAllowed;
    }

    /// <summary>
    /// Gets the conversion rate as a formatted percentage string suitable for display.
    /// </summary>
    /// <param name="variant">The A/B test variant.</param>
    /// <param name="format">The format specifier for the percentage (default: "P2").</param>
    /// <returns>A formatted percentage string representing the conversion rate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="variant"/> is null.</exception>
    public static string GetFormattedConversionRate(this ABTestVariant variant, string format = "P2")
    {
        ArgumentNullException.ThrowIfNull(variant);

        double conversionRate = variant.GetConversionRate();
        return conversionRate.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Determines if this variant is performing better than another variant based on conversion rate.
    /// </summary>
    /// <param name="variant">The current variant to compare.</param>
    /// <param name="other">The other variant to compare against.</param>
    /// <returns>True if this variant has a higher conversion rate; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="variant"/> or <paramref name="other"/> is null.</exception>
    public static bool IsPerformingBetterThan(this ABTestVariant variant, ABTestVariant other)
    {
        ArgumentNullException.ThrowIfNull(variant);
        ArgumentNullException.ThrowIfNull(other);

        double thisRate = variant.GetConversionRate();
        double otherRate = other.GetConversionRate();

        return thisRate > otherRate;
    }

    /// <summary>
    /// Gets the relative performance improvement of this variant compared to another variant.
    /// </summary>
    /// <param name="variant">The current variant to evaluate.</param>
    /// <param name="baseline">The baseline variant for comparison.</param>
    /// <returns>The relative performance improvement as a percentage, or 0 if baseline has 0 conversions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="variant"/> or <paramref name="baseline"/> is null.</exception>
    public static double GetPerformanceImprovement(this ABTestVariant variant, ABTestVariant baseline)
    {
        ArgumentNullException.ThrowIfNull(variant);
        ArgumentNullException.ThrowIfNull(baseline);

        double baselineRate = baseline.GetConversionRate();
        double variantRate = variant.GetConversionRate();

        if (baselineRate == 0)
            return 0;

        return ((variantRate - baselineRate) / baselineRate) * 100;
    }
}