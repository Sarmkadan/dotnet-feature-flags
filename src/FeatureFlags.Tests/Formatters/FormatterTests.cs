#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Formatters;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests.Formatters;

/// <summary>
/// Unit tests for CSV export and import functionality.
/// Tests serialization and deserialization of feature flags.
/// </summary>
public sealed class CsvFormatterTests
{
    [Fact]
    public void ExportFeatureFlags_ProducesValidCsv()
    {
        // Arrange
        var flags = new List<FeatureFlag>
        {
            new FeatureFlag
            {
                Id = 1,
                Key = "test-flag",
                DisplayName = "Test Flag",
                Description = "A test flag",
                IsEnabled = true,
                RolloutType = RolloutType.Percentage,
                PercentageRollout = 50,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "test-user"
            }
        };

        // Act
        var csv = CsvExporter.ExportFeatureFlags(flags);

        // Assert
        Assert.NotEmpty(csv);
        Assert.Contains("Id", csv);
        Assert.Contains("test-flag", csv);
        Assert.Contains("50", csv);
    }

    [Fact]
    public void ExportAuditLogs_ProducesValidCsv()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = 1,
                FeatureFlagId = 1,
                Action = AuditAction.Created,
                ChangedBy = "admin",
                Timestamp = DateTime.UtcNow,
                OldValue = null,
                NewValue = "created"
            }
        };

        // Act
        var csv = CsvExporter.ExportAuditLogs(logs);

        // Assert
        Assert.NotEmpty(csv);
        Assert.Contains("Created", csv);
        Assert.Contains("admin", csv);
    }

    [Fact]
    public void ParseCsv_ReadsValidCsv()
    {
        // Arrange
        var csv = @"Id,Key,DisplayName,Description,IsEnabled,RolloutType,PercentageRollout,CreatedAt,UpdatedAt,CreatedBy
1,test-flag,Test Flag,A test flag,True,1,50,2024-01-01T00:00:00Z,2024-01-01T00:00:00Z,test-user";

        // Act
        var flags = CsvParser.ParseFeatureFlags(csv);

        // Assert
        Assert.Single(flags);
        Assert.Equal("test-flag", flags[0].Key);
        Assert.Equal("Test Flag", flags[0].DisplayName);
    }

    [Fact]
    public void ParseCsv_HandlesQuotedValues()
    {
        // Arrange
        var csv = @"Id,Key,DisplayName,Description,IsEnabled,RolloutType,PercentageRollout,CreatedAt,UpdatedAt,CreatedBy
1,""test-flag"",""Test, Flag"",""A, test, flag"",True,1,50,2024-01-01T00:00:00Z,2024-01-01T00:00:00Z,test-user";

        // Act
        var flags = CsvParser.ParseFeatureFlags(csv);

        // Assert
        Assert.Single(flags);
        Assert.Equal("test-flag", flags[0].Key);
    }
}

/// <summary>
/// Unit tests for XML export and import functionality.
/// </summary>
public sealed class XmlFormatterTests
{
    [Fact]
    public void ExportFeatureFlags_ProducesValidXml()
    {
        // Arrange
        var flags = new List<FeatureFlag>
        {
            new FeatureFlag
            {
                Id = 1,
                Key = "test-flag",
                DisplayName = "Test Flag",
                Description = "A test flag",
                IsEnabled = true,
                RolloutType = RolloutType.Percentage,
                PercentageRollout = 50,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "test-user"
            }
        };

        // Act
        var xml = XmlExporter.ExportFeatureFlags(flags);

        // Assert
        Assert.NotEmpty(xml);
        Assert.Contains("<?xml", xml);
        Assert.Contains("<FeatureFlags>", xml);
        Assert.Contains("test-flag", xml);
        Assert.Contains("<IsEnabled>True</IsEnabled>", xml);
    }

    [Fact]
    public void ExportAuditLogs_ProducesValidXml()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = 1,
                FeatureFlagId = 1,
                Action = AuditAction.Created,
                ChangedBy = "admin",
                Timestamp = DateTime.UtcNow,
                OldValue = null,
                NewValue = "created"
            }
        };

        // Act
        var xml = XmlExporter.ExportAuditLogs(logs);

        // Assert
        Assert.NotEmpty(xml);
        Assert.Contains("<AuditLogs>", xml);
        Assert.Contains("Created", xml);
    }

    [Fact]
    public void ParseXml_ReadsValidXml()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<FeatureFlags>
    <FeatureFlag id=""1"" key=""test-flag"">
        <DisplayName>Test Flag</DisplayName>
        <Description>A test flag</Description>
        <IsEnabled>true</IsEnabled>
        <RolloutType>Percentage</RolloutType>
        <PercentageRollout>50</PercentageRollout>
        <CreatedAt>2024-01-01T00:00:00Z</CreatedAt>
        <UpdatedAt>2024-01-01T00:00:00Z</UpdatedAt>
        <CreatedBy>test-user</CreatedBy>
    </FeatureFlag>
</FeatureFlags>";

        // Act
        var flags = XmlParser.ParseFeatureFlags(xml);

        // Assert
        Assert.Single(flags);
        Assert.Equal("test-flag", flags[0].Key);
        Assert.Equal("Test Flag", flags[0].DisplayName);
        Assert.True(flags[0].IsEnabled);
    }

    [Fact]
    public void ParseXml_HandlesInvalidXml()
    {
        // Arrange
        var xml = "not valid xml";

        // Act
        var flags = XmlParser.ParseFeatureFlags(xml);

        // Assert
        Assert.Empty(flags);
    }
}

/// <summary>
/// Unit tests for JSON serialization with custom converters.
/// </summary>
public sealed class JsonFormatterTests
{
    [Fact]
    public void CreateOptions_ReturnsValidOptions()
    {
        // Arrange & Act
        var options = JsonFormatterOptions.CreateOptions();

        // Assert
        Assert.NotNull(options);
        Assert.True(options.PropertyNameCaseInsensitive);
    }

    [Fact]
    public void CreateCompactOptions_ReturnsValidOptions()
    {
        // Arrange & Act
        var options = JsonFormatterOptions.CreateCompactOptions();

        // Assert
        Assert.NotNull(options);
        Assert.False(options.WriteIndented);
    }

    [Fact]
    public void FeatureFlagConverter_SerializesCorrectly()
    {
        // Arrange
        var flag = new FeatureFlag
        {
            Id = 1,
            Key = "test-flag",
            DisplayName = "Test Flag",
            IsEnabled = true,
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 50
        };

        var options = JsonFormatterOptions.CreateOptions();

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(flag, options);

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("test-flag", json);
        Assert.Contains("Test Flag", json);
    }
}
