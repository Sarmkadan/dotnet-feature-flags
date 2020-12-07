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
        /// Validates the <see cref="ConfigurationException"/> instance and returns a list of human‑readable problems.
        /// </summary>
        /// <param name="value">The exception instance to validate.</param>
        /// <returns>A read‑only list of validation error messages. Empty if the instance is valid.</returns>
        public static IReadOnlyList<string> Validate(this ConfigurationException value)
        {
            var problems = new List<string>();

            if (value is null)
            {
                problems.Add("ConfigurationException instance is null.");
                return problems;
            }

            // Message should be non‑null and non‑empty
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
        public static bool IsValid(this ConfigurationException value)
        {
            return !value.Validate().Any();
        }

        /// <summary>
        /// Ensures that the <see cref="ConfigurationException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The exception instance to validate.</param>
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
