# FeatureFlagSearchBuilder

The `FeatureFlagSearchBuilder` class provides a fluent interface for constructing and executing queries against feature flags in the `dotnet-feature-flags` system. It enables programmatic filtering of feature flags by key, name, enabled status, rollout type, creation metadata, and pagination parameters, while maintaining a clean and composable API.

## API

### `WithKeyContaining(string keySubstring)`
Adds a filter to match feature flags whose key contains the specified substring. The comparison is case-sensitive.

*Parameters*
- `keySubstring`: The substring to search for within feature flag keys.

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

*Exceptions*
- Throws `ArgumentNullException` if `keySubstring` is `null`.

---

### `WithNameContaining(string nameSubstring)`
Adds a filter to match feature flags whose name contains the specified substring. The comparison is case-sensitive.

*Parameters*
- `nameSubstring`: The substring to search for within feature flag names.

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

*Exceptions*
- Throws `ArgumentNullException` if `nameSubstring` is `null`.

---
### `WithEnabledStatus(bool? enabled)`
Filters feature flags by their enabled status. Passing `null` removes any existing enabled status filter.

*Parameters*
- `enabled`: The enabled status to filter by (`true` for enabled, `false` for disabled, `null` to clear the filter).

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

---
### `WithRolloutType(RolloutType? rolloutType)`
Filters feature flags by their rollout type. Passing `null` removes any existing rollout type filter.

*Parameters*
- `rolloutType`: The rollout type to filter by (`RolloutType.Percentage`, `RolloutType.ABTest`, `RolloutType.RulesBased`, or `null` to clear the filter).

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

---
### `WithCreatedBy(string createdBy)`
Filters feature flags by the user who created them. Passing `null` or an empty string removes the filter.

*Parameters*
- `createdBy`: The username or identifier of the creator.

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

---
### `WithCreatedDateRange(DateTime? from, DateTime? to)`
Filters feature flags by their creation date range. Passing `null` for either parameter removes the respective bound.

*Parameters*
- `from`: The earliest creation date to include (inclusive).
- `to`: The latest creation date to include (inclusive).

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

*Exceptions*
- Throws `ArgumentOutOfRangeException` if `from` is later than `to`.

---
### `WithPaging(int pageSize, int pageIndex)`
Configures pagination for the query. The first page is index `0`.

*Parameters*
- `pageSize`: The number of items per page (must be positive).
- `pageIndex`: The zero-based index of the page to retrieve.

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

*Exceptions*
- Throws `ArgumentOutOfRangeException` if `pageSize` is not positive.
- Throws `ArgumentOutOfRangeException` if `pageIndex` is negative.

---
### `WithPage(int pageSize, int pageNumber)`
Alias for `WithPaging` provided for consistency with common pagination terminology.

*Parameters*
- `pageSize`: The number of items per page (must be positive).
- `pageNumber`: The one-based page number to retrieve.

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

*Exceptions*
- Throws `ArgumentOutOfRangeException` if `pageSize` is not positive.
- Throws `ArgumentOutOfRangeException` if `pageNumber` is less than `1`.

---
### `SortBy(SortDirection direction, string propertyName)`
Specifies the sorting behavior for the query results.

*Parameters*
- `direction`: The sort direction (`SortDirection.Ascending` or `SortDirection.Descending`).
- `propertyName`: The name of the property to sort by (e.g., `"Key"`, `"Name"`, `"CreatedDate"`).

*Return value*
The current `FeatureFlagSearchBuilder` instance for method chaining.

*Exceptions*
- Throws `ArgumentNullException` if `propertyName` is `null`.
- Throws `ArgumentException` if `propertyName` is not a valid sortable property.

---
### `IQueryable<FeatureFlag> Build()`
Constructs and returns an `IQueryable<FeatureFlag>` representing the current query configuration. This allows further customization or execution via LINQ providers.

*Return value*
An `IQueryable<FeatureFlag>` configured with all applied filters and sorting.

---
### `List<FeatureFlag> Execute()`
Executes the configured query and returns the results as a materialized list.

*Return value*
A `List<FeatureFlag>` containing the feature flags matching the current query configuration.

---
### `string GetSummary()`
Generates a human-readable summary of the current query configuration, including active filters and pagination settings.

*Return value*
A string describing the current search criteria.

---
### `static FeatureFlagSearchBuilder AllEnabled()`
Creates a search builder pre-configured to return only enabled feature flags.

*Return value*
A new `FeatureFlagSearchBuilder` instance with the enabled status filter set to `true`.

---
### `static FeatureFlagSearchBuilder AllDisabled()`
Creates a search builder pre-configured to return only disabled feature flags.

*Return value*
A new `FeatureFlagSearchBuilder` instance with the enabled status filter set to `false`.

---
### `static FeatureFlagSearchBuilder ModifiedInLastDays(int days)`
Creates a search builder pre-configured to return feature flags modified within the specified number of days.

*Parameters*
- `days`: The number of days to look back (must be positive).

*Return value*
A new `FeatureFlagSearchBuilder` instance with a creation date range filter set to the last `days` days.

*Exceptions*
- Throws `ArgumentOutOfRangeException` if `days` is not positive.

---
### `static FeatureFlagSearchBuilder AllPercentageRollouts()`
Creates a search builder pre-configured to return only feature flags with percentage-based rollouts.

*Return value*
A new `FeatureFlagSearchBuilder` instance with the rollout type filter set to `RolloutType.Percentage`.

---
### `static FeatureFlagSearchBuilder AllABTests()`
Creates a search builder pre-configured to return only feature flags with A/B test rollouts.

*Return value*
A new `FeatureFlagSearchBuilder` instance with the rollout type filter set to `RolloutType.ABTest`.

---
### `static FeatureFlagSearchBuilder AllRulesBased()`
Creates a search builder pre-configured to return only feature flags with rules-based rollouts.

*Return value*
A new `FeatureFlagSearchBuilder` instance with the rollout type filter set to `RolloutType.RulesBased`.

## Usage

```csharp
// Example 1: Find all enabled feature flags with keys containing "checkout" and sort by name
var enabledCheckoutFlags = new FeatureFlagSearchBuilder()
    .WithKeyContaining("checkout")
    .WithEnabledStatus(true)
    .SortBy(SortDirection.Ascending, "Name")
    .Execute();

// Example 2: Get the second page of disabled A/B test flags created in the last 30 days
var disabledABFlags = new FeatureFlagSearchBuilder()
    .AllDisabled()
    .AllABTests()
    .ModifiedInLastDays(30)
    .WithPaging(pageSize: 20, pageIndex: 1)
    .Execute();
```

## Notes

- The builder is not thread-safe; concurrent modifications or executions from multiple threads require external synchronization.
- Filters are additive; calling the same method multiple times with different values will replace the previous filter.
- The `Build()` method returns an `IQueryable<FeatureFlag>` that can be further refined or composed with other LINQ queries before materialization.
- Pagination parameters are ignored when using `Build()` unless explicitly applied to the resulting query.
- Date range filters use inclusive bounds; both `from` and `to` dates are included in the results.
- Empty or `null` string filters (e.g., `WithCreatedBy`) are treated as "no filter" and are removed from the query.
