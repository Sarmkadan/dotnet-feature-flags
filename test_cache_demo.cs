// This is a demonstration file showing how the cache works
// It's not meant to be compiled or run, just for documentation

using System;
using System.Threading.Tasks;
using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Example usage:
public class CacheDemo
{
    public static async Task Demo()
    {
        // Setup DI (normally done in Program.cs)
        var services = new ServiceCollection();

        services.AddLogging(configure => configure.AddConsole());
        services.AddFeatureFlagServices(null!); // Would normally pass configuration

        var serviceProvider = services.BuildServiceProvider();

        // Get services
        var featureFlagService = serviceProvider.GetRequiredService<IFeatureFlagService>();
        var featureFlagCache = serviceProvider.GetRequiredService<IFeatureFlagCache>();

        var userContext = new UserContext
        {
            UserId = "user123",
            Email = "user@example.com"
        };

        // First call - cache miss, hits database
        bool result1 = await featureFlagService.IsEnabledAsync("my-feature", userContext);
        Console.WriteLine($"First call result: {result1}");

        // Second call - cache hit, no database query
        bool result2 = await featureFlagService.IsEnabledAsync("my-feature", userContext);
        Console.WriteLine($"Second call result: {result2}");

        // Cache statistics (if available)
        // In a real application, you might add metrics here

        // Manual cache invalidation
        featureFlagCache.Invalidate("my-feature");
        Console.WriteLine("Cache invalidated for 'my-feature'");

        // Third call - cache miss again, hits database
        bool result3 = await featureFlagService.IsEnabledAsync("my-feature", userContext);
        Console.WriteLine($"Third call result after invalidation: {result3}");
    }
}

// Expected output:
// First call result: true/false
// Second call result: true/false (same as first, but from cache)
// Cache invalidated for 'my-feature'
// Third call result after invalidation: true/false (from database again)
