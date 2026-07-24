#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Tests for FlagEvaluationLogValidation.
// =============================================================================

using FeatureFlags.Models;
using Xunit;
using FluentAssertions;

namespace FeatureFlags.Tests;

public sealed class FlagEvaluationLogValidationTests
{
    [Fact]
    public void Validate_ValidLog_ReturnsEmptyList()
    {
        // Arrange
        var log = new FlagEvaluationLog
        {
            FlagName = "my-flag",
            UserId = "user-1",
            Timestamp = DateTime.UtcNow,
            Reason = "RulesBased"
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidLog_ReturnsErrors()
    {
        // Arrange
        var log = new FlagEvaluationLog
        {
            FlagName = "",
            UserId = "",
            Timestamp = DateTime.UtcNow.AddYears(-2), // Too old
            Reason = ""
        };

        // Act
        var errors = log.Validate();

        // Assert
        errors.Should().HaveCount(4);
        errors.Should().Contain("FlagName cannot be null, empty, or whitespace.");
        errors.Should().Contain("UserId cannot be null, empty, or whitespace.");
        errors.Should().Contain("Timestamp cannot be more than one year in the past.");
        errors.Should().Contain("Reason cannot be null, empty, or whitespace.");
    }

    [Fact]
    public void IsValid_ValidLog_ReturnsTrue()
    {
        // Arrange
        var log = new FlagEvaluationLog
        {
            FlagName = "my-flag",
            UserId = "user-1",
            Timestamp = DateTime.UtcNow,
            Reason = "RulesBased"
        };

        // Act
        var isValid = log.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidLog_ReturnsFalse()
    {
        // Arrange
        var log = new FlagEvaluationLog { FlagName = "" };

        // Act
        var isValid = log.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_NullInput_ReturnsFalse()
    {
        // Act
        var isValid = ((FlagEvaluationLog?)null).IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_ValidLog_DoesNotThrow()
    {
        // Arrange
        var log = new FlagEvaluationLog
        {
            FlagName = "my-flag",
            UserId = "user-1",
            Timestamp = DateTime.UtcNow,
            Reason = "RulesBased"
        };

        // Act
        Action act = () => log.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidLog_ThrowsArgumentException()
    {
        // Arrange
        var log = new FlagEvaluationLog { FlagName = "" };

        // Act
        Action act = () => log.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((FlagEvaluationLog)null!).Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
