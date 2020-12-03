using System;
using System.Diagnostics;

namespace dotnet_feature_flags.Benchmarks
{
    public static class FeatureFlagsBenchmarksExtensions
    {
        public static void Warmup(this FeatureFlagsBenchmarks benchmarks)
        {
            // Perform some initial evaluations to warm up the system
            benchmarks.PercentageRolloutEvaluation();
            benchmarks.RuleBasedEvaluation_Match();
            benchmarks.ABTestVariantAssignment();
        }

        public static double AverageEvaluationTime(this FeatureFlagsBenchmarks benchmarks, int iterations)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                benchmarks.PercentageRolloutEvaluation();
            }
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds / iterations;
        }

        public static (bool, string?) EvaluateFullFeatureFlag(this FeatureFlagsBenchmarks benchmarks, string featureFlagKey)
        {
            bool enabled = benchmarks.FullFeatureFlagEvaluation_Percentage();
            string? variant = benchmarks.ABTestVariantAssignment();
            return (enabled, variant);
        }
    }
}
