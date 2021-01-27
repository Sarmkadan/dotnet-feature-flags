using System;
using System.Diagnostics;

namespace dotnet_feature_flags.Benchmarks
{
    /// <summary>
    /// Provides extension methods for <see cref="FeatureFlagsBenchmarks"/> to facilitate benchmarking scenarios.
    /// </summary>
    public static class FeatureFlagsBenchmarksExtensions
    {
        /// <summary>
        /// Performs a warmup of the benchmark system by executing representative evaluations.
        /// This helps ensure subsequent measurements are not affected by JIT compilation and other startup costs.
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance to warm up. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
        public static void Warmup(this FeatureFlagsBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            // Perform some initial evaluations to warm up the system
            benchmarks.PercentageRolloutEvaluation();
            benchmarks.RuleBasedEvaluation_Match();
            benchmarks.ABTestVariantAssignment();
        }

        /// <summary>
        /// Measures the average execution time of percentage rollout evaluation across multiple iterations.
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance. Cannot be null.</param>
        /// <param name="iterations">Number of iterations to run. Must be positive.</param>
        /// <returns>The average evaluation time in milliseconds.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="iterations"/> is not positive.</exception>
        public static double AverageEvaluationTime(this FeatureFlagsBenchmarks benchmarks, int iterations)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterations);

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                benchmarks.PercentageRolloutEvaluation();
            }
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds / iterations;
        }

        /// <summary>
        /// Evaluates a feature flag using both percentage rollout and A/B test variant assignment.
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance. Cannot be null.</param>
        /// <param name="featureFlagKey">The feature flag key to evaluate. Cannot be null or empty.</param>
        /// <returns>A tuple containing the enabled status and the assigned variant (if any).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null or if <paramref name="featureFlagKey"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="featureFlagKey"/> is empty.</exception>
        public static (bool Enabled, string? Variant) EvaluateFullFeatureFlag(this FeatureFlagsBenchmarks benchmarks, string featureFlagKey)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentException.ThrowIfNullOrEmpty(featureFlagKey);

            bool enabled = benchmarks.FullFeatureFlagEvaluation_Percentage();
            string? variant = benchmarks.ABTestVariantAssignment();
            return (enabled, variant);
        }
    }
}
