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
/// Example: Integrating feature flags into a real application.
/// This demonstrates how to use flags in actual business logic
/// like checkout, recommendations, and notifications.
/// </summary>
public sealed class ApplicationIntegrationExample
{
    private readonly IFeatureFlagService _flagService;

    public ApplicationIntegrationExample(IFeatureFlagService flagService)
    {
        _flagService = flagService;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== Application Integration Examples ===\n");

        await CheckoutProcessAsync();
        await RecommendationEngineAsync();
        await NotificationSystemAsync();
    }

    private async Task CheckoutProcessAsync()
    {
        Console.WriteLine("1. Checkout Process Integration\n");

        // Setup: Create the flag
        var flag = new FeatureFlag
        {
            Key = "new-checkout-gateway",
            DisplayName = "New Payment Gateway",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 30,
            IsEnabled = true
        };
        await _flagService.CreateFeatureFlagAsync(flag);

        Console.WriteLine("Processing order for user...\n");

        // Simulate checkout
        var order = new OrderModel
        {
            Id = "ORDER-001",
            UserId = "user-shop-001",
            Total = 99.99m,
            Items = 3
        };

        var userContext = new UserContext
        {
            UserId = order.UserId,
            Email = "customer@example.com",
            Tier = "premium"
        };

        // Check which payment gateway to use
        bool useNewGateway = await _flagService.IsEnabledAsync("new-checkout-gateway", userContext);

        var paymentProcessor = useNewGateway
            ? new NewPaymentGateway()
            : (IPaymentProcessor)new LegacyPaymentGateway();

        Console.WriteLine($"User: {order.UserId}");
        Console.WriteLine($"Order Total: ${order.Total}");
        Console.WriteLine($"Payment Processor: {paymentProcessor.GetType().Name}");

        try
        {
            var result = await paymentProcessor.ProcessPaymentAsync(order.Total);
            Console.WriteLine($"✓ Payment successful: {result.TransactionId}\n");
        }
        catch
        {
            Console.WriteLine("✗ Payment failed\n");
        }
    }

    private async Task RecommendationEngineAsync()
    {
        Console.WriteLine("2. Recommendation Engine\n");

        // Setup: Create the flag
        var flag = new FeatureFlag
        {
            Key = "ml-recommendations",
            DisplayName = "ML-Based Product Recommendations",
            RolloutType = RolloutType.RulesBased,
            IsEnabled = true,
            Rules = new[]
            {
                new Rule
                {
                    Name = "Power Users",
                    Priority = 1,
                    ConditionLogic = "AND",
                    Conditions = new[]
                    {
                        new Condition
                        {
                            Attribute = "purchase_count",
                            Operator = ConditionOperator.GreaterThan,
                            Value = "10"
                        }
                    }
                }
            }
        };
        await _flagService.CreateFeatureFlagAsync(flag);

        var users = new[]
        {
            ("power-buyer", new UserContext
            {
                UserId = "power-buyer",
                CustomAttributes = new Dictionary<string, string> { ["purchase_count"] = "25" }
            }),
            ("casual-shopper", new UserContext
            {
                UserId = "casual-shopper",
                CustomAttributes = new Dictionary<string, string> { ["purchase_count"] = "3" }
            })
        };

        Console.WriteLine("Loading recommendations for users:\n");

        foreach (var (name, context) in users)
        {
            bool useMLRecommendations = await _flagService.IsEnabledAsync("ml-recommendations", context);

            var recommender = useMLRecommendations
                ? new MLRecommendationEngine()
                : (IRecommendationEngine)new RuleBasedRecommendationEngine();

            var recommendations = await recommender.GetRecommendationsAsync(context.UserId);

            Console.WriteLine($"  {name}:");
            Console.WriteLine($"    Engine: {recommender.GetType().Name}");
            Console.WriteLine($"    Products: {string.Join(", ", recommendations.Take(3))}");
        }

        Console.WriteLine();
    }

    private async Task NotificationSystemAsync()
    {
        Console.WriteLine("3. Notification System\n");

        var flag = new FeatureFlag
        {
            Key = "ai-summaries",
            DisplayName = "AI-Generated Email Summaries",
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 20,
            IsEnabled = true
        };
        await _flagService.CreateFeatureFlagAsync(flag);

        var notificationChannels = new[] { "user-001", "user-002", "user-003" };

        Console.WriteLine("Generating notifications:\n");

        foreach (var userId in notificationChannels)
        {
            var context = new UserContext { UserId = userId, Email = $"{userId}@example.com" };
            var useAISummary = await _flagService.IsEnabledAsync("ai-summaries", context);

            var summaryType = useAISummary ? "AI-Generated" : "Template-Based";
            Console.WriteLine($"  {userId}@example.com: {summaryType}");
        }
    }
}

// Mock models and interfaces for the example
public sealed class OrderModel
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public decimal Total { get; set; }
    public int Items { get; set; }
}

public sealed class PaymentResult
{
    public string TransactionId { get; set; }
    public bool Success { get; set; }
}

public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount);
}

{public sealed class LegacyPaymentGateway {
    public Task<PaymentResult> ProcessPaymentAsync(decimal amount)
    {
        return Task.FromResult(new PaymentResult
        {
            TransactionId = $"LEGACY-{Guid.NewGuid().ToString().Substring(0, 8)}",
            Success = true
        });
    }
}

{public sealed class NewPaymentGateway {
    public Task<PaymentResult> ProcessPaymentAsync(decimal amount)
    {
        return Task.FromResult(new PaymentResult
        {
            TransactionId = $"NEW-{Guid.NewGuid().ToString().Substring(0, 8)}",
            Success = true
        });
    }
}

public interface IRecommendationEngine
{
    Task<string[]> GetRecommendationsAsync(string userId);
}

{public sealed class RuleBasedRecommendationEngine {
    public Task<string[]> GetRecommendationsAsync(string userId)
    {
        return Task.FromResult(new[] { "Product-A", "Product-B", "Product-C" });
    }
}

{public sealed class MLRecommendationEngine {
    public Task<string[]> GetRecommendationsAsync(string userId)
    {
        return Task.FromResult(new[] { "ML-Product-X", "ML-Product-Y", "ML-Product-Z" });
    }
}
