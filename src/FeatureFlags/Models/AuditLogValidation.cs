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

        // Validate Id
        if (value.Id < 0)
        {
            errors.Add($"Id must be a non-negative integer, but was {value.Id}.");
        }

        // Validate FeatureFlagId
        if (value.FeatureFlagId <= 0)
        {
            errors.Add("FeatureFlagId must be a positive integer.");
        }

        // Validate Action
        if (!Enum.IsDefined(typeof(AuditAction), value.Action))
        {
            errors.Add($"Action '{value.Action}' is not a valid AuditAction value.");
        }

        // Validate ChangedBy
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
        else if (value.ChangedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("ChangedAt cannot be in the future.");
        }

        // Validate OldValue and NewValue based on Action type
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

            case AuditAction.Enabled:
            case AuditAction.Disabled:
            case AuditAction.Updated:
            case AuditAction.RolloutChanged:
            case AuditAction.RuleAdded:
            case AuditAction.RuleRemoved:
            case AuditAction.VariantUpdated:
            case AuditAction.Evaluated:
                // These actions should have both OldValue and NewValue
                if (string.IsNullOrWhiteSpace(value.OldValue) && string.IsNullOrWhiteSpace(value.NewValue))
                {
                    errors.Add("At least one of OldValue or NewValue must be specified for this action type.");
                }
                break;
        }

        // Validate Description
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

        if (value.UserAgent is not null)
        {
            if (value.UserAgent.Length > 512)
            {
                errors.Add("UserAgent exceeds maximum length of 512 characters.");
            }
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

        // Simple validation - check for common patterns
        // IPv4: 1-3 digits, 0-255, separated by dots
        // IPv6: hex groups separated by colons
        return ipAddress.Split('.').Length == 4 || ipAddress.Split(':').Length > 2;
    }
}
