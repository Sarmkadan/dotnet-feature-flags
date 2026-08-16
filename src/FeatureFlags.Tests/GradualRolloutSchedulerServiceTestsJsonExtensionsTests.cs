#nullable enable
// =============================================================================
// Unit tests for GradualRolloutSchedulerServiceTestsJsonExtensions
// =============================================================================

using System;
using System.Text.Json;
using Xunit;
using FeatureFlags.Tests.Services;

namespace FeatureFlags.Tests;

/// <summary>
/// Tests the JSON serialization helpers defined in
/// <see cref="GradualRolloutSchedulerServiceTestsJsonExtensions"/>.
/// </summary>
public sealed class GradualRolloutSchedulerServiceTestsJsonExtensionsTests
{
    /// <summary>
    /// Creates a minimal, non‑null instance of <see cref="GradualRolloutSchedulerServiceTests"/>
    /// that can be serialized. The concrete type is defined elsewhere in the test project,
    /// but it is expected to have a public parameterless constructor.
    /// </summary>
    private static GradualRolloutSchedulerServiceTests CreateSut()
    {
        // The class under test is part of the test project itself.
        // If it has a parameterless constructor this will succeed.
        // If the constructor requires parameters, the test project will need to be
        // updated accordingly – the current repository layout suggests a default ctor.
        return new GradualRolloutSchedulerServiceTests();
    }

    [Fact]
    public void ToJson_WithValidInstance_ReturnsJsonString()
    {
        // Arrange
        var instance = CreateSut();

        // Act
        string json = GradualRolloutSchedulerServiceTestsJsonExtensions.ToJson(instance);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        // The JSON should be deserializable back to the same type.
        var deserialized = JsonSerializer.Deserialize<GradualRolloutSchedulerServiceTests>(json);
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void ToJson_WithIndentation_ProducesIndentedJson()
    {
        // Arrange
        var instance = CreateSut();

        // Act
        string json = GradualRolloutSchedulerServiceTestsJsonExtensions.ToJson(instance, indented: true);

        // Assert
        // Indented JSON contains line‑break characters.
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void ToJson_NullArgument_ThrowsArgumentNullException()
    {
        // Arrange
        GradualRolloutSchedulerServiceTests? nullInstance = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            GradualRolloutSchedulerServiceTestsJsonExtensions.ToJson(nullInstance));
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsDeserializedObject()
    {
        // Arrange
        var original = CreateSut();
        string json = GradualRolloutSchedulerServiceTestsJsonExtensions.ToJson(original);

        // Act
        var result = GradualRolloutSchedulerServiceTestsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<GradualRolloutSchedulerServiceTests>(result);
    }

    [Fact]
    public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
    {
        // Null string
        Assert.Throws<ArgumentException>(() =>
            GradualRolloutSchedulerServiceTestsJsonExtensions.FromJson(null!));

        // Empty string
        Assert.Throws<ArgumentException>(() =>
            GradualRolloutSchedulerServiceTestsJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_MalformedJson_ThrowsJsonException()
    {
        // Arrange
        string malformedJson = "{ this is not valid json }";

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            GradualRolloutSchedulerServiceTestsJsonExtensions.FromJson(malformedJson));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var original = CreateSut();
        string json = GradualRolloutSchedulerServiceTestsJsonExtensions.ToJson(original);

        // Act
        bool success = GradualRolloutSchedulerServiceTestsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.IsType<GradualRolloutSchedulerServiceTests>(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        string invalidJson = "not a json string";

        // Act
        bool success = GradualRolloutSchedulerServiceTestsJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrEmptyJson_ThrowsArgumentException()
    {
        // Null input
        Assert.Throws<ArgumentException>(() =>
            GradualRolloutSchedulerServiceTestsJsonExtensions.TryFromJson(null!, out _));

        // Empty input
        Assert.Throws<ArgumentException>(() =>
            GradualRolloutSchedulerServiceTestsJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
