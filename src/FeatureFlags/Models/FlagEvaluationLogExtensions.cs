#nullable enable

namespace FeatureFlags.Models;

/// <summary>
/// Provides extension methods for <see cref="FlagEvaluationLog"/> to facilitate common operations
/// such as formatting, filtering, and conversion to other data structures.
/// </summary>
public static class FlagEvaluationLogExtensions
{
    /// <summary>
    /// Formats the evaluation log as a human-readable string for logging and debugging purposes.
    /// </summary>
    /// <param name="log">The evaluation log to format.</param>
    /// <returns>A formatted string containing all evaluation details.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
    public static string ToFormattedString(this FlagEvaluationLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return $"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] Feature Flag: '{log.FlagName}', User: '{log.UserId}', Result: {(log.Result ? "ENABLED" : "DISABLED")}, Reason: '{log.Reason}'";
    }

    /// <summary>
    /// Determines whether the evaluation result matches a specific expected value.
    /// </summary>
    /// <param name="log">The evaluation log to check.</param>
    /// <param name="expectedResult">The expected result value to compare against.</param>
    /// <returns>True if the result matches the expected value; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
    public static bool MatchesResult(this FlagEvaluationLog log, bool expectedResult)
    {
        ArgumentNullException.ThrowIfNull(log);
        return log.Result == expectedResult;
    }

    /// <summary>
    /// Creates a new <see cref="FlagEvaluationLog"/> with the same values except for the result.
    /// </summary>
    /// <param name="log">The original evaluation log.</param>
    /// <param name="newResult">The new result value to set.</param>
    /// <returns>A new evaluation log instance with the updated result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
    public static FlagEvaluationLog WithResult(this FlagEvaluationLog log, bool newResult)
    {
        ArgumentNullException.ThrowIfNull(log);

        return new FlagEvaluationLog
        {
            FlagName = log.FlagName,
            UserId = log.UserId,
            Result = newResult,
            Timestamp = log.Timestamp,
            Reason = log.Reason
        };
    }

    /// <summary>
    /// Determines whether the evaluation occurred within a specific time range.
    /// </summary>
    /// <param name="log">The evaluation log to check.</param>
    /// <param name="startTime">The start of the time range (inclusive).</param>
    /// <param name="endTime">The end of the time range (inclusive).</param>
    /// <returns>True if the timestamp is within the range; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
    public static bool IsWithinTimeRange(this FlagEvaluationLog log, DateTime startTime, DateTime endTime)
    {
        ArgumentNullException.ThrowIfNull(log);
        return log.Timestamp >= startTime && log.Timestamp <= endTime;
    }
}