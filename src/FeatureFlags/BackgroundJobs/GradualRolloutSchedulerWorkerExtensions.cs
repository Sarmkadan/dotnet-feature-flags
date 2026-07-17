#nullable enable
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FeatureFlags.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.BackgroundJobs;

/// <summary>
/// Extension methods that add useful introspection and one‑off execution capabilities
/// to <see cref="GradualRolloutSchedulerWorker"/> without modifying the original class.
/// </summary>
/// <remarks>
/// This static class cannot be inherited as it contains only extension methods.
/// </remarks>
public static class GradualRolloutSchedulerWorkerExtensions
{
    /// <summary>
    /// Retrieves the interval that the worker uses between successive checks.
    /// </summary>
    /// <param name="worker">The worker instance.</param>
    /// <returns>The <see cref="TimeSpan"/> configured for the check interval.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">The private field <c>_checkInterval</c> could not be found or contains an invalid value.</exception>
    public static TimeSpan GetCheckInterval(this GradualRolloutSchedulerWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        var field = typeof(GradualRolloutSchedulerWorker)
            .GetField("_checkInterval", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate the private field '_checkInterval'.");

        var value = field.GetValue(worker);
        return value switch
        {
            TimeSpan timeSpan => timeSpan,
            null => throw new InvalidOperationException("The '_checkInterval' field is null."),
            _ => throw new InvalidOperationException("The '_checkInterval' field contains an invalid value.")
        };
    }

    /// <summary>
    /// Determines whether the worker is currently enabled according to its configuration options.
    /// </summary>
    /// <param name="worker">The worker instance.</param>
    /// <returns><c>true</c> if the scheduler is enabled; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The private field <c>_options</c> could not be found, is null, or its <c>Enabled</c> property could not be found.
    /// </exception>
    public static bool IsEnabled(this GradualRolloutSchedulerWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        var optionsField = typeof(GradualRolloutSchedulerWorker)
            .GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate the private field '_options'.");

        var options = optionsField.GetValue(worker);
        if (options is null)
        {
            throw new InvalidOperationException("The '_options' field is null.");
        }

        var enabledProp = options.GetType()
            .GetProperty("Enabled", BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException("Unable to locate the 'Enabled' property on options.");

        var enabledValue = enabledProp.GetValue(options);
        return enabledValue switch
        {
            bool enabled => enabled,
            null => throw new InvalidOperationException("The 'Enabled' property is null."),
            _ => throw new InvalidOperationException("The 'Enabled' property contains an invalid value.")
        };
    }

    /// <summary>
    /// Executes a single rollout‑advancement cycle immediately, bypassing the regular timer.
    /// </summary>
    /// <param name="worker">The worker instance.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation. The default is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// The number of feature flags that were updated during the cycle.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The private field <c>_serviceProvider</c> could not be found or is null.
    /// </exception>
    /// <remarks>
    /// This method creates a new service scope for each invocation, ensuring proper disposal of scoped services.
    /// </remarks>
    public static async Task<int> RunImmediateAsync(
        this GradualRolloutSchedulerWorker worker,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(worker);

        var providerField = typeof(GradualRolloutSchedulerWorker)
            .GetField("_serviceProvider", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate the private field '_serviceProvider'.");

        var serviceProvider = providerField.GetValue(worker) as IServiceProvider;
        if (serviceProvider is null)
        {
            throw new InvalidOperationException("The '_serviceProvider' field is null or not of type IServiceProvider.");
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<IGradualRolloutSchedulerService>();

        return await scheduler.ProcessScheduledRolloutsAsync(cancellationToken).ConfigureAwait(false);
    }
}
