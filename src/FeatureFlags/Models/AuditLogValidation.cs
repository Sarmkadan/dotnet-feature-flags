#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Provides validation helpers for <see cref="AuditLog"/> instances to ensure data integrity
/// before persistence and to validate audit trail completeness for compliance requirements.
/// </summary>
/// <remarks>
/// This class uses extension methods to validate <see cref="AuditLog"/> instances against business rules,
/// ensuring all required fields are present and values are within acceptable ranges.
/// </remarks>
public static class AuditLogValidation
{
    /// <summary>
    /// Validates that an <see cref="AuditLog"/> instance contains all required fields and valid values.
    /// Returns a list of human-readable validation problems; empty list indicates the audit log is valid.
    /// </summary>
    /// <param name="value">The audit log to validate.</param>
    /// <returns>Collection of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AuditLog? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id - must be a positive integer
        if (value.Id <= 0)
        {
            errors.Add("Id must be a positive integer.");
        }

        // Validate FeatureFlagId - must reference an existing feature flag
        if (value.FeatureFlagId <= 0)
        {
            errors.Add("FeatureFlagId must be a positive integer.");
        }

        // Validate Action - must be a defined AuditAction enum value
        if (!Enum.IsDefined(typeof(AuditAction), value.Action))
        {
            errors.Add($"Action '{value.Action}' is not a valid AuditAction value.");
        }

        // Validate ChangedBy - must identify who made the change
        if (string.IsNullOrWhiteSpace(value.ChangedBy))
        {
            errors.Add("ChangedBy cannot be null, empty, or whitespace.");
        }
        else if (value.ChangedBy.Length > 256)
        {
            errors.Add("ChangedBy exceeds maximum length of 256 characters.");
        }

        // Validate ChangedAt
        if (value.ChangedAt == default)
        {
            errors.Add("ChangedAt cannot be the default DateTime value.");
        }
        else if (value.ChangedAt > DateTime.UtcNow.AddMinutes(1))
        {
            errors.Add("ChangedAt cannot be more than 1 minute in the future.");
        }

        // Validate OldValue and NewValue based on Action type using pattern matching
        switch (value.Action)
        {
            case AuditAction.Created:
                if (string.IsNullOrWhiteSpace(value.NewValue))
                {
                    errors.Add("NewValue must be specified for Created action.");
                }
                break;

            case AuditAction.Deleted:
                if (string.IsNullOrWhiteSpace(value.OldValue))
                {
                    errors.Add("OldValue must be specified for Deleted action.");
                }
                break;

            case AuditAction.Enabled or AuditAction.Disabled or AuditAction.Updated
            or AuditAction.RolloutChanged or AuditAction.RuleAdded or AuditAction.RuleRemoved
            or AuditAction.VariantUpdated or AuditAction.Evaluated:
                // These actions should have both OldValue and NewValue
                if (string.IsNullOrWhiteSpace(value.OldValue) && string.IsNullOrWhiteSpace(value.NewValue))
                {
                    errors.Add("At least one of OldValue or NewValue must be specified for this action type.");
                }
                break;
        }

        // Validate Description - provides context about the change
        if (string.IsNullOrWhiteSpace(value.Description))
        {
            errors.Add("Description cannot be null, empty, or whitespace.");
        }
        else if (value.Description.Length > 1024)
        {
            errors.Add("Description exceeds maximum length of 1024 characters.");
        }

        // Validate optional fields if present
        if (value.IpAddress is not null)
        {
            if (value.IpAddress.Length > 45) // IPv6 max length
            {
                errors.Add("IpAddress exceeds maximum length of 45 characters.");
            }
            else if (!IsValidIpAddress(value.IpAddress))
            {
                errors.Add("IpAddress is not a valid IP address format.");
            }
        }

        if (value.UserAgent is not null && value.UserAgent.Length > 512)
        {
            errors.Add("UserAgent exceeds maximum length of 512 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AuditLog"/> instance is valid.
    /// </summary>
    /// <param name="value">The audit log to check.</param>
    /// <returns>True if the audit log is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AuditLog? value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="AuditLog"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if the audit log is invalid.
    /// </summary>
    /// <param name="value">The audit log to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the audit log is invalid.</exception>
    public static void EnsureValid(this AuditLog? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"AuditLog is invalid. Validation errors: {string.Join(" ", errors)}");
    }

    /// <summary>
    /// Validates that a string is a valid IP address (IPv4 or IPv6).
    /// </summary>
    /// <param name="ipAddress">The IP address string to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    private static bool IsValidIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return false;
        }

        // Use built-in IPAddress.TryParse for accurate validation
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }
}
