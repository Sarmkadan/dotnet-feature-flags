#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace FeatureFlags.Events;

/// <summary>
/// Extension methods for <see cref="FeatureFlagEvent"/> that provide common operations
/// for filtering, metadata access, and event manipulation.
/// </summary>
public static class FeatureFlagEventExtensions
{
    /// <summary>
    /// Filters events by the specified event type.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <param name="eventType">The event type to match against.</param>
    /// <returns>True if the event matches the specified type; otherwise, false.</returns>
    public static bool IsType(this FeatureFlagEvent @event, string eventType)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrEmpty(eventType);

        return string.Equals(@event.EventType, eventType, StringComparison.Ordinal);
    }

    /// <summary>
    /// Checks if the event contains metadata with the specified key.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <param name="key">The metadata key to look for.</param>
    /// <returns>True if the metadata contains the key; otherwise, false.</returns>
    public static bool HasMetadataKey(this FeatureFlagEvent @event, string key)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return @event.Metadata.ContainsKey(key);
    }

    /// <summary>
    /// Gets the metadata value for the specified key, or returns the default value if the key doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="event">The event containing the metadata.</param>
    /// <param name="key">The metadata key to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
    /// <returns>The metadata value if it exists and is of the correct type; otherwise, the default value.</returns>
    public static T? GetMetadataValue<T>(this FeatureFlagEvent @event, string key, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (@event.Metadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Creates a new <see cref="FeatureFlagEvent"/> with the same properties except for the specified metadata key.
    /// </summary>
    /// <param name="event">The original event to clone.</param>
    /// <param name="key">The metadata key to set.</param>
    /// <param name="value">The metadata value to set.</param>
    /// <returns>A new event with the updated metadata.</returns>
    public static FeatureFlagEvent WithMetadata(this FeatureFlagEvent @event, string key, object? value)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var newEvent = new FeatureFlagEvent
        {
            EventType = @event.EventType,
            FeatureFlagId = @event.FeatureFlagId,
            FeatureFlagKey = @event.FeatureFlagKey,
            TriggeredBy = @event.TriggeredBy,
            OccurredAt = @event.OccurredAt,
            Metadata = new Dictionary<string, object?>(@event.Metadata)
        };

        newEvent.Metadata[key] = value;
        return newEvent;
    }

    /// <summary>
    /// Creates a new <see cref="FeatureFlagEvent"/> with updated occurred timestamp.
    /// </summary>
    /// <param name="event">The original event to clone.</param>
    /// <param name="occurredAt">The new timestamp for the event.</param>
    /// <returns>A new event with the updated timestamp.</returns>
    public static FeatureFlagEvent WithOccurredAt(this FeatureFlagEvent @event, DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new FeatureFlagEvent
        {
            EventType = @event.EventType,
            FeatureFlagId = @event.FeatureFlagId,
            FeatureFlagKey = @event.FeatureFlagKey,
            TriggeredBy = @event.TriggeredBy,
            OccurredAt = occurredAt,
            Metadata = new Dictionary<string, object?>(@event.Metadata)
        };
    }

    /// <summary>
    /// Checks if the event was triggered by the specified user or system.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <param name="triggeredBy">The user/system name to match against.</param>
    /// <returns>True if the event was triggered by the specified entity; otherwise, false.</returns>
    public static bool IsTriggeredBy(this FeatureFlagEvent @event, string triggeredBy)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrEmpty(triggeredBy);

        return string.Equals(@event.TriggeredBy, triggeredBy, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the event occurred within the specified time range.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <param name="start">The start of the time range (inclusive).</param>
    /// <param name="end">The end of the time range (inclusive).</param>
    /// <returns>True if the event occurred within the time range; otherwise, false.</returns>
    public static bool OccurredBetween(this FeatureFlagEvent @event, DateTime start, DateTime end)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event.OccurredAt >= start && @event.OccurredAt <= end;
    }

    /// <summary>
    /// Gets a string representation of the event for logging purposes.
    /// </summary>
    /// <param name="event">The event to format.</param>
    /// <returns>A formatted string representation of the event.</returns>
    public static string ToLogString(this FeatureFlagEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return $"FeatureFlagEvent {{ Type={@event.EventType}, Key={@event.FeatureFlagKey}, Id={@event.FeatureFlagId}, TriggeredBy={@event.TriggeredBy}, Time={@event.OccurredAt:O} }}";
    }

    /// <summary>
    /// Creates a shallow copy of the event.
    /// </summary>
    /// <param name="event">The event to copy.</param>
    /// <returns>A new event with the same property values.</returns>
    [return: NotNullIfNotNull(nameof(@event))]
    public static FeatureFlagEvent? Clone(this FeatureFlagEvent? @event)
    {
        if (@event is null)
        {
            return null;
        }

        return new FeatureFlagEvent
        {
            EventType = @event.EventType,
            FeatureFlagId = @event.FeatureFlagId,
            FeatureFlagKey = @event.FeatureFlagKey,
            TriggeredBy = @event.TriggeredBy,
            OccurredAt = @event.OccurredAt,
            Metadata = new Dictionary<string, object?>(@event.Metadata)
        };
    }
}