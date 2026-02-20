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
/// Example: Rule-based targeting for different user segments.
/// This demonstrates complex targeting rules using conditions
/// like user tier, country, and custom attributes.
/// </summary>
public class UserTargetingExample
{
    private readonly IFeatureFlagService _featureFlagService;

    public UserTargetingExample(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== User Targeting Example ===\n");

        // Example 1: Target premium users in specific regions
        await TargetPremiumUsersAsync();

        // Example 2: Target new users (account age < 30 days)
        await TargetNewUsersAsync();

        // Example 3: Target high-value customers
        await TargetHighValueCustomersAsync();
    }

    private async Task TargetPremiumUsersAsync()
    {
        Console.WriteLine("1. Premium Users in US/EU\n");

        var flag = new FeatureFlag
        {
            Key = "advanced-analytics",
            DisplayName = "Advanced Analytics Dashboard",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new[]
            {
                new Rule
                {
                    Name = "Premium US/EU Users",
                    Priority = 1,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "tier",
                            Operator = ConditionOperator.Equals,
                            Value = "premium"
                        },
                        new Condition
                        {
                            Attribute = "country",
                            Operator = ConditionOperator.In,
                            Value = "US,CA,DE,FR,GB,IT,ES"
                        }
                    }
                }
            }
        };

        var created = await _featureFlagService.CreateFeatureFlagAsync(flag);

        // Test with different users
        var testUsers = new[]
        {
            ("premium-us", new UserContext { UserId = "premium-us", Tier = "premium", Country = "US" }, true),
            ("premium-fr", new UserContext { UserId = "premium-fr", Tier = "premium", Country = "FR" }, true),
            ("premium-jp", new UserContext { UserId = "premium-jp", Tier = "premium", Country = "JP" }, false),
            ("free-us", new UserContext { UserId = "free-us", Tier = "free", Country = "US" }, false),
        };

        foreach (var (name, context, expected) in testUsers)
        {
            var result = await _featureFlagService.IsEnabledAsync("advanced-analytics", context);
            var status = result == expected ? "✓" : "✗";
            Console.WriteLine($"  {status} {name}: {result}");
        }

        Console.WriteLine();
    }

    private async Task TargetNewUsersAsync()
    {
        Console.WriteLine("2. Beta Feature for New Users\n");

        var flag = new FeatureFlag
        {
            Key = "experimental-ui",
            DisplayName = "Experimental UI (Beta)",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new[]
            {
                new Rule
                {
                    Name = "Users Less Than 30 Days Old",
                    Priority = 1,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "account_age_days",
                            Operator = ConditionOperator.LessThan,
                            Value = "30"
                        },
                        new Condition
                        {
                            Attribute = "beta_tester",
                            Operator = ConditionOperator.Equals,
                            Value = "true"
                        }
                    }
                }
            }
        };

        var created = await _featureFlagService.CreateFeatureFlagAsync(flag);

        // Test cases
        var testUsers = new[]
        {
            ("new-beta-user", new UserContext
            {
                UserId = "new-beta-user",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["account_age_days"] = "15",
                    ["beta_tester"] = "true"
                }
            }, true),
            ("old-user", new UserContext
            {
                UserId = "old-user",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["account_age_days"] = "180",
                    ["beta_tester"] = "true"
                }
            }, false),
        };

        foreach (var (name, context, expected) in testUsers)
        {
            var result = await _featureFlagService.IsEnabledAsync("experimental-ui", context);
            var status = result == expected ? "✓" : "✗";
            Console.WriteLine($"  {status} {name}: {result}");
        }

        Console.WriteLine();
    }

    private async Task TargetHighValueCustomersAsync()
    {
        Console.WriteLine("3. VIP Feature for High-Value Customers\n");

        var flag = new FeatureFlag
        {
            Key = "vip-concierge",
            DisplayName = "VIP Concierge Service",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new[]
            {
                new Rule
                {
                    Name = "High-Value VIP Customers",
                    Priority = 1,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "annual_spending",
                            Operator = ConditionOperator.GreaterThan,
                            Value = "10000"
                        },
                        new Condition
                        {
                            Attribute = "subscription_tier",
                            Operator = ConditionOperator.Equals,
                            Value = "enterprise"
                        }
                    }
                },
                // Fallback rule: enable for staff members
                new Rule
                {
                    Name = "Staff Members",
                    Priority = 2,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "employee_id",
                            Operator = ConditionOperator.NotEquals,
                            Value = ""
                        }
                    }
                }
            }
        };

        var created = await _featureFlagService.CreateFeatureFlagAsync(flag);

        var testUsers = new[]
        {
            ("vip-customer", new UserContext
            {
                UserId = "vip-customer",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["annual_spending"] = "50000",
                    ["subscription_tier"] = "enterprise"
                }
            }, true),
            ("regular-customer", new UserContext
            {
                UserId = "regular-customer",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["annual_spending"] = "1000",
                    ["subscription_tier"] = "basic"
                }
            }, false),
            ("staff-member", new UserContext
            {
                UserId = "staff-member",
                CustomAttributes = new Dictionary<string, string>
                {
                    ["employee_id"] = "EMP-001"
                }
            }, true),
        };

        foreach (var (name, context, expected) in testUsers)
        {
            var result = await _featureFlagService.IsEnabledAsync("vip-concierge", context);
            var status = result == expected ? "✓" : "✗";
            Console.WriteLine($"  {status} {name}: {result}");
        }
    }
}
