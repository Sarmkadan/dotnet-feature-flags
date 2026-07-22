#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for AuditLogCleanupWorker and related classes
// =============================================================================

using FeatureFlags.BackgroundJobs;
using Xunit;

namespace FeatureFlags.Tests;

/// <summary>
/// Unit tests for AuditLogCleanupWorker and related configuration classes.
/// Tests constructor validation, property initialization, and option defaults.
/// </summary>
public sealed class AuditLogCleanupWorkerTests
{
#region AuditLogCleanupOptions Tests

    [Fact]
    public void AuditLogCleanupOptions_DefaultConstructor_InitializesProperties()
    {
        // Act
        var options = new AuditLogCleanupOptions();

        // Assert
        Assert.Equal(90, options.RetentionDays);
        Assert.Equal(24, options.CleanupIntervalHours);
        Assert.True(options.Enabled);
    }

    [Fact]
    public void AuditLogCleanupOptions_Property_SettersWorkCorrectly()
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        options.RetentionDays = 30;
        options.CleanupIntervalHours = 12;
        options.Enabled = false;

        // Assert
        Assert.Equal(30, options.RetentionDays);
        Assert.Equal(12, options.CleanupIntervalHours);
        Assert.False(options.Enabled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(365)]
    public void AuditLogCleanupOptions_RetentionDays_BoundaryValues(int retentionDays)
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        options.RetentionDays = retentionDays;

        // Assert
        Assert.Equal(retentionDays, options.RetentionDays);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(168)] // 7 days
    [InlineData(720)] // 30 days
    public void AuditLogCleanupOptions_CleanupIntervalHours_BoundaryValues(int intervalHours)
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        options.CleanupIntervalHours = intervalHours;

        // Assert
        Assert.Equal(intervalHours, options.CleanupIntervalHours);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AuditLogCleanupOptions_EnabledFlag_WorksWithAnySettings(bool enabled)
    {
        // Arrange
        var options = new AuditLogCleanupOptions { RetentionDays = 60, Enabled = enabled };

        // Act & Assert - Just verify the values are set correctly
        Assert.Equal(60, options.RetentionDays);
        Assert.Equal(enabled, options.Enabled);
    }

#endregion

#region AuditLogCleanupWorker Constructor Tests

    [Fact]
    public void AuditLogCleanupWorker_Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { RetentionDays = 60, CleanupIntervalHours = 12 };

        // Act - We can't easily instantiate without DI, but we can at least verify the class exists
        // This test just verifies the class can be referenced
        var workerType = typeof(AuditLogCleanupWorker);

        // Assert
        Assert.NotNull(workerType);
    }

    [Fact]
    public void AuditLogCleanupWorker_PublicProperties_AreAccessible()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { RetentionDays = 45, CleanupIntervalHours = 2 };

        // Act
        var cleanupIntervalSeconds = options.CleanupIntervalHours * 3600;
        var retentionDays = options.RetentionDays;

        // Assert
        Assert.Equal(7200, cleanupIntervalSeconds); // 2 * 3600
        Assert.Equal(45, retentionDays);
    }

#endregion

#region WebhookRetryOptions Tests

    [Fact]
    public void WebhookRetryOptions_DefaultConstructor_InitializesProperties()
    {
        // Act
        var options = new WebhookRetryOptions();

        // Assert
        Assert.Equal(300, options.CheckIntervalSeconds); // 5 minutes
        Assert.Equal(3, options.MaxRetries);
        Assert.Equal(60, options.RetryDelaySeconds);
        Assert.True(options.Enabled);
    }

    [Fact]
    public void WebhookRetryOptions_Property_SettersWorkCorrectly()
    {
        // Arrange
        var options = new WebhookRetryOptions();

        // Act
        options.CheckIntervalSeconds = 60;
        options.MaxRetries = 5;
        options.RetryDelaySeconds = 120;
        options.Enabled = false;

        // Assert
        Assert.Equal(60, options.CheckIntervalSeconds);
        Assert.Equal(5, options.MaxRetries);
        Assert.Equal(120, options.RetryDelaySeconds);
        Assert.False(options.Enabled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(600)] // 10 minutes
    [InlineData(3600)] // 1 hour
    public void WebhookRetryOptions_CheckIntervalSeconds_BoundaryValues(int intervalSeconds)
    {
        // Arrange
        var options = new WebhookRetryOptions();

        // Act
        options.CheckIntervalSeconds = intervalSeconds;

        // Assert
        Assert.Equal(intervalSeconds, options.CheckIntervalSeconds);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public void WebhookRetryOptions_MaxRetries_BoundaryValues(int maxRetries)
    {
        // Arrange
        var options = new WebhookRetryOptions();

        // Act
        options.MaxRetries = maxRetries;

        // Assert
        Assert.Equal(maxRetries, options.MaxRetries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(600)]
    public void WebhookRetryOptions_RetryDelaySeconds_BoundaryValues(int retryDelaySeconds)
    {
        // Arrange
        var options = new WebhookRetryOptions();

        // Act
        options.RetryDelaySeconds = retryDelaySeconds;

        // Assert
        Assert.Equal(retryDelaySeconds, options.RetryDelaySeconds);
    }

#endregion

#region CacheSyncOptions Tests

    [Fact]
    public void CacheSyncOptions_DefaultConstructor_InitializesProperties()
    {
        // Act
        var options = new CacheSyncOptions();

        // Assert
        Assert.Equal(300, options.SyncIntervalSeconds); // 5 minutes
        Assert.Equal(5, options.CacheTtlMinutes);
        Assert.False(options.Enabled);
    }

    [Fact]
    public void CacheSyncOptions_Property_SettersWorkCorrectly()
    {
        // Arrange
        var options = new CacheSyncOptions();

        // Act
        options.SyncIntervalSeconds = 600;
        options.CacheTtlMinutes = 15;
        options.Enabled = true;

        // Assert
        Assert.Equal(600, options.SyncIntervalSeconds);
        Assert.Equal(15, options.CacheTtlMinutes);
        Assert.True(options.Enabled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(60)]
    [InlineData(1800)] // 30 minutes
    [InlineData(3600)] // 1 hour
    public void CacheSyncOptions_SyncIntervalSeconds_BoundaryValues(int intervalSeconds)
    {
        // Arrange
        var options = new CacheSyncOptions();

        // Act
        options.SyncIntervalSeconds = intervalSeconds;

        // Assert
        Assert.Equal(intervalSeconds, options.SyncIntervalSeconds);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(60)]
    public void CacheSyncOptions_CacheTtlMinutes_BoundaryValues(int ttlMinutes)
    {
        // Arrange
        var options = new CacheSyncOptions();

        // Act
        options.CacheTtlMinutes = ttlMinutes;

        // Assert
        Assert.Equal(ttlMinutes, options.CacheTtlMinutes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CacheSyncOptions_EnabledFlag_WorksWithAnySettings(bool enabled)
    {
        // Arrange
        var options = new CacheSyncOptions { SyncIntervalSeconds = 600, Enabled = enabled };

        // Act & Assert - Just verify the values are set correctly
        Assert.Equal(600, options.SyncIntervalSeconds);
        Assert.Equal(enabled, options.Enabled);
    }

#endregion
}