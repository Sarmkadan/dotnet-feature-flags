#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Integration;

/// <summary>
/// Provides validation helpers for <see cref="Webhook"/> instances.
/// </summary>
public static class WebhookValidation
{
    /// <summary>
    /// Validates a webhook instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The webhook to validate.</param>
    /// <returns>An immutable list of validation problems; empty if the webhook is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Webhook value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Url
        if (string.IsNullOrWhiteSpace(value.Url))
        {
            problems.Add("Url cannot be null or whitespace.");
        }
        else if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uri))
        {
            problems.Add("Url must be a valid absolute URI.");
        }
        else if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            problems.Add("Url must be a valid absolute HTTP or HTTPS URI.");
        }

        // Validate Description
        if (string.IsNullOrWhiteSpace(value.Description))
        {
            problems.Add("Description cannot be null or whitespace.");
        }

        // Validate CreatedBy
        if (string.IsNullOrWhiteSpace(value.CreatedBy))
        {
            problems.Add("CreatedBy cannot be null or whitespace.");
        }

        // Validate MaxRetries
        if (value.MaxRetries < 0)
        {
            problems.Add("MaxRetries cannot be negative.");
        }

        // Validate RetryDelaySeconds
        if (value.RetryDelaySeconds < 0)
        {
            problems.Add("RetryDelaySeconds cannot be negative.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt cannot be default(DateTime).");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt == default)
        {
            problems.Add("UpdatedAt cannot be default(DateTime).");
        }
        else if (value.UpdatedAt < value.CreatedAt)
        {
            problems.Add("UpdatedAt cannot be earlier than CreatedAt.");
        }

        // Validate LastTriggeredAt
        if (value.LastTriggeredAt is { } lastTriggered && lastTriggered == default)
        {
            problems.Add("LastTriggeredAt cannot be default(DateTime) if set.");
        }
        else if (value.LastTriggeredAt.HasValue && value.LastTriggeredAt.Value < value.CreatedAt)
        {
            problems.Add("LastTriggeredAt cannot be earlier than CreatedAt.");
        }

        // Validate FeatureFlagKey if set
        if (!string.IsNullOrWhiteSpace(value.FeatureFlagKey) &&
            string.IsNullOrWhiteSpace(value.FeatureFlagKey.Trim()))
        {
            problems.Add("FeatureFlagKey cannot be whitespace if set.");
        }

        // Validate AuthorizationHeader if set
        if (!string.IsNullOrWhiteSpace(value.AuthorizationHeader) &&
            string.IsNullOrWhiteSpace(value.AuthorizationHeader.Trim()))
        {
            problems.Add("AuthorizationHeader cannot be whitespace if set.");
        }

        // Validate Secret if set
        if (!string.IsNullOrWhiteSpace(value.Secret) &&
            string.IsNullOrWhiteSpace(value.Secret.Trim()))
        {
            problems.Add("Secret cannot be whitespace if set.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a webhook instance is valid.
    /// </summary>
    /// <param name="value">The webhook to check.</param>
    /// <returns>True if the webhook is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Webhook value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that a webhook instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message if validation fails.
    /// </summary>
    /// <param name="value">The webhook to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the webhook is invalid, with a detailed message.</exception>
    public static void EnsureValid(this Webhook value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
            $"Webhook validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
