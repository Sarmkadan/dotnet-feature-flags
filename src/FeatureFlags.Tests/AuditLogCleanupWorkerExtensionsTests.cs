#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for AuditLogCleanupWorkerExtensions
// =====================================================================

using FeatureFlags.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace FeatureFlags.Tests;

/// <summary>
/// Unit tests for AuditLogCleanupWorkerExtensions extension methods.
/// Tests option configuration and extension method behavior.
/// </summary>
public sealed class AuditLogCleanupWorkerExtensionsTests
{
    #region AddAuditLogCleanupWorker (parameterless) Tests

    [Fact]
    public void AddAuditLogCleanupWorker_Parameterless_ConfiguresDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuditLogCleanupWorker();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuditLogCleanupOptions>>().Value;

        Assert.Equal(90, options.RetentionDays);
        Assert.Equal(24, options.CleanupIntervalHours);
        Assert.True(options.Enabled);
    }

    [Fact]
    public void AddAuditLogCleanupWorker_Parameterless_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddAuditLogCleanupWorker());
    }

    [Fact]
    public void AddAuditLogCleanupWorker_Parameterless_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAuditLogCleanupWorker();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(services, result);
    }

    #endregion

    #region AddAuditLogCleanupWorker (with configureOptions) Tests

    [Fact]
    public void AddAuditLogCleanupWorker_WithConfigureOptions_ConfiguresCustomOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<AuditLogCleanupOptions> configureOptions = options =>
        {
            options.RetentionDays = 30;
            options.CleanupIntervalHours = 12;
            options.Enabled = false;
        };

        // Act
        services.AddAuditLogCleanupWorker(configureOptions);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuditLogCleanupOptions>>().Value;

        Assert.Equal(30, options.RetentionDays);
        Assert.Equal(12, options.CleanupIntervalHours);
        Assert.False(options.Enabled);
    }

    [Fact]
    public void AddAuditLogCleanupWorker_WithConfigureOptions_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        Action<AuditLogCleanupOptions> configureOptions = _ => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddAuditLogCleanupWorker(configureOptions));
    }

    [Fact]
    public void AddAuditLogCleanupWorker_WithConfigureOptions_WithNullConfigureOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<AuditLogCleanupOptions>? configureOptions = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddAuditLogCleanupWorker(configureOptions!));
    }

    [Fact]
    public void AddAuditLogCleanupWorker_WithConfigureOptions_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<AuditLogCleanupOptions> configureOptions = _ => { };

        // Act
        var result = services.AddAuditLogCleanupWorker(configureOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(services, result);
    }

    #endregion

    #region WithRetentionDays Tests

    [Fact]
    public void WithRetentionDays_SetsRetentionDaysAndReturnsOptions()
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        var result = options.WithRetentionDays(60);

        // Assert
        Assert.Equal(60, options.RetentionDays);
        Assert.Equal(60, result.RetentionDays);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithRetentionDays_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogCleanupOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options!.WithRetentionDays(30));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(365)]
    public void WithRetentionDays_BoundaryValues_AcceptsAll(int retentionDays)
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        var result = options.WithRetentionDays(retentionDays);

        // Assert
        Assert.Equal(retentionDays, options.RetentionDays);
        Assert.Equal(retentionDays, result.RetentionDays);
    }

    #endregion

    #region WithCleanupIntervalHours Tests

    [Fact]
    public void WithCleanupIntervalHours_SetsCleanupIntervalAndReturnsOptions()
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        var result = options.WithCleanupIntervalHours(48);

        // Assert
        Assert.Equal(48, options.CleanupIntervalHours);
        Assert.Equal(48, result.CleanupIntervalHours);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithCleanupIntervalHours_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogCleanupOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options!.WithCleanupIntervalHours(12));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(168)] // 7 days
    public void WithCleanupIntervalHours_BoundaryValues_AcceptsAll(int intervalHours)
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        var result = options.WithCleanupIntervalHours(intervalHours);

        // Assert
        Assert.Equal(intervalHours, options.CleanupIntervalHours);
        Assert.Equal(intervalHours, result.CleanupIntervalHours);
    }

    #endregion

    #region WithEnabled Tests

    [Fact]
    public void WithEnabled_SetsEnabledAndReturnsOptions()
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        var result = options.WithEnabled(false);

        // Assert
        Assert.False(options.Enabled);
        Assert.False(result.Enabled);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithEnabled_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogCleanupOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options!.WithEnabled(true));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WithEnabled_WorksWithAnyBooleanValue(bool enabled)
    {
        // Arrange
        var options = new AuditLogCleanupOptions();

        // Act
        var result = options.WithEnabled(enabled);

        // Assert
        Assert.Equal(enabled, options.Enabled);
        Assert.Equal(enabled, result.Enabled);
    }

    #endregion

    #region GetCleanupIntervalSeconds Tests

    [Fact]
    public void GetCleanupIntervalSeconds_ReturnsCorrectIntervalInSeconds()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { CleanupIntervalHours = 2 };
        var worker = new AuditLogCleanupWorker(
            new FakeServiceProvider(),
            new FakeLogger<AuditLogCleanupWorker>(),
            options);

        // Act
        var result = worker.GetCleanupIntervalSeconds();

        // Assert
        Assert.Equal(7200, result); // 2 * 3600
    }

    [Fact]
    public void GetCleanupIntervalSeconds_WithZeroHours_ReturnsZero()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { CleanupIntervalHours = 0 };
        var worker = new AuditLogCleanupWorker(
            new FakeServiceProvider(),
            new FakeLogger<AuditLogCleanupWorker>(),
            options);

        // Act
        var result = worker.GetCleanupIntervalSeconds();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetCleanupIntervalSeconds_WithLargeHours_ReturnsCorrectValue()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { CleanupIntervalHours = 720 };
        var worker = new AuditLogCleanupWorker(
            new FakeServiceProvider(),
            new FakeLogger<AuditLogCleanupWorker>(),
            options);

        // Act
        var result = worker.GetCleanupIntervalSeconds();

        // Assert
        Assert.Equal(2592000, result); // 720 * 3600
    }

    [Fact]
    public void GetCleanupIntervalSeconds_WithNullWorker_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogCleanupWorker? worker = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => worker!.GetCleanupIntervalSeconds());
    }

    #endregion

    #region GetRetentionDays Tests

    [Fact]
    public void GetRetentionDays_ReturnsCorrectRetentionPeriod()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { RetentionDays = 45 };
        var worker = new AuditLogCleanupWorker(
            new FakeServiceProvider(),
            new FakeLogger<AuditLogCleanupWorker>(),
            options);

        // Act
        var result = worker.GetRetentionDays();

        // Assert
        Assert.Equal(45, result);
    }

    [Fact]
    public void GetRetentionDays_WithZeroDays_ReturnsZero()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { RetentionDays = 0 };
        var worker = new AuditLogCleanupWorker(
            new FakeServiceProvider(),
            new FakeLogger<AuditLogCleanupWorker>(),
            options);

        // Act
        var result = worker.GetRetentionDays();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetRetentionDays_WithMaximumDays_ReturnsCorrectValue()
    {
        // Arrange
        var options = new AuditLogCleanupOptions { RetentionDays = 3650 };
        var worker = new AuditLogCleanupWorker(
            new FakeServiceProvider(),
            new FakeLogger<AuditLogCleanupWorker>(),
            options);

        // Act
        var result = worker.GetRetentionDays();

        // Assert
        Assert.Equal(3650, result);
    }

    [Fact]
    public void GetRetentionDays_WithNullWorker_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLogCleanupWorker? worker = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => worker!.GetRetentionDays());
    }

    #endregion

    #region Fake Implementations

    private sealed class FakeServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    #endregion
}
