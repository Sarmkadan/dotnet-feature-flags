using System;
using FeatureFlags.Models;
using FeatureFlags.Enums;
using Xunit;

namespace FeatureFlags.Tests;

public class FeatureFlagJsonExtensionsTests
{
    private readonly FeatureFlag _sampleFlag = new FeatureFlag
    {
        Id = 1,
        Key = "test-feature",
        DisplayName = "Test Feature",
        Description = "A test feature flag",
        IsEnabled = true,
        RolloutType = RolloutType.Percentage,
        PercentageRollout = 50,
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        UpdatedAt = DateTime.UtcNow,
        CreatedBy = "test-user",
        UpdatedBy = "test-user"
    };

    [Fact]
    public void ToJson_WithValidFeatureFlag_ReturnsValidJsonString()
    {
        var flag = _sampleFlag;
        var json = flag.ToJson();

        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("test-feature", json);
        Assert.Contains("Test Feature", json);
        Assert.Contains("true", json);
        Assert.Contains("50", json);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        var flag = _sampleFlag;
        var json = flag.ToJson(indented: true);

        Assert.NotNull(json);
        Assert.Contains(Environment.NewLine, json);
        Assert.Contains("{", json);
        Assert.Contains("}", json);
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        var flag = _sampleFlag;
        var json = flag.ToJson(indented: false);

        Assert.NotNull(json);
        Assert.DoesNotContain(Environment.NewLine, json);
    }

    [Fact]
    public void ToJson_WithNullFeatureFlag_ThrowsArgumentNullException()
    {
        FeatureFlag? flag = null;
        Assert.Throws<ArgumentNullException>(() => flag!.ToJson());
    }

    [Fact]
    public void ToJson_CamelCasePropertyNames_ReturnsCorrectNaming()
    {
        var flag = _sampleFlag;
        var json = flag.ToJson();

        Assert.Contains("key", json);
        Assert.Contains("displayName", json);
        Assert.Contains("isEnabled", json);
        Assert.Contains("rolloutType", json);
        Assert.Contains("percentageRollout", json);
    }

    [Fact]
    public void ToJson_NullValuesAreNotSerialized_ReturnsJsonWithoutNullProperties()
    {
        var flag = new FeatureFlag
        {
            Id = 2,
            Key = "minimal-feature",
            DisplayName = "Minimal Feature",
            Description = null,
            IsEnabled = false,
            RolloutType = RolloutType.None,
            PercentageRollout = null
        };

        var json = flag.ToJson();

        Assert.NotNull(json);
        Assert.DoesNotContain("null", json);
    }

    [Fact]
    public void FromJson_WithValidJsonString_ReturnsDeserializedFeatureFlag()
    {
        var json = _sampleFlag.ToJson();
        var flag = FeatureFlagJsonExtensions.FromJson(json);

        Assert.NotNull(flag);
        Assert.Equal(_sampleFlag.Key, flag.Key);
        Assert.Equal(_sampleFlag.DisplayName, flag.DisplayName);
        Assert.Equal(_sampleFlag.IsEnabled, flag.IsEnabled);
        Assert.Equal(_sampleFlag.PercentageRollout, flag.PercentageRollout);
    }

    [Fact]
    public void FromJson_WithNullJsonString_ThrowsArgumentNullException()
    {
        string? json = null;
        Assert.Throws<ArgumentNullException>(() => FeatureFlagJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_WithEmptyString_ReturnsNull()
    {
        var json = string.Empty;
        var flag = FeatureFlagJsonExtensions.FromJson(json);

        Assert.Null(flag);
    }

    [Fact]
    public void FromJson_WithWhitespaceString_ReturnsNull()
    {
        var json = "   ";
        var flag = FeatureFlagJsonExtensions.FromJson(json);

        Assert.Null(flag);
    }

    [Fact]
    public void FromJson_WithInvalidJson_ReturnsNull()
    {
        var json = "invalid json {{{";
        var flag = FeatureFlagJsonExtensions.FromJson(json);

        Assert.Null(flag);
    }

    [Fact]
    public void FromJson_WithCaseInsensitivePropertyNames_ReturnsDeserializedFeatureFlag()
    {
        var json = @"{ ""key"": ""case-test-feature"", ""displayName"": ""Case Test"", ""isEnabled"": true }";
        var flag = FeatureFlagJsonExtensions.FromJson(json);

        Assert.NotNull(flag);
        Assert.Equal("case-test-feature", flag.Key);
        Assert.Equal("Case Test", flag.DisplayName);
        Assert.True(flag.IsEnabled);
    }

    [Fact]
    public void FromJson_WithPartialJson_ReturnsDeserializedFeatureFlagWithDefaults()
    {
        var json = @"{ ""key"": ""partial-feature"", ""isEnabled"": true }";
        var flag = FeatureFlagJsonExtensions.FromJson(json);

        Assert.NotNull(flag);
        Assert.Equal("partial-feature", flag.Key);
        Assert.True(flag.IsEnabled);
        Assert.Equal(string.Empty, flag.DisplayName);
        Assert.True(flag.RolloutType == RolloutType.Percentage);
    }

    [Fact]
    public void TryFromJson_WithValidJsonString_ReturnsTrueAndDeserializedFeatureFlag()
    {
        var json = _sampleFlag.ToJson();
        FeatureFlag? flag = null;
        var result = FeatureFlagJsonExtensions.TryFromJson(json, out flag);

        Assert.True(result);
        Assert.NotNull(flag);
        Assert.Equal(_sampleFlag.Key, flag.Key);
        Assert.Equal(_sampleFlag.DisplayName, flag.DisplayName);
    }

    [Fact]
    public void TryFromJson_WithNullJsonString_ThrowsArgumentNullException()
    {
        string? json = null;
        FeatureFlag? flag = null;
        Assert.Throws<ArgumentNullException>(() => FeatureFlagJsonExtensions.TryFromJson(json!, out flag));
    }

    [Fact]
    public void TryFromJson_WithEmptyString_ReturnsFalseAndNull()
    {
        var json = string.Empty;
        FeatureFlag? flag = new FeatureFlag();
        var result = FeatureFlagJsonExtensions.TryFromJson(json, out flag);

        Assert.False(result);
        Assert.Null(flag);
    }

    [Fact]
    public void TryFromJson_WithWhitespaceString_ReturnsFalseAndNull()
    {
        var json = "   ";
        FeatureFlag? flag = new FeatureFlag();
        var result = FeatureFlagJsonExtensions.TryFromJson(json, out flag);

        Assert.False(result);
        Assert.Null(flag);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        var json = "invalid json {{{";
        FeatureFlag? flag = new FeatureFlag();
        var result = FeatureFlagJsonExtensions.TryFromJson(json, out flag);

        Assert.False(result);
        Assert.Null(flag);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_SetsOutParameterToNull()
    {
        var json = "invalid json {{{";
        FeatureFlag? flag = new FeatureFlag { Key = "original-key" };
        var result = FeatureFlagJsonExtensions.TryFromJson(json, out flag);

        Assert.False(result);
        Assert.Null(flag);
    }

    [Fact]
    public void RoundTripSerialization_WithFeatureFlag_ProducesEquivalentObject()
    {
        var originalFlag = _sampleFlag;
        var json = originalFlag.ToJson();
        var deserializedFlag = FeatureFlagJsonExtensions.FromJson(json);

        Assert.NotNull(deserializedFlag);
        Assert.Equal(originalFlag.Key, deserializedFlag.Key);
        Assert.Equal(originalFlag.DisplayName, deserializedFlag.DisplayName);
        Assert.Equal(originalFlag.Description, deserializedFlag.Description);
        Assert.Equal(originalFlag.IsEnabled, deserializedFlag.IsEnabled);
        Assert.Equal(originalFlag.RolloutType, deserializedFlag.RolloutType);
        Assert.Equal(originalFlag.PercentageRollout, deserializedFlag.PercentageRollout);
        Assert.Equal(originalFlag.CreatedAt.ToString("o"), deserializedFlag.CreatedAt.ToString("o"));
        Assert.Equal(originalFlag.UpdatedAt.ToString("o"), deserializedFlag.UpdatedAt.ToString("o"));
        Assert.Equal(originalFlag.CreatedBy, deserializedFlag.CreatedBy);
        Assert.Equal(originalFlag.UpdatedBy, deserializedFlag.UpdatedBy);
    }

    [Fact]
    public void RoundTripSerialization_WithTryFromJson_ProducesEquivalentObject()
    {
        var originalFlag = _sampleFlag;
        FeatureFlag? deserializedFlag = null;
        var json = originalFlag.ToJson();
        var result = FeatureFlagJsonExtensions.TryFromJson(json, out deserializedFlag);

        Assert.True(result);
        Assert.NotNull(deserializedFlag);
        Assert.Equal(originalFlag.Key, deserializedFlag.Key);
        Assert.Equal(originalFlag.DisplayName, deserializedFlag.DisplayName);
    }
}