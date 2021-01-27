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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsFeatureFlagNotFound(this FeatureFlagException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is FeatureFlagNotFoundException;
    }

    /// <summary>
    /// Determines whether the exception is an <see cref="InvalidFeatureFlagException"/>.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is an InvalidFeatureFlagException; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsInvalidFeatureFlag(this FeatureFlagException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidFeatureFlagException;
    }

    /// <summary>
    /// Determines whether the exception is a <see cref="RuleEvaluationException"/>.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is a RuleEvaluationException; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsRuleEvaluationError(this FeatureFlagException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is RuleEvaluationException;
    }

    /// <summary>
    /// Determines whether the exception is a <see cref="FeatureFlagDataException"/>.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is a FeatureFlagDataException; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsDataError(this FeatureFlagException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is FeatureFlagDataException;
    }

    /// <summary>
    /// Gets the error code from the exception if available.
    /// </summary>
    /// <param name="exception">The exception to get the error code from.</param>
    /// <returns>The error code if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string? GetErrorCode(this FeatureFlagException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.ErrorCode;
    }

    /// <summary>
    /// Creates a flattened exception message that includes all inner exceptions.
    /// </summary>
    /// <param name="exception">The exception to flatten.</param>
    /// <returns>A string containing the full exception hierarchy.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string GetFlattenedMessage(this FeatureFlagException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errorCode"/> is null.</exception>
    public static bool HasErrorCode(this FeatureFlagException exception, string errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(errorCode);

        return string.Equals(exception.ErrorCode, errorCode, StringComparison.Ordinal);
    }
}