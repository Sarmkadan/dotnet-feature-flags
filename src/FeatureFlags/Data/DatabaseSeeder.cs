// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;

namespace FeatureFlags.Data;

/// <summary>
/// Database seeder for populating the database with sample data for testing and demonstration.
/// Provides methods to seed feature flags, rules, variants, and audit logs.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with sample feature flags and related data.
    /// Idempotent - safe to call multiple times (will not duplicate data).
    /// </summary>
    public static async Task SeedSampleDataAsync(FeatureFlagDbContext dbContext)
    {
        // Avoid duplicate seeding
        if (await dbContext.FeatureFlags.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Create sample feature flags
        var paymentV2Flag = new FeatureFlag
        {
            Key = "payment-v2",
            DisplayName = "Payment System V2",
            Description = "New payment processing system with improved UX",
            IsEnabled = true,
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 25,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now.AddDays(-5),
            CreatedBy = "system",
            UpdatedBy = "admin@company.com"
        };

        var darkModeFlag = new FeatureFlag
        {
            Key = "dark-mode",
            DisplayName = "Dark Mode",
            Description = "Dark theme support across the application",
            IsEnabled = true,
            RolloutType = RolloutType.RulesBased,
            CreatedAt = now.AddDays(-15),
            UpdatedAt = now.AddDays(-2),
            CreatedBy = "system",
            UpdatedBy = "admin@company.com"
        };

        var analyticsFlag = new FeatureFlag
        {
            Key = "analytics-v3",
            DisplayName = "Analytics Platform V3",
            Description = "Advanced analytics with real-time dashboards",
            IsEnabled = false,
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 0,
            CreatedAt = now.AddDays(-60),
            UpdatedAt = now.AddDays(-20),
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        var abTestFlag = new FeatureFlag
        {
            Key = "checkout-flow-ab",
            DisplayName = "Checkout Flow A/B Test",
            Description = "Testing new checkout flow variations",
            IsEnabled = true,
            RolloutType = RolloutType.ABTest,
            CreatedAt = now.AddDays(-10),
            UpdatedAt = now,
            CreatedBy = "system",
            UpdatedBy = "admin@company.com"
        };

        // Add flags to context
        dbContext.FeatureFlags.AddRange(paymentV2Flag, darkModeFlag, analyticsFlag, abTestFlag);

        // Create sample rules for dark-mode flag
        var darkModeRule = new Rule
        {
            FeatureFlagId = 0, // Will be set after SaveChanges
            Name = "Enterprise Users",
            Priority = 1,
            IsActive = true,
            ConditionLogic = "AND"
        };

        var darkModeCondition = new Condition
        {
            RuleId = 0, // Will be set after SaveChanges
            FieldName = "tier",
            Operator = ConditionOperator.Equals,
            Value = "enterprise",
            CaseSensitive = false
        };

        darkModeRule.Conditions.Add(darkModeCondition);
        darkModeFlag.Rules.Add(darkModeRule);

        // Create sample A/B test variants
        var variantA = new ABTestVariant
        {
            FeatureFlagId = 0, // Will be set after SaveChanges
            VariantName = "Control",
            AllocationPercentage = 50,
            Description = "Original checkout flow"
        };

        var variantB = new ABTestVariant
        {
            FeatureFlagId = 0, // Will be set after SaveChanges
            VariantName = "Treatment",
            AllocationPercentage = 50,
            Description = "New checkout flow with simplified steps"
        };

        abTestFlag.Variants.Add(variantA);
        abTestFlag.Variants.Add(variantB);

        // Create sample audit logs
        var auditLog1 = new AuditLog
        {
            FeatureFlagId = 0,
            Action = AuditAction.Created,
            ChangedBy = "system",
            OldValue = null,
            NewValue = "payment-v2 created",
            Timestamp = now.AddDays(-30),
            Details = "Feature flag created with 0% rollout"
        };

        var auditLog2 = new AuditLog
        {
            FeatureFlagId = 0,
            Action = AuditAction.RolloutChanged,
            ChangedBy = "admin@company.com",
            OldValue = "0%",
            NewValue = "25%",
            Timestamp = now.AddDays(-5),
            Details = "Increased rollout to 25% after testing"
        };

        // Save changes
        await dbContext.SaveChangesAsync();

        // Now add audit logs (after flags are saved and have IDs)
        if (dbContext.FeatureFlags.Any())
        {
            var paymentFlagId = dbContext.FeatureFlags.First(f => f.Key == "payment-v2").Id;

            auditLog1.FeatureFlagId = paymentFlagId;
            auditLog2.FeatureFlagId = paymentFlagId;

            dbContext.AuditLogs.AddRange(auditLog1, auditLog2);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds minimal test data with just one feature flag.
    /// Useful for quick smoke tests.
    /// </summary>
    public static async Task SeedMinimalDataAsync(FeatureFlagDbContext dbContext)
    {
        if (await dbContext.FeatureFlags.AnyAsync())
        {
            return;
        }

        var testFlag = new FeatureFlag
        {
            Key = "test-feature",
            DisplayName = "Test Feature",
            Description = "A test feature flag",
            IsEnabled = true,
            RolloutType = RolloutType.Full,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "seeder",
            UpdatedBy = "seeder"
        };

        dbContext.FeatureFlags.Add(testFlag);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Clears all data from the database.
    /// WARNING: This is destructive and should only be used for testing.
    /// </summary>
    public static async Task ClearDatabaseAsync(FeatureFlagDbContext dbContext)
    {
        // Delete in correct order to respect foreign keys
        dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs);
        dbContext.ABTestVariants.RemoveRange(dbContext.ABTestVariants);
        dbContext.Conditions.RemoveRange(dbContext.Conditions);
        dbContext.Rules.RemoveRange(dbContext.Rules);
        dbContext.FeatureFlags.RemoveRange(dbContext.FeatureFlags);

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Generates a large set of feature flags for performance testing.
    /// </summary>
    public static async Task SeedPerformanceTestDataAsync(FeatureFlagDbContext dbContext, int flagCount = 1000)
    {
        if (await dbContext.FeatureFlags.CountAsync() > 100)
        {
            return; // Already seeded
        }

        var flags = new List<FeatureFlag>();
        var now = DateTime.UtcNow;

        for (int i = 1; i <= flagCount; i++)
        {
            flags.Add(new FeatureFlag
            {
                Key = $"perf-test-flag-{i:D6}",
                DisplayName = $"Performance Test Flag {i}",
                Description = $"Auto-generated flag {i} for performance testing",
                IsEnabled = i % 2 == 0,
                RolloutType = (RolloutType)(i % 4),
                PercentageRollout = i % 101,
                CreatedAt = now.AddHours(-(i % 720)),
                UpdatedAt = now.AddHours(-(i % 100)),
                CreatedBy = "perf-seeder",
                UpdatedBy = "perf-seeder"
            });

            // Batch save to avoid memory issues
            if (i % 100 == 0)
            {
                dbContext.FeatureFlags.AddRange(flags);
                await dbContext.SaveChangesAsync();
                flags.Clear();
            }
        }

        // Save remaining flags
        if (flags.Any())
        {
            dbContext.FeatureFlags.AddRange(flags);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gets statistics about the current database contents.
    /// </summary>
    public static async Task<DatabaseStatistics> GetStatisticsAsync(FeatureFlagDbContext dbContext)
    {
        return new DatabaseStatistics
        {
            TotalFeatureFlags = await dbContext.FeatureFlags.CountAsync(),
            EnabledFlags = await dbContext.FeatureFlags.CountAsync(f => f.IsEnabled),
            DisabledFlags = await dbContext.FeatureFlags.CountAsync(f => !f.IsEnabled),
            TotalRules = await dbContext.Rules.CountAsync(),
            TotalConditions = await dbContext.Conditions.CountAsync(),
            TotalVariants = await dbContext.ABTestVariants.CountAsync(),
            TotalAuditLogs = await dbContext.AuditLogs.CountAsync(),
            PercentageRolloutCount = await dbContext.FeatureFlags.CountAsync(f => f.RolloutType == RolloutType.Percentage),
            RulesBasedCount = await dbContext.FeatureFlags.CountAsync(f => f.RolloutType == RolloutType.RulesBased),
            ABTestCount = await dbContext.FeatureFlags.CountAsync(f => f.RolloutType == RolloutType.ABTest)
        };
    }
}

/// <summary>
/// Statistics about database contents.
/// </summary>
public class DatabaseStatistics
{
    public int TotalFeatureFlags { get; set; }
    public int EnabledFlags { get; set; }
    public int DisabledFlags { get; set; }
    public int TotalRules { get; set; }
    public int TotalConditions { get; set; }
    public int TotalVariants { get; set; }
    public int TotalAuditLogs { get; set; }
    public int PercentageRolloutCount { get; set; }
    public int RulesBasedCount { get; set; }
    public int ABTestCount { get; set; }
}
