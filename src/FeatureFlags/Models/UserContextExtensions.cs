using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FeatureFlags.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="UserContext"/>.
    /// </summary>
    public static class UserContextExtensions
    {
        /// <summary>
        /// Retrieves a custom attribute value by <paramref name="key"/> or returns <paramref name="defaultValue"/>
        /// when the attribute is not present.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/> instance.</param>
        /// <param name="key">The attribute key to look up.</param>
        /// <param name="defaultValue">The value to return when the attribute is missing.</param>
        /// <returns>The attribute value if it exists; otherwise <paramref name="defaultValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> is <c>null</c> or empty.</exception>
        public static string? GetCustomAttributeOrDefault(this UserContext context, string key, string? defaultValue = null)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentException.ThrowIfNullOrEmpty(key);

            return context.GetAttribute(key) ?? defaultValue;
        }

        /// <summary>
        /// Returns a read‑only dictionary that contains the core user properties together with all custom attributes.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyDictionary{TKey,TValue}"/> of attribute names and values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, string> GetAllAttributes(this UserContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["UserId"] = context.UserId,
                ["Email"] = context.Email
            };

            if (!string.IsNullOrEmpty(context.Country))
                result["Country"] = context.Country;

            if (!string.IsNullOrEmpty(context.Tier))
                result["Tier"] = context.Tier;

            if (!string.IsNullOrEmpty(context.Region))
                result["Region"] = context.Region;

            foreach (var kvp in context.CustomAttributes)
                result[kvp.Key] = kvp.Value;

            return new ReadOnlyDictionary<string, string>(result);
        }

        /// <summary>
        /// Determines whether the user belongs to the specified <paramref name="tier"/>, using a case‑insensitive comparison.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/> instance.</param>
        /// <param name="tier">The tier name to compare against.</param>
        /// <returns><c>true</c> if the user's tier matches <paramref name="tier"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tier"/> is <c>null</c> or empty.</exception>
        public static bool IsInTier(this UserContext context, string tier)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentException.ThrowIfNullOrEmpty(tier);

            return string.Equals(context.Tier, tier, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Calculates the age of the user context in whole days based on <see cref="UserContext.CreatedAt"/>.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/> instance.</param>
        /// <returns>The number of days elapsed since <see cref="UserContext.CreatedAt"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public static int GetAgeInDays(this UserContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // CreatedAt is already stored as UTC, no conversion needed
            return (int)(DateTime.UtcNow - context.CreatedAt).TotalDays;
        }
    }
}