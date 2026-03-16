#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Records and provides access to flag evaluation audit logs.
/// Implementations may store logs in-memory, a database, or an external sink.
/// </summary>
public interface IFlagEvaluationLogService
{
    /// <summary>Records a flag evaluation event.</summary>
    void Log(FlagEvaluationLog entry);

    /// <summary>Returns all recorded evaluation logs.</summary>
    IReadOnlyList<FlagEvaluationLog> GetAll();

    /// <summary>Returns evaluation logs for a specific user.</summary>
    IReadOnlyList<FlagEvaluationLog> GetByUserId(string userId);

    /// <summary>Returns evaluation logs for a specific flag.</summary>
    IReadOnlyList<FlagEvaluationLog> GetByFlagName(string flagName);
}
