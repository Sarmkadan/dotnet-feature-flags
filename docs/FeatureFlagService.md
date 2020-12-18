# FeatureFlagService

A service for managing and querying feature flags in a .NET application. It provides asynchronous methods to create, read, update, delete, enable, disable, and search feature flags, as well as retrieve their current state or variant values.

## API

### `FeatureFlagService`

Initializes a new instance of the `FeatureFlagService` class.

### `IsEnabledAsync`

Determines whether a feature flag is enabled for the current context.

- **Parameters**:
  - `key` (string): The key of the feature flag to check.
  - `userId` (string, optional): The user identifier for targeting rules.
  - `defaultValue` (bool, optional): The default value to return if the flag is not found. Defaults to `false`.
- **Return value**: A `Task<bool>` representing whether the feature is enabled.
- **Exceptions**: Throws if the underlying storage or evaluation fails.

### `GetFeatureFlagAsync`

Retrieves a feature flag by its unique identifier.

- **Parameters**:
  - `id` (Guid): The unique identifier of the feature flag.
- **Return value**: A `Task<FeatureFlag?>` containing the feature flag if found, otherwise `null`.
- **Exceptions**: Throws if the operation fails or the identifier is invalid.

### `GetFeatureFlagByKeyAsync`

Retrieves a feature flag by its key.

- **Parameters**:
  - `key` (string): The key of the feature flag to retrieve.
- **Return value**: A `Task<FeatureFlag?>` containing the feature flag if found, otherwise `null`.
- **Exceptions**: Throws if the operation fails or the key is invalid.

### `GetAllFeatureFlagsAsync`

Retrieves all feature flags stored in the system.

- **Return value**: A `Task<IEnumerable<FeatureFlag>>` containing all feature flags.
- **Exceptions**: Throws if the operation fails.

### `GetEnabledFeatureFlagsAsync`

Retrieves all enabled feature flags.

- **Return value**: A `Task<IEnumerable<FeatureFlag>>` containing all enabled feature flags.
- **Exceptions**: Throws if the operation fails.

### `CreateFeatureFlagAsync`

Creates a new feature flag in the system.

- **Parameters**:
  - `featureFlag` (FeatureFlag): The feature flag to create.
- **Return value**: A `Task<FeatureFlag>` containing the created feature flag.
- **Exceptions**: Throws if the flag already exists, the key is invalid, or the operation fails.

### `UpdateFeatureFlagAsync`

Updates an existing feature flag.

- **Parameters**:
  - `featureFlag` (FeatureFlag): The feature flag with updated values.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws if the flag does not exist, the key is invalid, or the operation fails.

### `DeleteFeatureFlagAsync`

Deletes a feature flag by its unique identifier.

- **Parameters**:
  - `id` (Guid): The unique identifier of the feature flag to delete.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws if the flag does not exist or the operation fails.

### `EnableFeatureFlagAsync`

Enables a feature flag by its unique identifier.

- **Parameters**:
  - `id` (Guid): The unique identifier of the feature flag to enable.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws if the flag does not exist or the operation fails.

### `DisableFeatureFlagAsync`

Disables a feature flag by its unique identifier.

- **Parameters**:
  - `id` (Guid): The unique identifier of the feature flag to disable.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws if the flag does not exist or the operation fails.

### `GetVariantAsync`

Retrieves the variant value for a feature flag and user context.

- **Parameters**:
  - `key` (string): The key of the feature flag.
  - `userId` (string, optional): The user identifier for targeting rules.
- **Return value**: A `Task<string?>` containing the variant value if applicable, otherwise `null`.
- **Exceptions**: Throws if the flag does not exist or the operation fails.

### `SearchFeatureFlagsAsync`

Searches for feature flags matching the provided criteria.

- **Parameters**:
  - `criteria` (object): Criteria to filter feature flags (e.g., enabled status, tags).
- **Return value**: A `Task<IEnumerable<FeatureFlag>>` containing matching feature flags.
- **Exceptions**: Throws if the operation fails.

## Usage

### Example 1: Checking a Feature Flag
