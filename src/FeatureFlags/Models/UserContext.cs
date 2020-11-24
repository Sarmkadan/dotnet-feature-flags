#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Represents the context of a user for evaluating feature flags.
/// Contains user identity and attributes used for targeting and rollout decisions.
/// </summary>
public sealed class UserContext
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string? Tier { get; set; }

    public string? Region { get; set; }

    public Dictionary<string, string> CustomAttributes { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Retrieves an attribute value from either standard properties or custom attributes.
    /// </summary>
    public string? GetAttribute(string attributeName)
    {
        return attributeName.ToLower() switch
        {
            "userid" => UserId,
            "email" => Email,
            "country" => Country,
            "tier" => Tier,
            "region" => Region,
            _ => CustomAttributes.TryGetValue(attributeName, out var value) ? value : null
        };
    }

    /// <summary>
    /// Sets a custom attribute value for advanced targeting scenarios.
    /// </summary>
    public void SetCustomAttribute(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Attribute key cannot be empty", nameof(key));

        CustomAttributes[key] = value;
    }

    /// <summary>
    /// Validates the user context has required identity information.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(Email);
    }

    /// <summary>
    /// Generates a stable hash for consistent percentage-based rollout evaluation.
    /// Uses userId as the stable identifier for rollout bucketing.
    /// </summary>
    public int GetConsistentHash(string featureFlagKey)
    {
        string canonicalUserId = UserId;
        if (long.TryParse(UserId, out long numericUserId))
        {
            // If UserId is numeric, use its canonical string representation
            canonicalUserId = numericUserId.ToString();
        }

        var combined = $"{canonicalUserId}:{featureFlagKey}";
        // Hotfix: Fix percentage rollout inconsistency across application restarts
        // Using a consistent hash algorithm instead of string.GetHashCode() which varies between app restarts
        uint hash = 0;
        foreach (char c in combined)
        {
            hash = (hash << 5) - hash + c;
        }
        return (int)(hash % 100);
    }
}
