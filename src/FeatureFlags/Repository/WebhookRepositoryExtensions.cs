#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeatureFlags.Integration;

namespace FeatureFlags.Repository;

/// <summary>
/// Extension methods for <see cref="WebhookRepository"/> providing additional query
/// capabilities and convenience methods for common webhook operations.
/// </summary>
public static class WebhookRepositoryExtensions
{
    /// <summary>
    /// Gets active webhooks that should trigger for a specific event type.
    /// </summary>
    /// <param name="repository">The webhook repository instance.</param>
    /// <param name="eventType">The event type to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active webhooks matching the event type, ordered by most recent first.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<List<Webhook>> GetActiveByEventTypeAsync(
        this WebhookRepository repository,
        WebhookEventType eventType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetByEventTypeAsync(eventType);
    }

    /// <summary>
    /// Gets active webhooks that have been failing recently.
    /// </summary>
    /// <param name="repository">The webhook repository instance.</param>
    /// <param name="maxAgeHours">Maximum age in hours to consider a failure recent.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active webhooks with recent failures.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<List<Webhook>> GetFailingAsync(
        this WebhookRepository repository,
        int maxAgeHours = 24,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetRecentFailuresAsync(maxAgeHours);
    }

    /// <summary>
    /// Gets a webhook by its identifier, returning null if it does not exist.
    /// </summary>
    /// <param name="repository">The webhook repository instance.</param>
    /// <param name="id">The webhook identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The webhook if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<Webhook?> FindByIdAsync(
        this WebhookRepository repository,
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetByIdAsync(id);
    }

    /// <summary>
    /// Determines whether a webhook with the specified identifier exists.
    /// </summary>
    /// <param name="repository">The webhook repository instance.</param>
    /// <param name="id">The webhook identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the webhook exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<bool> ExistsAsync(
        this WebhookRepository repository,
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetByIdAsync(id) is not null;
    }

    /// <summary>
    /// Gets the total number of registered webhooks.
    /// </summary>
    /// <param name="repository">The webhook repository instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total count of webhooks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<int> CountAsync(
        this WebhookRepository repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetCountAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the number of active webhooks.
    /// </summary>
    /// <param name="repository">The webhook repository instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of active webhooks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<int> CountActiveAsync(
        this WebhookRepository repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetActiveCountAsync(cancellationToken);
    }

    /// <summary>
    /// Gets active webhooks that should trigger for any of the specified event types.
    /// </summary>
    /// <param name="repository">The webhook repository instance.</param>
    /// <param name="eventTypes">Collection of event types to match against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active webhooks matching any of the event types.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository or eventTypes is null.</exception>
    public static async Task<List<Webhook>> GetActiveForEventTypesAsync(
        this WebhookRepository repository,
        IEnumerable<WebhookEventType> eventTypes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(eventTypes);

        var types = eventTypes.ToList();
        if (types.Count == 0)
        {
            return new List<Webhook>();
        }

        var active = await repository.GetActiveAsync();
        return active
            .Where(w => types.Any(t => w.ShouldTrigger(t)))
            .ToList();
    }
}