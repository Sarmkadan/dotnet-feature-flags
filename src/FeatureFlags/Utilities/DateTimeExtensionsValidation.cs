#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace FeatureFlags.Utilities;

/// <summary>
/// Validation helpers for DateTime values that are used with DateTimeExtensions methods.
/// Ensures values are semantically valid for the operations they represent.
/// </summary>
public static class DateTimeExtensionsValidation
{
    /// <summary>
    /// Validates a DateTime value for use with DateTimeExtensions methods.
    /// </summary>
    /// <param name="value">The DateTime value to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this DateTime? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the date is not MinValue or MaxValue which could cause issues
        if (value == DateTime.MinValue)
        {
            problems.Add("DateTime value cannot be DateTime.MinValue as it represents an invalid/uninitialized date.");
        }

        if (value == DateTime.MaxValue)
        {
            problems.Add("DateTime value cannot be DateTime.MaxValue as it represents an invalid/unbounded date.");
        }

        // Validate that the date is not in the future for operations that expect past dates
        // (IsPast, IsToday checks would fail with future dates, but that's not a validation issue)

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates two DateTime values for range operations (IsBetween, GetBusinessDaysBetween).
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either parameter is null.</exception>
    public static IReadOnlyList<string> ValidateRange(this DateTime? startDate, DateTime? endDate)
    {
        ArgumentNullException.ThrowIfNull(startDate);
        ArgumentNullException.ThrowIfNull(endDate);

        var problems = new List<string>();

        // Validate individual dates
        problems.AddRange(startDate.Validate());
        problems.AddRange(endDate.Validate());

        // Validate that startDate <= endDate for range operations
        if (startDate > endDate)
        {
            problems.Add("Start date must be less than or equal to end date for range operations.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a TimeSpan for RoundTo operations.
    /// </summary>
    /// <param name="span">The TimeSpan to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if span is null.</exception>
    public static IReadOnlyList<string> Validate(this TimeSpan? span)
    {
        ArgumentNullException.ThrowIfNull(span);

        var problems = new List<string>();

        // Validate that the TimeSpan is positive and non-zero
        if (span <= TimeSpan.Zero)
        {
            problems.Add("TimeSpan must be positive and non-zero for rounding operations.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a DateTime value is semantically valid for DateTimeExtensions operations.
    /// </summary>
    /// <param name="value">The DateTime value to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid(this DateTime? value)
    {
        if (value is null)
        {
            return false;
        }

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if two DateTime values form a valid range for DateTimeExtensions operations.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValidRange(this DateTime? startDate, DateTime? endDate)
    {
        if (startDate is null || endDate is null)
        {
            return false;
        }

        return startDate.ValidateRange(endDate).Count == 0;
    }

    /// <summary>
    /// Checks if a TimeSpan is valid for RoundTo operations.
    /// </summary>
    /// <param name="span">The TimeSpan to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid(this TimeSpan? span)
    {
        if (span is null)
        {
            return false;
        }

        return span.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a DateTime value is semantically valid for DateTimeExtensions operations.
    /// </summary>
    /// <param name="value">The DateTime value to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if value is not valid, with a message listing all problems.</exception>
    public static void EnsureValid(this DateTime? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that two DateTime values form a valid range for DateTimeExtensions operations.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <exception cref="ArgumentNullException">Thrown if either parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the range is not valid, with a message listing all problems.</exception>
    public static void EnsureValidRange(this DateTime? startDate, DateTime? endDate)
    {
        ArgumentNullException.ThrowIfNull(startDate);
        ArgumentNullException.ThrowIfNull(endDate);

        var problems = startDate.ValidateRange(endDate);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime range validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that a TimeSpan is valid for RoundTo operations.
    /// </summary>
    /// <param name="span">The TimeSpan to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if span is null.</exception>
    /// <exception cref="ArgumentException">Thrown if span is not valid, with a message listing all problems.</exception>
    public static void EnsureValid(this TimeSpan? span)
    {
        ArgumentNullException.ThrowIfNull(span);

        var problems = span.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"TimeSpan validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}