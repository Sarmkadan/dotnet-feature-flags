// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.Models;
using FeatureFlags.Services;

/// <summary>
/// Example: Advanced feature flag scenarios including canary deployments,
/// segment exclusions, and complex multi-rule configurations.
/// </summary>
public class AdvancedScenariosExample
{
    private readonly IFeatureFlagService _flagService;
    private readonly IAuditLogService _auditService;

    public AdvancedScenariosExample(IFeatureFlagService flagService, IAuditLogService auditService)
    {
        _flagService = flagService;
        _auditService = auditService;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== Advanced Scenarios ===\n");

        await CanaryDeploymentAsync();
        await SegmentExclusionAsync();
        await PriorityBasedRulesAsync();
        await FeatureDependenciesAsync();
    }

    private async Task CanaryDeploymentAsync()
    {
        Console.WriteLine("1. Canary Deployment Strategy\n");

        // Day 1: 5% canary
        var day1 = new FeatureFlag
        {
            Key = "database-migration-v2",
            DisplayName = "Database Migration V2",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 5,
            IsEnabled = true
        };

        var created = await _flagService.CreateFeatureFlagAsync(day1);
        Console.WriteLine("Day 1: Canary phase - 5% of users");
        Console.WriteLine("  Monitoring: latency, error rates, data consistency\n");

        // Simulate health check
        await Task.Delay(100);
        Console.WriteLine("✓ Canary healthy\n");

        // Day 3: Expand to 25%
        created.PercentageRollout = 25;
        await _flagService.UpdateFeatureFlagAsync(created.Id, created);
        Console.WriteLine("Day 3: Expand to 25%");
        Console.WriteLine("  Monitoring: performance degradation\n");

        // Day 7: Full rollout
        created.PercentageRollout = 100;
        await _flagService.UpdateFeatureFlagAsync(created.Id, created);
        Console.WriteLine("Day 7: Complete rollout to 100%\n");
    }

    private async Task SegmentExclusionAsync()
    {
        Console.WriteLine("2. Segment Exclusion Strategy\n");

        var flag = new FeatureFlag
        {
            Key = "breaking-api-change",
            DisplayName = "Breaking API Change (Rollout with Exclusions)",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new[]
            {
                // Rule 1: Exclude critical accounts
                new Rule
                {
                    Name = "Exclude Critical Accounts",
                    Priority = 1,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "account_type",
                            Operator = ConditionOperator.Equals,
                            Value = "critical"
                        }
                    }
                },
                // Rule 2: Exclude old SDKs
                new Rule
                {
                    Name = "Exclude Old SDKs",
                    Priority = 2,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "sdk_version",
                            Operator = ConditionOperator.LessThan,
                            Value = "2.0.0"
                        }
                    }
                }
            ]
        };

        var created = await _flagService.CreateFeatureFlagAsync(flag);

        var testCases = new[]
        {
            ("critical-bank", new UserContext
            {
                UserId = "critical-bank",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["account_type"] = "critical",
                    ["sdk_version"] = "3.0.0"
                }
            }, false),
            ("old-integration", new UserContext
            {
                UserId = "old-integration",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["account_type"] = "standard",
                    ["sdk_version"] = "1.5.0"
                }
            }, false),
            ("modern-standard", new UserContext
            {
                UserId = "modern-standard",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["account_type"] = "standard",
                    ["sdk_version"] = "3.0.0"
                }
            }, true),
        };

        Console.WriteLine("Rollout with exclusions:\n");
        foreach (var (name, context, expected) in testCases)
        {
            var result = await _flagService.IsEnabledAsync("breaking-api-change", context);
            var status = result == expected ? "✓" : "✗";
            Console.WriteLine($"  {status} {name}: {(result ? "included" : "excluded")}");
        }

        Console.WriteLine();
    }

    private async Task PriorityBasedRulesAsync()
    {
        Console.WriteLine("3. Priority-Based Rules (First Match Wins)\n");

        var flag = new FeatureFlag
        {
            Key = "complex-targeting",
            DisplayName = "Complex Rule Priority Example",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new[]
            {
                // Highest priority: blocklist
                new Rule
                {
                    Name = "Blocklist - Never Show",
                    Priority = 1,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "blocklisted",
                            Operator = ConditionOperator.Equals,
                            Value = "true"
                        }
                    }
                },
                // Medium priority: VIP allowlist
                new Rule
                {
                    Name = "VIP Allowlist - Always Show",
                    Priority = 2,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "vip",
                            Operator = ConditionOperator.Equals,
                            Value = "true"
                        }
                    }
                },
                // Lower priority: regular rules
                new Rule
                {
                    Name = "Premium Users",
                    Priority = 3,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "tier",
                            Operator = ConditionOperator.Equals,
                            Value = "premium"
                        }
                    }
                }
            ]
        };

        var created = await _flagService.CreateFeatureFlagAsync(flag);

        var scenarios = new[]
        {
            ("vip-and-blocklisted", new UserContext
            {
                UserId = "vip-and-blocklisted",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["blocklisted"] = "true",
                    ["vip"] = "true"
                }
            }, false, "Blocklist has priority"),
            ("vip-premium", new UserContext
            {
                UserId = "vip-premium",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["vip"] = "true",
                    ["tier"] = "premium"
                }
            }, true, "VIP priority matches"),
            ("just-premium", new UserContext
            {
                UserId = "just-premium",
                Tier = "premium",
                CustomAttributes = new Dictionary<string, string>()
            }, true, "Premium rule matches"),
        };

        Console.WriteLine("Rules applied by priority:\n");
        foreach (var (name, context, expected, reason) in scenarios)
        {
            var result = await _flagService.IsEnabledAsync("complex-targeting", context);
            var status = result == expected ? "✓" : "✗";
            Console.WriteLine($"  {status} {name}: {(result ? "enabled" : "disabled")}");
            Console.WriteLine($"     → {reason}");
        }

        Console.WriteLine();
    }

    private async Task FeatureDependenciesAsync()
    {
        Console.WriteLine("4. Feature Dependencies\n");

        // Parent feature
        var parentFlag = new FeatureFlag
        {
            Key = "new-api-v2",
            DisplayName = "New API V2",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 50,
            IsEnabled = true
        };

        // Child features that depend on parent
        var childFlags = new[]
        {
            new FeatureFlag
            {
                Key = "api-v2-pagination",
                DisplayName = "API V2: Advanced Pagination",
                RolloutType = RolloutType.Percentage,
                PercentageRollout = 100,
                IsEnabled = true
            },
            new FeatureFlag
            {
                Key = "api-v2-caching",
                DisplayName = "API V2: Response Caching",
                RolloutType = RolloutType.Percentage,
                PercentageRollout = 100,
                IsEnabled = true
            }
        };

        var parent = await _flagService.CreateFeatureFlagAsync(parentFlag);
        var children = new List<FeatureFlag>();
        foreach (var child in childFlags)
        {
            children.Add(await _flagService.CreateFeatureFlagAsync(child));
        }

        Console.WriteLine("Feature hierarchy:");
        Console.WriteLine("  new-api-v2 (50%)");
        Console.WriteLine("    ├─ api-v2-pagination (100%)");
        Console.WriteLine("    └─ api-v2-caching (100%)\n");

        var userId = "user-depends-001";
        var context = new UserContext { UserId = userId };

        var hasParent = await _flagService.IsEnabledAsync("new-api-v2", context);
        Console.WriteLine($"User {userId}:");
        Console.WriteLine($"  new-api-v2: {hasParent}");

        if (hasParent)
        {
            var hasPagination = await _flagService.IsEnabledAsync("api-v2-pagination", context);
            var hasCaching = await _flagService.IsEnabledAsync("api-v2-caching", context);
            Console.WriteLine($"  api-v2-pagination: {hasPagination}");
            Console.WriteLine($"  api-v2-caching: {hasCaching}");
        }
        else
        {
            Console.WriteLine("  (child features not available without parent)");
        }

        Console.WriteLine();
    }
}
