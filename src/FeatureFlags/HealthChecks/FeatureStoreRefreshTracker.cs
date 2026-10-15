#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading;

namespace FeatureFlags.HealthChecks;

/// <summary>
/// Records when feature flags were last read from the store. Singleton, so it is safe to share across requests.
/// </summary>
public sealed class FeatureStoreRefreshTracker
{
    private readonly TimeProvider _timeProvider;

    // UTC ticks of the last refresh; 0 means no refresh has been recorded.
    private long _lastRefreshTicks;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureStoreRefreshTracker"/> class.
    /// </summary>
    /// <param name="timeProvider">Clock used to stamp refreshes.</param>
    public FeatureStoreRefreshTracker(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Gets the UTC time of the most recent refresh, or <c>null</c> if none has been recorded.
    /// </summary>
    public DateTimeOffset? LastRefreshedUtc
    {
        get
        {
            var ticks = Interlocked.Read(ref _lastRefreshTicks);
            return ticks == 0 ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    /// <summary>
    /// Records that flags were just read from the store.
    /// </summary>
    public void MarkRefreshed()
    {
        Interlocked.Exchange(ref _lastRefreshTicks, _timeProvider.GetUtcNow().UtcTicks);
    }
}
