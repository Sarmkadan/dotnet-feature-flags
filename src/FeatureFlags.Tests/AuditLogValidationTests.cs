#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Tests for AuditLogValidation.
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests;

public sealed class AuditLogValidationTests
{
    [Fact]
    public void Validate_ValidCreatedLog_ReturnsEmptyListAndIsValid()
    {
        var log = CreateValidLog();

        AuditLogValidation.Validate(log).Should().BeEmpty();
        AuditLogValidation.IsValid(log).Should().BeTrue();
    }

    [Fact]
    public void Validate_MissingRequiredFields_ReportsEachProblem()
    {
        var log = new AuditLog
        {
            Id = 0,
            FeatureFlagId = 0,
            Action = AuditAction.Created,
            ChangedBy = "   ",
            ChangedAt = default,
            NewValue = "",
            Description = ""
        };

        var errors = AuditLogValidation.Validate(log);

        errors.Should().HaveCount(6);
        errors.Should().Contain("Id must be a positive integer.");
        errors.Should().Contain("FeatureFlagId must be a positive integer.");
        errors.Should().Contain("ChangedBy cannot be null, empty, or whitespace.");
        errors.Should().Contain("ChangedAt cannot be the default DateTime value.");
        errors.Should().Contain("NewValue must be specified for Created action.");
        errors.Should().Contain("Description cannot be null, empty, or whitespace.");
        AuditLogValidation.IsValid(log).Should().BeFalse();
    }

    [Theory]
    [InlineData(AuditAction.Deleted, "", "OldValue must be specified for Deleted action.")]
    [InlineData(AuditAction.Updated, " ", "At least one of OldValue or NewValue must be specified for this action type.")]
    [InlineData(AuditAction.Evaluated, "", "At least one of OldValue or NewValue must be specified for this action type.")]
    public void Validate_ActionRequiresValues_ReportsMissingValue(AuditAction action, string value, string expected)
    {
        var log = CreateValidLog();
        log.Action = action;
        log.OldValue = value;
        log.NewValue = value;

        AuditLogValidation.Validate(log).Should().Equal(expected);
    }

    [Fact]
    public void Validate_UndefinedAction_ReportsInvalidActionValue()
    {
        var log = CreateValidLog();
        log.Action = (AuditAction)999;

        AuditLogValidation.Validate(log).Should().Equal("Action '999' is not a valid AuditAction value.");
    }

    [Fact]
    public void Validate_ChangedAtAndLengthLimits_AcceptBoundaryAndRejectBeyond()
    {
        var atLimit = CreateValidLog();
        atLimit.ChangedBy = new string('a', 256);
        atLimit.Description = new string('d', 1024);
        atLimit.UserAgent = new string('u', 512);
        atLimit.ChangedAt = DateTime.UtcNow.AddSeconds(30);

        AuditLogValidation.Validate(atLimit).Should().BeEmpty();

        var overLimit = CreateValidLog();
        overLimit.ChangedBy = new string('a', 257);
        overLimit.Description = new string('d', 1025);
        overLimit.UserAgent = new string('u', 513);
        overLimit.ChangedAt = DateTime.UtcNow.AddMinutes(5);

        AuditLogValidation.Validate(overLimit).Should().Equal(
            "ChangedBy exceeds maximum length of 256 characters.",
            "ChangedAt cannot be more than 1 minute in the future.",
            "Description exceeds maximum length of 1024 characters.",
            "UserAgent exceeds maximum length of 512 characters.");
    }

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("::1", true)]
    [InlineData("not-an-ip", false)]
    [InlineData(null, true)]
    public void Validate_IpAddress_AcceptsValidFormatsAndOptionalNull(string? ipAddress, bool expectedValid)
    {
        var log = CreateValidLog();
        log.IpAddress = ipAddress;

        AuditLogValidation.IsValid(log).Should().Be(expectedValid);
    }

    [Fact]
    public void EnsureValid_InvalidLog_ThrowsArgumentExceptionWithMessages()
    {
        var log = CreateValidLog();
        log.ChangedBy = "";

        Action act = () => AuditLogValidation.EnsureValid(log);

        act.Should().Throw<ArgumentException>()
            .WithMessage("AuditLog is invalid. Validation errors: ChangedBy cannot be null, empty, or whitespace.");
    }

    [Fact]
    public void NullInput_ThrowsArgumentNullException_ForAllMethods()
    {
        Action validate = () => AuditLogValidation.Validate(null);
        Action isValid = () => AuditLogValidation.IsValid(null);
        Action ensureValid = () => AuditLogValidation.EnsureValid(null);

        validate.Should().Throw<ArgumentNullException>();
        isValid.Should().Throw<ArgumentNullException>();
        ensureValid.Should().Throw<ArgumentNullException>();
    }

    private static AuditLog CreateValidLog() => new()
    {
        Id = 1,
        FeatureFlagId = 1,
        Action = AuditAction.Created,
        ChangedBy = "admin",
        ChangedAt = DateTime.UtcNow,
        NewValue = "{\"enabled\":true}",
        Description = "Flag created"
    };
}
