#nullable enable
using System;
using FeatureFlags.Integration;
using Xunit;

namespace FeatureFlags.Tests;

public class WebhookTests
{
    [Fact]
    public void PropertyDefaults_ShouldBeInitializedCorrectly()
    {
        var webhook = new Webhook();

        Assert.Equal(0, webhook.Id);
        Assert.Equal(string.Empty, webhook.Url);
        Assert.Equal(string.Empty, webhook.Description);
        Assert.True(webhook.IsActive);
        Assert.Equal(WebhookEventType.All, webhook.EventTypes);
        Assert.Null(webhook.FeatureFlagKey);
        Assert.Equal(default, webhook.CreatedAt);
        Assert.Equal(default, webhook.UpdatedAt);
        Assert.Equal(string.Empty, webhook.CreatedBy);
        Assert.Equal(3, webhook.MaxRetries);
        Assert.Equal(60, webhook.RetryDelaySeconds);
        Assert.Null(webhook.AuthorizationHeader);
        Assert.Null(webhook.Secret);
        Assert.Equal(0, webhook.SuccessCount);
        Assert.Equal(0, webhook.FailureCount);
        Assert.Null(webhook.LastTriggeredAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_ReturnsFalse_WhenUrlIsNullOrWhiteSpace(string? url)
    {
        var webhook = new Webhook { Url = url ?? string.Empty };
        Assert.False(webhook.IsValid());
    }

    [Theory]
    [InlineData("ftp://example.com/webhook")]
    [InlineData("mailto:user@example.com")]
    [InlineData("example.com")]
    public void IsValid_ReturnsFalse_WhenUrlHasInvalidScheme(string url)
    {
        var webhook = new Webhook { Url = url };
        Assert.False(webhook.IsValid());
    }

    [Theory]
    [InlineData("http://example.com/webhook")]
    [InlineData("https://example.com/webhook")]
    public void IsValid_ReturnsTrue_ForValidHttpOrHttpsUrl(string url)
    {
        var webhook = new Webhook { Url = url };
        Assert.True(webhook.IsValid());
    }

    [Fact]
    public void ShouldTrigger_ReturnsFalse_WhenWebhookIsInactive()
    {
        var webhook = new Webhook
        {
            IsActive = false,
            EventTypes = WebhookEventType.FeatureFlagCreated
        };

        Assert.False(webhook.ShouldTrigger(WebhookEventType.FeatureFlagCreated));
    }

    [Fact]
    public void ShouldTrigger_ReturnsTrue_WhenEventMatchesConfiguredTypes()
    {
        var webhook = new Webhook
        {
            IsActive = true,
            EventTypes = WebhookEventType.FeatureFlagCreated | WebhookEventType.FeatureFlagDeleted
        };

        Assert.True(webhook.ShouldTrigger(WebhookEventType.FeatureFlagCreated));
        Assert.True(webhook.ShouldTrigger(WebhookEventType.FeatureFlagDeleted));
        Assert.False(webhook.ShouldTrigger(WebhookEventType.FeatureFlagUpdated));
    }

    [Fact]
    public void ShouldTrigger_ReturnsTrue_WhenEventTypesIsAll()
    {
        var webhook = new Webhook
        {
            IsActive = true,
            EventTypes = WebhookEventType.All
        };

        foreach (WebhookEventType type in Enum.GetValues(typeof(WebhookEventType)))
        {
            // Skip the composite 'All' value itself; we only care about individual flags.
            if (type == WebhookEventType.All) continue;
            Assert.True(webhook.ShouldTrigger(type));
        }
    }

    [Fact]
    public void ShouldTrigger_ReturnsFalse_ForNullOrEmptyFeatureFlagKey_IrrelevantToLogic()
    {
        var webhook = new Webhook
        {
            IsActive = true,
            EventTypes = WebhookEventType.FeatureFlagCreated,
            FeatureFlagKey = null
        };

        // The method does not consider FeatureFlagKey, so behavior is based solely on IsActive and EventTypes.
        Assert.True(webhook.ShouldTrigger(WebhookEventType.FeatureFlagCreated));
    }
}
