using System;
using System.Threading.Tasks;
using FeatureFlags.Models;
using FeatureFlags.Services;

namespace FeatureFlags.Examples
{
    /// <summary>
    /// Demonstrates the basic usage of the feature flag service.
    /// </summary>
    public class BasicUsage
    {
        private readonly IFeatureFlagService _featureFlagService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicUsage"/> class.
        /// </summary>
        /// <param name="featureFlagService">The feature flag service.</param>
        public BasicUsage(IFeatureFlagService featureFlagService)
        {
            _featureFlagService = featureFlagService;
        }

        /// <summary>
        /// Runs the example.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RunExampleAsync()
        {
            // 1. Create a user context for evaluation
            var userContext = new UserContext
            {
                UserId = "user-123",
                Email = "user@example.com"
            };

            // 2. Evaluate a flag
            // Check if 'new-feature-enabled' is enabled for this user
            bool isEnabled = await _featureFlagService.IsEnabledAsync("new-feature-enabled", userContext);

            if (isEnabled)
            {
                Console.WriteLine("New feature is enabled for this user.");
            }
            else
            {
                Console.WriteLine("New feature is disabled for this user.");
            }
        }
    }
}
