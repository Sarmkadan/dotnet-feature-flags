// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.Services;

/// <summary>
/// Service implementation for rule evaluation.
/// Evaluates complex targeting rules with support for AND/OR logic.
/// </summary>
public class RuleEvaluationService : IRuleEvaluationService
{
    private readonly IFeatureFlagRepository _repository;
    private readonly ILogger<RuleEvaluationService> _logger;

    public RuleEvaluationService(IFeatureFlagRepository repository, ILogger<RuleEvaluationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext)
    {
        if (featureFlag == null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (userContext == null)
            throw new ArgumentNullException(nameof(userContext));

        try
        {
            var flagWithRules = await _repository.GetWithRulesAsync(featureFlag.Id);
            if (flagWithRules == null)
                throw new FeatureFlagNotFoundException(featureFlag.Key);

            if (!flagWithRules.Rules.Any())
                return false;

            var applicableRules = flagWithRules.Rules
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.Priority)
                .ToList();

            if (!applicableRules.Any())
                return false;

            foreach (var rule in applicableRules)
            {
                if (await EvaluateRuleAsync(rule, userContext))
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rules for feature flag {Key}", featureFlag.Key);
            throw new RuleEvaluationException($"Failed to evaluate rules for feature flag '{featureFlag.Key}'", ex);
        }
    }

    public async Task<bool> EvaluateRuleAsync(Rule rule, UserContext userContext)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        if (userContext == null)
            throw new ArgumentNullException(nameof(userContext));

        if (!rule.IsActive)
            return false;

        if (!rule.Conditions.Any())
            return false;

        var activeConditions = rule.Conditions.Where(c => c.IsActive).ToList();
        if (!activeConditions.Any())
            return false;

        var results = activeConditions.Select(c => EvaluateCondition(c, userContext)).ToList();

        return rule.ConditionLogic.Equals("AND", StringComparison.OrdinalIgnoreCase)
            ? results.All(r => r)
            : results.Any(r => r);
    }

    public bool EvaluateCondition(Condition condition, UserContext userContext)
    {
        if (condition == null)
            throw new ArgumentNullException(nameof(condition));

        if (userContext == null)
            throw new ArgumentNullException(nameof(userContext));

        if (!condition.IsActive)
            return false;

        try
        {
            var contextValue = userContext.GetAttribute(condition.AttributeName);
            return condition.Evaluate(contextValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating condition {AttributeName}", condition.AttributeName);
            return false;
        }
    }

    public async Task<IEnumerable<Rule>> GetApplicableRulesAsync(FeatureFlag featureFlag, UserContext userContext)
    {
        if (featureFlag == null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (userContext == null)
            throw new ArgumentNullException(nameof(userContext));

        var flagWithRules = await _repository.GetWithRulesAsync(featureFlag.Id);
        if (flagWithRules == null)
            throw new FeatureFlagNotFoundException(featureFlag.Key);

        var applicable = new List<Rule>();

        foreach (var rule in flagWithRules.Rules.Where(r => r.IsActive).OrderByDescending(r => r.Priority))
        {
            if (await EvaluateRuleAsync(rule, userContext))
                applicable.Add(rule);
        }

        return applicable;
    }
}
