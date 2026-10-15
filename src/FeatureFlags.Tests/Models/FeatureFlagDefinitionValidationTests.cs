#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests.Models;

/// <summary>
/// Unit tests for <see cref="FeatureFlagDefinitionValidation"/>, covering each validation rule
/// and the aggregated reporting of all violations.
/// </summary>
public sealed class FeatureFlagDefinitionValidationTests
{
    private static FeatureFlag ValidPercentageFlag(string key = "checkout-v2", int? percentage = 50)
        => new()
        {
            Key = key,
            DisplayName = "Checkout v2",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = percentage
        };

    private static FeatureFlag ValidAbTestFlag(params int[] weights)
    {
        var flag = new FeatureFlag
        {
            Key = "pricing-ab",
            DisplayName = "Pricing A/B",
            RolloutType = RolloutType.ABTest
        };

        for (var i = 0; i < weights.Length; i++)
        {
            flag.Variants.Add(new ABTestVariant
            {
                VariantKey = $"variant-{i}",
                DisplayName = $"Variant {i}",
                Description = "test",
                AllocationPercentage = weights[i],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        return flag;
    }

    [Fact]
    public void Validate_ValidPercentageFlag_ReturnsNoViolations()
    {
        var violations = ValidPercentageFlag().Validate();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullFlag_ThrowsArgumentNullException()
    {
        FeatureFlag? flag = null;

        var act = () => flag.Validate();

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyOrWhitespaceKey_ReportsKeyRequired(string key)
    {
        var violations = ValidPercentageFlag(key).Validate();

        violations.Should().ContainSingle()
            .Which.Should().Be("Key is required and cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData("feature flag")]
    [InlineData("feature.flag")]
    [InlineData("feature/flag")]
    [InlineData(" leading-space")]
    [InlineData("emoji-🚀")]
    public void Validate_KeyOutsideDocumentedPattern_ReportsPatternViolation(string key)
    {
        var violations = ValidPercentageFlag(key).Validate();

        violations.Should().ContainSingle()
            .Which.Should().Contain("must match ^[a-zA-Z0-9_-]+$");
    }

    [Theory]
    [InlineData("payment-v2")]
    [InlineData("feature_flag_test")]
    [InlineData("FeatureFlag123")]
    public void Validate_KeyMatchingPattern_IsAccepted(string key)
    {
        var violations = ValidPercentageFlag(key).Validate();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Validate_KeyLongerThan100Characters_ReportsLengthViolation()
    {
        var violations = ValidPercentageFlag(new string('a', 101)).Validate();

        violations.Should().ContainSingle()
            .Which.Should().Be("Key must be 100 characters or less.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Validate_PercentageRolloutAtBounds_IsAccepted(int percentage)
    {
        var violations = ValidPercentageFlag(percentage: percentage).Validate();

        violations.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_PercentageRolloutOutsideRange_ReportsRangeViolation(int percentage)
    {
        var violations = ValidPercentageFlag(percentage: percentage).Validate();

        violations.Should().ContainSingle()
            .Which.Should().Be($"PercentageRollout must be between 0 and 100 inclusive (was {percentage}).");
    }

    [Fact]
    public void Validate_PercentageFlagWithoutPercentageRollout_ReportsRequired()
    {
        var violations = ValidPercentageFlag(percentage: null).Validate();

        violations.Should().ContainSingle()
            .Which.Should().Be("PercentageRollout is required when RolloutType is Percentage.");
    }

    [Fact]
    public void Validate_VariantWeightsSummingTo100_IsAccepted()
    {
        var violations = ValidAbTestFlag(40, 35, 25).Validate();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Validate_VariantWeightsNotSummingTo100_ReportsSumViolation()
    {
        var violations = ValidAbTestFlag(50, 40).Validate();

        violations.Should().ContainSingle()
            .Which.Should().Be("Variation weights must sum to 100 (current total: 90).");
    }

    [Fact]
    public void Validate_VariantWeightsOutsideRangeButSummingTo100_ReportsEachWeightViolation()
    {
        // Weights sum to 100, so only the per-variant range rule fires, once for each out-of-range weight.
        var violations = ValidAbTestFlag(120, -20).Validate();

        violations.Should().HaveCount(2);
        violations.Should().Contain("Variant 'variant-0' AllocationPercentage must be between 0 and 100 inclusive (was 120).");
        violations.Should().Contain("Variant 'variant-1' AllocationPercentage must be between 0 and 100 inclusive (was -20).");
    }

    [Fact]
    public void Validate_ABTestWithoutVariants_ReportsVariantRequired()
    {
        var violations = ValidAbTestFlag().Validate();

        violations.Should().ContainSingle()
            .Which.Should().Be("ABTest requires at least one variant.");
    }

    [Fact]
    public void Validate_MultipleViolations_ReturnsAllOfThemInOneList()
    {
        var flag = new FeatureFlag
        {
            Key = "bad key!",
            DisplayName = "Bad",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 150
        };
        flag.Variants.Add(new ABTestVariant { VariantKey = "a", AllocationPercentage = 30 });

        var violations = flag.Validate();

        violations.Should().HaveCount(3);
        violations.Should().Contain(v => v.Contains("must match ^[a-zA-Z0-9_-]+$"));
        violations.Should().Contain("PercentageRollout must be between 0 and 100 inclusive (was 150).");
        violations.Should().Contain("Variation weights must sum to 100 (current total: 30).");
    }

    [Fact]
    public void EnsureValid_ValidFlag_DoesNotThrow()
    {
        var act = () => ValidPercentageFlag().EnsureValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_NullFlag_ThrowsArgumentNullException()
    {
        FeatureFlag? flag = null;

        var act = () => flag.EnsureValid();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_MultipleViolations_ThrowsSingleExceptionListingAllViolations()
    {
        var flag = new FeatureFlag
        {
            Key = "bad key!",
            DisplayName = "Bad",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 150
        };
        flag.Variants.Add(new ABTestVariant { VariantKey = "a", AllocationPercentage = 30 });

        var act = () => flag.EnsureValid();

        var exception = act.Should().Throw<InvalidFlagDefinitionException>().Which;
        exception.FlagKey.Should().Be("bad key!");
        exception.Violations.Should().HaveCount(3);
        exception.ErrorCode.Should().Be("FF_INVALID_CONFIG");
        exception.Message.Should().Contain("(3 violation(s))");
        exception.Message.Should().Contain("must match ^[a-zA-Z0-9_-]+$");
        exception.Message.Should().Contain("PercentageRollout must be between 0 and 100 inclusive (was 150).");
        exception.Message.Should().Contain("Variation weights must sum to 100 (current total: 30).");
    }

    [Fact]
    public void EnsureValid_SingleViolation_ExceptionMessageListsIt()
    {
        var act = () => ValidPercentageFlag(percentage: 101).EnsureValid();

        var exception = act.Should().Throw<InvalidFlagDefinitionException>().Which;
        exception.Violations.Should().ContainSingle();
        exception.Message.Should().Contain("(1 violation(s))");
        exception.Message.Should().Contain("PercentageRollout must be between 0 and 100 inclusive (was 101).");
    }
}
