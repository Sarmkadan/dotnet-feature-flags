#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Integration;
using FeatureFlags.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Integration;

/// <summary>
/// Unit tests for WebhookService covering registration, validation, and webhook operations.
/// </summary>
public sealed class WebhookServiceTests
{
    private readonly Mock<IWebhookRepository> _webhookRepositoryMock;
    private readonly Mock<IWebhookDeliveryRepository> _deliveryRepositoryMock;
    private readonly Mock<FeatureFlags.Integration.IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<WebhookService>> _loggerMock;
    private readonly WebhookService _service;

    public WebhookServiceTests()
    {
        _webhookRepositoryMock = new Mock<IWebhookRepository>();
        _deliveryRepositoryMock = new Mock<IWebhookDeliveryRepository>();
        _httpClientFactoryMock = new Mock<FeatureFlags.Integration.IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateWebhookClient()).Returns(() => new HttpClient());
        _loggerMock = new Mock<ILogger<WebhookService>>();

        _service = new WebhookService(
            _webhookRepositoryMock.Object,
            _deliveryRepositoryMock.Object,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithValidUrl_CreatesWebhook()
    {
        // Arrange
        var url = "https://example.com/webhook";
        var description = "Test Webhook";
        var eventTypes = WebhookEventType.FeatureFlagUpdated | WebhookEventType.FeatureFlagCreated;
        var createdBy = "admin";

        var webhook = new Webhook
        {
            Id = 1,
            Url = url,
            Description = description,
            EventTypes = eventTypes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _webhookRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Webhook>()))
            .ReturnsAsync(webhook);

        // Act
        var result = await _service.RegisterWebhookAsync(url, description, eventTypes, null, null, createdBy);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Be(url);
        result.Description.Should().Be(description);
        result.EventTypes.Should().Be(eventTypes);
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithInvalidUrl_ThrowsArgumentException()
    {
        // Arrange
        var invalidUrl = "not-a-valid-url";
        var description = "Test Webhook";
        var eventTypes = WebhookEventType.FeatureFlagUpdated;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterWebhookAsync(invalidUrl, description, eventTypes, null, null, "admin"));
    }

    [Fact]
    public async Task GetWebhookAsync_WithValidId_ReturnsWebhook()
    {
        // Arrange
        var webhookId = 1;
        var webhook = new Webhook
        {
            Id = webhookId,
            Url = "https://example.com/webhook",
            Description = "Test Webhook",
            IsActive = true
        };

        _webhookRepositoryMock
            .Setup(r => r.GetByIdAsync(webhookId))
            .ReturnsAsync(webhook);

        // Act
        var result = await _service.GetWebhookAsync(webhookId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(webhookId);
        result.Url.Should().Be("https://example.com/webhook");
    }

    [Fact]
    public async Task GetWebhookAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _webhookRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Webhook?)null);

        // Act
        var result = await _service.GetWebhookAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveWebhooksAsync_WithMatchingEventType_ReturnsWebhooks()
    {
        // Arrange
        var eventType = WebhookEventType.FeatureFlagUpdated;
        var webhooks = new List<Webhook>
        {
            new Webhook
            {
                Id = 1,
                Url = "https://example.com/webhook",
                EventTypes = WebhookEventType.FeatureFlagUpdated,
                IsActive = true
            },
            new Webhook
            {
                Id = 2,
                Url = "https://example.com/webhook2",
                EventTypes = WebhookEventType.FeatureFlagCreated,
                IsActive = true
            }
        };

        _webhookRepositoryMock
            .Setup(r => r.GetActiveAsync())
            .ReturnsAsync(webhooks);

        // Act
        var result = await _service.GetActiveWebhooksAsync(eventType);

        // Assert
        result.Should().HaveCount(1);
        result[0].EventTypes.Should().HaveFlag(WebhookEventType.FeatureFlagUpdated);
    }

    [Fact]
    public async Task GetActiveWebhooksAsync_WithNoneMatching_ReturnsEmpty()
    {
        // Arrange
        var eventType = WebhookEventType.FeatureFlagDeleted;
        _webhookRepositoryMock
            .Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Webhook>());

        // Act
        var result = await _service.GetActiveWebhooksAsync(eventType);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateWebhookAsync_WithValidData_UpdatesWebhook()
    {
        // Arrange
        var webhookId = 1;
        var newUrl = "https://example.com/webhook-updated";
        var newDescription = "Updated Webhook";
        var newEventTypes = WebhookEventType.FeatureFlagUpdated | WebhookEventType.FeatureFlagDeleted;

        var existingWebhook = new Webhook
        {
            Id = webhookId,
            Url = "https://example.com/webhook",
            Description = "Original",
            EventTypes = WebhookEventType.FeatureFlagUpdated,
            IsActive = true
        };

        _webhookRepositoryMock
            .Setup(r => r.GetByIdAsync(webhookId))
            .ReturnsAsync(existingWebhook);

        _webhookRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Webhook>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateWebhookAsync(webhookId, newUrl, newDescription, newEventTypes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateWebhookAsync_WithNonexistentId_ReturnsFalse()
    {
        // Arrange
        _webhookRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Webhook?)null);

        // Act
        var result = await _service.UpdateWebhookAsync(999, "https://example.com/webhook", "New Description", WebhookEventType.FeatureFlagUpdated);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteWebhookAsync_WithValidId_DeletesWebhook()
    {
        // Arrange
        var webhookId = 1;
        var webhook = new Webhook { Id = webhookId, Url = "https://example.com/webhook", IsActive = true };

        _webhookRepositoryMock
            .Setup(r => r.GetByIdAsync(webhookId))
            .ReturnsAsync(webhook);

        _webhookRepositoryMock
            .Setup(r => r.DeleteAsync(webhookId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteWebhookAsync(webhookId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWebhookAsync_WithNonexistentId_ReturnsFalse()
    {
        // Arrange
        _webhookRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Webhook?)null);

        // Act
        var result = await _service.DeleteWebhookAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TriggerWebhooksAsync_WithMatchingEventType_CallsActiveWebhooks()
    {
        // Arrange
        var eventType = WebhookEventType.FeatureFlagUpdated;
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var webhooks = new List<Webhook>
        {
            new Webhook
            {
                Id = 1,
                Url = "https://example.com/webhook",
                EventTypes = WebhookEventType.FeatureFlagUpdated,
                IsActive = true
            }
        };

        _webhookRepositoryMock
            .Setup(r => r.GetActiveAsync())
            .ReturnsAsync(webhooks);

        // Act
        await _service.TriggerWebhooksAsync(eventType, flag, "admin");

        // Assert
        _webhookRepositoryMock.Verify(r => r.GetActiveAsync(), Times.Once);
    }

    [Fact]
    public async Task RetryFailedDeliveriesAsync_ProcessesRetries()
    {
        // Arrange
        var deliveries = new List<WebhookDelivery>
        {
            new WebhookDelivery
            {
                Id = 1,
                WebhookId = 1,
                Payload = "{}"
            }
        };

        _deliveryRepositoryMock
            .Setup(r => r.GetPendingRetriesAsync())
            .ReturnsAsync(deliveries);

        _webhookRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Webhook { Id = 1, Url = "https://example.com/webhook", IsActive = true });

        // Act
        await _service.RetryFailedDeliveriesAsync();

        // Assert - Should attempt to retry
        _deliveryRepositoryMock.Verify(r => r.GetPendingRetriesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetActiveWebhooksAsync_FiltersInactiveWebhooks()
    {
        // Arrange
        var webhooks = new List<Webhook>
        {
            new Webhook
            {
                Id = 1,
                Url = "https://example.com/webhook1",
                EventTypes = WebhookEventType.FeatureFlagUpdated,
                IsActive = true
            },
            new Webhook
            {
                Id = 2,
                Url = "https://example.com/webhook2",
                EventTypes = WebhookEventType.FeatureFlagUpdated,
                IsActive = false
            }
        };

        _webhookRepositoryMock
            .Setup(r => r.GetActiveAsync())
            .ReturnsAsync(webhooks.Where(w => w.IsActive).ToList());

        // Act
        var result = await _service.GetActiveWebhooksAsync(WebhookEventType.FeatureFlagUpdated);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterWebhookAsync_WithFeatureFlagKey_CreatesWebhookWithFilter()
    {
        // Arrange
        var url = "https://example.com/webhook";
        var description = "Test Webhook";
        var eventTypes = WebhookEventType.FeatureFlagUpdated;
        var featureFlagKey = "specific-flag";
        var secret = "webhook-secret";
        var createdBy = "admin";

        var webhook = new Webhook
        {
            Id = 1,
            Url = url,
            Description = description,
            EventTypes = eventTypes,
            FeatureFlagKey = featureFlagKey,
            Secret = secret,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _webhookRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Webhook>()))
            .ReturnsAsync(webhook);

        // Act
        var result = await _service.RegisterWebhookAsync(url, description, eventTypes, featureFlagKey, secret, createdBy);

        // Assert
        result.Should().NotBeNull();
        result.FeatureFlagKey.Should().Be(featureFlagKey);
        result.Secret.Should().Be(secret);
    }
}
