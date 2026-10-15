#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Exceptions;

/// <summary>
/// Thrown when a feature flag definition violates one or more rules.
/// All violations found in a single pass are carried in <see cref="Violations"/> and listed in the message.
/// </summary>
public sealed class InvalidFlagDefinitionException : InvalidFeatureFlagException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidFlagDefinitionException"/> class.
    /// </summary>
    /// <param name="flagKey">The key of the flag whose definition is invalid.</param>
    /// <param name="violations">Every rule violation found for the flag. Must not be empty.</param>
    public InvalidFlagDefinitionException(string flagKey, IReadOnlyList<string> violations)
        : base(BuildMessage(flagKey, violations))
    {
        FlagKey = flagKey;
        Violations = violations;
    }

    /// <summary>Gets the key of the flag whose definition is invalid.</summary>
    public string FlagKey { get; }

    /// <summary>Gets every rule violation found for the flag.</summary>
    public IReadOnlyList<string> Violations { get; }

    private static string BuildMessage(string flagKey, IReadOnlyList<string> violations)
        => $"Feature flag definition '{flagKey}' is invalid ({violations.Count} violation(s)):{Environment.NewLine}- {
            string.Join($"{Environment.NewLine}- ", violations)}";
}
