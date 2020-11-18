#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Utilities;
using Xunit;

namespace FeatureFlags.Tests.Utilities;

/// <summary>
/// Unit tests for string extension methods.
/// Tests hashing, validation, and string transformation utilities.
/// </summary>
public sealed class StringExtensionTests
{
    [Fact]
    public void ToSha256_ProducesConsistentHash()
    {
        // Arrange
        var input = "test-input";

        // Act
        var hash1 = input.ToSha256();
        var hash2 = input.ToSha256();

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.True(hash1.Length > 0);
    }

    [Fact]
    public void ToHash32_ReturnsValueIn0To99()
    {
        // Arrange
        var inputs = new[] { "user1", "user2", "test", "example@email.com" };

        // Act & Assert
        foreach (var input in inputs)
        {
            var hash = input.ToHash32();
            Assert.InRange(hash, 0, 99);
        }
    }

    [Fact]
    public void IsValidEmail_AcceptsValidEmails()
    {
        // Arrange & Act & Assert
        Assert.True("user@example.com".IsValidEmail());
        Assert.True("john.doe@company.org".IsValidEmail());
    }

    [Fact]
    public void IsValidEmail_RejectsInvalidEmails()
    {
        // Arrange & Act & Assert
        Assert.False("invalid-email".IsValidEmail());
        Assert.False("@example.com".IsValidEmail());
        Assert.False(string.Empty.IsValidEmail());
        Assert.False(((string?)null).IsValidEmail());
    }

    [Fact]
    public void SnakeCaseToPascalCase_ConvertsCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal("FeatureFlagKey", "feature_flag_key".SnakeCaseToPascalCase());
        Assert.Equal("UserContext", "user_context".SnakeCaseToPascalCase());
    }

    [Fact]
    public void ToSnakeCase_ConvertsCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal("feature_flag_key", "FeatureFlagKey".ToSnakeCase());
        Assert.Equal("user_context", "UserContext".ToSnakeCase());
    }

    [Fact]
    public void Truncate_TruncatesCorrectly()
    {
        // Arrange & Act
        var result = "This is a long string".Truncate(10);

        // Assert
        Assert.Equal("This is a ...", result);
    }

    [Fact]
    public void ToIntOrDefault_ParsesSuccessfully()
    {
        // Arrange & Act & Assert
        Assert.Equal(42, "42".ToIntOrDefault());
        Assert.Equal(0, "not-a-number".ToIntOrDefault());
        Assert.Equal(-1, "not-a-number".ToIntOrDefault(-1));
    }

    [Fact]
    public void ContainsAny_DetectsMatchingSubstrings()
    {
        // Arrange
        var input = "feature-flag-engine";

        // Act & Assert
        Assert.True(input.ContainsAny("flag", "engine"));
        Assert.True(input.ContainsAny("FEATURE"));
        Assert.False(input.ContainsAny("webhook", "cache"));
    }

    [Fact]
    public void Repeat_RepeatsStringCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal("aaaa", "a".Repeat(4));
        Assert.Equal("test-test-test-", "test-".Repeat(3));
    }
}

/// <summary>
/// Unit tests for DateTime extension methods.
/// Tests time calculations and conversions.
/// </summary>
public sealed class DateTimeExtensionTests
{
    [Fact]
    public void ToUnixTimestamp_ConvertsCorrectly()
    {
        // Arrange
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var timestamp = dt.ToUnixTimestamp();

        // Assert
        Assert.Equal(0, timestamp);
    }

    [Fact]
    public void FromUnixTimestamp_ConvertsCorrectly()
    {
        // Arrange
        var timestamp = 0L;

        // Act
        var dt = DateTimeExtensions.FromUnixTimestamp(timestamp);

        // Assert
        Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), dt);
    }

    [Fact]
    public void IsBetween_DetectsRangeCorrectly()
    {
        // Arrange
        var date = DateTime.Now;
        var start = date.AddDays(-1);
        var end = date.AddDays(1);

        // Act & Assert
        Assert.True(date.IsBetween(start, end));
        Assert.False(date.AddDays(2).IsBetween(start, end));
    }

    [Fact]
    public void StartOfDay_ReturnsBeginningOfDay()
    {
        // Arrange
        var dt = new DateTime(2024, 5, 15, 14, 30, 45);

        // Act
        var result = dt.StartOfDay();

        // Assert
        Assert.Equal(new DateTime(2024, 5, 15, 0, 0, 0), result);
    }

    [Fact]
    public void EndOfDay_ReturnsEndOfDay()
    {
        // Arrange
        var dt = new DateTime(2024, 5, 15, 14, 30, 45);

        // Act
        var result = dt.EndOfDay();

        // Assert
        Assert.Equal(15, result.Day);
        Assert.True(result.TimeOfDay.Hours >= 23);
    }

    [Fact]
    public void StartOfMonth_ReturnsFirstDayOfMonth()
    {
        // Arrange
        var dt = new DateTime(2024, 5, 15);

        // Act
        var result = dt.StartOfMonth();

        // Assert
        Assert.Equal(1, result.Day);
        Assert.Equal(5, result.Month);
    }

    [Fact]
    public void IsToday_DetectsCurrentDate()
    {
        // Arrange
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // Act & Assert
        Assert.True(today.IsToday());
        Assert.False(tomorrow.IsToday());
    }

    [Fact]
    public void IsPast_DetectsPastDates()
    {
        // Arrange
        var past = DateTime.UtcNow.AddHours(-1);
        var future = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        Assert.True(past.IsPast());
        Assert.False(future.IsPast());
    }
}

/// <summary>
/// Unit tests for validation extension methods.
/// </summary>
public sealed class ValidationExtensionTests
{
    [Fact]
    public void IsNullOrEmpty_DetectsEmpty()
    {
        // Arrange & Act & Assert
        Assert.True(((List<int>?)null).IsNullOrEmpty());
        Assert.True(new List<int>().IsNullOrEmpty());
        Assert.False(new List<int> { 1 }.IsNullOrEmpty());
    }

    [Fact]
    public void IsValidPercentage_DetectsValidRange()
    {
        // Arrange & Act & Assert
        Assert.True(0.IsValidPercentage());
        Assert.True(50.IsValidPercentage());
        Assert.True(100.IsValidPercentage());
        Assert.False((-1).IsValidPercentage());
        Assert.False(101.IsValidPercentage());
    }

    [Fact]
    public void IsValidKeyFormat_AcceptsValidKeys()
    {
        // Arrange & Act & Assert
        Assert.True("payment-v2".IsValidKeyFormat());
        Assert.True("feature_flag_test".IsValidKeyFormat());
        Assert.True("FeatureFlag123".IsValidKeyFormat());
    }

    [Fact]
    public void IsValidKeyFormat_RejectsInvalidKeys()
    {
        // Arrange & Act & Assert
        Assert.False("".IsValidKeyFormat());
        Assert.False("feature flag".IsValidKeyFormat()); // Contains space
    }

    [Fact]
    public void IsAlphanumeric_ChecksCorrectly()
    {
        // Arrange & Act & Assert
        Assert.True("abc123".IsAlphanumeric());
        Assert.False("abc-123".IsAlphanumeric());
        Assert.False("".IsAlphanumeric());
    }

    [Fact]
    public void HasDuplicates_DetectsDuplicates()
    {
        // Arrange & Act & Assert
        Assert.True(new[] { 1, 2, 2, 3 }.HasDuplicates());
        Assert.False(new[] { 1, 2, 3 }.HasDuplicates());
    }
}
