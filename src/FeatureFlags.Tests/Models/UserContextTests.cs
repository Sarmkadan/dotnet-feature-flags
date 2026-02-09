// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using Xunit;
using FluentAssertions;

namespace FeatureFlags.Tests.Models;

/// <summary>
/// Unit tests for UserContext model.
/// Tests attribute retrieval, validation, and consistent hashing.
/// </summary>
public class UserContextTests
{
    [Fact]
    public void IsValid_WithRequiredFields_ReturnsTrue()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };

        // Act
        var result = userContext.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithoutUserId_ReturnsFalse()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = string.Empty,
            Email = "user@example.com"
        };

        // Act
        var result = userContext.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithoutEmail_ReturnsFalse()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = string.Empty
        };

        // Act
        var result = userContext.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAttribute_StandardAttribute_ReturnsValue()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com",
            Country = "US"
        };

        // Act
        var result = userContext.GetAttribute("Country");

        // Assert
        result.Should().Be("US");
    }

    [Fact]
    public void GetAttribute_CustomAttribute_ReturnsValue()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };
        userContext.SetCustomAttribute("subscriptionLevel", "premium");

        // Act
        var result = userContext.GetAttribute("subscriptionLevel");

        // Assert
        result.Should().Be("premium");
    }

    [Fact]
    public void GetAttribute_NonExistentAttribute_ReturnsNull()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };

        // Act
        var result = userContext.GetAttribute("nonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SetCustomAttribute_AddsAttribute()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };

        // Act
        userContext.SetCustomAttribute("department", "engineering");

        // Assert
        userContext.CustomAttributes.Should().ContainKey("department");
        userContext.CustomAttributes["department"].Should().Be("engineering");
    }

    [Fact]
    public void SetCustomAttribute_WithEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => userContext.SetCustomAttribute(string.Empty, "value"));
    }

    [Fact]
    public void GetConsistentHash_SameInput_ReturnsSameHash()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };
        var flagKey = "new-feature";

        // Act
        var hash1 = userContext.GetConsistentHash(flagKey);
        var hash2 = userContext.GetConsistentHash(flagKey);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetConsistentHash_DifferentUsers_ReturnsDifferentHashes()
    {
        // Arrange
        var user1 = new UserContext { UserId = "user1", Email = "user1@example.com" };
        var user2 = new UserContext { UserId = "user2", Email = "user2@example.com" };
        var flagKey = "new-feature";

        // Act
        var hash1 = user1.GetConsistentHash(flagKey);
        var hash2 = user2.GetConsistentHash(flagKey);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetConsistentHash_ReturnsValueBetween0And99()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };

        // Act
        var hash = userContext.GetConsistentHash("flag");

        // Assert
        hash.Should().BeGreaterThanOrEqualTo(0);
        hash.Should().BeLessThan(100);
    }

    [Fact]
    public void GetAttribute_CaseInsensitive_ReturnsValue()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };

        // Act
        var result = userContext.GetAttribute("USERID");

        // Assert
        result.Should().Be("user123");
    }
}
