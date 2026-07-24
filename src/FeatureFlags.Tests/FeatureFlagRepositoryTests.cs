#nullable enable

using FeatureFlags.Data;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace FeatureFlags.Tests;

public class FeatureFlagRepositoryTests
{
    private readonly Mock<FeatureFlagDbContext> _mockContext;
    private readonly Mock<ILogger<FeatureFlagRepository>> _mockLogger;
    private readonly FeatureFlagRepository _repository;
    private readonly List<FeatureFlag> _testFlags;

    public FeatureFlagRepositoryTests()
    {
        _mockContext = new Mock<FeatureFlagDbContext>();
        _mockLogger = new Mock<ILogger<FeatureFlagRepository>>();
        _repository = new FeatureFlagRepository(_mockContext.Object, _mockLogger.Object);

        _testFlags = new List<FeatureFlag>
        {
            new FeatureFlag
            {
                Id = 1,
                Key = "feature-1",
                DisplayName = "Feature One",
                Description = "First test feature",
                IsEnabled = true,
                RolloutType = Enums.RolloutType.Percentage,
                PercentageRollout = 50,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "user1",
                UpdatedBy = "user1"
            },
            new FeatureFlag
            {
                Id = 2,
                Key = "feature-2",
                DisplayName = "Feature Two",
                Description = "Second test feature",
                IsEnabled = false,
                RolloutType = Enums.RolloutType.None,
                PercentageRollout = null,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "user2",
                UpdatedBy = "user2"
            },
            new FeatureFlag
            {
                Id = 3,
                Key = "feature-3",
                DisplayName = "Feature Three",
                Description = "Third test feature",
                IsEnabled = true,
                RolloutType = Enums.RolloutType.Percentage,
                PercentageRollout = 100,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedBy = "user1",
                UpdatedBy = "user1"
            }
        };
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsFeatureFlag()
    {
        // Arrange
        var expectedFlag = _testFlags[0];
        var mockSet = CreateMockDbSet(new List<FeatureFlag> { expectedFlag });
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedFlag);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeyAsync_WithExistingKey_ReturnsFeatureFlag()
    {
        // Arrange
        var expectedFlag = _testFlags[1];
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetByKeyAsync("feature-2");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedFlag);
    }

    [Fact]
    public async Task GetByKeyAsync_WithNonExistingKey_ReturnsNull()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetByKeyAsync("non-existing-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeyAsync_WithNullKey_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByKeyAsync(null!));
    }

    [Fact]
    public async Task GetByKeyAsync_WithEmptyKey_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByKeyAsync(""));
    }

    [Fact]
    public async Task GetByKeyAsync_WithWhitespaceKey_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByKeyAsync("   "));
    }

    [Fact]
    public async Task GetAllAsync_WithFlags_ReturnsAllFlags()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(_testFlags);
    }

    [Fact]
    public async Task GetAllAsync_WithNoFlags_ReturnsEmptyCollection()
    {
        // Arrange
        var mockSet = CreateMockDbSet(new List<FeatureFlag>());
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEnabledAsync_WithEnabledFlags_ReturnsOnlyEnabledFlags()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetEnabledAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(f => f.IsEnabled.Should().BeTrue());
    }

    [Fact]
    public async Task GetEnabledAsync_WithNoEnabledFlags_ReturnsEmptyCollection()
    {
        // Arrange
        var disabledFlags = new List<FeatureFlag>
        {
            new FeatureFlag { Id = 1, Key = "f1", IsEnabled = false },
            new FeatureFlag { Id = 2, Key = "f2", IsEnabled = false }
        };
        var mockSet = CreateMockDbSet(disabledFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetEnabledAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCreatorAsync_WithExistingCreator_ReturnsFlagsOrderedByDateDescending()
    {
        // Arrange
        var expectedFlags = new List<FeatureFlag>
        {
            new FeatureFlag
            {
                Id = 1,
                Key = "f1",
                CreatedBy = "user1",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new FeatureFlag
            {
                Id = 2,
                Key = "f2",
                CreatedBy = "user1",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new FeatureFlag
            {
                Id = 3,
                Key = "f3",
                CreatedBy = "user1",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };
        var mockSet = CreateMockDbSet(expectedFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetByCreatorAsync("user1");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(f => f.CreatedAt);
    }

    [Fact]
    public async Task GetByCreatorAsync_WithNonExistingCreator_ReturnsEmptyCollection()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetByCreatorAsync("non-existing-user");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCreatorAsync_WithNullCreator_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByCreatorAsync(null!));
    }

    [Fact]
    public async Task GetByCreatorAsync_WithEmptyCreator_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByCreatorAsync(""));
    }

    [Fact]
    public async Task GetModifiedSinceAsync_WithRecentDate_ReturnsRecentlyModifiedFlags()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-1);
        var expectedFlags = new List<FeatureFlag>
        {
            new FeatureFlag
            {
                Id = 1,
                Key = "f1",
                UpdatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new FeatureFlag
            {
                Id = 2,
                Key = "f2",
                UpdatedAt = DateTime.UtcNow.AddHours(-6)
            }
        };
        var mockSet = CreateMockDbSet(expectedFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetModifiedSinceAsync(cutoffDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(f => f.UpdatedAt.Should().BeAfter(cutoffDate));
    }

    [Fact]
    public async Task GetModifiedSinceAsync_WithOldDate_ReturnsAllFlags()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddYears(-1);
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetModifiedSinceAsync(cutoffDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetModifiedSinceAsync_WithNoModifiedFlags_ReturnsEmptyCollection()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(1);
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetModifiedSinceAsync(cutoffDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetTotalCountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetPagedAsync_WithValidParameters_ReturnsCorrectPage()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetPagedAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(f => f.CreatedAt);
    }

    [Fact]
    public async Task GetPagedAsync_WithPageTwo_ReturnsSecondPage()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetPagedAsync(2, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_WithPageNumberLessThanOne_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetPagedAsync(0, 10));
    }

    [Fact]
    public async Task GetPagedAsync_WithPageSizeLessThanOne_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetPagedAsync(1, 0));
    }

    [Fact]
    public async Task GetPagedAsync_WithPageSizeLargerThanCollection_ReturnsAllItems()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetPagedAsync(1, 100);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task KeyExistsAsync_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.KeyExistsAsync("feature-1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task KeyExistsAsync_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.KeyExistsAsync("non-existing-key");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task KeyExistsAsync_WithNullKey_ReturnsFalse()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.KeyExistsAsync(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task KeyExistsAsync_WithEmptyKey_ReturnsFalse()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.KeyExistsAsync("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task KeyExistsAsync_WithWhitespaceKey_ReturnsFalse()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.KeyExistsAsync("   ");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetRecentlyModifiedAsync_WithPositiveCount_ReturnsMostRecentFlags()
    {
        // Arrange
        var expectedFlags = new List<FeatureFlag>
        {
            new FeatureFlag { Id = 1, Key = "f1", UpdatedAt = DateTime.UtcNow.AddDays(-3) },
            new FeatureFlag { Id = 2, Key = "f2", UpdatedAt = DateTime.UtcNow.AddDays(-1) },
            new FeatureFlag { Id = 3, Key = "f3", UpdatedAt = DateTime.UtcNow.AddDays(-2) }
        };
        var mockSet = CreateMockDbSet(expectedFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetRecentlyModifiedAsync(2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(f => f.UpdatedAt);
    }

    [Fact]
    public async Task GetRecentlyModifiedAsync_WithCountOne_ReturnsSingleMostRecentFlag()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetRecentlyModifiedAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetRecentlyModifiedAsync_WithCountZero_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetRecentlyModifiedAsync(0));
    }

    [Fact]
    public async Task GetStaleFlagsAsync_WithPositiveTimeSpan_ReturnsStaleFlags()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-1);
        var expectedFlags = new List<FeatureFlag>
        {
            new FeatureFlag { Id = 1, Key = "f1", UpdatedAt = DateTime.UtcNow.AddDays(-3) },
            new FeatureFlag { Id = 2, Key = "f2", UpdatedAt = DateTime.UtcNow.AddDays(-2) },
            new FeatureFlag { Id = 3, Key = "f3", UpdatedAt = DateTime.UtcNow.AddHours(-12) }
        };
        var mockSet = CreateMockDbSet(expectedFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetStaleFlagsAsync(TimeSpan.FromDays(2));

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(f => f.UpdatedAt.Should().BeBefore(cutoffDate));
        result.Should().BeInAscendingOrder(f => f.UpdatedAt);
    }

    [Fact]
    public async Task GetStaleFlagsAsync_WithZeroTimeSpan_ReturnsAllFlags()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.GetStaleFlagsAsync(TimeSpan.Zero);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetStaleFlagsAsync_WithNegativeTimeSpan_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetStaleFlagsAsync(TimeSpan.FromDays(-1)));
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        var mockSet = CreateMockDbSet(_testFlags);
        _mockContext.Setup(c => c.FeatureFlags).Returns(mockSet.Object);

        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType).Returns(queryable.ElementType);
        return mockSet;
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })!
            .MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }
}