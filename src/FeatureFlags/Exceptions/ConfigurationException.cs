#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Exceptions;

/// <summary>
/// Thrown when there's an issue with application configuration.
/// </summary>
public class ConfigurationException : FeatureFlagException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConfigurationException(string message) : base(message, "CONFIG_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConfigurationException(string message, Exception innerException) : base(message, "CONFIG_ERROR", innerException)
    {
    }
}

/// <summary>
/// Thrown when database configuration is invalid or missing.
/// </summary>
public class DatabaseConfigurationException : ConfigurationException
{
    public DatabaseConfigurationException(string message) : base(message)
    {
    }

    public DatabaseConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when HTTP client configuration is invalid.
/// </summary>
public class HttpClientConfigurationException : ConfigurationException
{
    public HttpClientConfigurationException(string message) : base(message)
    {
    }

    public HttpClientConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}