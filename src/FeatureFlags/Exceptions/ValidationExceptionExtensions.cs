using System;
using System.Collections.Generic;
using System.Linq;

namespace FeatureFlags.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ValidationException"/> to simplify error handling and inspection.
    /// </summary>
    public static class ValidationExceptionExtensions
    {
        /// <summary>
        /// Determines whether the exception contains any validation errors.
        /// </summary>
        /// <param name="exception">The exception to check for errors.</param>
        /// <returns>True if the exception has errors; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static bool HasErrors(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.Errors?.Any() == true;
        }

        /// <summary>
        /// Retrieves the error message associated with the specified key.
        /// </summary>
        /// <param name="exception">The exception containing the errors.</param>
        /// <param name="key">The error key to look up.</param>
        /// <returns>The error message if found; otherwise, an empty string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        public static string GetErrorMessage(this ValidationException exception, string key)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(key);

            return exception.Errors?.TryGetValue(key, out var value) == true
                ? value
                : string.Empty;
        }

        /// <summary>
        /// Converts the exception's error dictionary into a flattened dictionary.
        /// </summary>
        /// <param name="exception">The exception containing the errors.</param>
        /// <returns>A new dictionary containing all error key-value pairs.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static Dictionary<string, string> ToFlattenedErrorDictionary(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            var flattenedErrors = new Dictionary<string, string>(StringComparer.Ordinal);
            if (exception.Errors != null)
            {
                foreach (var error in exception.Errors)
                {
                    flattenedErrors.Add(error.Key, error.Value);
                }
            }

            return flattenedErrors;
        }
    }
}