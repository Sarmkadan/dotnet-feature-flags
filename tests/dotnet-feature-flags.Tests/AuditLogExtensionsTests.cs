using System;
using System.Collections.Generic;
using System.Linq;
using FeatureFlags.Models;
using FeatureFlags.Enums;
using Xunit;

namespace FeatureFlags.Tests
{
    public class AuditLogExtensionsTests
    {
        [Fact]
        public void IsMutation_EvaluatedAction_ReturnsFalse()
        {
            var log = new AuditLog { Action = AuditAction.Evaluated };
            Assert.False(log.IsMutation());
        }

        [Theory]
        [InlineData(AuditAction.Created)]
        [InlineData(AuditAction.Updated)]
        [InlineData(AuditAction.Enabled)]
        [InlineData(AuditAction.Disabled)]
        [InlineData(AuditAction.Deleted)]
        public void IsMutation_OtherActions_ReturnsTrue(AuditAction action)
        {
            var log = new AuditLog { Action = action };
            Assert.True(log.IsMutation());
        }

        [Fact]
        public void ToDisplayString_ReturnsFormattedString()
        {
            var time = new DateTime(2026, 8, 17, 10, 0, 0, DateTimeKind.Utc);
            var log = new AuditLog 
            { 
                ChangedAt = time, 
                Action = AuditAction.Created, 
                ChangedBy = "admin", 
                Description = "Created flag" 
            };
            var expected = "[2026-08-17 10:00:00] Created by admin: Created flag";
            Assert.Equal(expected, log.ToDisplayString());
        }

        [Fact]
        public void Since_FiltersLogsCorrectly()
        {
            var now = DateTimeOffset.UtcNow;
            var logs = new List<AuditLog>
            {
                new AuditLog { ChangedAt = now.AddHours(-2).UtcDateTime },
                new AuditLog { ChangedAt = now.AddHours(-1).UtcDateTime },
                new AuditLog { ChangedAt = now.AddHours(1).UtcDateTime }
            };

            var result = logs.AsEnumerable().Since(now.AddHours(-1.5));

            Assert.Equal(2, result.Count());
            Assert.Contains(logs[1], result);
            Assert.Contains(logs[2], result);
        }
    }
}
