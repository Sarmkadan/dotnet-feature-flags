#nullable enable

namespace FeatureFlags.Models;

/// <summary>
/// Additional helper extension methods for <see cref="FlagEvaluationLog"/>.
/// </summary>
public static class FlagEvaluationLogHelperExtensions
{
    /// <summary>
    /// Determines whether the evaluation occurred within the last specified time window.
    /// </summary>
    /// <param name="log">The evaluation log to check.</param>
    /// <param name="window">The time window to check against (from now backwards).</param>
    /// <returns>True if the timestamp is within the last window; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
    public static bool IsRecent(this FlagEvaluationLog log, TimeSpan window)
    {
        ArgumentNullException.ThrowIfNull(log);
        var now = DateTime.UtcNow;
        return log.Timestamp >= now - window && log.Timestamp <= now;
    }
}