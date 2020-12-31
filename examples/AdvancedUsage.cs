using System;
using System.Threading.Tasks;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Services;

namespace FeatureFlags.Examples
{
    /// <summary>
    /// Demonstrates advanced usage of the feature flag service, including custom options and error handling.
    /// </summary>
    public class AdvancedUsage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedUsage"/> class.
        /// </summary>
        /// <param name="featureFlagService">The feature flag service to use.</param>
        public AdvancedUsage(IFeatureFlagService featureFlagService)
        {
            _featureFlagService = featureFlagService;
        }

        /// <summary>
        /// Runs the example, demonstrating how to evaluate a feature flag with custom attributes.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
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
