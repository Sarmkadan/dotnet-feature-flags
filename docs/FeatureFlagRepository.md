# FeatureFlagRepository

Central repository for managing feature flags in a .NET application. Provides CRUD operations, querying capabilities, and support for retrieving feature flags with related entities such as rules, variants, and audit logs.

## API

### `FeatureFlagRepository()`
Initializes a new instance of the `FeatureFlagRepository` class.

### `async Task<FeatureFlag?> GetByIdAsync(Guid id)`
Retrieves a feature flag by its unique identifier.

- **Parameters**
  - `id`: The unique identifier of the feature flag.
- **Returns**
  - A `FeatureFlag` instance if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is an empty GUID.

### `async Task<FeatureFlag?> GetByKeyAsync(string key)`
Retrieves a feature flag by its unique key.

- **Parameters**
  - `key`: The unique key of the feature flag.
- **Returns**
  - A `FeatureFlag` instance if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `key` is `null` or whitespace.

### `async Task<IEnumerable<FeatureFlag>> GetAllAsync()`
Retrieves all feature flags.

- **Returns**
  - An enumerable collection of all `FeatureFlag` instances.

### `async Task<IEnumerable<FeatureFlag>> GetEnabledAsync()`
Retrieves all enabled feature flags.

- **Returns**
  - An enumerable collection of enabled `FeatureFlag` instances.

### `async Task<IEnumerable<FeatureFlag>> GetByCreatorAsync(string creatorId)`
Retrieves feature flags created by a specific creator.

- **Parameters**
  - `creatorId`: The identifier of the creator.
- **Returns**
  - An enumerable collection of `FeatureFlag` instances created by the specified creator.

### `async Task<IEnumerable<FeatureFlag>> GetModifiedSinceAsync(DateTimeOffset modifiedSince)`
Retrieves feature flags modified after a specified timestamp.

- **Parameters**
  - `modifiedSince`: The timestamp to compare against.
- **Returns**
  - An enumerable collection of `FeatureFlag` instances modified after the specified timestamp.

### `async Task<int> GetTotalCountAsync()`
Retrieves the total number of feature flags.

- **Returns**
  - The total count of feature flags.

### `async Task<IEnumerable<FeatureFlag>> GetPagedAsync(int skip, int take)`
Retrieves a paged subset of feature flags.

- **Parameters**
  - `skip`: The number of items to skip.
  - `take`: The number of items to return.
- **Returns**
  - An enumerable collection of `FeatureFlag` instances for the specified page.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `skip` or `take` is negative.

### `async Task<IEnumerable<FeatureFlag>> SearchAsync(string query)`
Searches feature flags by a query string.

- **Parameters**
  - `query`: The search query.
- **Returns**
  - An enumerable collection of `FeatureFlag` instances matching the query.

### `async Task<FeatureFlag?> GetWithRulesAsync(Guid id)`
Retrieves a feature flag by its unique identifier, including its associated rules.

- **Parameters**
  - `id`: The unique identifier of the feature flag.
- **Returns**
  - A `FeatureFlag` instance with rules if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is an empty GUID.

### `async Task<FeatureFlag?> GetWithVariantsAsync(Guid id)`
Retrieves a feature flag by its unique identifier, including its associated variants.

- **Parameters**
  - `id`: The unique identifier of the feature flag.
- **Returns**
  - A `FeatureFlag` instance with variants if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is an empty GUID.

### `async Task<FeatureFlag?> GetWithAuditLogsAsync(Guid id)`
Retrieves a feature flag by its unique identifier, including its associated audit logs.

- **Parameters**
  - `id`: The unique identifier of the feature flag.
- **Returns**
  - A `FeatureFlag` instance with audit logs if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is an empty GUID.

### `async Task<bool> KeyExistsAsync(string key)`
Checks whether a feature flag with the specified key exists.

- **Parameters**
  - `key`: The key to check.
- **Returns**
  - `true` if a feature flag with the specified key exists; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentException` if `key` is `null` or whitespace.

### `async Task<IEnumerable<FeatureFlag>> GetRecentlyModifiedAsync(int count)`
Retrieves the most recently modified feature flags.

- **Parameters**
  - `count`: The number of recently modified feature flags to return.
- **Returns**
  - An enumerable collection of the most recently modified `FeatureFlag` instances.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` is negative.

### `async Task<FeatureFlag> AddAsync(FeatureFlag flag)`
Adds a new feature flag.

- **Parameters**
  - `flag`: The feature flag to add.
- **Returns**
  - The added `FeatureFlag` instance.
- **Exceptions**
  - Throws `ArgumentNullException` if `flag` is `null`.
  - Throws `ArgumentException` if `flag.Key` is `null` or whitespace.

### `async Task UpdateAsync(FeatureFlag flag)`
Updates an existing feature flag.

- **Parameters**
  - `flag`: The feature flag to update.
- **Exceptions**
  - Throws `ArgumentNullException` if `flag` is `null`.
  - Throws `ArgumentException` if `flag.Key` is `null` or whitespace.
  - Throws `InvalidOperationException` if the feature flag does not exist.

### `async Task DeleteAsync(Guid id)`
Deletes a feature flag by its unique identifier.

- **Parameters**
  - `id`: The unique identifier of the feature flag to delete.
- **Exceptions**
  - Throws `ArgumentException` if `id` is an empty GUID.
  - Throws `InvalidOperationException` if the feature flag does not exist.

### `async Task<bool> ExistsAsync(Guid id)`
Checks whether a feature flag with the specified identifier exists.

- **Parameters**
  - `id`: The unique identifier to check.
- **Returns**
  - `true` if a feature flag with the specified identifier exists; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is an empty GUID.

## Usage

### Basic CRUD Operations
