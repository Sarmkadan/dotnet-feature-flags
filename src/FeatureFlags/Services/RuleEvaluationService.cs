#nullable enable
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
public class RuleEvaluationService : IRuleEvaluationService {
    private readonly IFeatureFlagRepository _repository;
    private readonly ILogger<RuleEvaluationService> _logger;

    public RuleEvaluationService(IFeatureFlagRepository repository, ILogger<RuleEvaluationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext, CancellationToken cancellationToken = default)
    {
        if (featureFlag is null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        _logger.LogInformation("Starting evaluation for feature flag {FeatureFlagId}", featureFlag.Id);
        try
        {
            var flagWithRules = await _repository.GetWithRulesAsync(featureFlag.Id);
            if (flagWithRules is null)
            {
                _logger.LogInformation("Evaluation finished for feature flag {FeatureFlagId}: flag not found", featureFlag.Id);
                throw new FeatureFlagNotFoundException(featureFlag.Key);
            }

            if (!flagWithRules.Rules.Any())
            {
                _logger.LogInformation("Evaluation finished for feature flag {FeatureFlagId}: no rules", featureFlag.Id);
                return false;
            }

            var applicableRules = flagWithRules.Rules
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.Priority)
                .ToList();

            if (!applicableRules.Any())
            {
                _logger.LogInformation("Evaluation finished for feature flag {FeatureFlagId}: no applicable rules", featureFlag.Id);
                return false;
            }

            foreach (var rule in applicableRules)
            {
                if (await EvaluateRuleAsync(rule, userContext))
                {
                    _logger.LogInformation("Evaluation finished for feature flag {FeatureFlagId}: rule {RuleId} matched", featureFlag.Id, rule.Id);
                    return true;
                }
            }

            _logger.LogInformation("Evaluation finished for feature flag {FeatureFlagId}: no matching rules", featureFlag.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rules for feature flag {Key}", featureFlag.Key);
            throw new RuleEvaluationException($"Failed to evaluate rules for feature flag '{featureFlag.Key}'", ex);
        }
    }

    public async Task<bool> EvaluateRuleAsync(Rule rule, UserContext userContext, CancellationToken cancellationToken = default)
    {
        if (rule is null)
            throw new ArgumentNullException(nameof(rule));

        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        _logger.LogInformation("Starting rule evaluation for {RuleId}", rule.Id);
        if (!rule.IsActive)
        {
            _logger.LogWarning("Rule {RuleId} is inactive", rule.Id);
            return false;
        }

        if (!rule.Conditions.Any())
        {
            _logger.LogWarning("Rule {RuleId} has no conditions", rule.Id);
            return false;
        }

        var activeConditions = rule.Conditions.Where(c => c.IsActive).ToList();
        if (!activeConditions.Any())
        {
            _logger.LogWarning("Rule {RuleId} has no active conditions", rule.Id);
            return false;
        }

        var results = activeConditions.Select(c => EvaluateCondition(c, userContext)).ToList();

        _logger.LogInformation("Completed rule evaluation for {RuleId}: {Result}", rule.Id, results.All(r => r));
        return rule.ConditionLogic.Equals("AND", StringComparison.OrdinalIgnoreCase)
            ? results.All(r => r)
            : results.Any(r => r);
    }

    public bool EvaluateCondition(Condition condition, UserContext userContext)
    {
        if (condition is null)
            throw new ArgumentNullException(nameof(condition));

        if (userContext is null)
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
        if (featureFlag is null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        var flagWithRules = await _repository.GetWithRulesAsync(featureFlag.Id);
        if (flagWithRules is null)
            throw new FeatureFlagNotFoundException(featureFlag.Key);

        var applicable = new List<Rule>();

        foreach (var rule in flagWithRules.Rules.Where(r => r.IsActive).OrderByDescending(r => r.Priority))
        {
            if (await EvaluateRuleAsync(rule, userContext))
                applicable.Add(rule);
        }

        return applicable;
    }

    Task<bool> IRuleEvaluationService.EvaluateAsync(FeatureFlag featureFlag, UserContext userContext) => EvaluateAsync(featureFlag, userContext);
    Task<bool> IRuleEvaluationService.EvaluateRuleAsync(Rule rule, UserContext userContext) => EvaluateRuleAsync(rule, userContext);
}
