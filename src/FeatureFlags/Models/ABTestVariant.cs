#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Represents a variant in an A/B test for a feature flag.
/// Tracks allocation percentage and metrics for statistical analysis.
/// </summary>
public sealed class ABTestVariant
{
    /// <summary>
    /// Gets or sets the unique identifier for the variant.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the feature flag this variant belongs to.
    /// </summary>
    public int FeatureFlagId { get; set; }

    private string _variantKey = string.Empty;

    /// <summary>
    /// Gets or sets the unique key used to identify this variant (e.g., "control", "treatment").
    /// </summary>
    public string VariantKey
    {
        get => _variantKey;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _variantKey = value;
        }
    }

    private string _displayName = string.Empty;

    /// <summary>
    /// Gets or sets the display name for this variant.
    /// </summary>
    public string DisplayName
    {
        get => _displayName;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _displayName = value;
        }
    }

    private string _description = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of this variant.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _description = value;
        }
    }

    /// <summary>
    /// Gets or sets the percentage of traffic allocated to this variant (0-100).
    /// </summary>
    public int AllocationPercentage { get; set; }

    /// <summary>
    /// Gets or sets the count of users assigned to this variant.
    /// </summary>
    public long UserCount { get; set; }

    /// <summary>
    /// Gets or sets the count of conversions recorded for this variant.
    /// </summary>
    public long ConversionCount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this variant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this variant was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether this variant is the control group.
    /// </summary>
    public bool IsControl { get; set; }

    // Navigation properties
    private FeatureFlag? _featureFlag;

    /// <summary>
    /// Gets or sets the feature flag associated with this variant.
    /// </summary>
    public FeatureFlag? FeatureFlag
    {
        get => _featureFlag;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _featureFlag = value;
        }
    }

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

    public override string ToString() => $"ABTestVariant {{ Id = {Id}, FeatureFlagId = {FeatureFlagId}, VariantKey = {VariantKey}, DisplayName = {DisplayName}, Description = {Description}, AllocationPercentage = {AllocationPercentage} }}";
}
