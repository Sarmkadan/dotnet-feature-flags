#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Provides validation helpers for <see cref="FlagEvaluationLog"/> instances.
/// </summary>
public static class FlagEvaluationLogValidation
{
    /// <summary>
    /// Validates a <see cref="FlagEvaluationLog"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The log entry to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this FlagEvaluationLog value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.FlagName))
        {
            errors.Add("FlagName cannot be null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.UserId))
        {
            errors.Add("UserId cannot be null, empty, or whitespace.");
        }

        if (value.Timestamp == default)
        {
            errors.Add("Timestamp must be a valid UTC date and cannot be the default DateTime value.");
        }
        else if (value.Timestamp.Kind != DateTimeKind.Utc)
        {
            errors.Add("Timestamp must be in UTC kind.");
        }
        else if (value.Timestamp > DateTime.UtcNow)
        {
            errors.Add("Timestamp cannot be in the future.");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("Timestamp cannot be more than one year in the past.");
        }

        if (string.IsNullOrWhiteSpace(value.Reason))
        {
            errors.Add("Reason cannot be null, empty, or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="FlagEvaluationLog"/> instance is valid.
    /// </summary>
    /// <param name="value">The log entry to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this FlagEvaluationLog? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="FlagEvaluationLog"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The log entry to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, with a message listing all problems.</exception>
    public static void EnsureValid(this FlagEvaluationLog value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"FlagEvaluationLog is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}