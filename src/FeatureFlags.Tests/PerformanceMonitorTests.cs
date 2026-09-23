#nullable enable

using FeatureFlags.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FeatureFlags.Tests;

public class PerformanceMonitorTests
{
    private readonly Mock<ILogger<PerformanceMonitor>> _mockLogger = new();

    [Fact]
    public void Constructor_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PerformanceMonitor(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PerformanceMonitor("test", null!));
    }

    [Fact]
    public void Constructor_StartsStopwatchAndLogsStart()
    {
        // Arrange
        const string operationName = "TestOperation";

        // Act
        var monitor = new PerformanceMonitor(operationName, _mockLogger.Object);

        // Assert
        Assert.InRange(monitor.ElapsedMilliseconds, 0L, 100L); // Should have started recently
        // Verify that LogInformation was called with the correct starting message
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Starting {operationName}")),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public void Stop_WhenNotDisposed_StopsStopwatchAndLogsResult()
    {
        // Arrange
        var monitor = new PerformanceMonitor("TestOperation", _mockLogger.Object);
        // Let it run for a bit
        System.Threading.Thread.Sleep(10);

        // Act
        monitor.Stop();

        // Assert
        Assert.True(monitor.ElapsedMilliseconds >= 10);
        // Verify that LogInformation was called for stopping
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Stopping TestOperation")),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
        // Verify that some logging happened from LogResult (either warning or debug)
        _mockLogger.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Operation completed: TestOperation in") ||
                    v.ToString()!.Contains("Performance warning: TestOperation took")),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void Stop_WhenAlreadyDisposed_DoesNothing()
    {
        // Arrange
        var monitor = new PerformanceMonitor("TestOperation", _mockLogger.Object);
        monitor.Dispose();

        // Act
        monitor.Stop();

        // Assert - Should not log anything additional for stopping
        // Count invocations before Stop
        var invocationsBefore = _mockLogger.Invocations.Count(i =>
            i.Method.Name == "Log" &&
            i.Arguments.Count > 2 &&
            i.Arguments[2] is not null &&
            i.Arguments[2].ToString()!.Contains("Stopping TestOperation"));

        // Act
        monitor.Stop();

        // Assert - No additional stopping invocations should have been added
        var invocationsAfter = _mockLogger.Invocations.Count(i =>
            i.Method.Name == "Log" &&
            i.Arguments.Count > 2 &&
            i.Arguments[2] is not null &&
            i.Arguments[2].ToString()!.Contains("Stopping TestOperation"));

        Assert.Equal(invocationsBefore, invocationsAfter);
    }

    [Fact]
    public void Dispose_WhenNotDisposed_StopsStopwatchAndLogsResult()
    {
        // Arrange
        var monitor = new PerformanceMonitor("TestOperation", _mockLogger.Object);
        System.Threading.Thread.Sleep(10);

        // Act
        monitor.Dispose();

        // Assert
        Assert.True(monitor.ElapsedMilliseconds >= 10);
        // Verify that LogInformation was called for stopping
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Stopping TestOperation")),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public void Dispose_WhenAlreadyDisposed_DoesNothing()
    {
        // Arrange
        var monitor = new PerformanceMonitor("TestOperation", _mockLogger.Object);
        monitor.Dispose();

        // Act
        monitor.Dispose();

        // Assert - Should not log anything additional
        // Count invocations before Dispose
        var invocationsBefore = _mockLogger.Invocations.Count(i =>
            i.Method.Name == "Log" &&
            i.Arguments.Count > 2 &&
            i.Arguments[2] is not null &&
            i.Arguments[2].ToString()!.Contains("Stopping TestOperation"));

        // Act
        monitor.Dispose();

        // Assert - No additional stopping invocations should have been added
        var invocationsAfter = _mockLogger.Invocations.Count(i =>
            i.Method.Name == "Log" &&
            i.Arguments.Count > 2 &&
            i.Arguments[2] is not null &&
            i.Arguments[2].ToString()!.Contains("Stopping TestOperation"));

        Assert.Equal(invocationsBefore, invocationsAfter);
    }

    [Fact]
    public void Measure_ExecutesOperationAndReturnsResult()
    {
        // Arrange
        const string operationName = "MeasureTest";
        Func<int> operation = () => 42;

        // Act
        var result = PerformanceMonitor.Measure(operationName, operation, _mockLogger.Object);

        // Assert
        Assert.Equal(42, result);
        // Verify that LogInformation was called for starting
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Starting {operationName}")),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task MeasureAsync_ExecutesAsyncOperationAndReturnsResult()
    {
        // Arrange
        const string operationName = "MeasureAsyncTest";
        Func<Task<int>> operation = async () =>
        {
            await Task.Yield();
            return 123;
        };

        // Act
        var result = await PerformanceMonitor.MeasureAsync(operationName, operation, _mockLogger.Object);

        // Assert
        Assert.Equal(123, result);
        // Verify that LogInformation was called for starting
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Starting {operationName}")),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task MeasureAsync_Void_ExecutesAsyncOperation()
    {
        // Arrange
        const string operationName = "MeasureAsyncVoidTest";
        bool executed = false;
        Func<Task> operation = async () =>
        {
            await Task.Yield();
            executed = true;
        };

        // Act
        await PerformanceMonitor.MeasureAsync(operationName, operation, _mockLogger.Object);

        // Assert
        Assert.True(executed);
        // Verify that LogInformation was called for starting
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Starting {operationName}")),
                null,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }
}