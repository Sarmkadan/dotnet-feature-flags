#nullable enable

using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FeatureFlags.Tests;

/// <summary>
/// Unit tests for stable percentage-rollout bucketing and validation.
/// </summary>
public sealed class PercentageRolloutServiceTests
{
    private readonly PercentageRolloutService _service =
        new(new Mock<ILogger<PercentageRolloutService>>().Object);

    [Fact]
    public void IsUserInRollout_AtZeroPercent_DisablesEveryUser()
    {
        var results = Enumerable.Range(0, 1_000)
            .Select(index => _service.IsUserInRollout(CreateUser(index), "zero-percent", 0));

        results.Should().OnlyContain(result => !result);
    }

    [Fact]
    public void IsUserInRollout_AtOneHundredPercent_EnablesEveryUser()
    {
        var results = Enumerable.Range(0, 1_000)
            .Select(index => _service.IsUserInRollout(CreateUser(index), "full-rollout", 100));

        results.Should().OnlyContain(result => result);
    }

    [Fact]
    public void Bucketing_WithSameUserAndKey_IsConsistentAcrossCalls()
    {
        var userContext = CreateUser(42);

        var buckets = Enumerable.Range(0, 20)
            .Select(_ => _service.GetUserBucket(userContext, "stable-flag"))
            .ToArray();
        var decisions = Enumerable.Range(0, 20)
            .Select(_ => _service.IsUserInRollout(userContext, "stable-flag", 50))
            .ToArray();

        buckets.Should().OnlyContain(bucket => bucket == buckets[0]);
        decisions.Should().OnlyContain(decision => decision == decisions[0]);
    }

    [Fact]
    public void GetUserBucket_WithDifferentKeys_CanProduceDifferentBucketsForSameUser()
    {
        var userContext = CreateUser(42);
        var buckets = Enumerable.Range(0, 20)
            .Select(index => _service.GetUserBucket(userContext, $"feature-{index}"));

        buckets.Distinct().Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void GetUserBucket_ForManyUsersAndKeys_IsAlwaysWithinValidRange()
    {
        var buckets = from userIndex in Enumerable.Range(0, 1_000)
                      from keyIndex in Enumerable.Range(0, 3)
                      select _service.GetUserBucket(CreateUser(userIndex), $"feature-{keyIndex}");

        buckets.Should().OnlyContain(bucket => bucket >= 0 && bucket < 100);
    }

    [Fact]
    public async Task EvaluateAsync_WithNullFeatureFlag_ThrowsArgumentNullException()
    {
        var action = () => _service.EvaluateAsync(null!, CreateUser(1));

        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("featureFlag");
    }

    [Fact]
    public async Task EvaluateAsync_WithNullUserContext_ThrowsArgumentNullException()
    {
        var featureFlag = new FeatureFlag { Key = "test-flag", PercentageRollout = 50 };
        var action = () => _service.EvaluateAsync(featureFlag, null!);

        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("userContext");
    }

    [Fact]
    public async Task EvaluateAsync_WithoutPercentageRollout_ThrowsInvalidFeatureFlagException()
    {
        var featureFlag = new FeatureFlag { Key = "test-flag", PercentageRollout = null };
        var action = () => _service.EvaluateAsync(featureFlag, CreateUser(1));

        await action.Should().ThrowAsync<InvalidFeatureFlagException>()
            .WithMessage("*does not have a percentage rollout configured*");
    }

    [Fact]
    public void IsUserInRollout_WithNullUserContext_ThrowsArgumentNullException()
    {
        var action = () => _service.IsUserInRollout(null!, "test-flag", 50);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("userContext");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsUserInRollout_WithEmptyKey_ThrowsArgumentException(string? key)
    {
        var action = () => _service.IsUserInRollout(CreateUser(1), key!, 50);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("featureFlagKey");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void IsUserInRollout_WithPercentageOutsideRange_ThrowsArgumentException(int percentage)
    {
        var action = () => _service.IsUserInRollout(CreateUser(1), "test-flag", percentage);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("rolloutPercentage");
    }

    [Fact]
    public void GetUserBucket_WithNullUserContext_ThrowsArgumentNullException()
    {
        var action = () => _service.GetUserBucket(null!, "test-flag");

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("userContext");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetUserBucket_WithEmptyKey_ThrowsArgumentException(string? key)
    {
        var action = () => _service.GetUserBucket(CreateUser(1), key!);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("featureFlagKey");
    }

    [Fact]
    public void IsUserInRollout_AtFiftyPercent_HasReasonableDistribution()
    {
        const int totalUsers = 1_000;
        var enabledCount = Enumerable.Range(0, totalUsers)
            .Count(index => _service.IsUserInRollout(CreateUser(index), "distribution-flag", 50));

        enabledCount.Should().BeInRange(425, 575);
    }

    private static UserContext CreateUser(int index) => new()
    {
        UserId = $"user-{index}",
        Email = $"user-{index}@example.com"
    };
}
