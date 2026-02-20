#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace FeatureFlags.Utilities;

/// <summary>
/// Performance monitoring utility for measuring operation execution time and tracking metrics.
/// Helps identify performance bottlenecks and track service performance over time.
/// </summary>
{public sealed class PerformanceMonitor {
    private readonly Stopwatch _stopwatch;
    private readonly string _operationName;
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly long _warningThresholdMs;
    private bool _disposed;

    public PerformanceMonitor(string operationName, ILogger<PerformanceMonitor> logger, long warningThresholdMs = 1000)
    {
        _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _warningThresholdMs = warningThresholdMs;
        _stopwatch = Stopwatch.StartNew();
    }

    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    public void Stop()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            LogResult();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            LogResult();
            _disposed = true;
        }
    }

    private void LogResult()
    {
        var elapsedMs = _stopwatch.ElapsedMilliseconds;

        if (elapsedMs > _warningThresholdMs)
        {
            _logger.LogWarning("Performance warning: {Operation} took {ElapsedMs}ms (threshold: {Threshold}ms)",
                _operationName, elapsedMs, _warningThresholdMs);
        }
        else
        {
            _logger.LogDebug("Operation completed: {Operation} in {ElapsedMs}ms",
                _operationName, elapsedMs);
        }
    }

    /// <summary>
    /// Creates a performance monitor and executes a synchronous operation, returning the result.
    /// </summary>
    public static T Measure<T>(string operationName, Func<T> operation, ILogger<PerformanceMonitor> logger, long warningThresholdMs = 1000)
    {
        using var monitor = new PerformanceMonitor(operationName, logger, warningThresholdMs);
        return operation();
    }

    /// <summary>
    /// Creates a performance monitor and executes an async operation, returning the result.
    /// </summary>
    public static async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation, ILogger<PerformanceMonitor> logger, long warningThresholdMs = 1000)
    {
        using var monitor = new PerformanceMonitor(operationName, logger, warningThresholdMs);
        return await operation();
    }

    /// <summary>
    /// Creates a performance monitor and executes an async operation without return value.
    /// </summary>
    public static async Task MeasureAsync(string operationName, Func<Task> operation, ILogger<PerformanceMonitor> logger, long warningThresholdMs = 1000)
    {
        using var monitor = new PerformanceMonitor(operationName, logger, warningThresholdMs);
        await operation();
    }
}

/// <summary>
/// Collects and aggregates performance metrics over time.
/// </summary>
public class PerformanceMetrics
{
    private readonly Dictionary<string, List<long>> _metrics = new();
    private readonly object _lockObj = new();

    public void RecordOperation(string operationName, long elapsedMilliseconds)
    {
        lock (_lockObj)
        {
            if (!_metrics.ContainsKey(operationName))
            {
                _metrics[operationName] = new List<long>();
            }

            _metrics[operationName].Add(elapsedMilliseconds);

            // Keep only last 1000 measurements
            if (_metrics[operationName].Count > 1000)
            {
                _metrics[operationName].RemoveRange(0, _metrics[operationName].Count - 1000);
            }
        }
    }

    /// <summary>
    /// Gets statistics for an operation.
    /// </summary>
    public OperationStats? GetStatistics(string operationName)
    {
        lock (_lockObj)
        {
            if (!_metrics.TryGetValue(operationName, out var measurements) || measurements.Count == 0)
            {
                return null;
            }

            var sorted = measurements.OrderBy(m => m).ToList();
            var avg = (long)measurements.Average();
            var min = sorted.First();
            var max = sorted.Last();
            var p95 = sorted[(int)(sorted.Count * 0.95)];
            var p99 = sorted[(int)(sorted.Count * 0.99)];

            return new OperationStats
            {
                OperationName = operationName,
                CallCount = measurements.Count,
                AverageMs = avg,
                MinMs = min,
                MaxMs = max,
                P95Ms = p95,
                P99Ms = p99
            };
        }
    }

    /// <summary>
    /// Gets statistics for all operations.
    /// </summary>
    public List<OperationStats> GetAllStatistics()
    {
        lock (_lockObj)
        {
            return _metrics.Keys
                .Select(key => GetStatistics(key))
                .Where(s => s is not null)
                .Cast<OperationStats>()
                .ToList();
        }
    }

    public void Clear()
    {
        lock (_lockObj)
        {
            _metrics.Clear();
        }
    }
}

/// <summary>
/// Statistics for a specific operation.
/// </summary>
public class OperationStats
{
    public string OperationName { get; set; } = string.Empty;
    public int CallCount { get; set; }
    public long AverageMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public long P95Ms { get; set; }
    public long P99Ms { get; set; }
}
