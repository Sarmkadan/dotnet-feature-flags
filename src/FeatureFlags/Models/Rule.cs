#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Represents a targeting rule that applies conditions to determine if a feature flag should be enabled.
/// Rules can be combined using AND/OR logic to create complex targeting scenarios.
/// </summary>
public sealed class Rule
{
    /// <summary>
    /// Gets or sets the unique identifier of the rule.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the feature flag this rule belongs to.
    /// </summary>
    public int FeatureFlagId { get; set; }

    /// <summary>
    /// Gets or sets the name of the rule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the rule.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority of the rule; rules with higher priority are evaluated first.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the logic used to combine conditions; either "AND" or "OR".
    /// </summary>
    public string ConditionLogic { get; set; } = "AND"; // AND or OR

    /// <summary>
    /// Gets or sets the date and time when the rule was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the rule was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the feature flag this rule belongs to.
    /// </summary>
    public FeatureFlag? FeatureFlag { get; set; }

    /// <summary>
    /// Gets or sets the collection of conditions that make up this rule.
    /// </summary>
    public ICollection<Condition> Conditions { get; set; } = new List<Condition>();

    /// <summary>
    /// Validates that the rule has at least one condition and proper naming.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (!Conditions?.Any() ?? true)
            return false;

        if (ConditionLogic != "AND" && ConditionLogic != "OR")
            return false;

        if (Priority < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the total number of active conditions in this rule.
    /// </summary>
    public int GetActiveConditionCount()
    {
        return Conditions?.Count(c => c.IsActive) ?? 0;
    }

    /// <summary>
    /// Determines evaluation order; rules with higher priority are evaluated first.
    /// </summary>
    public int GetEvaluationPriority()
    {
        return Priority;
    }

    public override string ToString()
    {
        return $"Rule {{ Id = {Id}, FeatureFlagId = {FeatureFlagId}, Name = {Name}, Description = {Description}, Priority = {Priority}, IsActive = {IsActive} }}";
    }
}
