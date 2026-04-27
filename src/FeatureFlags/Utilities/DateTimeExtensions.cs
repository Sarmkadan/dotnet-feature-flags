#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime in UTC.
    /// </summary>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
    }

    /// <summary>
    /// Gets the start of day (00:00:00) for the given date.
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of day (23:59:59.999) for the given date.
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of week (Monday) for the given date.
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
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
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of month (last day) for the given date.
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddDays(-1).EndOfDay();
    }

    /// <summary>
    /// Gets the start of year (January 1) for the given date.
    /// </summary>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Checks if the date falls within the specified range (inclusive).
    /// </summary>
    public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
        return dateTime >= startDate && dateTime <= endDate;
    }

    /// <summary>
    /// Calculates the number of business days between two dates (excluding weekends).
    /// </summary>
    public static int GetBusinessDaysBetween(this DateTime startDate, DateTime endDate)
    {
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
    public static string ToRelativeTime(this DateTime dateTime)
    {
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
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }

    /// <summary>
    /// Checks if the given date is in the past.
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the given date is in the future.
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Rounds DateTime to nearest specified interval (e.g., 5 minutes, 1 hour).
    /// </summary>
    public static DateTime RoundTo(this DateTime dateTime, TimeSpan span)
    {
        var delta = (dateTime.Ticks % span.Ticks + span.Ticks / 2) / span.Ticks;
        return new DateTime(((dateTime.Ticks / span.Ticks) + delta) * span.Ticks);
    }
}
