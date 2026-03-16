#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IFlagEvaluationLogService"/>.
/// Suitable for single-instance deployments and testing.
/// </summary>
public sealed class FlagEvaluationLogService : IFlagEvaluationLogService
{
    private readonly ConcurrentQueue<FlagEvaluationLog> _logs = new();

    /// <inheritdoc />
    public void Log(FlagEvaluationLog entry)
    {
        if (entry is null)
            throw new ArgumentNullException(nameof(entry));

        _logs.Enqueue(entry);
    }

    /// <inheritdoc />
    public IReadOnlyList<FlagEvaluationLog> GetAll() =>
        _logs.ToArray();

    /// <inheritdoc />
    public IReadOnlyList<FlagEvaluationLog> GetByUserId(string userId) =>
        _logs.Where(l => string.Equals(l.UserId, userId, StringComparison.OrdinalIgnoreCase))
             .ToArray();

    /// <inheritdoc />
    public IReadOnlyList<FlagEvaluationLog> GetByFlagName(string flagName) =>
        _logs.Where(l => string.Equals(l.FlagName, flagName, StringComparison.OrdinalIgnoreCase))
             .ToArray();
}
