// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Caching;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Unit tests for the in-memory cache service implementation.
/// Tests cache operations including get, set, remove, and expiration handling.
/// </summary>
public class CacheServiceTests
{
    private readonly ILogger<InMemoryCacheService> _logger;

    public CacheServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<InMemoryCacheService>();
    }

    [Fact]
    public void Set_And_Get_ReturnsValue()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger);
        var key = "test_key";
        var value = "test_value";

        // Act
        cache.Set(key, value);
        var result = cache.Get<string>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result);
    }

    [Fact]
    public void Get_NonExistentKey_ReturnsNull()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger);

        // Act
        var result = cache.Get<string>("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Remove_DeletesValue()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger);
        var key = "test_key";
        cache.Set(key, "test_value");

        // Act
        cache.Remove(key);
        var result = cache.Get<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Set_WithCustomTtl_ExpiresAfterTimeout()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger, TimeSpan.FromSeconds(1));
        var key = "test_key";

        // Act
        cache.Set(key, "test_value", TimeSpan.FromMilliseconds(100));
        var resultBefore = cache.Get<string>(key);

        // Wait for expiration
        System.Threading.Thread.Sleep(150);
        var resultAfter = cache.Get<string>(key);

        // Assert
        Assert.NotNull(resultBefore);
        Assert.Null(resultAfter);
    }

    [Fact]
    public void Set_ComplexObject_StoresAndRetrieves()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger);
        var key = "complex_key";
        var obj = new { Id = 1, Name = "Test", Active = true };

        // Act
        cache.Set(key, obj);
        var result = cache.Get<object>(key);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_Works()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger);
        var key = "async_key";
        var value = "async_value";

        // Act
        await cache.SetAsync(key, value);
        var result = await cache.GetAsync<string>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task Clear_RemovesAllEntries()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger);
        cache.Set("key1", "value1");
        cache.Set("key2", "value2");

        // Act
        await cache.ClearAsync();
        var result1 = cache.Get<string>("key1");
        var result2 = cache.Get<string>("key2");

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public void Set_NullKey_DoesNotThrow()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger);

        // Act & Assert - Should not throw
        cache.Set(null, "value");
        cache.Set(string.Empty, "value");
    }

    [Fact]
    public void Set_OverwritesExistingKey()
    {
        // Arrange
        var cache = new InMemoryCacheService(_logger);
        var key = "test_key";

        // Act
        cache.Set(key, "value1");
        cache.Set(key, "value2");
        var result = cache.Get<string>(key);

        // Assert
        Assert.Equal("value2", result);
    }
}

/// <summary>
/// Unit tests for the cache-related utilities and helpers.
/// </summary>
public class CacheUtilityTests
{
    [Fact]
    public void ValidateAndNormalizePaging_OutOfRangePage_NormalizesCorrectly()
    {
        // Arrange & Act
        var (page, size) = Utilities.PaginationHelper.ValidateAndNormalizePaging(-1, 0);

        // Assert
        Assert.Equal(1, page);
        Assert.Equal(Utilities.PaginationHelper.MinPageSize, size);
    }

    [Fact]
    public void ValidateAndNormalizePaging_MaxPageSize_ClampedCorrectly()
    {
        // Arrange & Act
        var (page, size) = Utilities.PaginationHelper.ValidateAndNormalizePaging(1, 5000);

        // Assert
        Assert.Equal(1, page);
        Assert.Equal(Utilities.PaginationHelper.MaxPageSize, size);
    }

    [Fact]
    public void CalculateOffset_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        Assert.Equal(0, Utilities.PaginationHelper.CalculateOffset(1, 20));
        Assert.Equal(20, Utilities.PaginationHelper.CalculateOffset(2, 20));
        Assert.Equal(40, Utilities.PaginationHelper.CalculateOffset(3, 20));
    }

    [Fact]
    public void CreateMetadata_CalculatesCorrectly()
    {
        // Arrange & Act
        var metadata = Utilities.PaginationHelper.CreateMetadata(2, 10, 35);

        // Assert
        Assert.Equal(2, metadata.PageNumber);
        Assert.Equal(10, metadata.PageSize);
        Assert.Equal(35, metadata.TotalCount);
        Assert.Equal(4, metadata.TotalPages);
        Assert.True(metadata.HasNextPage);
        Assert.True(metadata.HasPreviousPage);
    }
}
