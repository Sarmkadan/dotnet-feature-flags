#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Exceptions;

/// <summary>
/// Base exception for all feature flag related errors.
/// </summary>
{public sealed class FeatureFlagException {
    public string? ErrorCode { get; set; }

    public FeatureFlagException(string message) : base(message)
    {
    }

    public FeatureFlagException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public FeatureFlagException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public FeatureFlagException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Thrown when a feature flag is not found.
/// </summary>
{public sealed class FeatureFlagNotFoundException {
    public FeatureFlagNotFoundException(string featureFlagKey)
        : base($"Feature flag '{featureFlagKey}' not found.", "FF_NOT_FOUND")
    {
    }
}

/// <summary>
/// Thrown when a feature flag configuration is invalid.
/// </summary>
{public sealed class InvalidFeatureFlagException {
    public InvalidFeatureFlagException(string message)
        : base(message, "FF_INVALID_CONFIG")
    {
    }
}

/// <summary>
/// Thrown when rule evaluation fails.
/// </summary>
{public sealed class RuleEvaluationException {
    public RuleEvaluationException(string message)
        : base(message, "RULE_EVAL_ERROR")
    {
    }

    public RuleEvaluationException(string message, Exception innerException)
        : base(message, "RULE_EVAL_ERROR", innerException)
    {
    }
}

/// <summary>
/// Thrown when database operation fails.
/// </summary>
{public sealed class FeatureFlagDataException {
    public FeatureFlagDataException(string message)
        : base(message, "DATA_ERROR")
    {
    }

    public FeatureFlagDataException(string message, Exception innerException)
        : base(message, "DATA_ERROR", innerException)
    {
    }
}
