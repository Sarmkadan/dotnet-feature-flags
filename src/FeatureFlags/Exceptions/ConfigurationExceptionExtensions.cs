using System;

namespace FeatureFlags.Exceptions
{
	/// <summary>
	/// Provides extension methods for <see cref="ConfigurationException"/> and its derived types.
	/// </summary>
	public static class ConfigurationExceptionExtensions
	{
		/// <summary>
		/// Checks if the exception is a DatabaseConfigurationException
		/// </summary>
		/// <param name="exception">The exception to check. Cannot be null.</param>
		/// <returns>True if the exception is a DatabaseConfigurationException; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
		public static bool IsDatabaseConfigurationError(this ConfigurationException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);
			return exception is DatabaseConfigurationException;
		}

		/// <summary>
		/// Checks if the exception is a HttpClientConfigurationException
		/// </summary>
		/// <param name="exception">The exception to check. Cannot be null.</param>
		/// <returns>True if the exception is a HttpClientConfigurationException; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
		public static bool IsHttpClientConfigurationError(this ConfigurationException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);
			return exception is HttpClientConfigurationException;
		}

		/// <summary>
		/// Gets the root cause message by traversing the inner exception chain
		/// </summary>
		/// <param name="exception">The exception to analyze. Cannot be null.</param>
		/// <returns>The message of the root ConfigurationException in the inner exception chain.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
		public static string GetRootCauseMessage(this ConfigurationException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);

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
		/// <param name="exception">The exception to format. Cannot be null.</param>
		/// <returns>A formatted message containing the error code (if available) and the exception message.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
		public static string ToFormattedMessage(this ConfigurationException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);

			return !string.IsNullOrEmpty(exception.ErrorCode)
				? $"[{exception.ErrorCode}] {exception.Message}"
				: exception.Message;
		}
	}
}