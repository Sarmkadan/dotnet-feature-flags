#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Provides validation helpers for <see cref="UserContext"/> instances.
/// Validates all user context properties including required fields, format constraints,
/// and semantic validation rules.
/// </summary>
public static class UserContextValidation
{
    /// <summary>
    /// Validates the user context and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The user context to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UserContext value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(value.UserId))
        {
            errors.Add("UserId is required and cannot be null or whitespace.");
        }
        else if (value.UserId.Length > 100)
        {
            errors.Add("UserId exceeds maximum length of 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.Email))
        {
            errors.Add("Email is required and cannot be null or whitespace.");
        }
        else if (value.Email.Length > 254)
        {
            errors.Add("Email exceeds maximum length of 254 characters.");
        }
        else if (!IsValidEmail(value.Email))
        {
            errors.Add("Email format is invalid.");
        }

        // Validate optional fields with constraints
        if (value.Country is not null)
        {
            if (value.Country.Length > 2)
            {
                errors.Add("Country code exceeds maximum length of 2 characters.");
            }

            if (!IsValidCountryCode(value.Country))
            {
                errors.Add("Country code format is invalid. Expected ISO 3166-1 alpha-2 format.");
            }
        }

        if (value.Tier is not null)
        {
            if (value.Tier.Length > 50)
            {
                errors.Add("Tier exceeds maximum length of 50 characters.");
            }

            if (!IsValidTier(value.Tier))
            {
                errors.Add("Tier contains invalid characters. Allowed: alphanumeric, hyphen, underscore.");
            }
        }

        if (value.Region is not null && value.Region.Length > 100)
        {
            errors.Add("Region exceeds maximum length of 100 characters.");
        }

        // Validate CreatedAt (must be in the past and reasonable)
        var utcNow = DateTime.UtcNow;
        if (value.CreatedAt > utcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }
        else if (value.CreatedAt < DateTime.UnixEpoch)
        {
            errors.Add("CreatedAt is before the Unix epoch.");
        }

        // Validate CustomAttributes
        if (value.CustomAttributes.Count > 100)
        {
            errors.Add("CustomAttributes dictionary exceeds maximum size of 100 entries.");
        }

        foreach (var kvp in value.CustomAttributes)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
            {
                errors.Add("CustomAttributes contains an entry with null or empty key.");
                break;
            }

            if (kvp.Key.Length > 100)
            {
                errors.Add("CustomAttributes key exceeds maximum length of 100 characters.");
                break;
            }

            if (kvp.Value is not null && kvp.Value.Length > 1000)
            {
                errors.Add("CustomAttributes value exceeds maximum length of 1000 characters.");
                break;
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the user context is valid.
    /// </summary>
    /// <param name="value">The user context to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UserContext value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the user context is valid, throwing an exception with all validation errors if not.
    /// </summary>
    /// <param name="value">The user context to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the user context contains validation errors.</exception>
    public static void EnsureValid(this UserContext value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"UserContext validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Basic email validation without regex for performance
            var atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex >= email.Length - 1)
                return false;

            var dotIndex = email.LastIndexOf('.');
            if (dotIndex <= atIndex + 1 || dotIndex == email.Length - 1)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return true; // null/empty is valid

        if (countryCode.Length != 2)
            return false;

        // Check if it's alphabetic
        return char.IsLetter(countryCode[0]) && char.IsLetter(countryCode[1]);
    }

    private static bool IsValidTier(string tier)
    {
        if (string.IsNullOrWhiteSpace(tier))
            return true;

        foreach (var c in tier)
        {
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                return false;
        }

        return true;
    }
}