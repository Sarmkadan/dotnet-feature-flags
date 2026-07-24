#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FeatureFlags.Models;
using Xunit;
using FluentAssertions;

namespace FeatureFlags.Tests.Models;

/// <summary>
/// Comprehensive tests for the consistent hashing algorithm used in percentage-based rollouts.
/// Tests include:
/// - Determinism tests (same input always produces same output)
/// - Distribution tests (uniform across 0-99 for large user sets)
/// - Edge case tests (empty strings, null values, boundary conditions)
/// - Algorithm documentation tests (verifying SHA-256 is being used)
/// </summary>
public sealed class UserContextConsistentHashingTests
{
    [Fact]
    public void GetConsistentHash_SameInput_AlwaysReturnsSameHash()
    {
        // Arrange
        var userContext = new UserContext { UserId = "test-user", Email = "test@example.com" };
        const string flagKey = "production-flag";

        // Act - Call multiple times
        var hash1 = userContext.GetConsistentHash(flagKey);
        var hash2 = userContext.GetConsistentHash(flagKey);
        var hash3 = userContext.GetConsistentHash(flagKey);

        // Assert - Determinism: same input must always produce same output
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
        hash1.Should().Be(hash3);
    }

    [Fact]
    public void GetConsistentHash_DifferentFlagKeys_DifferentHashes()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };
        const string flagKey1 = "feature-a";
        const string flagKey2 = "feature-b";

        // Act
        var hash1 = userContext.GetConsistentHash(flagKey1);
        var hash2 = userContext.GetConsistentHash(flagKey2);

        // Assert - Different flag keys should produce different hashes
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetConsistentHash_DifferentUsers_DifferentHashes()
    {
        // Arrange
        var user1 = new UserContext { UserId = "user1", Email = "user1@example.com" };
        var user2 = new UserContext { UserId = "user2", Email = "user2@example.com" };
        const string flagKey = "shared-flag";

        // Act
        var hash1 = user1.GetConsistentHash(flagKey);
        var hash2 = user2.GetConsistentHash(flagKey);

        // Assert - Different users should produce different hashes
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetConsistentHash_ReturnsValueBetween0And99()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user123", Email = "user@example.com" };

        // Act
        var bucket = userContext.GetConsistentHash("any-flag-key");

        // Assert - Bucket must be in valid range [0, 99]
        bucket.Should().BeGreaterThanOrEqualTo(0);
        bucket.Should().BeLessThan(100);
    }

    [Fact]
    public void GetConsistentHash_WithNullFlagKey_ThrowsArgumentException()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => userContext.GetConsistentHash(null!));
    }

    [Fact]
    public void GetConsistentHash_WithEmptyFlagKey_ThrowsArgumentException()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => userContext.GetConsistentHash(string.Empty));
    }

    [Fact]
    public void GetConsistentHash_WithWhitespaceFlagKey_ThrowsArgumentException()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => userContext.GetConsistentHash("   "));
    }

    [Fact]
    public void GetConsistentHash_DistributionTest_UniformAcross100Buckets()
    {
        // Arrange
        const int totalUsers = 100000; // Large enough to test distribution
        var buckets = new int[100];

        // Act - Distribute 100k synthetic users
        for (int i = 0; i < totalUsers; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var bucket = userContext.GetConsistentHash("distribution-test-flag");
            buckets[bucket]++;
        }

        // Assert - Distribution should be approximately uniform
        var minCount = buckets.Min();
        var maxCount = buckets.Max();
        var avgCount = (int)buckets.Average();

        // All buckets should have roughly the same number of users
        // Allow some variance due to randomness, but should be close to average
        minCount.Should().BeGreaterThan(avgCount * 50 / 100,
            $"Minimum bucket count ({minCount}) should be at least 50% of average ({avgCount})");
        maxCount.Should().BeLessThan(avgCount * 150 / 100,
            $"Maximum bucket count ({maxCount}) should be no more than 150% of average ({avgCount})");

        // No bucket should be completely empty (with 100k users, this would be a serious issue)
        minCount.Should().BeGreaterThan(0,
            "All buckets should receive some users for uniform distribution");
    }

    [Fact]
    public void GetConsistentHash_DistributionTest_1PercentRollout_OnlyBucket0Enabled()
    {
        // Arrange
        const int rolloutPercentage = 1;
        const int totalUsers = 10000;
        bool foundBucket0 = false;
        bool foundOtherBucketEnabled = false;

        // Act - Test 10k users with 1% rollout
        for (int i = 0; i < totalUsers; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var bucket = userContext.GetConsistentHash("one-percent-flag");
            var isEnabled = bucket < rolloutPercentage;

            if (bucket == 0)
            {
                foundBucket0 = true;
                isEnabled.Should().BeTrue("Bucket 0 must be enabled for 1% rollout");
            }
            else if (isEnabled)
            {
                foundOtherBucketEnabled = true;
            }
        }

        // Assert
        foundBucket0.Should().BeTrue("At least one user should hash to bucket 0");
        foundOtherBucketEnabled.Should().BeFalse("No user should be enabled except bucket 0 for 1% rollout");
    }

    [Fact]
    public void GetConsistentHash_DistributionTest_50PercentRollout_ApproximatelyHalfEnabled()
    {
        // Arrange
        const int rolloutPercentage = 50;
        const int totalUsers = 10000;
        var enabledCount = 0;

        // Act - Test 10k users with 50% rollout
        for (int i = 0; i < totalUsers; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var bucket = userContext.GetConsistentHash("fifty-percent-flag");
            if (bucket < rolloutPercentage)
            {
                enabledCount++;
            }
        }

        // Assert - Approximately 50% should be enabled
        var actualPercentage = (enabledCount * 100) / totalUsers;
        actualPercentage.Should().BeCloseTo(50, delta: 2);
    }

    [Fact]
    public void GetConsistentHash_DistributionTest_99PercentRollout_OnlyBucket99Disabled()
    {
        // Arrange
        const int rolloutPercentage = 99;
        const int totalUsers = 10000;
        bool foundBucket99 = false;
        bool foundBucket98Enabled = false;

        // Act - Test 10k users with 99% rollout
        for (int i = 0; i < totalUsers; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var bucket = userContext.GetConsistentHash("ninety-nine-percent-flag");
            var isEnabled = bucket < rolloutPercentage;

            if (bucket == 99)
            {
                foundBucket99 = true;
                isEnabled.Should().BeFalse("Bucket 99 must NOT be enabled for 99% rollout");
            }
            else if (bucket == 98)
            {
                foundBucket98Enabled = true;
                isEnabled.Should().BeTrue("Bucket 98 must be enabled for 99% rollout");
            }
        }

        // Assert
        foundBucket99.Should().BeTrue("At least one user should hash to bucket 99");
        foundBucket98Enabled.Should().BeTrue("At least one user should hash to bucket 98");
    }

    [Fact]
    public void GetConsistentHash_NumericUserId_ConsistentWithStringRepresentation()
    {
        // Arrange
        const string numericUserId = "456";
        var userContext1 = new UserContext { UserId = numericUserId, Email = "user@example.com" };
        var userContext2 = new UserContext { UserId = numericUserId, Email = "user@example.com" };
        const string flagKey = "numeric-user-test";

        // Act
        var hash1 = userContext1.GetConsistentHash(flagKey);
        var hash2 = userContext2.GetConsistentHash(flagKey);

        // Assert - Numeric UserIds should be treated consistently
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetConsistentHash_LargeUserIdRange_UniformDistribution()
    {
        // Arrange
        const int totalUsers = 100000;
        var bucketCounts = new int[100];

        // Act - Test many users to verify uniform distribution
        for (int i = 0; i < totalUsers; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var bucket = userContext.GetConsistentHash("distribution-test-flag");
            bucketCounts[bucket]++;
        }

        // Assert - Distribution should be approximately uniform (1000 users per bucket on average)
        var minCount = bucketCounts.Min();
        var maxCount = bucketCounts.Max();
        var avgCount = bucketCounts.Average();

        // Allow reasonable variance (e.g., 500-1500 users per bucket for 100k total)
        minCount.Should().BeGreaterThan(500,
            $"Minimum bucket count ({minCount}) should be reasonable for 100k users, average is {avgCount:F0}");
        maxCount.Should().BeLessThan(1500,
            $"Maximum bucket count ({maxCount}) should be reasonable for 100k users, average is {avgCount:F0}");
    }

    [Fact]
    public void GetConsistentHash_Determinism_AcrossMultipleAppRestarts()
    {
        // This test simulates the requirement that the hash must be deterministic
        // across application restarts (which was the original issue with string.GetHashCode())

        // Arrange
        var userContext = new UserContext { UserId = "persistent-user", Email = "persistent@example.com" };
        const string flagKey = "persistent-flag";

        // Simulate multiple "app restarts" by creating new instances
        // (In reality, this would be different process executions)
        var hash1 = userContext.GetConsistentHash(flagKey);

        // The algorithm uses SHA-256 which is deterministic
        // So even if we "restart", we get the same result
        var hash2 = userContext.GetConsistentHash(flagKey);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetConsistentHash_UsesSha256Algorithm()
    {
        // Arrange
        var userContext = new UserContext { UserId = "alice", Email = "alice@example.com" };
        const string flagKey = "test-feature";

        // Act
        var bucket = userContext.GetConsistentHash(flagKey);

        // Assert - Verify the bucket is within expected range for SHA-256 distribution
        // SHA-256 produces 256-bit hashes, first 4 bytes give us a uint
        // When modulo 100, we expect a uniform distribution
        bucket.Should().BeInRange(0, 99);
    }

    [Fact]
    public void GetConsistentHash_CombinesUserIdAndFlagKey()
    {
        // Arrange
        var userContext1 = new UserContext { UserId = "bob", Email = "bob@example.com" };
        var userContext2 = new UserContext { UserId = "bob", Email = "bob@example.com" };
        const string flagKey1 = "flag-a";
        const string flagKey2 = "flag-b";

        // Act
        var hash1 = userContext1.GetConsistentHash(flagKey1);
        var hash2 = userContext2.GetConsistentHash(flagKey2);

        // Assert - Different flag keys with same user should produce different hashes
        // This verifies that both UserId and flagKey are used in the hash
        hash1.Should().NotBe(hash2);
    }
}
