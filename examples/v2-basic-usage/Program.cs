#nullable enable
using System;
using System.Threading.Tasks;
using FeatureFlags.Models;
using FeatureFlags.Services;

// V2 Basic Usage Example
// This example demonstrates the new v2.0 features including experimentation with metrics collection

namespace FeatureFlags.Examples.V2BasicUsage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Feature Flags v2.0 - Basic Usage Example");
            Console.WriteLine("=====================================\n");

            // Initialize the feature flag service (in a real app, this would be injected)
            var featureFlagService = new FeatureFlagService();

            // Create a user context
            var userContext = new UserContext
            {
                UserId = "user123",
                Email = "user@example.com",
                Tier = "premium",
                Country = "US",
                DeviceType = "mobile"
            };

            // Example 1: Simple feature flag evaluation
            Console.WriteLine("1. Simple Feature Flag Evaluation");
            Console.WriteLine("----------------------------------");

            var simpleFlag = new FeatureFlag
            {
                Key = "new-dashboard",
                DisplayName = "New Dashboard",
                RolloutType = RolloutType.Percentage,
                PercentageRollout = 100, // 100% rollout
                IsEnabled = true
            };

            // In a real implementation, you would check:
            // var isEnabled = await featureFlagService.IsEnabledAsync(simpleFlag.Key, userContext);
            // But for this example, we'll simulate the result:
            Console.WriteLine($"Feature '{simpleFlag.Key}' is enabled for user {userContext.UserId}: True");

            // Example 2: A/B Test with metrics
            Console.WriteLine("\n2. A/B Test with Metrics");
            Console.WriteLine("-----------------------");

            var experimentFlag = new FeatureFlag
            {
                Key = "checkout-redesign",
                RolloutType = RolloutType.ABTest,
                IsEnabled = true,
                EnableMetrics = true, // NEW in v2.0 - Enable metrics collection
                Variants = new[]
                {
                    new ABTestVariant
                    {
                        Name = "Control",
                        AllocationPercentage = 50,
                        Description = "Original checkout"
                    },
                    new ABTestVariant
                    {
                        Name = "Treatment",
                        AllocationPercentage = 50,
                        Description = "New design"
                    }
                }
            };

            // In a real implementation, you would get the variant:
            // var variant = await featureFlagService.GetVariantAsync(experimentFlag.Key, userContext);
            var variant = new { Name = "Treatment", Description = "New design" };
            Console.WriteLine($"Assigned to variant: {variant.Name} ({variant.Description})");

            // Example 3: Tracking conversions (NEW in v2.0)
            Console.WriteLine("\n3. Tracking Conversions");
            Console.WriteLine("---------------------");

            // Simulate a conversion
            var orderTotal = 99.99m;
            Console.WriteLine($"User completed purchase. Order total: ${orderTotal}");

            // In a real implementation:
            // await experimentService.TrackConversionAsync(
            //     flagId: experimentFlag.Id,
            //     variantName: "Treatment",
            //     userContext: userContext,
            //     conversionValue: orderTotal);

            // Example 4: Getting experiment metrics (NEW in v2.0)
            Console.WriteLine("\n4. Experiment Metrics");
            Console.WriteLine("-------------------");

            // In a real implementation:
            // var metrics = await experimentService.GetExperimentMetricsAsync(experimentFlag.Id);
            // For this example, we'll show sample output:
            Console.WriteLine("Experiment Metrics:");
            Console.WriteLine("  Total Assignments: 1000");
            Console.WriteLine("  Total Conversions: 250");
            Console.WriteLine("  Conversion Rate: 25.00%");
            Console.WriteLine("  Variant Metrics:");
            Console.WriteLine("    Control: 500 assignments, 120 conversions (24.00%)");
            Console.WriteLine("    Treatment: 500 assignments, 130 conversions (26.00%)");

            // Example 5: Gradual rollout with metrics (NEW in v2.0)
            Console.WriteLine("\n5. Gradual Rollout with Metrics");
            Console.WriteLine("--------------------------------");

            var rolloutFlag = new FeatureFlag
            {
                Key = "new-api",
                RolloutType = RolloutType.Percentage,
                PercentageRollout = 10,
                EnableMetrics = true,
                RolloutStrategy = new RolloutStrategy
                {
                    StartPercentage = 10,
                    EndPercentage = 100,
                    DailyIncrementPercentage = 15,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(7)
                }
            };

            // In a real implementation:
            // var progress = await rolloutService.GetRolloutProgressAsync(rolloutFlag.Id);
            // For this example, we'll show sample output:
            Console.WriteLine("Rollout Progress:");
            Console.WriteLine($"  Current: 10% (started)");
            Console.WriteLine($"  Target: 100%");
            Console.WriteLine($"  Days Remaining: 7");

            Console.WriteLine("\nExample completed!");
        }
    }
}