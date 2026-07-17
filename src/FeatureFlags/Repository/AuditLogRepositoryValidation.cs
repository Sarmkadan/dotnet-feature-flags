#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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
    /// Determines whether the <see cref="AuditLogRepository"/> instance is not null.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns>True if the instance is not null; otherwise, false.</returns>
    [ExcludeFromCodeCoverage]
    public static bool IsValid(this AuditLogRepository? value)
        => value is not null;

    /// <summary>
    /// Ensures that the <see cref="AuditLogRepository"/> instance is not null.
    /// Throws an <see cref="ArgumentNullException"/> if the instance is null.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when the repository is null.</exception>
    [ExcludeFromCodeCoverage]
    public static void EnsureValid(this AuditLogRepository? value)
        => ArgumentNullException.ThrowIfNull(value);
}