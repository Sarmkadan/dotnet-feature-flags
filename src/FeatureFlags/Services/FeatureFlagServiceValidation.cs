#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.ComponentModel;
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
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeatureFlagService"/> instance
    /// is valid according to business rules.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns>True if the service instance is valid; otherwise, false.</returns>
    public static bool IsValid(this FeatureFlagService? value) => Validate(value).Count == 0;

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

        if (errors.Count != 0)
        {
            throw new ArgumentException(
                $"FeatureFlagService validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.IsEnabledAsync(string, UserContext, CancellationToken)"/>.
    /// </summary>
    /// <param name="featureFlagKey">The feature flag key.</param>
    /// <param name="userContext">The user context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentException">Thrown when featureFlagKey is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when userContext is null.</exception>
    public static IReadOnlyList<string> ValidateForIsEnabledAsync(
        string? featureFlagKey,
        UserContext? userContext,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlagKey);
        ArgumentNullException.ThrowIfNull(userContext);

        if (userContext.IsValid() is false)
        {
            errors.Add("User context is invalid.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.GetFeatureFlagAsync(int, CancellationToken)"/>.
    /// </summary>
    /// <param name="id">The feature flag ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentException">Thrown when id is less than or equal to 0.</exception>
    public static IReadOnlyList<string> ValidateForGetFeatureFlagAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.GetFeatureFlagByKeyAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="key">The feature flag key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentException">Thrown when key is null, empty, or whitespace.</exception>
    public static IReadOnlyList<string> ValidateForGetFeatureFlagByKeyAsync(
        string? key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.CreateFeatureFlagAsync(FeatureFlag, string, CancellationToken)"/>.
    /// </summary>
    /// <param name="featureFlag">The feature flag to create.</param>
    /// <param name="createdBy">The user who created the flag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when featureFlag is null.</exception>
    /// <exception cref="ArgumentException">Thrown when createdBy is null or empty, or when feature flag properties are invalid.</exception>
    public static IReadOnlyList<string> ValidateForCreateFeatureFlagAsync(
        FeatureFlag? featureFlag,
        string? createdBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureFlag);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        var errors = new List<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlag.Key);
        if (featureFlag.Key.Length > 100)
        {
            errors.Add("Feature flag key cannot exceed 100 characters.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlag.DisplayName);
        if (featureFlag.DisplayName.Length > 200)
        {
            errors.Add("Feature flag display name cannot exceed 200 characters.");
        }

        if (featureFlag.Description?.Length > 1000)
        {
            errors.Add("Feature flag description cannot exceed 1000 characters.");
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when featureFlag is null.</exception>
    /// <exception cref="ArgumentException">Thrown when updatedBy is null or empty, or when feature flag properties are invalid.</exception>
    public static IReadOnlyList<string> ValidateForUpdateFeatureFlagAsync(
        FeatureFlag? featureFlag,
        string? updatedBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureFlag);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);

        var errors = new List<string>();

        if (featureFlag.Id <= 0)
        {
            errors.Add("Feature flag ID must be greater than 0.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlag.Key);
        if (featureFlag.Key.Length > 100)
        {
            errors.Add("Feature flag key cannot exceed 100 characters.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlag.DisplayName);
        if (featureFlag.DisplayName.Length > 200)
        {
            errors.Add("Feature flag display name cannot exceed 200 characters.");
        }

        if (featureFlag.Description?.Length > 1000)
        {
            errors.Add("Feature flag description cannot exceed 1000 characters.");
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentException">Thrown when id is less than or equal to 0, or when deletedBy is null or empty.</exception>
    public static IReadOnlyList<string> ValidateForDeleteFeatureFlagAsync(
        int id,
        string? deletedBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);
        ArgumentException.ThrowIfNullOrWhiteSpace(deletedBy);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.EnableFeatureFlagAsync(int, string, CancellationToken)"/>
    /// and <see cref="FeatureFlagService.DisableFeatureFlagAsync(int, string, CancellationToken)"/>.
    /// </summary>
    /// <param name="id">The feature flag ID.</param>
    /// <param name="modifiedBy">The user who modified the flag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentException">Thrown when id is less than or equal to 0, or when modifiedBy is null or empty.</exception>
    public static IReadOnlyList<string> ValidateForModifyFeatureFlagAsync(
        int id,
        string? modifiedBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.GetVariantAsync(string, UserContext, CancellationToken)"/>.
    /// </summary>
    /// <param name="featureFlagKey">The feature flag key.</param>
    /// <param name="userContext">The user context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentException">Thrown when featureFlagKey is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when userContext is null.</exception>
    public static IReadOnlyList<string> ValidateForGetVariantAsync(
        string? featureFlagKey,
        UserContext? userContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlagKey);
        ArgumentNullException.ThrowIfNull(userContext);

        if (userContext.IsValid() is false)
        {
            return new[] { "User context is invalid." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the parameters for <see cref="FeatureFlagService.SearchFeatureFlagsAsync(string)"/>.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation errors.</returns>
    public static IReadOnlyList<string> ValidateForSearchFeatureFlagsAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Length > 100)
        {
            return new[] { "Search term cannot exceed 100 characters." };
        }

        return Array.Empty<string>();
    }
}