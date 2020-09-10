#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Constants;

/// <summary>
/// Central location for all feature flag engine constants and configuration limits.
/// </summary>
public static class FeatureFlagConstants
{
    // Percentage rollout boundaries
    public const int MinPercentage = 0;
    public const int MaxPercentage = 100;
    public const int DefaultPercentage = 50;

    // Feature flag key constraints
    public const int MaxKeyLength = 128;
    public const int MinKeyLength = 3;
    public const int MaxDisplayNameLength = 256;
    public const int MaxDescriptionLength = 1000;

    // Rule priorities
    public const int MinPriority = 0;
    public const int MaxPriority = 1000;
    public const int DefaultPriority = 500;

    // Rollout constraints
    public const int MaxDailyIncrementPercentage = 100;
    public const int MinDailyIncrementPercentage = 1;

    // Cache and performance settings
    public const int MaxConditionsPerRule = 50;
    public const int MaxRulesPerFeatureFlag = 100;
    public const int MaxVariantsPerFeatureFlag = 10;

    // API response constraints
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 1000;
    public const int MinPageSize = 1;

    // Audit log retention
    public const int AuditLogRetentionDays = 365;

    // Condition value limits
    public const int MaxConditionValueLength = 500;

    // Timing
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan AuditLogArchiveThreshold = TimeSpan.FromDays(30);

    // Default values
    public const string DefaultConditionLogic = "AND";
    public const bool DefaultIsEnabled = false;
    public const bool DefaultIsActive = true;

    // Error messages
    public const string ErrorFeatureFlagNotFound = "The requested feature flag could not be found.";
    public const string ErrorInvalidConfiguration = "The feature flag configuration is invalid.";
    public const string ErrorRuleEvaluationFailed = "Rule evaluation encountered an error.";
    public const string ErrorDatabaseOperation = "A database operation failed. Please try again.";
    public const string ErrorInvalidUserContext = "The provided user context is invalid or incomplete.";
    public const string ErrorMaxAllocationExceeded = "The total allocation percentage exceeds 100%.";
}
