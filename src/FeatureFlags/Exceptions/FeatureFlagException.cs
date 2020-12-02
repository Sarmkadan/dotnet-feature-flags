#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Exceptions;

/// <summary>
/// Base exception for all feature flag related errors.
/// </summary>
public class FeatureFlagException : Exception {
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
public class FeatureFlagNotFoundException : FeatureFlagException {
    public FeatureFlagNotFoundException(string featureFlagKey)
        : base($"Feature flag '{featureFlagKey}' not found.", "FF_NOT_FOUND")
    {
    }
}

/// <summary>
/// Thrown when a feature flag configuration is invalid.
/// </summary>
public class InvalidFeatureFlagException : FeatureFlagException {
    public InvalidFeatureFlagException(string message)
        : base(message, "FF_INVALID_CONFIG")
    {
    }
}

/// <summary>
/// Thrown when rule evaluation fails.
/// </summary>
public class RuleEvaluationException : FeatureFlagException {
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
public class FeatureFlagDataException : FeatureFlagException {
    public FeatureFlagDataException(string message)
        : base(message, "DATA_ERROR")
    {
    }

    public FeatureFlagDataException(string message, Exception innerException)
        : base(message, "DATA_ERROR", innerException)
    {
    }
}
