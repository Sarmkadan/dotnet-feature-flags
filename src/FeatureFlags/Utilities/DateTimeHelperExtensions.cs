#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace FeatureFlags.Utilities;

/// <summary>
/// Additional extension methods for DateTime operations.
/// Provides supplementary date/time utilities not found in the main DateTimeExtensions class.
/// </summary>
public static class DateTimeHelperExtensions
{
    /// <summary>
    /// Gets the end of the year (December 31, 23:59:59.999) for the given date.
    /// </summary>
    /// <param name="dateTime">The DateTime to get the end of year for.</param>
    /// <returns>DateTime representing December 31 of the current year at 23:59:59.999.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static DateTime EndOfYear(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
    }
}