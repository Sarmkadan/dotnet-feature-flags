using System;
using FeatureFlags.Models;
using FeatureFlags.Enums;
using Xunit;

namespace FeatureFlags.Tests;

public class AuditLogExtensionsTests
{
    private readonly AuditLog _testLog = new()
    {
        Id = 1,
        FeatureFlagId = 100,
        Action = AuditAction.Updated,
        ChangedBy = "test-user",
        ChangedAt = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc),
        OldValue = "{\"enabled\": false}",
        NewValue = "{\"enabled\": true}",
        Description = "Updated feature flag configuration",
        IpAddress = "192.168.1.1",
        UserAgent = "Test Agent"
    };

    [Fact]
    public void IsStateChange_WithUpdatedAction_ReturnsTrue()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Updated };

        // Act
        var result = log.IsStateChange();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsStateChange_WithEnabledAction_ReturnsTrue()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Enabled };

        // Act
        var result = log.IsStateChange();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsStateChange_WithDisabledAction_ReturnsTrue()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Disabled };

        // Act
        var result = log.IsStateChange();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsStateChange_WithCreatedAction_ReturnsFalse()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Created };

        // Act
        var result = log.IsStateChange();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsStateChange_WithDeletedAction_ReturnsFalse()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Deleted };

        // Act
        var result = log.IsStateChange();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsStateChange_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.IsStateChange());
    }

    [Fact]
    public void IsCreation_WithCreatedAction_ReturnsTrue()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Created };

        // Act
        var result = log.IsCreation();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCreation_WithUpdatedAction_ReturnsFalse()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Updated };

        // Act
        var result = log.IsCreation();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCreation_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.IsCreation());
    }

    [Fact]
    public void IsDeletion_WithDeletedAction_ReturnsTrue()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Deleted };

        // Act
        var result = log.IsDeletion();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDeletion_WithCreatedAction_ReturnsFalse()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Created };

        // Act
        var result = log.IsDeletion();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDeletion_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.IsDeletion());
    }

    [Fact]
    public void GetTimeSinceChange_WithRecentChange_ReturnsJustNow()
    {
        // Arrange
        var recentLog = new AuditLog
        {
            ChangedAt = DateTime.UtcNow.AddSeconds(-30)
        };

        // Act
        var result = recentLog.GetTimeSinceChange();

        // Assert
        Assert.Equal("just now", result);
    }

    [Fact]
    public void GetTimeSinceChange_WithMinutesAgo_ReturnsMinutesFormat()
    {
        // Arrange
        var minutesAgo = new AuditLog
        {
            ChangedAt = DateTime.UtcNow.AddMinutes(-45)
        };

        // Act
        var result = minutesAgo.GetTimeSinceChange();

        // Assert
        Assert.Equal("45 minutes ago", result);
    }

    [Fact]
    public void GetTimeSinceChange_WithHoursAgo_ReturnsHoursFormat()
    {
        // Arrange
        var hoursAgo = new AuditLog
        {
            ChangedAt = DateTime.UtcNow.AddHours(-5)
        };

        // Act
        var result = hoursAgo.GetTimeSinceChange();

        // Assert
        Assert.Equal("5 hours ago", result);
    }

    [Fact]
    public void GetTimeSinceChange_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.GetTimeSinceChange());
    }

    [Fact]
    public void GetTimeSinceChange_WithEmptyFormat_ThrowsArgumentException()
    {
        // Arrange
        var log = _testLog;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => log.GetTimeSinceChange(string.Empty));
    }

    [Fact]
    public void GetTimeSinceChange_WithNullFormat_ThrowsArgumentException()
    {
        // Arrange
        var log = _testLog;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => log.GetTimeSinceChange(null!));
    }

    [Fact]
    public void GetDetailedChangeDescription_WithValidLog_ReturnsFormattedDescription()
    {
        // Arrange
        var log = new AuditLog
        {
            Action = AuditAction.Updated,
            ChangedBy = "admin",
            ChangedAt = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc),
            Description = "Updated enabled status",
            OldValue = "false",
            NewValue = "true"
        };

        // Act
        var result = log.GetDetailedChangeDescription();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Updated enabled status", result);
        Assert.Contains("Old: false", result);
        Assert.Contains("New: true", result);
    }

    [Fact]
    public void GetDetailedChangeDescription_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.GetDetailedChangeDescription());
    }

    [Fact]
    public void IsRecent_WithRecentChange_ReturnsTrue()
    {
        // Arrange
        var recentLog = new AuditLog
        {
            ChangedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        // Act
        var result = recentLog.IsRecent(30);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRecent_WithOldChange_ReturnsFalse()
    {
        // Arrange
        var oldLog = new AuditLog
        {
            ChangedAt = DateTime.UtcNow.AddHours(-2)
        };

        // Act
        var result = oldLog.IsRecent(30);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRecent_WithDefaultThreshold_ReturnsTrue()
    {
        // Arrange
        var recentLog = new AuditLog
        {
            ChangedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        // Act
        var result = recentLog.IsRecent();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRecent_WithNegativeThreshold_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var log = _testLog;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => log.IsRecent(-5));
    }

    [Fact]
    public void IsRecent_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.IsRecent());
    }

    [Fact]
    public void GetActionDisplayName_WithCreatedAction_ReturnsCreated()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Created };

        // Act
        var result = log.GetActionDisplayName();

        // Assert
        Assert.Equal("Created", result);
    }

    [Fact]
    public void GetActionDisplayName_WithUpdatedAction_ReturnsUpdated()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Updated };

        // Act
        var result = log.GetActionDisplayName();

        // Assert
        Assert.Equal("Updated", result);
    }

    [Fact]
    public void GetActionDisplayName_WithEnabledAction_ReturnsEnabled()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Enabled };

        // Act
        var result = log.GetActionDisplayName();

        // Assert
        Assert.Equal("Enabled", result);
    }

    [Fact]
    public void GetActionDisplayName_WithDisabledAction_ReturnsDisabled()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Disabled };

        // Act
        var result = log.GetActionDisplayName();

        // Assert
        Assert.Equal("Disabled", result);
    }

    [Fact]
    public void GetActionDisplayName_WithDeletedAction_ReturnsDeleted()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.Deleted };

        // Act
        var result = log.GetActionDisplayName();

        // Assert
        Assert.Equal("Deleted", result);
    }

    [Fact]
    public void GetActionDisplayName_WithRolloutChangedAction_ReturnsActionName()
    {
        // Arrange
        var log = new AuditLog { Action = AuditAction.RolloutChanged };

        // Act
        var result = log.GetActionDisplayName();

        // Assert
        Assert.Equal("RolloutChanged", result);
    }

    [Fact]
    public void GetActionDisplayName_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.GetActionDisplayName());
    }
}