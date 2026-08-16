using System;
using System.Collections.Generic;
using FeatureFlags.Formatters;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests
{
    public class CsvExporterTests
    {
        [Fact]
        public void ExportFeatureFlags_WithEmptyCollection_ReturnsHeaderOnly()
        {
            // Act
            var csv = CsvExporter.ExportFeatureFlags(new List<FeatureFlag>());

            // Assert
            Assert.Equal("Id,Key,DisplayName,Description,IsEnabled,RolloutType,PercentageRollout,CreatedAt,UpdatedAt,CreatedBy" + Environment.NewLine, csv);
        }

        [Fact]
        public void ExportFeatureFlags_WithSpecialCharacters_EscapesCorrectly()
        {
            // Arrange
            var flags = new List<FeatureFlag>
            {
                new FeatureFlag
                {
                    Id = 1,
                    Key = "test,flag",
                    DisplayName = "Display\"Name",
                    Description = "Line\nBreak",
                    IsEnabled = true,
                    CreatedBy = "user"
                }
            };

            // Act
            var csv = CsvExporter.ExportFeatureFlags(flags);

            // Assert
            Assert.Contains("\"test,flag\"", csv);
            Assert.Contains("\"Display\"\"Name\"", csv);
            Assert.Contains("\"Line\nBreak\"", csv);
        }

        [Fact]
        public void ExportAuditLogs_WithEmptyCollection_ReturnsHeaderOnly()
        {
            // Act
            var csv = CsvExporter.ExportAuditLogs(new List<AuditLog>());

            // Assert
            Assert.Equal("Id,FeatureFlagId,Action,ChangedBy,Timestamp,OldValue,NewValue" + Environment.NewLine, csv);
        }

        [Fact]
        public void ExportAuditLogs_WithSpecialCharacters_EscapesCorrectly()
        {
            // Arrange
            var logs = new List<AuditLog>
            {
                new AuditLog
                {
                    Id = 1,
                    FeatureFlagId = 1,
                    Action = FeatureFlags.Enums.AuditAction.Created,
                    ChangedBy = "user,name",
                    OldValue = "old\"value",
                    NewValue = "new\nvalue"
                }
            };

            // Act
            var csv = CsvExporter.ExportAuditLogs(logs);

            // Assert
            Assert.Contains("\"user,name\"", csv);
            Assert.Contains("\"old\"\"value\"", csv);
            Assert.Contains("\"new\nvalue\"", csv);
        }

        [Fact]
        public void ParseFeatureFlags_WithInvalidCsv_ReturnsEmptyList()
        {
            // Act
            var flags = CsvParser.ParseFeatureFlags("invalid-csv");

            // Assert
            Assert.Empty(flags);
        }

        [Fact]
        public void ParseFeatureFlags_WithNullInput_ThrowsNullReferenceException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => CsvParser.ParseFeatureFlags(null!));
        }
    }
}
