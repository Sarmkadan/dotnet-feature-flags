using System;
using System.Collections.Generic;
using System.Linq;

namespace FeatureFlags.Exceptions
{
    /// <summary>
    /// Provides validation helpers for <see cref="ConfigurationException"/>.
    /// </summary>
    public static class ConfigurationExceptionValidation
    {
        /// <summary>
        /// Validates the <see cref="ConfigurationException"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The exception instance to validate.</param>
        /// <returns>A read-only list of validation error messages. Empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this ConfigurationException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Message should be non-null and non-empty
            if (string.IsNullOrWhiteSpace(value.Message))
            {
                problems.Add("Message must not be null, empty, or whitespace.");
            }

            // InnerException can be any exception, but if it is a ConfigurationException we can recursively validate it
            if (value.InnerException is ConfigurationException innerConfig)
            {
                var innerProblems = innerConfig.Validate();
                if (innerProblems.Any())
                {
                    problems.AddRange(innerProblems.Select(p => $"Inner exception: {p}"));
                }
            }

            return problems;
        }

        /// <summary>
        /// Determines whether the <see cref="ConfigurationException"/> instance is valid.
        /// </summary>
        /// <param name="value">The exception instance to check.</param>
        /// <returns>True if no validation problems are found; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this ConfigurationException value)
            => value?.Validate().Count is 0 or null;

        /// <summary>
        /// Ensures that the <see cref="ConfigurationException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The exception instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when validation problems are found.</exception>
        public static void EnsureValid(this ConfigurationException value)
        {
            var problems = value.Validate();
            if (problems.Any())
            {
                var message = $"ConfigurationException validation failed: {string.Join("; ", problems)}";
                throw new ArgumentException(message, nameof(value));
            }
        }
    }
}
