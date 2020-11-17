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
    Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext);

    Task<bool> EvaluateRuleAsync(Rule rule, UserContext userContext);

    bool EvaluateCondition(Condition condition, UserContext userContext);

    Task<IEnumerable<Rule>> GetApplicableRulesAsync(FeatureFlag featureFlag, UserContext userContext);
}
