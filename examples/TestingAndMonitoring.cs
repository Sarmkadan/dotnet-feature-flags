#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.Models;
using FeatureFlags.Services;

/// <summary>
/// Example: Testing feature flags and monitoring their behavior in production.
/// This covers unit testing patterns, integration testing, and performance monitoring.
/// </summary>

// Unit Testing Example
public class FeatureFlagServiceTestExample
{
    public void TestPercentageRollout()
    {
        // Arrange
        var mockRepository = new MockFeatureFlagRepository();
        var service = new FeatureFlagService(mockRepository, null!);

        var flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Key = "test-flag",
            PercentageRollout = 50,
            IsEnabled = true
        };

        // Act
        var context = new UserContext { UserId = "test-user-001" };

        // Assert
        // With consistent hashing, the same user should always get the same result
        // Test runs multiple times to verify consistency
    }

    public void TestRuleBasedEvaluation()
    {
        // Test that rules are evaluated correctly
    }

    public void TestABTestVariantAssignment()
    {
        // Test that variants are assigned consistently
    }
}

// Performance Monitoring Example
public class FeatureFlagPerformanceMonitor
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly List<EvaluationMetric> _metrics = new();

    public FeatureFlagPerformanceMonitor(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public async Task MonitorEvaluationPerformanceAsync(string flagKey, int iterations = 1000)
    {
        Console.WriteLine($"\n=== Performance Monitoring: {flagKey} ===\n");

        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;
        var errorCount = 0;
        var timings = new List<long>();

        for (int i = 0; i < iterations; i++)
        {
            var context = new UserContext { UserId = $"user{i:D6}" };
            var iterationStopwatch = Stopwatch.StartNew();

            try
            {
                var result = await _featureFlagService.IsEnabledAsync(flagKey, context);
                iterationStopwatch.Stop();

                timings.Add(iterationStopwatch.ElapsedMilliseconds);
                successCount++;
            }
            catch
            {
                errorCount++;
            }
        }

        stopwatch.Stop();

        // Calculate statistics
        var avgTime = timings.Average();
        var minTime = timings.Min();
        var maxTime = timings.Max();
        var p95Time = GetPercentile(timings, 0.95);
        var p99Time = GetPercentile(timings, 0.99);
        var throughput = iterations / stopwatch.Elapsed.TotalSeconds;

        Console.WriteLine($"Iterations: {iterations}");
        Console.WriteLine($"Success: {successCount}, Errors: {errorCount}");
        Console.WriteLine($"Total Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"\nResponse Times:");
        Console.WriteLine($"  Average: {avgTime:F2}ms");
        Console.WriteLine($"  Min: {minTime}ms");
        Console.WriteLine($"  Max: {maxTime}ms");
        Console.WriteLine($"  P95: {p95Time:F2}ms");
        Console.WriteLine($"  P99: {p99Time:F2}ms");
        Console.WriteLine($"\nThroughput: {throughput:F0} evaluations/second\n");

        // Store metrics
        _metrics.Add(new EvaluationMetric
        {
            FlagKey = flagKey,
            Timestamp = DateTime.UtcNow,
            AverageMs = avgTime,
            MaxMs = maxTime,
            P95Ms = p95Time,
            ThroughputPerSecond = throughput
        });
    }

    public async Task CompareImplementationsAsync(string flagKey)
    {
        Console.WriteLine($"\n=== Comparing Implementations: {flagKey} ===\n");

        // Test with and without caching
        var withCacheTime = await MeasureEvaluationAsync(flagKey, withCache: true);
        var withoutCacheTime = await MeasureEvaluationAsync(flagKey, withCache: false);

        var improvement = ((withoutCacheTime - withCacheTime) / withoutCacheTime) * 100;

        Console.WriteLine($"Without Cache: {withoutCacheTime:F2}ms");
        Console.WriteLine($"With Cache: {withCacheTime:F2}ms");
        Console.WriteLine($"Improvement: {improvement:F1}%\n");
    }

    private async Task<double> MeasureEvaluationAsync(string flagKey, bool withCache)
    {
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 100; i++)
        {
            var context = new UserContext { UserId = $"user{i}" };
            await _featureFlagService.IsEnabledAsync(flagKey, context);
        }

        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds / 100;
    }

    public void PrintMetricsReport()
    {
        if (!_metrics.Any())
        {
            Console.WriteLine("No metrics recorded");
            return;
        }

        Console.WriteLine("\n=== Performance Report ===\n");
        Console.WriteLine("Flag\t\t\tAvg (ms)\tP95 (ms)\tThroughput");
        Console.WriteLine("----\t\t\t-------\t\t-------\t----------");

        foreach (var metric in _metrics.OrderBy(m => m.Timestamp))
        {
            Console.WriteLine($"{metric.FlagKey,-20}\t{metric.AverageMs:F2}\t\t{metric.P95Ms:F2}\t\t{metric.ThroughputPerSecond:F0}");
        }

        Console.WriteLine();
    }

    private long GetPercentile(List<long> values, double percentile)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var index = (int)((sorted.Count - 1) * percentile);
        return sorted[index];
    }
}

// Production Monitoring: Health Checks
public class FeatureFlagHealthCheck
{
    private readonly IFeatureFlagService _featureFlagService;

    public FeatureFlagHealthCheck(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public async Task<HealthStatus> CheckHealthAsync()
    {
        var status = new HealthStatus { IsHealthy = true };

        // Test 1: Database connectivity
        try
        {
            var testFlag = new UserContext { UserId = "health-check-user" };
            await _featureFlagService.IsEnabledAsync("system-health-check", testFlag);
            status.DatabaseConnectivity = true;
        }
        catch (Exception ex)
        {
            status.DatabaseConnectivity = false;
            status.Issues.Add($"Database connectivity failed: {ex.Message}");
            status.IsHealthy = false;
        }

        // Test 2: Flag evaluation performance
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var context = new UserContext { UserId = "perf-test" };
            for (int i = 0; i < 10; i++)
            {
                await _featureFlagService.IsEnabledAsync("performance-test", context);
            }
            stopwatch.Stop();

            var avgMs = stopwatch.Elapsed.TotalMilliseconds / 10;
            if (avgMs > 100)
            {
                status.Issues.Add($"Slow response time: {avgMs:F2}ms average");
            }
            else
            {
                status.PerformanceOk = true;
            }
        }
        catch
        {
            status.Issues.Add("Performance test failed");
            status.IsHealthy = false;
        }

        return status;
    }
}

// Load Testing Example
public class FeatureFlagLoadTester
{
    private readonly IFeatureFlagService _featureFlagService;

    public FeatureFlagLoadTester(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public async Task RunLoadTestAsync(string flagKey, int concurrentUsers = 100, int durationSeconds = 60)
    {
        Console.WriteLine($"\n=== Load Test: {flagKey} ===\n");
        Console.WriteLine($"Concurrent Users: {concurrentUsers}");
        Console.WriteLine($"Duration: {durationSeconds}s");
        Console.WriteLine();

        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;
        var errorCount = 0;
        var responseTimes = new List<long>();

        var tasks = new List<Task>();
        var semaphore = new System.Threading.SemaphoreSlim(concurrentUsers);

        while (stopwatch.Elapsed.TotalSeconds < durationSeconds)
        {
            for (int i = 0; i < concurrentUsers; i++)
            {
                var userId = $"user{i:D6}";
                tasks.Add(ExecuteEvaluationAsync(userId, semaphore));
            }

            // Report progress every 10 iterations
            if (successCount % (concurrentUsers * 10) == 0)
            {
                Console.WriteLine($"[{stopwatch.Elapsed:mm\\:ss\\.ff}] Completed: {successCount}, Errors: {errorCount}");
            }
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        Console.WriteLine($"\n=== Load Test Results ===\n");
        Console.WriteLine($"Total Requests: {successCount + errorCount}");
        Console.WriteLine($"Successful: {successCount}");
        Console.WriteLine($"Errors: {errorCount}");
        Console.WriteLine($"Total Time: {stopwatch.Elapsed:mm\\:ss}");
        Console.WriteLine($"Throughput: {(successCount + errorCount) / stopwatch.Elapsed.TotalSeconds:F0} req/s");
    }

    private async Task ExecuteEvaluationAsync(string userId, System.Threading.SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        try
        {
            var context = new UserContext { UserId = userId };
            await _featureFlagService.IsEnabledAsync("load-test-flag", context);
        }
        finally
        {
            semaphore.Release();
        }
    }
}

// Supporting classes
public class EvaluationMetric
{
    public string FlagKey { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public double AverageMs { get; set; }
    public long MaxMs { get; set; }
    public double P95Ms { get; set; }
    public double ThroughputPerSecond { get; set; }
}

public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public bool DatabaseConnectivity { get; set; }
    public bool PerformanceOk { get; set; }
    public List<string> Issues { get; set; } = new();
}

// Mock repository for testing
{public sealed class MockFeatureFlagRepository {
    private readonly Dictionary<Guid, FeatureFlag> _flags = new();

    public Task<FeatureFlag?> GetByIdAsync(Guid id) =>
        Task.FromResult(_flags.TryGetValue(id, out var flag) ? flag : null);

    public Task<FeatureFlag?> GetByKeyAsync(string key) =>
        Task.FromResult(_flags.Values.FirstOrDefault(f => f.Key == key));

    // ... implement other methods ...

    public Task<bool> AddAsync(FeatureFlag entity)
    {
        _flags[entity.Id] = entity;
        return Task.FromResult(true);
    }

    public Task<IEnumerable<FeatureFlag>> GetAllAsync() =>
        Task.FromResult(_flags.Values.AsEnumerable());

    public Task<bool> UpdateAsync(FeatureFlag entity)
    {
        _flags[entity.Id] = entity;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id) =>
        Task.FromResult(_flags.Remove(id));

    public Task<bool> ExistsAsync(Guid id) =>
        Task.FromResult(_flags.ContainsKey(id));

    // Implement other IFeatureFlagRepository methods...
    public Task<IEnumerable<FeatureFlag>> GetEnabledAsync() => GetAllAsync();
    public Task<IEnumerable<FeatureFlag>> GetByCreatorAsync(string creatorEmail) => Task.FromResult(Enumerable.Empty<FeatureFlag>());
    public Task<IEnumerable<FeatureFlag>> GetModifiedSinceAsync(DateTime date) => GetAllAsync();
    public Task<(IEnumerable<FeatureFlag>, int)> GetPagedAsync(int pageNumber, int pageSize) => Task.FromResult((GetAllAsync().Result, _flags.Count));
    public Task<IEnumerable<FeatureFlag>> SearchAsync(string query) => GetAllAsync();
    public Task<FeatureFlag?> GetWithRulesAsync(Guid id) => GetByIdAsync(id);
    public Task<FeatureFlag?> GetWithVariantsAsync(Guid id) => GetByIdAsync(id);
    public Task<FeatureFlag?> GetWithAuditLogsAsync(Guid id) => GetByIdAsync(id);
    public Task<bool> KeyExistsAsync(string key) => Task.FromResult(_flags.Values.Any(f => f.Key == key));
    public Task<IEnumerable<FeatureFlag>> GetRecentlyModifiedAsync(int count) => GetAllAsync();
}
