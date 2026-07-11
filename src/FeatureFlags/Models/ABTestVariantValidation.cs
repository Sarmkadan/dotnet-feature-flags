#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Provides validation helpers for <see cref="ABTestVariant"/> instances.
/// Validates all constraints including ranges, required fields, and business rules.
/// </summary>
public static class ABTestVariantValidation
{
    /// <summary>
    /// Validates the given ABTestVariant and returns a list of human-readable problems.
    /// Returns an empty list if the variant is valid.
    /// </summary>
    /// <param name="value">The variant to validate.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ABTestVariant? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id < 0)
        {
            errors.Add("Id must be a non-negative integer.");
        }

        // Validate FeatureFlagId
        if (value.FeatureFlagId <= 0)
        {
            errors.Add("FeatureFlagId must be a positive integer.");
        }

        // Validate VariantKey
        if (string.IsNullOrWhiteSpace(value.VariantKey))
        {
            errors.Add("VariantKey is required and cannot be empty or whitespace.");
        }
        else if (value.VariantKey.Length > 100)
        {
            errors.Add("VariantKey must be 100 characters or less.");
        }

        // Validate DisplayName
        if (string.IsNullOrWhiteSpace(value.DisplayName))
        {
            errors.Add("DisplayName is required and cannot be empty or whitespace.");
        }
        else if (value.DisplayName.Length > 200)
        {
            errors.Add("DisplayName must be 200 characters or less.");
        }

        // Validate Description
        if (value.Description.Length > 1000)
        {
            errors.Add("Description must be 1000 characters or less.");
        }

        // Validate AllocationPercentage
        if (value.AllocationPercentage < 0 || value.AllocationPercentage > 100)
        {
            errors.Add("AllocationPercentage must be between 0 and 100 inclusive.");
        }

        // Validate UserCount
        if (value.UserCount < 0)
        {
            errors.Add("UserCount cannot be negative.");
        }

        // Validate ConversionCount
        if (value.ConversionCount < 0)
        {
            errors.Add("ConversionCount cannot be negative.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("CreatedAt must be in UTC.");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be set to a valid DateTime.");
        }
        else if (value.UpdatedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("UpdatedAt must be in UTC.");
        }

        // Validate business rule: UserCount >= ConversionCount
        if (value.UserCount < value.ConversionCount)
        {
            errors.Add("ConversionCount cannot exceed UserCount.");
        }

        // Validate business rule: UpdatedAt should be >= CreatedAt
        if (value.UpdatedAt < value.CreatedAt)
        {
            errors.Add("UpdatedAt must be equal to or after CreatedAt.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the given ABTestVariant is valid.
    /// </summary>
    /// <param name="value">The variant to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this ABTestVariant? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the given ABTestVariant is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The variant to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the variant has validation errors.</exception>
    public static void EnsureValid(this ABTestVariant? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ABTestVariant validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", errors)}");
    }
}
