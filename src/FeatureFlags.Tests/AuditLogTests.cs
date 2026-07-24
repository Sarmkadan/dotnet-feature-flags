using System;
using FeatureFlags.Enums;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests
{
    public class AuditLogTests
    {
        [Fact]
        public void Constructor_DefaultValues_InitializesPropertiesCorrectly()
        {
            // Act
            var auditLog = new AuditLog();

            // Assert
            Assert.Equal(0, auditLog.Id);
            Assert.Equal(0, auditLog.FeatureFlagId);
            Assert.Equal(AuditAction.Created, auditLog.Action);
            Assert.Equal(string.Empty, auditLog.ChangedBy);
            Assert.Equal(DateTime.UtcNow, auditLog.ChangedAt, TimeSpan.FromSeconds(1));
            Assert.Equal(string.Empty, auditLog.OldValue);
            Assert.Equal(string.Empty, auditLog.NewValue);
            Assert.Equal(string.Empty, auditLog.Description);
            Assert.Null(auditLog.IpAddress);
            Assert.Null(auditLog.UserAgent);
            Assert.Null(auditLog.FeatureFlag);
        }

        [Fact]
        public void GetSummary_WithValidAuditLog_ReturnsFormattedString()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                Action = AuditAction.Updated,
                ChangedBy = "testuser",
                ChangedAt = new DateTime(2024, 1, 1, 12, 0, 0),
                Description = "Updated feature flag configuration"
            };

            // Act
            var summary = auditLog.GetSummary();

            // Assert
            Assert.Equal("Updated by testuser at 2024-01-01 12:00:00: Updated feature flag configuration", summary);
        }

        [Fact]
        public void GetSummary_WithNullDescription_HandlesGracefully()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                Action = AuditAction.Created,
                ChangedBy = "admin",
                Description = null
            };

            // Act
            var summary = auditLog.GetSummary();

            // Assert
            Assert.StartsWith("Created by admin at ", summary);
        }

        [Fact]
        public void IsRollbackOf_WithNullPreviousLog_ReturnsFalse()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                OldValue = "old",
                NewValue = "new"
            };

            // Act
            var result = auditLog.IsRollbackOf(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsRollbackOf_WithRollbackValues_ReturnsTrue()
        {
            // Arrange
            var currentLog = new AuditLog
            {
                OldValue = "newValue",
                NewValue = "oldValue"
            };

            var previousLog = new AuditLog
            {
                OldValue = "oldValue",
                NewValue = "newValue"
            };

            // Act
            var result = currentLog.IsRollbackOf(previousLog);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_WithValidAuditLog_ReturnsTrue()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                FeatureFlagId = 1,
                ChangedBy = "admin",
                Action = AuditAction.Updated
            };

            // Act
            var result = auditLog.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_WithInvalidFeatureFlagId_ReturnsFalse()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                FeatureFlagId = 0,
                ChangedBy = "admin",
                Action = AuditAction.Updated
            };

            // Act
            var result = auditLog.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_WithEmptyChangedBy_ReturnsFalse()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                FeatureFlagId = 1,
                ChangedBy = "",
                Action = AuditAction.Updated
            };

            // Act
            var result = auditLog.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetChangeDetails_WithValues_ReturnsTupleWithOldAndNew()
        {
            // Arrange
            var oldValue = "{\"enabled\": false}";
            var newValue = "{\"enabled\": true}";
            var auditLog = new AuditLog
            {
                OldValue = oldValue,
                NewValue = newValue
            };

            // Act
            var (oldState, newState) = auditLog.GetChangeDetails();

            // Assert
            Assert.Equal(oldValue, oldState);
            Assert.Equal(newValue, newState);
        }

        [Fact]
        public void ChangedAt_DefaultsToUtcNow()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
            var auditLog = new AuditLog();
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Act & Assert
            Assert.InRange(auditLog.ChangedAt, beforeCreation, afterCreation);
        }

        [Fact]
        public void Properties_CanBeSetAndRead()
        {
            // Arrange
            var auditLog = new AuditLog();
            var expectedId = 42;
            var expectedFeatureFlagId = 10;
            var expectedAction = AuditAction.Deleted;
            var expectedChangedBy = "testuser@example.com";
            var expectedChangedAt = DateTime.UtcNow.AddHours(-1);
            var expectedOldValue = "old config";
            var expectedNewValue = "new config";
            var expectedDescription = "Test description";
            var expectedIpAddress = "192.168.1.1";
            var expectedUserAgent = "Mozilla/5.0";

            // Act
            auditLog.Id = expectedId;
            auditLog.FeatureFlagId = expectedFeatureFlagId;
            auditLog.Action = expectedAction;
            auditLog.ChangedBy = expectedChangedBy;
            auditLog.ChangedAt = expectedChangedAt;
            auditLog.OldValue = expectedOldValue;
            auditLog.NewValue = expectedNewValue;
            auditLog.Description = expectedDescription;
            auditLog.IpAddress = expectedIpAddress;
            auditLog.UserAgent = expectedUserAgent;

            // Assert
            Assert.Equal(expectedId, auditLog.Id);
            Assert.Equal(expectedFeatureFlagId, auditLog.FeatureFlagId);
            Assert.Equal(expectedAction, auditLog.Action);
            Assert.Equal(expectedChangedBy, auditLog.ChangedBy);
            Assert.Equal(expectedChangedAt, auditLog.ChangedAt);
            Assert.Equal(expectedOldValue, auditLog.OldValue);
            Assert.Equal(expectedNewValue, auditLog.NewValue);
            Assert.Equal(expectedDescription, auditLog.Description);
            Assert.Equal(expectedIpAddress, auditLog.IpAddress);
            Assert.Equal(expectedUserAgent, auditLog.UserAgent);
        }
    }
}