using System;

namespace FeatureFlags.Exceptions
{
    public static class ConfigurationExceptionExtensions
    {
        /// <summary>
        /// Checks if the exception is a DatabaseConfigurationException
        /// </summary>
        public static bool IsDatabaseConfigurationError(this ConfigurationException exception)
        {
            return exception is DatabaseConfigurationException;
        }

        /// <summary>
        /// Checks if the exception is a HttpClientConfigurationException
        /// </summary>
        public static bool IsHttpClientConfigurationError(this ConfigurationException exception)
        {
            return exception is HttpClientConfigurationException;
        }

        /// <summary>
        /// Gets the root cause message by traversing the inner exception chain
        /// </summary>
        public static string GetRootCauseMessage(this ConfigurationException exception)
        {
            var current = exception;
            while (current.InnerException is ConfigurationException innerConfigException)
            {
                current = innerConfigException;
            }
            return current.Message;
        }

        /// <summary>
        /// Gets a formatted error message including error code if available
        /// </summary>
        public static string ToFormattedMessage(this ConfigurationException exception)
        {
            var message = exception.Message;
            if (!string.IsNullOrEmpty(exception.ErrorCode))
            {
                message = $"[{exception.ErrorCode}] {message}";
            }
            return message;
        }
    }
}
