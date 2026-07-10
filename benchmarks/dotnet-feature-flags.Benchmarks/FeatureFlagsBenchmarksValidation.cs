using System;
using System.Collections.Generic;
using System.Globalization;

namespace dotnet_feature_flags.Benchmarks
{
    /// <summary>
    /// Provides validation helpers for <see cref="FeatureFlagsBenchmarks"/> instances.
    /// Validates benchmark state to ensure benchmarks can run correctly.
    /// </summary>
    public static class FeatureFlagsBenchmarksValidation
    {
        /// <summary>
        /// Validates that a <see cref="FeatureFlagsBenchmarks"/> instance is in a valid state.
        /// </summary>
        /// <param name="value">The benchmarks instance to validate.</param>
        /// <returns>An enumerable of validation error messages, or empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this FeatureFlagsBenchmarks value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate Setup method (should not throw)
            try
            {
                value.Setup();
            }
            catch (Exception ex)
            {
                errors.Add($"Setup() failed: {ex.Message}");
            }

            // Validate GetConsistentHash returns a valid hash
            try
            {
                var hash = value.GetConsistentHash();
                if (hash == 0)
                {
                    errors.Add("GetConsistentHash() returned 0, which may indicate an issue with hash calculation.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"GetConsistentHash() threw: {ex.Message}");
            }

            // Validate percentage rollout benchmarks
            ValidatePercentageBenchmark(value.PercentageRolloutEvaluation, "PercentageRolloutEvaluation", errors);
            ValidatePercentageBenchmark(value.PercentageRolloutEvaluation_100, "PercentageRolloutEvaluation_100", errors);
            ValidatePercentageBenchmark(value.PercentageRolloutEvaluation_0, "PercentageRolloutEvaluation_0", errors);

            // Validate rule-based evaluation benchmarks
            ValidateBooleanBenchmark(value.RuleBasedEvaluation_Match, "RuleBasedEvaluation_Match", errors);
            ValidateBooleanBenchmark(value.RuleBasedEvaluation_NoMatch, "RuleBasedEvaluation_NoMatch", errors);
            ValidateBooleanBenchmark(value.RuleBasedEvaluation_SingleCondition, "RuleBasedEvaluation_SingleCondition", errors);

            // Validate A/B test benchmarks
            ValidateStringBenchmark(value.ABTestVariantAssignment, "ABTestVariantAssignment", errors);
            ValidateStringBenchmark(value.ABTestVariantAssignment_MultipleVariants, "ABTestVariantAssignment_MultipleVariants", errors);

            // Validate full evaluation benchmarks
            ValidateBooleanBenchmark(value.FullFeatureFlagEvaluation_Percentage, "FullFeatureFlagEvaluation_Percentage", errors);
            ValidateBooleanBenchmark(value.FullFeatureFlagEvaluation_RuleBased, "FullFeatureFlagEvaluation_RuleBased", errors);
            ValidateBooleanBenchmark(value.FullFeatureFlagEvaluation_ABTest, "FullFeatureFlagEvaluation_ABTest", errors);

            // Validate complex rule evaluation benchmarks
            ValidateBooleanBenchmark(value.ComplexRuleEvaluation_ManyConditions, "ComplexRuleEvaluation_ManyConditions", errors);
            ValidateBooleanBenchmark(value.ComplexRuleEvaluation_ORLogic, "ComplexRuleEvaluation_ORLogic", errors);

            // Validate cache-related benchmarks
            ValidateBooleanBenchmark(value.PercentageRollout_WithCache_Hit, "PercentageRollout_WithCache_Hit", errors);
            ValidateBooleanBenchmark(value.RuleBased_WithCache_Hit, "RuleBased_WithCache_Hit", errors);
            ValidateStringBenchmark(value.ABTest_WithCache_Hit, "ABTest_WithCache_Hit", errors);
            ValidateBooleanBenchmark(value.FullEvaluation_WithCache_Miss, "FullEvaluation_WithCache_Miss", errors);

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Checks if a <see cref="FeatureFlagsBenchmarks"/> instance is valid.
        /// </summary>
        /// <param name="value">The benchmarks instance to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        public static bool IsValid(this FeatureFlagsBenchmarks value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="FeatureFlagsBenchmarks"/> instance is valid.
        /// </summary>
        /// <param name="value">The benchmarks instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing validation errors.</exception>
        public static void EnsureValid(this FeatureFlagsBenchmarks value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"FeatureFlagsBenchmarks instance is invalid. Validation failed with {errors.Count} error(s):{Environment.NewLine}"
                    + string.Join(Environment.NewLine, errors));
            }
        }

        private static void ValidatePercentageBenchmark(
            Func<bool> benchmark,
            string benchmarkName,
            ICollection<string> errors)
        {
            try
            {
                var result = benchmark();
                // Percentage rollout benchmarks should return a boolean result
                // No specific range validation needed as the benchmark itself handles the logic
            }
            catch (Exception ex)
            {
                errors.Add($"{benchmarkName}() threw: {ex.Message}");
            }
        }

        private static void ValidateBooleanBenchmark(
            Func<bool> benchmark,
            string benchmarkName,
            ICollection<string> errors)
        {
            try
            {
                var _ = benchmark();
                // Boolean benchmarks should execute without throwing
            }
            catch (Exception ex)
            {
                errors.Add($"{benchmarkName}() threw: {ex.Message}");
            }
        }

        private static void ValidateStringBenchmark(
            Func<string?> benchmark,
            string benchmarkName,
            ICollection<string> errors)
        {
            try
            {
                var result = benchmark();
                // String benchmarks may return null for some variants, which is acceptable
            }
            catch (Exception ex)
            {
                errors.Add($"{benchmarkName}() threw: {ex.Message}");
            }
        }
    }
}
