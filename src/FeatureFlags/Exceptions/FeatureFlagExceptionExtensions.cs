#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace FeatureFlags.Exceptions;

/// <summary>
/// Extension methods for <see cref="FeatureFlagException"/> and derived exception types.
/// </summary>
public static class FeatureFlagExceptionExtensions
{
    /// <summary>
    /// Determines whether the exception is a <see cref="FeatureFlagNotFoundException"/>.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is a FeatureFlagNotFoundException; otherwise, false.</returns>
    public static bool IsFeatureFlagNotFound(this FeatureFlagException exception)
    {
        return exception is FeatureFlagNotFoundException;
    }

    /// <summary>
    /// Determines whether the exception is an <see cref="InvalidFeatureFlagException"/>.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is an InvalidFeatureFlagException; otherwise, false.</returns>
    public static bool IsInvalidFeatureFlag(this FeatureFlagException exception)
    {
        return exception is InvalidFeatureFlagException;
    }

    /// <summary>
    /// Determines whether the exception is a <see cref="RuleEvaluationException"/>.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is a RuleEvaluationException; otherwise, false.</returns>
    public static bool IsRuleEvaluationError(this FeatureFlagException exception)
    {
        return exception is RuleEvaluationException;
    }

    /// <summary>
    /// Determines whether the exception is a <see cref="FeatureFlagDataException"/>.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is a FeatureFlagDataException; otherwise, false.</returns>
    public static bool IsDataError(this FeatureFlagException exception)
    {
        return exception is FeatureFlagDataException;
    }

    /// <summary>
    /// Gets the error code from the exception if available.
    /// </summary>
    /// <param name="exception">The exception to get the error code from.</param>
    /// <returns>The error code if available; otherwise, null.</returns>
    public static string? GetErrorCode(this FeatureFlagException exception)
    {
        return exception?.ErrorCode;
    }

    /// <summary>
    /// Creates a flattened exception message that includes all inner exceptions.
    /// </summary>
    /// <param name="exception">The exception to flatten.</param>
    /// <returns>A string containing the full exception hierarchy.</returns>
    public static string GetFlattenedMessage(this FeatureFlagException exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        var messages = new List<string>();
        var current = exception;

        while (current != null)
        {
            messages.Add(current.Message);
            current = current.InnerException as FeatureFlagException;
        }

        return string.Join(" | ", messages);
    }

    /// <summary>
    /// Determines whether the exception has a specific error code.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <param name="errorCode">The error code to match.</param>
    /// <returns>True if the exception has the specified error code; otherwise, false.</returns>
    public static bool HasErrorCode(this FeatureFlagException exception, string errorCode)
    {
        return string.Equals(exception?.ErrorCode, errorCode, StringComparison.Ordinal);
    }
}
