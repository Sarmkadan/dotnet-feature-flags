#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.HealthChecks;

/// <summary>
/// Options for <see cref="FeatureStoreHealthCheck"/>.
/// </summary>
public sealed class FeatureStoreHealthCheckOptions
{
    /// <summary>
    /// Maximum age of the last flag refresh before the check reports Degraded. Must be greater than zero.
    /// </summary>
    public TimeSpan StaleAfter { get; set; } = TimeSpan.FromHours(1);
}
