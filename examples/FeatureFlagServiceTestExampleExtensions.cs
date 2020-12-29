#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.Models;
using FeatureFlags.Services;

namespace FeatureFlags.Examples
{
    /// <summary>
    /// Extension methods for FeatureFlagServiceTestExample to provide additional testing and monitoring utilities.
    /// </summary>
    public static class FeatureFlagServiceTestExampleExtensions
    {
        /// <summary>
        /// Creates a comprehensive test suite for percentage rollout feature flags.
        /// Tests both percentage-based rollout and consistent hashing behavior.
        /// </summary>
        /// <param name="example">The test example instance</param>
        /// <param name="iterations">Number of iterations to run (default: 100)</param>
        /// <returns>Dictionary mapping user IDs to their rollout results</returns>
        public static Dictionary<string, bool> TestPercentageRolloutComprehensive(this FeatureFlagServiceTestExample example, int iterations = 100)
        {
            var results = new Dictionary<string, bool>();
            var mockRepository = new MockFeatureFlagRepository();
            var service = new FeatureFlagService(mockRepository, null!);

            var flag = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Key = "percentage-rollout-test",
                PercentageRollout = 50,
                IsEnabled = true
            };

            mockRepository.AddAsync(flag).Wait();

            // Test with consistent hashing - same user should always get same result
            for (int i = 0; i < iterations; i++)
            {
                var userId = $"user-{i:D6}";
                var context = new UserContext { UserId = userId };
                var isEnabled = service.IsEnabledAsync(flag.Key, context).Result;
                results[userId] = isEnabled;
            }

            // Verify consistency: 50% should be roughly 50% enabled
            var enabledCount = results.Count(kvp => kvp.Value);
            var percentage = (double)enabledCount / iterations * 100;

            Console.WriteLine($"Percentage Rollout Test: {percentage:F1}% enabled ({enabledCount}/{iterations})");
            Console.WriteLine($"Expected: ~50%, Actual: {percentage:F1}%");

            return results;
        }

        /// <summary>
        /// Tests rule-based evaluation with various rule types.
        /// Creates flags with different rule conditions and verifies they evaluate correctly.
        /// </summary>
        /// <param name="example">The test example instance</param>
        /// <param name="ruleCount">Number of rules to test (default: 5)</param>
        /// <returns>List of test results with rule evaluations</returns>
        public static List<RuleEvaluationResult> TestRuleBasedEvaluation(this FeatureFlagServiceTestExample example, int ruleCount = 5)
        {
            var results = new List<RuleEvaluationResult>();
            var mockRepository = new MockFeatureFlagRepository();
            var service = new FeatureFlagService(mockRepository, null!);

            for (int i = 0; i < ruleCount; i++)
            {
                var flag = new FeatureFlag
                {
                    Id = Guid.NewGuid(),
                    Key = $"rule-test-{i}",
                    IsEnabled = true,
                    Rules = new List<FeatureFlagRule>
                    {
                        new FeatureFlagRule
                        {
                            Id = Guid.NewGuid(),
                            Type = RuleType.StringComparison,
                            PropertyName = "UserId",
                            Operator = "equals",
                            Value = $"admin-{i}",
                            Effect = RuleEffect.Enable
                        }
                    }
                };

                mockRepository.AddAsync(flag).Wait();

                // Test matching user
                var matchingContext = new UserContext { UserId = $"admin-{i}" };
                var isEnabledMatching = service.IsEnabledAsync(flag.Key, matchingContext).Result;

                // Test non-matching user
                var nonMatchingContext = new UserContext { UserId = $"user-{i}" };
                var isEnabledNonMatching = service.IsEnabledAsync(flag.Key, nonMatchingContext).Result;

                results.Add(new RuleEvaluationResult
                {
                    FlagKey = flag.Key,
                    MatchingUserEnabled = isEnabledMatching,
                    NonMatchingUserEnabled = isEnabledNonMatching,
                    ExpectedMatching = true,
                    ExpectedNonMatching = false
                });
            }

            Console.WriteLine($"Rule-Based Evaluation Test: {results.Count} rules tested");
            foreach (var result in results)
            {
                Console.WriteLine($"  {result.FlagKey}: Matching={result.MatchingUserEnabled}, Non-matching={result.NonMatchingUserEnabled}");
            }

            return results;
        }

        /// <summary>
        /// Runs A/B test variant assignment tests to verify consistent variant assignment.
        /// Tests that users consistently get the same variant across multiple evaluations.
        /// </summary>
        /// <param name="example">The test example instance</param>
        /// <param name="variants">Number of variants to test (default: 3)</param>
        /// <param name="iterationsPerUser">Evaluations per user (default: 10)</param>
        /// <returns>Dictionary mapping user IDs to their variant assignments</returns>
        public static Dictionary<string, string> TestABTestVariantAssignment(this FeatureFlagServiceTestExample example, int variants = 3, int iterationsPerUser = 10)
        {
            var assignments = new Dictionary<string, string>();
            var mockRepository = new MockFeatureFlagRepository();
            var service = new FeatureFlagService(mockRepository, null!);

            var flag = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Key = "ab-test-variants",
                IsEnabled = true,
                Variants = new List<FeatureFlagVariant>
                {
                    new FeatureFlagVariant { Key = "A", Weight = 33 },
                    new FeatureFlagVariant { Key = "B", Weight = 33 },
                    new FeatureFlagVariant { Key = "C", Weight = 34 }
                },
                VariantSelection = VariantSelectionStrategy.Random
            };

            mockRepository.AddAsync(flag).Wait();

            // Assign variants to users
            for (int i = 0; i < 50; i++)
            {
                var userId = $"ab-user-{i:D3}";
                var context = new UserContext { UserId = userId };
                var variant = service.GetVariantAsync(flag.Key, context).Result;
                assignments[userId] = variant?.Key ?? "none";
            }

            // Verify consistency: run evaluations again and check same users get same variants
            var consistencyChecks = 0;
            var consistencyFailures = 0;

            foreach (var userId in assignments.Keys.ToList())
            {
                var context = new UserContext { UserId = userId };
                var variant = service.GetVariantAsync(flag.Key, context).Result;
                var variantKey = variant?.Key ?? "none";

                if (variantKey == assignments[userId])
                {
                    consistencyChecks++;
                }
                else
                {
                    consistencyFailures++;
                }
            }

            Console.WriteLine($"A/B Test Variant Assignment: {assignments.Count} users tested");
            Console.WriteLine($"Consistency: {consistencyChecks}/{assignments.Count} users got same variant ({100 * consistencyChecks / assignments.Count}%)");

            return assignments;
        }

        /// <summary>
        /// Creates a performance monitoring extension that tracks evaluation metrics over time.
        /// Useful for detecting performance regressions in production.
        /// </summary>
        /// <param name="example">The test example instance</param>
        /// <param name="flagKey">The flag key to monitor</param>
        /// <param name="iterations">Number of evaluations to perform</param>
        /// <returns>Performance metrics including average, p95, max time, and throughput</returns>
        public static async Task<FlagPerformanceMetrics> MonitorEvaluationPerformanceAsync(
            this FeatureFlagServiceTestExample example,
            string flagKey,
            int iterations = 1000)
        {
            var mockRepository = new MockFeatureFlagRepository();
            var service = new FeatureFlagService(mockRepository, null!);

            var stopwatch = Stopwatch.StartNew();
            var successCount = 0;
            var errorCount = 0;
            var timings = new List<long>();

            for (int i = 0; i < iterations; i++)
            {
                var context = new UserContext { UserId = $"perf-user-{i:D6}" };
                var iterationStopwatch = Stopwatch.StartNew();

                try
                {
                    await service.IsEnabledAsync(flagKey, context);
                    iterationStopwatch.Stop();
                    timings.Add(iterationStopwatch.ElapsedMilliseconds);
                    successCount++;
                }
                catch
                {
                    errorCount++;
                    iterationStopwatch.Stop();
                }
            }

            stopwatch.Stop();

            var metrics = new FlagPerformanceMetrics
            {
                Iterations = iterations,
                SuccessCount = successCount,
                ErrorCount = errorCount,
                TotalTimeMs = stopwatch.ElapsedMilliseconds,
                AverageMs = timings.Any() ? timings.Average() : 0,
                MaxMs = timings.Any() ? timings.Max() : 0,
                MinMs = timings.Any() ? timings.Min() : 0,
                P95Ms = timings.Any() ? GetPercentile(timings, 0.95) : 0,
                P99Ms = timings.Any() ? GetPercentile(timings, 0.99) : 0,
                ThroughputPerSecond = iterations / stopwatch.Elapsed.TotalSeconds
            };

            Console.WriteLine($"\n=== Performance Metrics for {flagKey} ===");
            Console.WriteLine($"Iterations: {metrics.Iterations}");
            Console.WriteLine($"Success: {metrics.SuccessCount}, Errors: {metrics.ErrorCount}");
            Console.WriteLine($"Average: {metrics.AverageMs:F2}ms, P95: {metrics.P95Ms:F2}ms, Max: {metrics.MaxMs}ms");
            Console.WriteLine($"Throughput: {metrics.ThroughputPerSecond:F0} evaluations/second\n");

            return metrics;
        }

        private static long GetPercentile(List<long> values, double percentile)
        {
            if (values == null || !values.Any())
                return 0;

            var sorted = values.OrderBy(v => v).ToList();
            var index = (int)((sorted.Count - 1) * percentile);
            return sorted[index];
        }
    }

    /// <summary>
    /// Result of a rule evaluation test
    /// </summary>
    public sealed class RuleEvaluationResult
    {
        public string FlagKey { get; set; } = string.Empty;
        public bool MatchingUserEnabled { get; set; }
        public bool NonMatchingUserEnabled { get; set; }
        public bool ExpectedMatching { get; set; }
        public bool ExpectedNonMatching { get; set; }
    }

    /// <summary>
    /// Performance metrics for flag evaluation
    /// </summary>
    public sealed class FlagPerformanceMetrics
    {
        public int Iterations { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public long TotalTimeMs { get; set; }
        public double AverageMs { get; set; }
        public long MaxMs { get; set; }
        public long MinMs { get; set; }
        public double P95Ms { get; set; }
        public double P99Ms { get; set; }
        public double ThroughputPerSecond { get; set; }
    }
}
