using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.Models;

namespace FeatureFlags.Repository;

/// <summary>
/// Provides extension methods for the <see cref="FeatureFlagRepository"/> class.
/// </summary>
public static class FeatureFlagRepositoryExtensions
{
    /// <summary>
    /// Retrieves a feature flag by its key, only if it is currently enabled.
    /// </summary>
    /// <param name="repository">The feature flag repository instance.</param>
    /// <param name="key">The unique key of the feature flag.</param>
    /// <returns>The enabled feature flag, or <c>null</c> if it does not exist or is disabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public static async Task<FeatureFlag?> GetEnabledByKeyAsync(this FeatureFlagRepository repository, string key)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var flag = await repository.GetByKeyAsync(key);
        return flag != null && flag.IsEnabled ? flag : null;
    }

    /// <summary>
    /// Determines whether a feature flag exists and is currently enabled.
    /// </summary>
    /// <param name="repository">The feature flag repository instance.</param>
    /// <param name="key">The unique key of the feature flag.</param>
    /// <returns><c>true</c> if the flag exists and is enabled; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public static async Task<bool> IsKeyEnabledAsync(this FeatureFlagRepository repository, string key)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var flag = await repository.GetByKeyAsync(key);
        return flag != null && flag.IsEnabled;
    }

    /// <summary>
    /// Retrieves all enabled feature flags created by a specific user.
    /// </summary>
    /// <param name="repository">The feature flag repository instance.</param>
    /// <param name="creator">The username or ID of the creator.</param>
    /// <returns>A collection of enabled feature flags.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    /// <exception cref="ArgumentException">Thrown when creator is null or empty.</exception>
    public static async Task<IEnumerable<FeatureFlag>> GetEnabledByCreatorAsync(this FeatureFlagRepository repository, string creator)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(creator);

        var flags = await repository.GetByCreatorAsync(creator);
        return flags.Where(f => f.IsEnabled).ToList();
    }
}
