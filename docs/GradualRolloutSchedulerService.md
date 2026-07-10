# GradualRolloutSchedulerService
The `GradualRolloutSchedulerService` class is designed to manage and execute gradual rollout schedules for feature flags. It provides methods to process scheduled rollouts, retrieve the status of a rollout schedule, and advance a rollout. This service is intended to be used in scenarios where features need to be rolled out gradually to a subset of users over a period of time.

## API
### Constructors
* `public GradualRolloutSchedulerService`: Initializes a new instance of the `GradualRolloutSchedulerService` class.

### Methods
* `public async Task<int> ProcessScheduledRolloutsAsync`: Processes all scheduled rollouts. Returns the number of rollouts processed. This method may throw exceptions if there are issues with the underlying data storage or if the rollout schedules are invalid.
* `public async Task<RolloutScheduleStatus?> GetScheduleStatusAsync`: Retrieves the status of a rollout schedule. Returns the status of the rollout schedule, or `null` if the schedule is not found. This method may throw exceptions if there are issues with the underlying data storage.
* `public async Task<bool> AdvanceRolloutAsync`: Advances a rollout to the next stage. Returns `true` if the rollout was advanced successfully, `false` otherwise. This method may throw exceptions if there are issues with the underlying data storage or if the rollout is not in a valid state to be advanced.

## Usage
The following examples demonstrate how to use the `GradualRolloutSchedulerService` class:
```csharp
// Example 1: Process all scheduled rollouts
var schedulerService = new GradualRolloutSchedulerService();
var rolloutsProcessed = await schedulerService.ProcessScheduledRolloutsAsync();
Console.WriteLine($"Processed {rolloutsProcessed} rollouts");
```

```csharp
// Example 2: Get the status of a rollout schedule and advance it if it's not complete
var schedulerService = new GradualRolloutSchedulerService();
var scheduleStatus = await schedulerService.GetScheduleStatusAsync();
if (scheduleStatus != null && scheduleStatus.Status != RolloutScheduleStatus.Complete)
{
    var advanced = await schedulerService.AdvanceRolloutAsync();
    if (advanced)
    {
        Console.WriteLine("Rollout advanced successfully");
    }
    else
    {
        Console.WriteLine("Failed to advance rollout");
    }
}
```

## Notes
The `GradualRolloutSchedulerService` class is designed to be thread-safe, allowing it to be used concurrently from multiple threads. However, it's still important to ensure that the underlying data storage is also thread-safe to avoid any potential issues. Additionally, the `ProcessScheduledRolloutsAsync` method may throw exceptions if there are issues with the underlying data storage, so it's recommended to handle these exceptions accordingly. The `GetScheduleStatusAsync` and `AdvanceRolloutAsync` methods may also throw exceptions if the rollout schedule is not found or if the rollout is not in a valid state to be advanced, so it's recommended to check for these conditions before calling these methods.
