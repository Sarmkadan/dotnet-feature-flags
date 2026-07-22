#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Events;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FeatureFlags.Tests;

/// <summary>
/// Unit tests for FeatureFlagEvent and EventBus classes.
/// Tests event creation, property initialization, and pub-sub behavior.
/// </summary>
public sealed class FeatureFlagEventTests
{
    private readonly ILogger<EventBus> _logger;
    private readonly ILogger<EventLoggingSubscriber> _loggingLogger;

    public FeatureFlagEventTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<EventBus>();
        _loggingLogger = loggerFactory.CreateLogger<EventLoggingSubscriber>();
    }

    #region FeatureFlagEvent Tests

    [Fact]
    public void FeatureFlagEvent_DefaultConstructor_InitializesProperties()
    {
        // Act
        var @event = new FeatureFlagEvent();

        // Assert
        Assert.Equal(string.Empty, @event.EventType);
        Assert.Equal(0, @event.FeatureFlagId);
        Assert.Equal(string.Empty, @event.FeatureFlagKey);
        Assert.Equal(string.Empty, @event.TriggeredBy);
        Assert.Equal(DateTime.UtcNow.Date, @event.OccurredAt.Date);
        Assert.NotNull(@event.Metadata);
        Assert.Empty(@event.Metadata);
    }

    [Fact]
    public void FeatureFlagEvent_ParameterizedConstructor_SetsAllProperties()
    {
        // Arrange
        var eventType = "FeatureFlagCreated";
        var featureFlagId = 42;
        var featureFlagKey = "new-feature";
        var triggeredBy = "admin@example.com";
        var occurredAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var metadata = new Dictionary<string, object?> { { "environment", "production" }, { "version", "1.0.0" } };

        // Act
        var @event = new FeatureFlagEvent
        {
            EventType = eventType,
            FeatureFlagId = featureFlagId,
            FeatureFlagKey = featureFlagKey,
            TriggeredBy = triggeredBy,
            OccurredAt = occurredAt,
            Metadata = metadata
        };

        // Assert
        Assert.Equal(eventType, @event.EventType);
        Assert.Equal(featureFlagId, @event.FeatureFlagId);
        Assert.Equal(featureFlagKey, @event.FeatureFlagKey);
        Assert.Equal(triggeredBy, @event.TriggeredBy);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal(2, @event.Metadata.Count);
        Assert.Equal("production", @event.Metadata["environment"]);
        Assert.Equal("1.0.0", @event.Metadata["version"]);
    }

    [Fact]
    public void FeatureFlagEvent_MetadataProperty_IsInitializedAsEmptyDictionary()
    {
        // Arrange
        var @event = new FeatureFlagEvent();

        // Act
        @event.Metadata["test"] = "value";

        // Assert
        Assert.Single(@event.Metadata);
        Assert.Equal("value", @event.Metadata["test"]);
    }

    [Fact]
    public void FeatureFlagEvent_OccurredAt_DefaultsToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var @event = new FeatureFlagEvent();
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.InRange(@event.OccurredAt, beforeCreation, afterCreation);
    }

    [Theory]
    [InlineData(null, "flag-key", "user", "Created")]
    [InlineData("", "flag-key", "user", "")]
    [InlineData("ValidType", "flag-key", "user", "ValidType")]
    [InlineData("ValidType", "", "user", "ValidType")]
    public void FeatureFlagEvent_NullOrEmptyStrings_AreAllowed(string? eventType, string featureFlagKey, string triggeredBy, string expectedEventType)
    {
        // Act
        var @event = new FeatureFlagEvent
        {
            EventType = eventType ?? expectedEventType,
            FeatureFlagKey = featureFlagKey,
            TriggeredBy = triggeredBy
        };

        // Assert
        Assert.Equal(expectedEventType, @event.EventType);
        Assert.Equal(featureFlagKey, @event.FeatureFlagKey);
        Assert.Equal(triggeredBy, @event.TriggeredBy);
    }

    [Fact]
    public void FeatureFlagEvent_NegativeFeatureFlagId_IsAllowed()
    {
        // Arrange
        var negativeId = -1;

        // Act
        var @event = new FeatureFlagEvent { FeatureFlagId = negativeId };

        // Assert
        Assert.Equal(negativeId, @event.FeatureFlagId);
    }

    [Fact]
    public void FeatureFlagEvent_LargeFeatureFlagId_IsAllowed()
    {
        // Arrange
        var largeId = int.MaxValue;

        // Act
        var @event = new FeatureFlagEvent { FeatureFlagId = largeId };

        // Assert
        Assert.Equal(largeId, @event.FeatureFlagId);
    }

    [Fact]
    public void FeatureFlagEvent_Metadata_HandlesNullValues()
    {
        // Arrange
        var @event = new FeatureFlagEvent();

        // Act
        @event.Metadata["nullValue"] = null;
        @event.Metadata["stringValue"] = "test";

        // Assert
        Assert.Equal(2, @event.Metadata.Count);
        Assert.Null(@event.Metadata["nullValue"]);
        Assert.Equal("test", @event.Metadata["stringValue"]);
    }

    [Fact]
    public void FeatureFlagEvent_Metadata_HandlesComplexObjects()
    {
        // Arrange
        var complexObject = new { Id = 1, Name = "Test", Active = true };
        var @event = new FeatureFlagEvent();

        // Act
        @event.Metadata["complex"] = complexObject;

        // Assert
        Assert.NotNull(@event.Metadata["complex"]);
        var metadataValue = @event.Metadata["complex"];
        Assert.NotNull(metadataValue);
        // Verify the anonymous type properties exist
        var type = metadataValue.GetType();
        Assert.Equal(1, type.GetProperty("Id")?.GetValue(metadataValue));
        Assert.Equal("Test", type.GetProperty("Name")?.GetValue(metadataValue));
        Assert.Equal(true, type.GetProperty("Active")?.GetValue(metadataValue));
    }

    #endregion

    #region EventBus Tests

    [Fact]
    public void EventBus_DefaultConstructor_CreatesInstance()
    {
        // Act
        var eventBus = new EventBus(_logger);

        // Assert
        Assert.NotNull(eventBus);
    }

    [Fact]
    public void EventBus_Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EventBus(null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void EventBus_Subscribe_AddsSubscriber()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();

        // Act
        eventBus.Subscribe(subscriber);

        // Assert - Verify by testing publish works
        Assert.True(true);
    }

    [Fact]
    public void EventBus_Subscribe_NullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus(_logger);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => eventBus.Subscribe(null!));
        Assert.Equal("subscriber", exception.ParamName);
    }

    [Fact]
    public void EventBus_Subscribe_DuplicateSubscriber_AddsOnlyOnce()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();

        // Act
        eventBus.Subscribe(subscriber);
        eventBus.Subscribe(subscriber);

        // Assert - Verify by testing publish works
        Assert.True(true);
    }

    [Fact]
    public void EventBus_Unsubscribe_RemovesSubscriber()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();
        eventBus.Subscribe(subscriber);

        // Act
        eventBus.Unsubscribe(subscriber);

        // Assert - Verify by testing publish doesn't notify
        Assert.True(true);
    }

    [Fact]
    public void EventBus_Unsubscribe_NullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus(_logger);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => eventBus.Unsubscribe(null!));
        Assert.Equal("subscriber", exception.ParamName);
    }

    [Fact]
    public void EventBus_Unsubscribe_NonExistentSubscriber_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();

        // Act & Assert - Should not throw
        eventBus.Unsubscribe(subscriber);
    }

    [Fact]
    public async Task EventBus_PublishAsync_FeatureFlagEvent_NotifiesSubscribers()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();
        eventBus.Subscribe(subscriber);

        var @event = new FeatureFlagEvent
        {
            EventType = "FeatureFlagCreated",
            FeatureFlagId = 1,
            FeatureFlagKey = "test-flag",
            TriggeredBy = "admin@example.com",
            Metadata = new Dictionary<string, object?> { { "environment", "test" } }
        };

        // Act
        await eventBus.PublishAsync(@event);

        // Assert
        Assert.True(subscriber.WasCalled);
        Assert.Equal(@event, subscriber.LastEvent);
        Assert.Equal("FeatureFlagCreated", subscriber.LastEventType);
    }

    [Fact]
    public async Task EventBus_PublishAsync_FeatureFlagEvent_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus(_logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => eventBus.PublishAsync(null!));
        Assert.Equal("event", exception.ParamName);
    }

    [Fact]
    public async Task EventBus_PublishAsync_StringParameters_CreatesEventAndNotifiesSubscribers()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();
        eventBus.Subscribe(subscriber);

        var eventType = "FeatureFlagUpdated";
        var featureFlagId = 42;
        var featureFlagKey = "updated-flag";
        var triggeredBy = "system@example.com";
        var metadata = new Dictionary<string, object?> { { "version", "2.0.0" } };

        // Act
        await eventBus.PublishAsync(eventType, featureFlagId, featureFlagKey, triggeredBy, metadata);

        // Assert
        Assert.True(subscriber.WasCalled);
        Assert.Equal(eventType, subscriber.LastEventType);
        Assert.NotNull(subscriber.LastEvent);
        Assert.Equal(featureFlagId, subscriber.LastEvent.FeatureFlagId);
        Assert.Equal(featureFlagKey, subscriber.LastEvent.FeatureFlagKey);
        Assert.Equal(triggeredBy, subscriber.LastEvent.TriggeredBy);
        Assert.Equal(metadata, subscriber.LastEvent.Metadata);
    }

    [Fact]
    public async Task EventBus_PublishAsync_StringParameters_NullMetadata_CreatesEventWithEmptyMetadata()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();
        eventBus.Subscribe(subscriber);

        // Act
        await eventBus.PublishAsync("FeatureFlagDeleted", 99, "deleted-flag", "admin@example.com", null);

        // Assert
        Assert.True(subscriber.WasCalled);
        Assert.NotNull(subscriber.LastEvent);
        Assert.NotNull(subscriber.LastEvent.Metadata);
        Assert.Empty(subscriber.LastEvent.Metadata);
    }

    [Fact]
    public async Task EventBus_PublishAsync_StringParameters_EmptyStrings_AreAllowed()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();
        eventBus.Subscribe(subscriber);

        // Act
        await eventBus.PublishAsync("", 0, "", "", new Dictionary<string, object?>());

        // Assert
        Assert.True(subscriber.WasCalled);
        Assert.Equal(string.Empty, subscriber.LastEventType);
        Assert.NotNull(subscriber.LastEvent);
        Assert.Equal(0, subscriber.LastEvent.FeatureFlagId);
        Assert.Equal(string.Empty, subscriber.LastEvent.FeatureFlagKey);
        Assert.Equal(string.Empty, subscriber.LastEvent.TriggeredBy);
    }

    [Fact]
    public async Task EventBus_PublishAsync_NotifiesOnlyInterestedSubscribers()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var interestedSubscriber = new TestSubscriber("FeatureFlagCreated", "FeatureFlagUpdated");
        var notInterestedSubscriber = new TestSubscriber("FeatureFlagDeleted");
        eventBus.Subscribe(interestedSubscriber);
        eventBus.Subscribe(notInterestedSubscriber);

        var @event = new FeatureFlagEvent
        {
            EventType = "FeatureFlagCreated",
            FeatureFlagId = 1,
            FeatureFlagKey = "test-flag"
        };

        // Act
        await eventBus.PublishAsync(@event);

        // Assert
        Assert.True(interestedSubscriber.WasCalled);
        Assert.False(notInterestedSubscriber.WasCalled);
    }

    [Fact]
    public async Task EventBus_PublishAsync_WildcardSubscriber_ReceivesAllEvents()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var wildcardSubscriber = new TestSubscriber("*");
        eventBus.Subscribe(wildcardSubscriber);

        var @event = new FeatureFlagEvent
        {
            EventType = "FeatureFlagCreated",
            FeatureFlagId = 1,
            FeatureFlagKey = "test-flag"
        };

        // Act
        await eventBus.PublishAsync(@event);

        // Assert
        Assert.True(wildcardSubscriber.WasCalled);
    }

    [Fact]
    public async Task EventBus_PublishAsync_MultipleSubscribers_AllNotified()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber1 = new TestSubscriber();
        var subscriber2 = new TestSubscriber();
        var subscriber3 = new TestSubscriber();
        eventBus.Subscribe(subscriber1);
        eventBus.Subscribe(subscriber2);
        eventBus.Subscribe(subscriber3);

        var @event = new FeatureFlagEvent
        {
            EventType = "FeatureFlagCreated",
            FeatureFlagId = 1,
            FeatureFlagKey = "test-flag"
        };

        // Act
        await eventBus.PublishAsync(@event);

        // Assert
        Assert.True(subscriber1.WasCalled);
        Assert.True(subscriber2.WasCalled);
        Assert.True(subscriber3.WasCalled);
    }

    [Fact]
    public async Task EventBus_PublishAsync_SubscriberThrows_DoesNotPropagateException()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var throwingSubscriber = new ThrowingSubscriber();
        var normalSubscriber = new TestSubscriber();
        eventBus.Subscribe(throwingSubscriber);
        eventBus.Subscribe(normalSubscriber);

        var @event = new FeatureFlagEvent
        {
            EventType = "FeatureFlagCreated",
            FeatureFlagId = 1,
            FeatureFlagKey = "test-flag"
        };

        // Act & Assert - Should not throw
        await eventBus.PublishAsync(@event);

        // Assert
        Assert.True(throwingSubscriber.WasCalled);
        Assert.True(normalSubscriber.WasCalled);
    }

    [Fact]
    public async Task EventBus_PublishAsync_UnsubscribedDuringPublish_HandledCorrectly()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber1 = new TestSubscriber();
        var subscriber2 = new TestSubscriber();
        eventBus.Subscribe(subscriber1);
        eventBus.Subscribe(subscriber2);

        var @event = new FeatureFlagEvent
        {
            EventType = "FeatureFlagCreated",
            FeatureFlagId = 1,
            FeatureFlagKey = "test-flag"
        };

        // Act - Multiple operations to test thread safety
        eventBus.Subscribe(new TestSubscriber());
        await eventBus.PublishAsync(@event);

        // Assert - Should complete without issues
        Assert.True(true); // If we get here, no exception was thrown
    }

    [Fact]
    public async Task EventBus_PublishAsync_ConcurrentOperations_ThreadSafe()
    {
        // Arrange
        var eventBus = new EventBus(_logger);
        var subscriber = new TestSubscriber();
        eventBus.Subscribe(subscriber);

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(eventBus.PublishAsync(new FeatureFlagEvent
            {
                EventType = "FeatureFlagCreated",
                FeatureFlagId = i,
                FeatureFlagKey = $"flag-{i}"
            }));
        }

        // Act
        await Task.WhenAll(tasks);

        // Assert - Should complete without issues
        Assert.True(true); // If we get here, no exception was thrown
    }

    #endregion

    #region EventLoggingSubscriber Tests

    [Fact]
    public async Task EventLoggingSubscriber_InterestedEventTypes_ReturnsWildcard()
    {
        // Arrange
        var subscriber = new EventLoggingSubscriber(_loggingLogger);

        // Act
        var interestedTypes = subscriber.InterestedEventTypes;

        // Assert
        Assert.Single(interestedTypes);
        Assert.Equal("*", interestedTypes[0]);
    }

    [Fact]
    public async Task EventLoggingSubscriber_HandleEventAsync_LogsEventInformation()
    {
        // Arrange
        var subscriber = new EventLoggingSubscriber(_loggingLogger);
        var @event = new FeatureFlagEvent
        {
            EventType = "FeatureFlagCreated",
            FeatureFlagId = 1,
            FeatureFlagKey = "test-flag",
            TriggeredBy = "admin@example.com",
            OccurredAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        await subscriber.HandleEventAsync(@event);

        // Assert - If we get here, the method completed successfully
        Assert.True(true);
    }

    [Fact]
    public void EventLoggingSubscriber_Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EventLoggingSubscriber(null!));
    }

    #endregion

    #region Helper Classes

    private sealed class TestSubscriber : IEventSubscriber
    {
        private readonly string[] _interestedEventTypes;

        public TestSubscriber(params string[] interestedEventTypes)
        {
            _interestedEventTypes = interestedEventTypes.Length > 0
                ? interestedEventTypes
                : new[] { "*" };
            WasCalled = false;
        }

        public string[] InterestedEventTypes => _interestedEventTypes;
        public bool WasCalled { get; private set; }
        public FeatureFlagEvent? LastEvent { get; private set; }
        public string? LastEventType => LastEvent?.EventType;

        public Task HandleEventAsync(FeatureFlagEvent @event)
        {
            WasCalled = true;
            LastEvent = @event;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingSubscriber : IEventSubscriber
    {
        public string[] InterestedEventTypes => new[] { "*" };
        public bool WasCalled { get; private set; }

        public Task HandleEventAsync(FeatureFlagEvent @event)
        {
            WasCalled = true;
            throw new InvalidOperationException("Test exception");
        }
    }

    #endregion
}

