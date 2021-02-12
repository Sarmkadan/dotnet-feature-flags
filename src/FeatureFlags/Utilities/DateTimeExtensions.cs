#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace FeatureFlags.Utilities;

/// <summary>
/// Extension methods for DateTime operations including comparisons, formatting, and range calculations.
/// Simplifies common date/time operations used in audit logging and scheduling.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds since epoch) for consistent time representation.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>Unix timestamp in seconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime in UTC.
    /// </summary>
    /// <param name="timestamp">Unix timestamp in seconds.</param>
    /// <returns>DateTime in UTC.</returns>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
    }

    /// <summary>
    /// Gets the start of day (00:00:00) for the given date.
    /// </summary>
    /// <param name="dateTime">The DateTime to get the start of day for.</param>
    /// <returns>DateTime representing the start of the day.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of day (23:59:59.999) for the given date.
    /// </summary>
    /// <param name="dateTime">The DateTime to get the end of day for.</param>
    /// <returns>DateTime representing the end of the day.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of week (Monday) for the given date.
    /// </summary>
    /// <param name="dateTime">The DateTime to get the start of week for.</param>
    /// <returns>DateTime representing the start of the week (Monday at 00:00:00).</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        var daysToSubtract = (int)dateTime.DayOfWeek - (int)DayOfWeek.Monday;
        if (daysToSubtract < 0)
        {
            daysToSubtract += 7;
        }

        return dateTime.AddDays(-daysToSubtract).StartOfDay();
    }

    /// <summary>
    /// Gets the start of month (first day) for the given date.
    /// </summary>
    /// <param name="dateTime">The DateTime to get the start of month for.</param>
    /// <returns>DateTime representing the first day of the month at 00:00:00.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of month (last day) for the given date.
    /// </summary>
    /// <param name="dateTime">The DateTime to get the end of month for.</param>
    /// <returns>DateTime representing the last day of the month at 23:59:59.999.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime.StartOfMonth().AddMonths(1).AddDays(-1).EndOfDay();
    }

    /// <summary>
    /// Gets the start of year (January 1) for the given date.
    /// </summary>
    /// <param name="dateTime">The DateTime to get the start of year for.</param>
    /// <returns>DateTime representing January 1 of the current year at 00:00:00.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Checks if the date falls within the specified range (inclusive).
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <param name="startDate">The start of the range.</param>
    /// <param name="endDate">The end of the range.</param>
    /// <returns>True if the dateTime falls within the range; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime >= startDate && dateTime <= endDate;
    }

    /// <summary>
    /// Calculates the number of business days between two dates (excluding weekends).
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>The number of business days between the two dates.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the startDate parameter is null.</exception>
    public static int GetBusinessDaysBetween(this DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(startDate);

        var count = 0;
        var current = startDate;

        while (current < endDate)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                count++;
            }

            current = current.AddDays(1);
        }

        return count;
    }

    /// <summary>
    /// Gets a human-readable time difference string (e.g., "2 hours ago", "in 3 days").
    /// </summary>
    /// <param name="dateTime">The DateTime to get the relative time for.</param>
    /// <returns>A human-readable string representing the time difference.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        var span = DateTime.UtcNow - dateTime;

        return span.TotalSeconds < 60
            ? "just now"
            : span.TotalMinutes < 60
                ? $"{(int)span.TotalMinutes} minute{((int)span.TotalMinutes != 1 ? "s" : "")} ago"
                : span.TotalHours < 24
                    ? $"{(int)span.TotalHours} hour{((int)span.TotalHours != 1 ? "s" : "")} ago"
                    : span.TotalDays < 30
                        ? $"{(int)span.TotalDays} day{((int)span.TotalDays != 1 ? "s" : "")} ago"
                        : dateTime.ToShortDateString();
    }

    /// <summary>
    /// Checks if the given date is today.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <returns>True if the date is today; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static bool IsToday(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime.Date == DateTime.Today;
    }

    /// <summary>
    /// Checks if the given date is in the past.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <returns>True if the date is in the past; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static bool IsPast(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the given date is in the future.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <returns>True if the date is in the future; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    public static bool IsFuture(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Rounds DateTime to nearest specified interval (e.g., 5 minutes, 1 hour).
    /// </summary>
    /// <param name="dateTime">The DateTime to round.</param>
    /// <param name="span">The TimeSpan interval to round to.</param>
    /// <returns>DateTime rounded to the nearest specified interval.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dateTime parameter is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the span parameter is zero or negative.</exception>
    public static DateTime RoundTo(this DateTime dateTime, TimeSpan span)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(span, TimeSpan.Zero);

        var delta = (dateTime.Ticks % span.Ticks + span.Ticks / 2) / span.Ticks;
        return new DateTime(((dateTime.Ticks / span.Ticks) + delta) * span.Ticks);
    }
}
