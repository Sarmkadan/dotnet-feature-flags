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
        /// <param name="exception">The <see cref="ValidationException"/> instance to check for errors.</param>
        /// <returns>True if the <see cref="ValidationException.Errors"/> collection contains any entries; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static bool HasErrors(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.Errors?.Any() == true;
        }

        /// <summary>
        /// Retrieves the error message associated with the specified key from the exception's error collection.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance containing the errors.</param>
        /// <param name="key">The key to look up in the error collection.</param>
        /// <returns>
        /// The error message corresponding to the specified <paramref name="key"/> if found;
        /// otherwise, an empty string.
        /// </returns>
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
        /// Converts the exception's error dictionary into a new flattened dictionary containing all key-value pairs.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance containing the errors.</param>
        /// <returns>
        /// A new <see cref="Dictionary{String, String}"/> containing all key-value pairs from the
        /// <see cref="ValidationException.Errors"/> collection. If the error collection is null or empty,
        /// an empty dictionary is returned.
        /// </returns>
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
