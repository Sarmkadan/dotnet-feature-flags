// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Represents a variant in an A/B test for a feature flag.
/// Tracks allocation percentage and metrics for statistical analysis.
/// </summary>
public class ABTestVariant
{
    public int Id { get; set; }

    public int FeatureFlagId { get; set; }

    public string VariantKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int AllocationPercentage { get; set; }

    public long UserCount { get; set; }

    public long ConversionCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsControl { get; set; }

    // Navigation properties
    public FeatureFlag? FeatureFlag { get; set; }

    /// <summary>
    /// Calculates the conversion rate for this variant.
    /// </summary>
    public double GetConversionRate()
    {
        if (UserCount == 0)
            return 0;

        return (double)ConversionCount / UserCount;
    }

    /// <summary>
    /// Records a user assignment to this variant for tracking purposes.
    /// </summary>
    public void RecordUserAssignment()
    {
        UserCount++;
    }

    /// <summary>
    /// Records a conversion event for this variant in A/B test analysis.
    /// </summary>
    public void RecordConversion()
    {
        ConversionCount++;
    }

    /// <summary>
    /// Validates the variant configuration ensures allocation percentages are reasonable.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(VariantKey))
            return false;

        if (string.IsNullOrWhiteSpace(DisplayName))
            return false;

        if (AllocationPercentage < 0 || AllocationPercentage > 100)
            return false;

        if (FeatureFlagId <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the statistical confidence level for this variant's conversion rate.
    /// Returns a simple confidence based on user count.
    /// </summary>
    public string GetStatisticalConfidence()
    {
        return UserCount switch
        {
            < 100 => "Very Low",
            < 500 => "Low",
            < 1000 => "Medium",
            < 5000 => "High",
            _ => "Very High"
        };
    }
}
