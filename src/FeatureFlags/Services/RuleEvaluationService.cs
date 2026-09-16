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

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleEvaluationService"/> class.
    /// </summary>
    /// <param name="repository">The feature flag repository.</param>
    /// <param name="logger">The logger.</param>
    public RuleEvaluationService(IFeatureFlagRepository repository, ILogger<RuleEvaluationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Evaluates the targeting rules for the given feature flag against the user context.
    /// Returns <c>true</c> if any applicable rule matches, otherwise <c>false</c>.
    /// </summary>
    /// <param name="featureFlag">The feature flag to evaluate.</param>
    /// <param name="userContext">The user context to evaluate against.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the feature flag is enabled for the user; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Evaluates a single rule against the user context using its configured condition logic.
    /// </summary>
    /// <param name="rule">The rule to evaluate.</param>
    /// <param name="userContext">The user context to evaluate against.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the rule matches the user context; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Evaluates a single condition against the user context.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="userContext">The user context to evaluate against.</param>
    /// <returns><c>true</c> if the condition matches the user context; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Gets the active rules of the feature flag that match the user context.
    /// </summary>
    /// <param name="featureFlag">The feature flag whose rules are evaluated.</param>
    /// <param name="userContext">The user context to evaluate against.</param>
    /// <returns>The collection of applicable rules.</returns>
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
