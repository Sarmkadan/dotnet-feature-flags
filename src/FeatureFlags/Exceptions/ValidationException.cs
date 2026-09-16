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

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the validation error.</param>
    public ValidationException(string message) : base(message, "VALIDATION_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and a collection of field errors.
    /// </summary>
    /// <param name="message">The message that describes the validation error.</param>
    /// <param name="errors">A dictionary of field names to their corresponding validation error messages.</param>
    public ValidationException(string message, Dictionary<string, string> errors) : base(message, "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the validation error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
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