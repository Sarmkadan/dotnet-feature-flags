# HealthController
The `HealthController` class is designed to provide information about the health and status of an application, including its liveness, readiness, and version. It is typically used in scenarios where the application's health needs to be monitored, such as in a Kubernetes environment. The class provides a simple way to check the application's status and dependencies, making it easier to diagnose and troubleshoot issues.

## API
* `HealthController`: The constructor for the `HealthController` class.
* `GetLiveness`: Returns an `IActionResult` indicating whether the application is alive and responding to requests.
* `GetReadiness`: Returns an asynchronous `IActionResult` indicating whether the application is ready to handle requests. This method checks the application's dependencies and returns a result based on their status.
* `Status`: A string property that returns the current status of the application.
* `Timestamp`: A `DateTime` property that returns the timestamp when the application started.
* `Version`: A string property that returns the version of the application.
* `Uptime`: A string property that returns the uptime of the application.
* `Dependencies`: A `Dictionary<string, bool>` property that returns a dictionary of the application's dependencies and their status.

## Usage
The following examples demonstrate how to use the `HealthController` class:
```csharp
// Example 1: Checking the application's liveness
var healthController = new HealthController();
var result = healthController.GetLiveness();
if (result != null)
{
    Console.WriteLine("Application is alive and responding to requests.");
}
```

```csharp
// Example 2: Checking the application's readiness and dependencies
var healthController = new HealthController();
var readinessResult = await healthController.GetReadiness();
if (readinessResult != null)
{
    Console.WriteLine("Application is ready to handle requests.");
    var dependencies = healthController.Dependencies;
    if (dependencies != null)
    {
        foreach (var dependency in dependencies)
        {
            Console.WriteLine($"{dependency.Key}: {dependency.Value}");
        }
    }
}
```

## Notes
The `HealthController` class is designed to be thread-safe, and its properties and methods can be accessed concurrently without fear of data corruption or other threading issues. However, the `GetReadiness` method may throw an exception if there is an issue checking the application's dependencies. Additionally, the `Dependencies` property may return null if the application's dependencies have not been initialized or if there is an issue retrieving their status. It is also worth noting that the `Uptime` property returns a string representation of the application's uptime, which may not be suitable for all use cases. In such cases, the `Timestamp` property can be used to calculate the uptime programmatically.
