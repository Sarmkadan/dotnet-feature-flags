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

    /// <summary>
    /// Convenience overload that builds a <see cref="FlagEvaluationLog"/> from an evaluated
    /// feature flag and user context, then records it.
    /// </summary>
    public void LogEvaluation(FeatureFlag flag, UserContext userContext, bool result)
    {
        if (flag is null)
            throw new ArgumentNullException(nameof(flag));

        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        if (string.IsNullOrWhiteSpace(flag.Key))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(flag));

        if (string.IsNullOrWhiteSpace(userContext.UserId))
            throw new ArgumentException("User ID cannot be empty", nameof(userContext));

        Log(new FlagEvaluationLog
        {
            FlagName = flag.Key,
            UserId = userContext.UserId,
            Result = result,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>Alias for <see cref="GetAll"/>.</summary>
    public IReadOnlyList<FlagEvaluationLog> GetEvaluationLogs() => GetAll();

    /// <summary>Alias for <see cref="GetByUserId"/>.</summary>
    public IReadOnlyList<FlagEvaluationLog> GetEvaluationLogsForUser(string userId) => GetByUserId(userId);

    /// <summary>Alias for <see cref="GetByFlagName"/>.</summary>
    public IReadOnlyList<FlagEvaluationLog> GetEvaluationLogsForFlag(string flagName) => GetByFlagName(flagName);

    /// <summary>Removes all recorded evaluation logs.</summary>
    public void ClearLogs() => _logs.Clear();
}
