# GradualRolloutSchedulerWorker

The `GradualRolloutSchedulerWorker` is a background service designed to periodically evaluate and apply gradual feature flag rollout rules. It ensures that feature flag state updates, which rely on time-based or incremental changes, are processed and synchronized across the application at a defined interval, enabling controlled and phased feature releases.

## API

### GradualRolloutSchedulerWorker()
Initializes a new instance of the `GradualRolloutSchedulerWorker` class.

### CheckIntervalMinutes
`public int CheckIntervalMinutes { get; set; }`
Gets or sets the frequency, in minutes, at which the worker performs its update cycle. Setting this value dictates how often the scheduler checks for new rollout rule evaluations.

### Enabled
`public bool Enabled { get; set; }`
Gets or sets a value indicating whether the background worker is currently active. If `false`, the worker will skip its update cycles, allowing temporary suspension of rollout processing.

### AddGradualRolloutScheduler
`public static IServiceCollection AddGradualRolloutScheduler(this IServiceCollection services)`
An extension method for `IServiceCollection` used to register the `GradualRolloutSchedulerWorker` and its required dependencies into the application's service container.
*   **Parameters:** `services` - The `IServiceCollection` to add the service to.
*   **Returns:** The modified `IServiceCollection` instance.

## Usage

### Registering the service in the DI container
```csharp
using Microsoft.Extensions.DependencyInjection;
using YourProject.Namespace;

var builder = Host.CreateApplicationBuilder(args);

// Register the Gradual Rollout Scheduler
builder.Services.AddGradualRolloutScheduler();

var host = builder.Build();
host.Run();
```

### Adjusting scheduler settings at runtime
```csharp
public class FeatureManagementController : ControllerBase
{
    private readonly GradualRolloutSchedulerWorker _scheduler;

    public FeatureManagementController(GradualRolloutSchedulerWorker scheduler)
    {
        _scheduler = scheduler;
    }

    [HttpPost("pause-updates")]
    public IActionResult PauseUpdates()
    {
        // Temporarily disable the background rollout scheduler
        _scheduler.Enabled = false;
        return Ok();
    }
}
```

## Notes

*   **Thread Safety:** The `Enabled` and `CheckIntervalMinutes` properties are intended to be accessed or modified at runtime. Implementations of this worker must ensure that these properties are accessed in a thread-safe manner if they are updated while the background task is running.
*   **Interval Constraints:** While `CheckIntervalMinutes` accepts any valid integer, setting this value to zero or a negative number may result in undefined behavior or tight-loop execution depending on the internal implementation of the background task. It is recommended to use positive, non-zero values.
*   **Registration:** The `AddGradualRolloutScheduler` method assumes that necessary underlying feature management services have been registered. Failure to register dependencies may lead to runtime exceptions when the `GradualRolloutSchedulerWorker` starts.
