# DatabaseSeeder

The `DatabaseSeeder` type provides static asynchronous helpers for populating, clearing, and querying the feature‑flag database used by the *dotnet-feature-flags* project. It also exposes read‑only integer properties that represent the latest statistics retrieved from the database.

## API

### SeedSampleDataAsync
- **Purpose**: Inserts a realistic set of sample feature flags, rules, conditions, variants, and audit logs into the database. Intended for demonstration, UI testing, or development environments.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the sample data has been persisted.
- **Exceptions**: 
  - `DbUpdateException` if the underlying database operation fails.
  - `InvalidOperationException` if the application’s `DbContext` has not been configured.

### SeedMinimalDataAsync
- **Purpose**: Inserts the smallest viable dataset required for the application to start (e.g., a single default feature flag). Useful for automated tests that need a clean but functional state.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the minimal data has been persisted.
- **Exceptions**: Same as `SeedSampleDataAsync`.

### ClearDatabaseAsync
- **Purpose**: Removes all rows from the feature‑flag tables, effectively resetting the database to an empty state.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the truncation operation finishes.
- **Exceptions**: 
  - `DbUpdateException` if a deletion fails.
  - `InvalidOperationException` if the database connection is unavailable.

### SeedPerformanceTestDataAsync
- **Purpose**: Populates the database with a large volume of feature flags, rules, and associated data to stress‑test query performance and API latency.
- **Parameters**: None.
- **Return Value**: A `Task` that completes when the performance dataset has been inserted.
- **Exceptions**: Same as `SeedSampleDataAsync`.

### GetStatisticsAsync
- **Purpose**: Queries the database for aggregate counts of various entities and returns them encapsulated in a `DatabaseStatistics` object.
- **Parameters**: None.
- **Return Value**: A `Task<DatabaseStatistics>`; the result contains the following properties:
  - `TotalFeatureFlags`
  - `EnabledFlags`
  - `DisabledFlags`
  - `TotalRules`
  - `TotalConditions`
  - `TotalVariants`
  - `TotalAuditLogs`
  - `PercentageRolloutCount`
  - `RulesBasedCount`
  - `ABTestCount`
- **Exceptions**: 
  - `DbUpdateException` if the query cannot be executed.
  - `InvalidOperationException` if the context is not initialized.

### TotalFeatureFlags
- **Purpose**: Gets the total number of feature‑flag records present in the database (as returned by the last call to `GetStatisticsAsync`).
- **Return Value**: `int` count.
- **Exceptions**: None.

### EnabledFlags
- **Purpose**: Gets the number of feature flags that are currently enabled.
- **Return Value**: `int` count.
- **Exceptions**: None.

### DisabledFlags
- **Purpose**: Gets the number of feature flags that are currently disabled.
- **Return Value**: `int` count.
- **Exceptions**: None.

### TotalRules
- **Purpose**: Gets the total number of rule records associated with feature flags.
- **Return Value**: `int` count.
- **Exceptions**: None.

### TotalConditions
- **Purpose**: Gets the total number of condition records within all rules.
- **Return Value**: `int` count.
- **Exceptions**: None.

### TotalVariants
- **Purpose**: Gets the total number of variant records (used for A/B testing and percentage rollouts).
- **Return Value**: `int` count.
- **Exceptions**: None.

### TotalAuditLogs
- **Purpose**: Gets the total number of audit‑log entries stored for feature‑flag changes.
- **Return Value**: `int` count.
- **Exceptions**: None.

### PercentageRolloutCount
- **Purpose**: Gets the number of feature flags configured with a percentage rollout filter.
- **Return Value**: `int` count.
- **Exceptions**: None.

### RulesBasedCount
- **Purpose**: Gets the number of feature flags that use at least one rule‑based filter.
- **Return Value**: `int` count.
- **Exceptions**: None.

### ABTestCount
- **Purpose**: Gets the number of feature flags configured as A/B tests.
- **Return Value**: `int` count.
- **Exceptions**: None.

## Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using DotNetFeatureFlags.Data; // Namespace containing DatabaseSeeder

// Example 1: Seed sample data at application startup
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<FeatureFlagContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("Default")));
        // other service registrations …
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    await DatabaseSeeder.SeedSampleDataAsync();
}
host.Run();
```

```csharp
using DotNetFeatureFlags.Data;

// Example 2: Retrieve and display statistics after seeding
var stats = await DatabaseSeeder.GetStatisticsAsync();

Console.WriteLine($"Feature Flags: {stats.TotalFeatureFlags}");
Console.WriteLine($"Enabled: {stats.EnabledFlags}, Disabled: {stats.DisabledFlags}");
Console.WriteLine($"Rules: {stats.TotalRules}, Conditions: {stats.TotalConditions}");
Console.WriteLine($"Variants: {stats.TotalVariants}");
Console.WriteLine($"Audit Logs: {stats.TotalAuditLogs}");
Console.WriteLine($"Percentage Rollouts: {stats.PercentageRolloutCount}");
Console.WriteLine($"Rules‑Based: {stats.RulesBasedCount}");
Console.WriteLine($"A/B Tests: {stats.ABTestCount}");
```

## Notes

- The static methods operate on the application’s configured `DbContext`. If multiple threads invoke seeding or clearing methods concurrently, race conditions may lead to duplicate keys or incomplete transactions; external synchronization is recommended when concurrent calls are required.
- The integer properties (`TotalFeatureFlags`, `EnabledFlags`, etc.) reflect the values returned by the most recent call to `GetStatisticsAsync`. They are **not** automatically updated when the database is modified by other means; calling `GetStatisticsAsync` again is necessary to obtain fresh counts.
- Calling any of the `Seed*DataAsync` methods repeatedly without clearing the database first may result in duplicate records, as the methods do not perform existence checks before inserting.
- `ClearDatabaseAsync` removes all data indiscriminately; it should be used only in test or development scenarios where data loss is acceptable.
- None of the members throw exceptions for invalid arguments because they accept no parameters; all error conditions relate to database access or configuration issues.
