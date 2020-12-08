# FeatureFlagsBenchmarks
The `FeatureFlagsBenchmarks` type is designed to provide a set of benchmarking methods for evaluating the performance of feature flag evaluations. It offers a range of methods that simulate different evaluation scenarios, allowing developers to test and optimize the performance of their feature flag implementations.

## API
The `FeatureFlagsBenchmarks` type exposes the following public members:
* `Setup`: Sets up the benchmarking environment. This method does not take any parameters and does not return a value.
* `GetConsistentHash`: Returns a consistent hash value. This method does not take any parameters and returns an `int` value.
* `PercentageRolloutEvaluation`: Evaluates a percentage rollout scenario. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `PercentageRolloutEvaluation_100`: Evaluates a percentage rollout scenario with a 100% rollout. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `PercentageRolloutEvaluation_0`: Evaluates a percentage rollout scenario with a 0% rollout. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `RuleBasedEvaluation_Match`: Evaluates a rule-based scenario with a matching condition. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `RuleBasedEvaluation_NoMatch`: Evaluates a rule-based scenario with a non-matching condition. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `RuleBasedEvaluation_SingleCondition`: Evaluates a rule-based scenario with a single condition. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `ABTestVariantAssignment`: Assigns a variant for an A/B test. This method does not take any parameters and returns a `string?` value representing the assigned variant.
* `ABTestVariantAssignment_MultipleVariants`: Assigns a variant for an A/B test with multiple variants. This method does not take any parameters and returns a `string?` value representing the assigned variant.
* `FullFeatureFlagEvaluation_Percentage`: Evaluates a full feature flag scenario with a percentage rollout. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `FullFeatureFlagEvaluation_RuleBased`: Evaluates a full feature flag scenario with a rule-based evaluation. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `FullFeatureFlagEvaluation_ABTest`: Evaluates a full feature flag scenario with an A/B test. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `ComplexRuleEvaluation_ManyConditions`: Evaluates a complex rule-based scenario with many conditions. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `ComplexRuleEvaluation_ORLogic`: Evaluates a complex rule-based scenario with OR logic. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `PercentageRollout_WithCache_Hit`: Evaluates a percentage rollout scenario with a cache hit. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `PercentageRollout_WithCache_Miss`: Evaluates a percentage rollout scenario with a cache miss. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `RuleBased_WithCache_Hit`: Evaluates a rule-based scenario with a cache hit. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.
* `ABTest_WithCache_Hit`: Assigns a variant for an A/B test with a cache hit. This method does not take any parameters and returns a `string` value representing the assigned variant.
* `FullEvaluation_WithCache_Miss`: Evaluates a full feature flag scenario with a cache miss. This method does not take any parameters and returns a `bool` value indicating whether the evaluation was successful.

## Usage
The following examples demonstrate how to use the `FeatureFlagsBenchmarks` type:
```csharp
// Example 1: Evaluating a percentage rollout scenario
var benchmarks = new FeatureFlagsBenchmarks();
benchmarks.Setup();
var result = benchmarks.PercentageRolloutEvaluation();
Console.WriteLine($"Percentage rollout evaluation result: {result}");

// Example 2: Assigning a variant for an A/B test
var benchmarks = new FeatureFlagsBenchmarks();
benchmarks.Setup();
var variant = benchmarks.ABTestVariantAssignment();
Console.WriteLine($"Assigned variant: {variant}");
```

## Notes
When using the `FeatureFlagsBenchmarks` type, consider the following edge cases and thread-safety remarks:
* The `Setup` method should be called before using any other methods to ensure the benchmarking environment is properly set up.
* The `GetConsistentHash` method returns a consistent hash value, but its usage is not explicitly defined in the provided API.
* The `PercentageRolloutEvaluation` and related methods evaluate percentage rollout scenarios, but the exact logic and parameters used are not specified.
* The `RuleBasedEvaluation` methods evaluate rule-based scenarios, but the exact rules and conditions used are not specified.
* The `ABTestVariantAssignment` methods assign variants for A/B tests, but the exact logic and parameters used are not specified.
* The `FullFeatureFlagEvaluation` methods evaluate full feature flag scenarios, but the exact logic and parameters used are not specified.
* The `ComplexRuleEvaluation` methods evaluate complex rule-based scenarios, but the exact rules and conditions used are not specified.
* The `WithCache_Hit` and `WithCache_Miss` methods evaluate scenarios with cache hits and misses, respectively, but the exact cache logic and parameters used are not specified.
* The `FeatureFlagsBenchmarks` type is not explicitly defined as thread-safe, so caution should be exercised when using it in multi-threaded environments.
