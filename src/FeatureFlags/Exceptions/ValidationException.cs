#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Exceptions;

/// <summary>
/// Thrown when input validation fails.
/// </summary>
public class ValidationException : FeatureFlagException
{
    public Dictionary<string, string> Errors { get; } = new();

    public ValidationException(string message) : base(message, "VALIDATION_ERROR")
    {
    }

    public ValidationException(string message, Dictionary<string, string> errors) : base(message, "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string message, Exception innerException) : base(message, "VALIDATION_ERROR", innerException)
    {
    }
}

/// <summary>
/// Thrown when webhook validation fails.
/// </summary>
public class WebhookValidationException : ValidationException
{
    public WebhookValidationException(string message) : base(message)
    {
    }

    public WebhookValidationException(string message, Dictionary<string, string> errors) : base(message, errors)
    {
    }
}