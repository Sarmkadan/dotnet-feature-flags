#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
    /// Service interface for evaluating targeting rules against user contexts.
    /// Handles complex rule logic with AND/OR conditions.
    /// </summary>
    public interface IRuleEvaluationService
    {
        /// <summary>Evaluates all rules for a feature flag against the user context.</summary>
        /// <param name="featureFlag">The feature flag to evaluate.</param>
        /// <param name="userContext">The user context for evaluation.</param>
        /// <returns>True if any rule matches, false otherwise.</returns>
        Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext);

        /// <summary>Evaluates a single rule against the user context.</summary>
        /// <param name="rule">The rule to evaluate.</param>
        /// <param name="userContext">The user context for evaluation.</param>
        /// <returns>True if the rule matches, false otherwise.</returns>
        Task<bool> EvaluateRuleAsync(Rule rule, UserContext userContext);

        /// <summary>Evaluates a single condition against the user context.</summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="userContext">The user context for evaluation.</param>
        /// <returns>True if the condition matches, false otherwise.</returns>
        bool EvaluateCondition(Condition condition, UserContext userContext);

        /// <summary>Gets all applicable rules for a feature flag and user context.</summary>
        /// <param name="featureFlag">The feature flag.</param>
        /// <param name="userContext">The user context.</param>
        /// <returns>An enumerable collection of applicable rules.</returns>
        Task<IEnumerable<Rule>> GetApplicableRulesAsync(FeatureFlag featureFlag, UserContext userContext);
    }

