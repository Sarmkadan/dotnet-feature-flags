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
    public ConfigurationException(string message) : base(message, "CONFIG_ERROR")
    {
    }

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