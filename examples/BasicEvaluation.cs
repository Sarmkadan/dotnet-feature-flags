#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using FeatureFlags.Models;
using FeatureFlags.Services;

/// <summary>
/// Example: Basic feature flag evaluation with percentage rollout.
/// This demonstrates the simplest use case: toggling a feature on/off
/// with percentage-based rollout to control exposure.
/// </summary>
public class BasicEvaluationExample
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IAuditLogService _auditLogService;

    public BasicEvaluationExample(IFeatureFlagService featureFlagService, IAuditLogService auditLogService)
    {
        _featureFlagService = featureFlagService;
        _auditLogService = auditLogService;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== Basic Feature Flag Evaluation ===\n");

        // Step 1: Create a simple feature flag with 50% rollout
        var flag = new FeatureFlag
        {
            Key = "dark-mode-ui",
            DisplayName = "Dark Mode UI",
            Description = "New dark mode user interface",
            IsEnabled = true,
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 50
        };

        var createdFlag = await _featureFlagService.CreateFeatureFlagAsync(flag);
        Console.WriteLine($"✓ Created feature flag: {createdFlag.Key}");
        Console.WriteLine($"  Rollout: {createdFlag.PercentageRollout}%\n");

        // Step 2: Evaluate the flag for multiple users
        var userIds = new[] { "user001", "user002", "user003", "user004", "user005" };

        Console.WriteLine("Evaluating for 5 users:");
        int enabledCount = 0;

        foreach (var userId in userIds)
        {
            var context = new UserContext { UserId = userId };
            var isEnabled = await _featureFlagService.IsEnabledAsync("dark-mode-ui", context);

            Console.WriteLine($"  {userId}: {(isEnabled ? "✓ Dark Mode ON" : "✗ Dark Mode OFF")}");
            if (isEnabled) enabledCount++;
        }

        Console.WriteLine($"\nResult: {enabledCount}/5 users (expected ~50%)\n");

        // Step 3: Disable the flag
        Console.WriteLine("Disabling feature flag...");
        await _featureFlagService.DisableFeatureFlagAsync(createdFlag.Id);
        Console.WriteLine("✓ Flag disabled\n");

        // Step 4: Verify it's now disabled for all users
        Console.WriteLine("Re-evaluating after disable:");
        foreach (var userId in userIds)
        {
            var context = new UserContext { UserId = userId };
            var isEnabled = await _featureFlagService.IsEnabledAsync("dark-mode-ui", context);
            Console.WriteLine($"  {userId}: {(isEnabled ? "✓ Dark Mode ON" : "✗ Dark Mode OFF")}");
        }

        // Step 5: View audit log
        Console.WriteLine($"\nAudit Log:");
        var auditLogs = await _auditLogService.GetAuditLogsAsync(createdFlag.Id);
        foreach (var log in auditLogs)
        {
            Console.WriteLine($"  [{log.Timestamp:yyyy-MM-dd HH:mm:ss}] {log.Action}");
            Console.WriteLine($"    By: {log.ChangedBy}");
        }
    }

    /// <summary>
    /// Demonstrates gradual rollout - starting at 0% and increasing daily
    /// </summary>
    public async Task GradualRolloutExampleAsync()
    {
        Console.WriteLine("\n=== Gradual Rollout Example ===\n");

        var flag = new FeatureFlag
        {
            Key = "new-payment-gateway",
            DisplayName = "New Payment Gateway",
            RolloutType = RolloutType.Percentage,
            IsEnabled = true,
            PercentageRollout = 10,
            RolloutStrategy = new RolloutStrategy
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(9),
                StartPercentage = 10,
                EndPercentage = 100,
                DailyIncrementPercentage = 10
            }
        };

        var created = await _featureFlagService.CreateFeatureFlagAsync(flag);
        Console.WriteLine("✓ Created gradual rollout flag:");
        Console.WriteLine("  Day 1: 10%");
        Console.WriteLine("  Day 2: 20%");
        Console.WriteLine("  ...");
        Console.WriteLine("  Day 9: 100%");
    }
}
