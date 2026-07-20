using System;
using System.Collections.Generic;
using System.Linq;
using FeatureFlags.Enums;
using FeatureFlags.Models;
using FeatureFlags.Utilities;
using Xunit;

namespace FeatureFlags.Tests.Utilities;

/// <summary>
/// Tests for FeatureFlagSearchBuilder to ensure correct query construction and filtering behavior.
/// </summary>
public class SearchQueryBuilderTests
{
    private readonly List<FeatureFlag> _testFlags;

    public SearchQueryBuilderTests()
    {
        _testFlags = new List<FeatureFlag>
        {
            new FeatureFlag
            {
                Key = "feature-a",
                DisplayName = "Feature A",
                IsEnabled = true,
                RolloutType = RolloutType.Percentage,
                PercentageRollout = 50,
                CreatedBy = "user1",
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new FeatureFlag
            {
                Key = "feature-b",
                DisplayName = "Feature B",
                IsEnabled = false,
                RolloutType = RolloutType.RulesBased,
                CreatedBy = "user2",
                CreatedAt = new DateTime(2024, 1, 2)
            },
            new FeatureFlag
            {
                Key = "feature-c",
                DisplayName = "Feature C",
                IsEnabled = true,
                RolloutType = RolloutType.ABTest,
                CreatedBy = "user1",
                CreatedAt = new DateTime(2024, 1, 3)
            },
            new FeatureFlag
            {
                Key = "feature-d",
                DisplayName = "Feature D",
                IsEnabled = true,
                RolloutType = RolloutType.Percentage,
                PercentageRollout = 75,
                CreatedBy = "user3",
                CreatedAt = new DateTime(2024, 1, 4)
            },
            new FeatureFlag
            {
                Key = "special-feature",
                DisplayName = "Special Feature",
                IsEnabled = false,
                RolloutType = RolloutType.Full,
                CreatedBy = "user1",
                CreatedAt = new DateTime(2024, 1, 5)
            }
        };
    }

    [Fact]
    public void EmptyQuery_ReturnsAllFlags()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder();

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void EmptyQuery_BuildsCorrectIQueryable()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder();
        var queryable = _testFlags.AsQueryable();

        // Act
        var result = builder.Build(queryable);

        // Assert
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public void WithKeyContaining_FiltersByKey()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithKeyContaining("feature-a");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("feature-a", result[0].Key);
    }

    [Fact]
    public void WithKeyContaining_CaseInsensitive()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithKeyContaining("FEATURE-A");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("feature-a", result[0].Key);
    }

    [Fact]
    public void WithKeyContaining_FiltersBySubstring()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithKeyContaining("feature");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void WithNameContaining_FiltersByName()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithNameContaining("Feature B");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("Feature B", result[0].DisplayName);
    }

    [Fact]
    public void WithNameContaining_CaseInsensitiveName()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithNameContaining("feature c");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("Feature C", result[0].DisplayName);
    }

    [Fact]
    public void WithEnabledStatus_FiltersByEnabled()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithEnabledStatus(true);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, flag => Assert.True(flag.IsEnabled));
    }

    [Fact]
    public void WithEnabledStatus_FiltersByDisabled()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithEnabledStatus(false);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, flag => Assert.False(flag.IsEnabled));
    }

    [Fact]
    public void WithRolloutType_FiltersByRolloutType()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithRolloutType(RolloutType.Percentage);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, flag => Assert.Equal(RolloutType.Percentage, flag.RolloutType));
    }

    [Fact]
    public void WithCreatedBy_FiltersByCreator()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithCreatedBy("user1");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, flag => Assert.Equal("user1", flag.CreatedBy));
    }

    [Fact]
    public void WithCreatedDateRange_FiltersByDateRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 2);
        var endDate = new DateTime(2024, 1, 4);
        var builder = new FeatureFlagSearchBuilder()
            .WithCreatedDateRange(startDate, endDate);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, flag =>
        {
            Assert.True(flag.CreatedAt >= startDate);
            Assert.True(flag.CreatedAt <= endDate);
        });
    }

    [Fact]
    public void WithCreatedDateRange_OnlyStartDate()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 3);
        var builder = new FeatureFlagSearchBuilder()
            .WithCreatedDateRange(startDate, null);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, flag => Assert.True(flag.CreatedAt >= startDate));
    }

    [Fact]
    public void WithCreatedDateRange_OnlyEndDate()
    {
        // Arrange
        var endDate = new DateTime(2024, 1, 3);
        var builder = new FeatureFlagSearchBuilder()
            .WithCreatedDateRange(null, endDate);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, flag => Assert.True(flag.CreatedAt <= endDate));
    }

    [Fact]
    public void MultipleCriteria_CombineCorrectly()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithKeyContaining("feature")
            .WithEnabledStatus(true)
            .WithRolloutType(RolloutType.Percentage);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count);
        var flag1 = result[0];
        var flag2 = result[1];
        Assert.Equal("feature-a", flag1.Key);
        Assert.Equal("feature-d", flag2.Key);
        Assert.True(flag1.IsEnabled);
        Assert.True(flag2.IsEnabled);
        Assert.Equal(RolloutType.Percentage, flag1.RolloutType);
        Assert.Equal(RolloutType.Percentage, flag2.RolloutType);
    }

    [Fact]
    public void WithPaging_AppliesPagination()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithPaging(1, 2);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void WithPage_AppliesPagination()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithPage(2, 2);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void SortBy_KeyAscending()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .SortBy("Key");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal("feature-a", result[0].Key);
        Assert.Equal("feature-b", result[1].Key);
        Assert.Equal("feature-c", result[2].Key);
        Assert.Equal("feature-d", result[3].Key);
        Assert.Equal("special-feature", result[4].Key);
    }

    [Fact]
    public void SortBy_KeyDescending()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .SortBy("Key", descending: true);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal("special-feature", result[0].Key);
        Assert.Equal("feature-d", result[1].Key);
        Assert.Equal("feature-c", result[2].Key);
        Assert.Equal("feature-b", result[3].Key);
        Assert.Equal("feature-a", result[4].Key);
    }

    [Fact]
    public void SortBy_NameAscending()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .SortBy("Name");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal("Feature A", result[0].DisplayName);
        Assert.Equal("Feature B", result[1].DisplayName);
        Assert.Equal("Feature C", result[2].DisplayName);
        Assert.Equal("Feature D", result[3].DisplayName);
        Assert.Equal("Special Feature", result[4].DisplayName);
    }

    [Fact]
    public void SortBy_EnabledStatus()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .SortBy("Enabled");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.False(result[0].IsEnabled);
        Assert.False(result[1].IsEnabled);
        Assert.True(result[2].IsEnabled);
        Assert.True(result[3].IsEnabled);
        Assert.True(result[4].IsEnabled);
    }

    [Fact]
    public void SortBy_CreatedAt()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .SortBy("Created");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(new DateTime(2024, 1, 1), result[0].CreatedAt);
        Assert.Equal(new DateTime(2024, 1, 2), result[1].CreatedAt);
        Assert.Equal(new DateTime(2024, 1, 3), result[2].CreatedAt);
        Assert.Equal(new DateTime(2024, 1, 4), result[3].CreatedAt);
        Assert.Equal(new DateTime(2024, 1, 5), result[4].CreatedAt);
    }

    [Fact]
    public void SpecialCharacters_InKeyFilter()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithKeyContaining("special-feature");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("special-feature", result[0].Key);
    }

    [Fact]
    public void SpecialCharacters_InNameFilter()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithNameContaining("Special");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("Special Feature", result[0].DisplayName);
    }

    [Fact]
    public void BuilderReuse_ProducesIndependentQueries()
    {
        // Arrange
        var builder1 = new FeatureFlagSearchBuilder()
            .WithKeyContaining("feature-a");
        var builder2 = new FeatureFlagSearchBuilder()
            .WithKeyContaining("feature-b");

        // Act
        var result1 = builder1.Execute(_testFlags.AsQueryable());
        var result2 = builder2.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result1);
        Assert.Equal("feature-a", result1[0].Key);

        Assert.Single(result2);
        Assert.Equal("feature-b", result2[0].Key);
    }

    [Fact]
    public void BuilderReuse_IndependentState()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder();

        // First query
        var result1 = builder
            .WithKeyContaining("feature-a")
            .Execute(_testFlags.AsQueryable());

        // Second query - builder is stateful, so it accumulates filters
        // We need to create a new builder for a fresh query
        var builder2 = new FeatureFlagSearchBuilder();
        var result2 = builder2
            .WithKeyContaining("feature-b")
            .Execute(_testFlags.AsQueryable());

        // Third query - new builder for fresh query
        var builder3 = new FeatureFlagSearchBuilder();
        var result3 = builder3.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result1);
        Assert.Single(result2);
        Assert.Equal(5, result3.Count);
    }

    [Fact]
    public void GetSummary_ReturnsDebugInformation()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithKeyContaining("test")
            .WithNameContaining("Feature")
            .WithEnabledStatus(true)
            .WithRolloutType(RolloutType.Percentage)
            .WithCreatedBy("user1")
            .SortBy("Key", descending: true)
            .WithPaging(10, 50);

        // Act
        var summary = builder.GetSummary();

        // Assert
        Assert.Contains("Key contains 'test'", summary);
        Assert.Contains("Name contains 'Feature'", summary);
        Assert.Contains("IsEnabled = True", summary);
        Assert.Contains("RolloutType = Percentage", summary);
        Assert.Contains("CreatedBy = 'user1'", summary);
        Assert.Contains("Sort: Key DESC", summary);
        Assert.Contains("Paging: Skip 10, Take 50", summary);
    }

    [Fact]
    public void GetSummary_EmptyBuilder()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder();

        // Act
        var summary = builder.GetSummary();

        // Assert
        Assert.Contains("Sort: Key ASC", summary);
        Assert.Contains("Paging: Skip 0, Take 20", summary);
    }

    [Fact]
    public void SearchQueryPresets_AllEnabled()
    {
        // Arrange & Act
        var builder = SearchQueryPresets.AllEnabled();
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, flag => Assert.True(flag.IsEnabled));
    }

    [Fact]
    public void SearchQueryPresets_AllDisabled()
    {
        // Arrange & Act
        var builder = SearchQueryPresets.AllDisabled();
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, flag => Assert.False(flag.IsEnabled));
    }

    [Fact]
    public void SearchQueryPresets_ModifiedInLastDays()
    {
        // Arrange - use a date range that includes our test flags
        var startDate = new DateTime(2023, 12, 1); // Before all test flags
        var endDate = new DateTime(2024, 2, 1); // After all test flags
        var builder = new FeatureFlagSearchBuilder()
            .WithCreatedDateRange(startDate, endDate);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void SearchQueryPresets_AllPercentageRollouts()
    {
        // Arrange & Act
        var builder = SearchQueryPresets.AllPercentageRollouts();
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, flag => Assert.Equal(RolloutType.Percentage, flag.RolloutType));
    }

    [Fact]
    public void SearchQueryPresets_AllABTests()
    {
        // Arrange & Act
        var builder = SearchQueryPresets.AllABTests();
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal(RolloutType.ABTest, result[0].RolloutType);
    }

    [Fact]
    public void SearchQueryPresets_AllRulesBased()
    {
        // Arrange & Act
        var builder = SearchQueryPresets.AllRulesBased();
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal(RolloutType.RulesBased, result[0].RolloutType);
    }

    [Fact]
    public void EmptyStringFilters_AreIgnored()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithKeyContaining("")
            .WithNameContaining(string.Empty)
            .WithCreatedBy("");

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void NullFilters_AreIgnored()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithKeyContaining(null)
            .WithNameContaining(null)
            .WithCreatedBy(null);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void WithPaging_NegativeSkip_ClampedToZero()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithPaging(-5, 10);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void WithPaging_TakeOver1000_ClampedTo1000()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithPaging(0, 2000);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void WithPage_InvalidPageNumber_Handled()
    {
        // Arrange
        var builder = new FeatureFlagSearchBuilder()
            .WithPage(0, 10);

        // Act
        var result = builder.Execute(_testFlags.AsQueryable());

        // Assert
        Assert.Equal(5, result.Count);
    }
}