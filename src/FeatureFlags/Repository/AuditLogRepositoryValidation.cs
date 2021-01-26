#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using FeatureFlags.Models;

namespace FeatureFlags.Repository;

/// <summary>
/// Provides validation helpers for <see cref="AuditLogRepository"/> instances to ensure
/// they are properly initialized and ready for use.
/// </summary>
public static class AuditLogRepositoryValidation
{
    /// <summary>
    /// Validates that the <see cref="AuditLogRepository"/> instance is properly initialized
    /// and all required dependencies are available.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    [ExcludeFromCodeCoverage]
    public static IReadOnlyList<string> Validate(this AuditLogRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate injected dependencies
        if (value.GetType().GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            errors.Add("AuditLogRepository._context is null");
        }

        if (value.GetType().GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            errors.Add("AuditLogRepository._logger is null");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="AuditLogRepository"/> instance is valid
    /// and ready for use.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [ExcludeFromCodeCoverage]
    public static bool IsValid(this AuditLogRepository? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="AuditLogRepository"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the repository is invalid.</exception>
    [ExcludeFromCodeCoverage]
    public static void EnsureValid(this AuditLogRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AuditLogRepository is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}