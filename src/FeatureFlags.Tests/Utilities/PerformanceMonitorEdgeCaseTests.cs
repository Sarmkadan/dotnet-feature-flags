// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FeatureFlags.Tests.Utilities;

public class PerformanceMonitorEdgeCaseTests
{
    private readonly ILogger<PerformanceMonitor> _logger;

    public PerformanceMonitorEdgeCaseTests()
    {
        _logger = LoggerFactory.Create(b => { }).CreateLogger<PerformanceMonitor>();
    }

    [Fact]
    public void Constructor_NullOperationName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PerformanceMonitor(null!, _logger));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PerformanceMonitor("test", null!));
    }

    [Fact]
    public void ElapsedMilliseconds_ImmediatelyAfterCreation_IsSmall()
    {
        var monitor = new PerformanceMonitor("test", _logger);
        Assert.True(monitor.ElapsedMilliseconds < 1000);
    }

    [Fact]
    public void Stop_CalledMultipleTimes_DoesNotThrow()
    {
        var monitor = new PerformanceMonitor("test", _logger);
        monitor.Stop();
        monitor.Stop();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var monitor = new PerformanceMonitor("test", _logger);
        monitor.Dispose();
        monitor.Dispose();
    }

    [Fact]
    public void Stop_FreezesElapsedTime()
    {
        var monitor = new PerformanceMonitor("test", _logger, warningThresholdMs: 100000);
        Thread.Sleep(10);
        monitor.Stop();
        var afterStop = monitor.ElapsedMilliseconds;
        Thread.Sleep(20);
        Assert.Equal(afterStop, monitor.ElapsedMilliseconds);
    }

    [Fact]
    public void CustomThreshold_AcceptsCustomValue()
    {
        var monitor = new PerformanceMonitor("fast-op", _logger, warningThresholdMs: 50);
        Assert.NotNull(monitor);
    }
}
