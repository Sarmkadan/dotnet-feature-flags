using Microsoft.Extensions.DependencyInjection;
using FeatureFlags.Configuration;
using FeatureFlags.Services;

namespace FeatureFlags.Examples
{
    // Showing how to wire into ASP.NET DI
    public class IntegrationExample
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register necessary services for Feature Flags
            // Assuming DependencyInjectionExtensions provides a setup method
            services.AddFeatureFlags(options => 
            {
                options.EnableCache = true;
                options.CacheDurationMinutes = 10;
            });

            // Now FeatureFlagService and other services can be injected into controllers/services
        }
    }
}
