using System;
using System.Threading.Tasks;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Services;

namespace FeatureFlags.Examples
{
    // Configuration, custom options, and error handling
    public class AdvancedUsage
    {
        private readonly IFeatureFlagService _featureFlagService;

        public AdvancedUsage(IFeatureFlagService featureFlagService)
        {
            _featureFlagService = featureFlagService;
        }

        public async Task RunExampleAsync()
        {
            try
            {
                // Create a context with custom attributes for complex targeting
                var userContext = new UserContext
                {
                    UserId = "user-789"
                };
                userContext.SetCustomAttribute("subscription", "pro");
                userContext.SetCustomAttribute("region", "eu-central");

                // Evaluate a flag that might use rules based on these custom attributes
                bool isEnabled = await _featureFlagService.IsEnabledAsync("advanced-analytics-dashboard", userContext);

                Console.WriteLine($"Advanced feature enabled: {isEnabled}");
            }
            catch (FeatureFlagException ex)
            {
                // Handle specific exceptions related to flag evaluation
                Console.WriteLine($"Error evaluating feature flag: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle general errors
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
