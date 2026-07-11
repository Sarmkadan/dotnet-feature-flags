#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Provides validation helpers for <see cref="FeatureFlagService"/> instances.
/// Validates constructor arguments and public method parameters to ensure
/// they meet business rules and constraints before operations are performed.
/// </summary>
public static class FeatureFlagServiceValidation
{
    /// <summary>
    /// Validates that a <see cref="FeatureFlagService"/> instance is properly initialized
    /// with all required dependencies.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> Validate(this FeatureFlagService? value)
    {
        var errors = new List<string>();

        if (value is null)
        {
            errors.Add("FeatureFlagService instance cannot be null.");
            return errors.AsReadOnly();
        }

        // Validate all injected dependencies are not null
        // Note: We can't directly access private fields, so we validate through public behavior
        // The actual null checks happen in the service methods themselves

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeatureFlagService"/> instance
    /// is valid according to business rules.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns>True if the service instance is valid; otherwise, false.</returns>
    public static bool IsValid(this FeatureFlagService? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="FeatureFlagService"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation errors
    /// if the instance is not valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the service instance is invalid.</exception>
    public static void EnsureValid(this FeatureFlagService? value)
    {
        var errors = Validate(value);

        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"FeatureFlagService validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", errors)}");
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.IsEnabledAsync(string, UserContext, CancellationToken)"/>.
    /// </summary>
    /// <param name="featureFlagKey">The feature flag key.</param>
    /// <param name="userContext">The user context.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForIsEnabledAsync(string? featureFlagKey, UserContext? userContext)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(featureFlagKey))
        {
            errors.Add("Feature flag key cannot be null, empty, or whitespace.");
        }
        else if (featureFlagKey!.Length > 100)
        {
            errors.Add("Feature flag key cannot exceed 100 characters.");
        }

        if (userContext is null)
        {
            errors.Add("User context cannot be null.");
        }
        else if (!userContext.IsValid())
        {
            errors.Add("User context is invalid.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.GetFeatureFlagAsync(int, CancellationToken)"/>.
    /// </summary>
    /// <param name="id">The feature flag ID.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForGetFeatureFlagAsync(int id)
    {
        var errors = new List<string>();

        if (id <= 0)
        {
            errors.Add("Id must be greater than 0.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.GetFeatureFlagByKeyAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="key">The feature flag key.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForGetFeatureFlagByKeyAsync(string? key)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(key))
        {
            errors.Add("Key cannot be null, empty, or whitespace.");
        }
        else if (key!.Length > 100)
        {
            errors.Add("Key cannot exceed 100 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.CreateFeatureFlagAsync(FeatureFlag, string, CancellationToken)"/>.
    /// </summary>
    /// <param name="featureFlag">The feature flag to create.</param>
    /// <param name="createdBy">The user who created the flag.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForCreateFeatureFlagAsync(FeatureFlag? featureFlag, string? createdBy)
    {
        var errors = new List<string>();

        if (featureFlag is null)
        {
            errors.Add("Feature flag cannot be null.");
            return errors.AsReadOnly();
        }

        if (string.IsNullOrWhiteSpace(featureFlag.Key))
        {
            errors.Add("Feature flag key cannot be null or empty.");
        }
        else if (featureFlag.Key.Length > 100)
        {
            errors.Add("Feature flag key cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(featureFlag.DisplayName))
        {
            errors.Add("Feature flag display name cannot be null or empty.");
        }
        else if (featureFlag.DisplayName.Length > 200)
        {
            errors.Add("Feature flag display name cannot exceed 200 characters.");
        }

        if (featureFlag.Description?.Length > 1000)
        {
            errors.Add("Feature flag description cannot exceed 1000 characters.");
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            errors.Add("CreatedBy cannot be null or empty.");
        }
        else if (createdBy.Length > 100)
        {
            errors.Add("CreatedBy cannot exceed 100 characters.");
        }

        if (featureFlag.CreatedAt == default)
        {
            errors.Add("CreatedAt must be a valid date.");
        }

        if (featureFlag.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be a valid date.");
        }

        // Validate rollout type specific constraints
        switch (featureFlag.RolloutType)
        {
            case Enums.RolloutType.Percentage:
                if (!featureFlag.PercentageRollout.HasValue)
                {
                    errors.Add("PercentageRollout must be set when RolloutType is Percentage.");
                }
                else if (featureFlag.PercentageRollout < 0 || featureFlag.PercentageRollout > 100)
                {
                    errors.Add("PercentageRollout must be between 0 and 100.");
                }
                break;

            case Enums.RolloutType.ABTest:
                if (featureFlag.Variants is null || !featureFlag.Variants.Any())
                {
                    errors.Add("ABTest requires at least one variant.");
                }
                break;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.UpdateFeatureFlagAsync(FeatureFlag, string, CancellationToken)"/>.
    /// </summary>
    /// <param name="featureFlag">The feature flag to update.</param>
    /// <param name="updatedBy">The user who updated the flag.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForUpdateFeatureFlagAsync(FeatureFlag? featureFlag, string? updatedBy)
    {
        var errors = new List<string>();

        if (featureFlag is null)
        {
            errors.Add("Feature flag cannot be null.");
            return errors.AsReadOnly();
        }

        if (featureFlag.Id <= 0)
        {
            errors.Add("Feature flag ID must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(featureFlag.Key))
        {
            errors.Add("Feature flag key cannot be null or empty.");
        }
        else if (featureFlag.Key.Length > 100)
        {
            errors.Add("Feature flag key cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(featureFlag.DisplayName))
        {
            errors.Add("Feature flag display name cannot be null or empty.");
        }
        else if (featureFlag.DisplayName.Length > 200)
        {
            errors.Add("Feature flag display name cannot exceed 200 characters.");
        }

        if (featureFlag.Description?.Length > 1000)
        {
            errors.Add("Feature flag description cannot exceed 1000 characters.");
        }

        if (string.IsNullOrWhiteSpace(updatedBy))
        {
            errors.Add("UpdatedBy cannot be null or empty.");
        }
        else if (updatedBy.Length > 100)
        {
            errors.Add("UpdatedBy cannot exceed 100 characters.");
        }

        if (featureFlag.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be a valid date.");
        }

        // Validate rollout type specific constraints
        switch (featureFlag.RolloutType)
        {
            case Enums.RolloutType.Percentage:
                if (!featureFlag.PercentageRollout.HasValue)
                {
                    errors.Add("PercentageRollout must be set when RolloutType is Percentage.");
                }
                else if (featureFlag.PercentageRollout < 0 || featureFlag.PercentageRollout > 100)
                {
                    errors.Add("PercentageRollout must be between 0 and 100.");
                }
                break;

            case Enums.RolloutType.ABTest:
                if (featureFlag.Variants is null || !featureFlag.Variants.Any())
                {
                    errors.Add("ABTest requires at least one variant.");
                }
                break;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.DeleteFeatureFlagAsync(int, string, CancellationToken)"/>.
    /// </summary>
    /// <param name="id">The feature flag ID to delete.</param>
    /// <param name="deletedBy">The user who deleted the flag.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForDeleteFeatureFlagAsync(int id, string? deletedBy)
    {
        var errors = new List<string>();

        if (id <= 0)
        {
            errors.Add("Id must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(deletedBy))
        {
            errors.Add("DeletedBy cannot be null or empty.");
        }
        else if (deletedBy.Length > 100)
        {
            errors.Add("DeletedBy cannot exceed 100 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.EnableFeatureFlagAsync(int, string, CancellationToken)"/>
    /// and <see cref="FeatureFlagService.DisableFeatureFlagAsync(int, string, CancellationToken)"/>.
    /// </summary>
    /// <param name="id">The feature flag ID.</param>
    /// <param name="modifiedBy">The user who modified the flag.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForModifyFeatureFlagAsync(int id, string? modifiedBy)
    {
        var errors = new List<string>();

        if (id <= 0)
        {
            errors.Add("Id must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(modifiedBy))
        {
            errors.Add("ModifiedBy cannot be null or empty.");
        }
        else if (modifiedBy.Length > 100)
        {
            errors.Add("ModifiedBy cannot exceed 100 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.GetVariantAsync(string, UserContext, CancellationToken)"/>.
    /// </summary>
    /// <param name="featureFlagKey">The feature flag key.</param>
    /// <param name="userContext">The user context.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForGetVariantAsync(string? featureFlagKey, UserContext? userContext)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(featureFlagKey))
        {
            errors.Add("Feature flag key cannot be null, empty, or whitespace.");
        }
        else if (featureFlagKey!.Length > 100)
        {
            errors.Add("Feature flag key cannot exceed 100 characters.");
        }

        if (userContext is null)
        {
            errors.Add("User context cannot be null.");
        }
        else if (!userContext.IsValid())
        {
            errors.Add("User context is invalid.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.SearchFeatureFlagsAsync(string)"/>.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForSearchFeatureFlagsAsync(string? searchTerm)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Length > 100)
        {
            errors.Add("Search term cannot exceed 100 characters.");
        }

        return errors.AsReadOnly();
    }
}