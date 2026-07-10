# PerformanceMonitor
The `PerformanceMonitor` class is designed to measure and track the performance of operations within an application. It provides a simple and efficient way to collect statistics on the execution time of methods, allowing developers to identify performance bottlenecks and optimize their code.

## API
### Constructors
* `public PerformanceMonitor()`: Initializes a new instance of the `PerformanceMonitor` class.

### Methods
* `public void Stop()`: Stops the performance monitoring.
* `public void Dispose()`: Releases all resources used by the `PerformanceMonitor` instance.
* `public static T Measure<T>(...)`: Measures the execution time of a method and returns the result of the method invocation.
* `public static async Task<T> MeasureAsync<T>(...)`: Asynchronously measures the execution time of a method and returns the result of the method invocation.
* `public static async Task MeasureAsync(...)`: Asynchronously measures the execution time of a method.
* `public void RecordOperation(...)`: Records the execution time of an operation.
* `public OperationStats? GetStatistics(...)`: Retrieves the statistics for a specific operation.
* `public List<OperationStats> GetAllStatistics(...)`: Retrieves the statistics for all operations.
* `public void Clear(...)`: Clears the performance monitoring data.

### Properties
* `public string OperationName { get; }`: Gets the name of the operation being monitored.
* `public int CallCount { get; }`: Gets the number of times the operation has been called.
* `public long AverageMs { get; }`: Gets the average execution time of the operation in milliseconds.
* `public long MinMs { get; }`: Gets the minimum execution time of the operation in milliseconds.
* `public long MaxMs { get; }`: Gets the maximum execution time of the operation in milliseconds.
* `public long P95Ms { get; }`: Gets the 95th percentile execution time of the operation in milliseconds.
* `public long P99Ms { get; }`: Gets the 99th percentile execution time of the operation in milliseconds.

## Usage
The following examples demonstrate how to use the `PerformanceMonitor` class to measure the performance of methods:
```csharp
// Example 1: Measuring the execution time of a synchronous method
var monitor = new PerformanceMonitor();
var result = PerformanceMonitor.Measure(() => MyMethod());
Console.WriteLine($"Execution time: {monitor.AverageMs}ms");

// Example 2: Measuring the execution time of an asynchronous method
var monitor = new PerformanceMonitor();
await PerformanceMonitor.MeasureAsync(async () => await MyAsyncMethod());
Console.WriteLine($"Execution time: {monitor.AverageMs}ms");
```

## Notes
* The `PerformanceMonitor` class is not thread-safe. If you need to monitor performance across multiple threads, you should create a separate instance of the class for each thread.
* The `Measure` and `MeasureAsync` methods will throw an exception if the method being measured throws an exception.
* The `RecordOperation` method will throw an exception if the operation name is null or empty.
* The `GetStatistics` and `GetAllStatistics` methods will return null if no statistics are available for the specified operation.
* The `Clear` method will reset all performance monitoring data, including the call count and execution time statistics.
