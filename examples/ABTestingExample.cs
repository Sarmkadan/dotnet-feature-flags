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
/// Example: A/B testing with multiple variants and conversion tracking.
/// This demonstrates how to run controlled experiments with variant
/// allocation and measure conversion rates.
/// </summary>
public class ABTestingExample
{
    private readonly IFeatureFlagService _featureFlagService;

    public ABTestingExample(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== A/B Testing Example ===\n");

        // Create an A/B test for checkout flow
        await CheckoutFlowTestAsync();

        // Create a multivariate test (3 variants)
        await MultivariateTestAsync();

        // Track conversion metrics
        await ConversionTrackingAsync();
    }

    private async Task CheckoutFlowTestAsync()
    {
        Console.WriteLine("1. Simple A/B Test: Checkout Flow\n");

        var flag = new FeatureFlag
        {
            Key = "checkout-redesign",
            DisplayName = "Checkout Flow A/B Test",
            Description = "Testing new checkout design vs original",
            RolloutType = RolloutType.ABTest,
            IsEnabled = true,
            Variants = new[]
            {
                new ABTestVariant
                {
                    Name = "Control",
                    Description = "Original checkout experience",
                    AllocationPercentage = 50
                },
                new ABTestVariant
                {
                    Name = "Treatment",
                    Description = "New streamlined checkout",
                    AllocationPercentage = 50
                }
            }
        };

        var created = await _featureFlagService.CreateFeatureFlagAsync(flag);
        Console.WriteLine($"✓ Created A/B test: {created.Key}");
        Console.WriteLine($"  Control: 50%, Treatment: 50%\n");

        // Simulate user variant allocation
        var allocationDistribution = new Dictionary<string, int> { ["Control"] = 0, ["Treatment"] = 0 };

        Console.WriteLine("Allocating 100 users:");
        for (int i = 0; i < 100; i++)
        {
            var context = new UserContext { UserId = $"user{i:D3}" };
            var variant = await _featureFlagService.GetVariantAsync("checkout-redesign", context);
            allocationDistribution[variant.Name]++;
        }

        Console.WriteLine($"  Control: {allocationDistribution["Control"]}%");
        Console.WriteLine($"  Treatment: {allocationDistribution["Treatment"]}%");
        Console.WriteLine("  (Expected: 50/50)\n");
    }

    private async Task MultivariateTestAsync()
    {
        Console.WriteLine("2. Multivariate Test: Landing Page Designs\n");

        var flag = new FeatureFlag
        {
            Key = "landing-page-mvt",
            DisplayName = "Landing Page Multivariate Test",
            RolloutType = RolloutType.ABTest,
            IsEnabled = true,
            Variants = new[]
            {
                new ABTestVariant
                {
                    Name = "Control",
                    Description = "Current design",
                    AllocationPercentage = 34
                },
                new ABTestVariant
                {
                    Name = "Variant_A",
                    Description = "Design A - Bold colors",
                    AllocationPercentage = 33
                },
                new ABTestVariant
                {
                    Name = "Variant_B",
                    Description = "Design B - Minimal style",
                    AllocationPercentage = 33
                }
            ]
        };

        var created = await _featureFlagService.CreateFeatureFlagAsync(flag);
        Console.WriteLine($"✓ Created MVT: {created.Key}");
        Console.WriteLine($"  Control: 34%, Variant_A: 33%, Variant_B: 33%\n");

        // Show variant allocation
        var variants = new[] { "user-100", "user-101", "user-102", "user-103", "user-104" };
        Console.WriteLine("Sample user allocations:");
        foreach (var userId in variants)
        {
            var context = new UserContext { UserId = userId };
            var variant = await _featureFlagService.GetVariantAsync("landing-page-mvt", context);
            Console.WriteLine($"  {userId}: {variant.Name} ({variant.Description})");
        }

        Console.WriteLine();
    }

    private async Task ConversionTrackingAsync()
    {
        Console.WriteLine("3. Conversion Tracking\n");

        var flag = new FeatureFlag
        {
            Key = "pricing-page-test",
            DisplayName = "Pricing Page A/B Test",
            RolloutType = RolloutType.ABTest,
            IsEnabled = true,
            Variants = new[]
            {
                new ABTestVariant
                {
                    Name = "Control",
                    AllocationPercentage = 50
                },
                new ABTestVariant
                {
                    Name = "HigherPrice",
                    AllocationPercentage = 50
                }
            ]
        };

        var created = await _featureFlagService.CreateFeatureFlagAsync(flag);

        // Simulate user interactions
        Console.WriteLine("Simulating 1000 user sessions:\n");

        var conversionRates = new Dictionary<string, (int assignments, int conversions)>
        {
            ["Control"] = (0, 0),
            ["HigherPrice"] = (0, 0)
        };

        var random = new Random(42);

        for (int i = 0; i < 1000; i++)
        {
            var userId = $"session-{i}";
            var context = new UserContext { UserId = userId };
            var variant = await _featureFlagService.GetVariantAsync("pricing-page-test", context);

            // Track assignment
            conversionRates[variant.Name].assignments++;

            // Simulate conversion (Control: 25%, HigherPrice: 18%)
            double conversionChance = variant.Name == "Control" ? 0.25 : 0.18;
            if (random.NextDouble() < conversionChance)
            {
                conversionRates[variant.Name].conversions++;
            }
        }

        Console.WriteLine("Results:");
        foreach (var (variant, (assignments, conversions)) in conversionRates)
        {
            double rate = (double)conversions / assignments * 100;
            Console.WriteLine($"  {variant}:");
            Console.WriteLine($"    Assignments: {assignments}");
            Console.WriteLine($"    Conversions: {conversions}");
            Console.WriteLine($"    Conversion Rate: {rate:F2}%");
        }

        // Calculate lift
        double controlRate = (double)conversionRates["Control"].conversions / conversionRates["Control"].assignments;
        double treatmentRate = (double)conversionRates["HigherPrice"].conversions / conversionRates["HigherPrice"].assignments;
        double lift = (treatmentRate - controlRate) / controlRate * 100;

        Console.WriteLine($"\n  Lift: {lift:F2}%");
        Console.WriteLine($"  Winner: {(lift > 0 ? "HigherPrice (but negative)" : "Control")}");
    }
}
