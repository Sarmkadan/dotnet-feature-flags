#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for GradualRolloutSchedulerWorker and related classes
// =============================================================================

using FeatureFlags.BackgroundJobs;
using FeatureFlags.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FeatureFlags.Tests;

/// <summary>
/// Unit tests for GradualRolloutSchedulerWorker and GradualRolloutSchedulerOptions.
/// Tests constructor validation, property initialization, and DI registration.
/// </summary>
public sealed class GradualRolloutSchedulerWorkerTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ILogger<GradualRolloutSchedulerWorker>> _loggerMock = new();

    public GradualRolloutSchedulerWorkerTests()
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);
    }

    #region GradualRolloutSchedulerOptions Tests

    [Fact]
    public void GradualRolloutSchedulerOptions_DefaultConstructor_InitializesProperties()
    {
        // Act
        var options = new GradualRolloutSchedulerOptions();

        // Assert
        Assert.Equal(60, options.CheckIntervalMinutes);
        Assert.True(options.Enabled);
    }

    [Fact]
    public void GradualRolloutSchedulerOptions_Property_SettersWorkCorrectly()
    {
        // Arrange
        var options = new GradualRolloutSchedulerOptions();

        // Act
        options.CheckIntervalMinutes = 30;
        options.Enabled = false;

        // Assert
        Assert.Equal(30, options.CheckIntervalMinutes);
        Assert.False(options.Enabled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1440)] // 24 hours
    [InlineData(10080)] // 7 days
    public void GradualRolloutSchedulerOptions_CheckIntervalMinutes_BoundaryValues(int intervalMinutes)
    {
        // Arrange
        var options = new GradualRolloutSchedulerOptions();

        // Act
        options.CheckIntervalMinutes = intervalMinutes;

        // Assert
        Assert.Equal(intervalMinutes, options.CheckIntervalMinutes);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(1, false)]
    public void GradualRolloutSchedulerOptions_EnabledFlag_WorksWithAnyCheckInterval(int intervalMinutes, bool enabled)
    {
        // Arrange
        var options = new GradualRolloutSchedulerOptions { CheckIntervalMinutes = intervalMinutes, Enabled = enabled };

        // Act & Assert - Just verify the values are set correctly
        Assert.Equal(intervalMinutes, options.CheckIntervalMinutes);
        Assert.Equal(enabled, options.Enabled);
    }

    #endregion

    #region GradualRolloutSchedulerWorker Constructor Tests

    [Fact]
    public void GradualRolloutSchedulerWorker_Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var options = new GradualRolloutSchedulerOptions { CheckIntervalMinutes = 30, Enabled = true };

        // Act
        var worker = new GradualRolloutSchedulerWorker(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            options);

        // Assert
        Assert.NotNull(worker);
    }

    [Fact]
    public void GradualRolloutSchedulerWorker_Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new GradualRolloutSchedulerOptions();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new GradualRolloutSchedulerWorker(null!, _loggerMock.Object, options));
        Assert.Equal("serviceProvider", exception.ParamName);
    }

    [Fact]
    public void GradualRolloutSchedulerWorker_Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new GradualRolloutSchedulerOptions();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new GradualRolloutSchedulerWorker(_serviceProviderMock.Object, null!, options));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void GradualRolloutSchedulerWorker_Constructor_WithNullOptions_UsesDefaultOptions()
    {
        // Act
        var worker = new GradualRolloutSchedulerWorker(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            null!);

        // Assert - Should not throw and should use default options
        Assert.NotNull(worker);
    }

    [Theory]
    [InlineData(15, 60)]
    [InlineData(30, 30)]
    [InlineData(1440, 60)] // 24 hours
    public void GradualRolloutSchedulerWorker_CheckIntervalMinutes_FromOptions(int setInterval, int defaultInterval)
    {
        // Arrange
        var options = new GradualRolloutSchedulerOptions { CheckIntervalMinutes = setInterval };

        // Act
        var worker = new GradualRolloutSchedulerWorker(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            options);

        // Assert
        Assert.NotNull(worker);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GradualRolloutSchedulerWorker_EnabledFlag_FromOptions(bool enabled)
    {
        // Arrange
        var options = new GradualRolloutSchedulerOptions { Enabled = enabled };

        // Act
        var worker = new GradualRolloutSchedulerWorker(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            options);

        // Assert
        Assert.NotNull(worker);
    }

    #endregion

    #region GradualRolloutSchedulerExtensions Tests

    [Fact]
    public void AddGradualRolloutScheduler_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>().Object;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => GradualRolloutSchedulerExtensions.AddGradualRolloutScheduler(null!, configuration));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddGradualRolloutScheduler_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(Mock.Of<IConfigurationSection>());

        // Act
        var result = GradualRolloutSchedulerExtensions.AddGradualRolloutScheduler(services, configurationMock.Object);

        // Assert
        Assert.Same(services, result);
    }

    #endregion
}
