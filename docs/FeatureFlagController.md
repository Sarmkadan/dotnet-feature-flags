# FeatureFlagController

A controller for managing feature flags in a .NET application. Provides endpoints to evaluate feature flags, retrieve flag variants, and perform administrative operations such as enabling, disabling, creating, and updating flags.

## API

### `FeatureFlagController()`

Initializes a new instance of the `FeatureFlagController` class.

### `public async Task<IActionResult> EvaluateFeatureFlag(string key, string userId, string email, string? country = null, string? tier = null, string? region = null, Dictionary<string, string>? customAttributes = null)`

Evaluates whether a feature flag is enabled for a given user.

- **Parameters**
  - `key`: The key of the feature flag to evaluate.
  - `userId`: The unique identifier of the user.
  - `email`: The email address of the user.
  - `country` (optional): The country of the user.
  - `tier` (optional): The subscription tier of the user.
  - `region` (optional): The region of the user.
  - `customAttributes` (optional): Additional custom attributes for targeting.
- **Return value**: An `IActionResult` indicating whether the feature flag is enabled.
- **Exceptions**: Throws if the flag does not exist or evaluation fails.

### `public async Task<IActionResult> GetVariant(string key, string userId, string email, string? country = null, string? tier = null, string? region = null, Dictionary<string, string>? customAttributes = null)`

Retrieves the variant of a feature flag for a given user.

- **Parameters**
  - `key`: The key of the feature flag.
  - `userId`: The unique identifier of the user.
  - `email`: The email address of the user.
  - `country` (optional): The country of the user.
  - `tier` (optional): The subscription tier of the user.
  - `region` (optional): The region of the user.
  - `customAttributes` (optional): Additional custom attributes for targeting.
- **Return value**: An `IActionResult` containing the variant of the feature flag.
- **Exceptions**: Throws if the flag does not exist or retrieval fails.

### `public async Task<IActionResult> GetAll()`

Retrieves all feature flags.

- **Return value**: An `IActionResult` containing a list of all feature flags.
- **Exceptions**: Throws if retrieval fails.

### `public async Task<IActionResult> GetByKey(string key)`

Retrieves a feature flag by its key.

- **Parameters**
  - `key`: The key of the feature flag to retrieve.
- **Return value**: An `IActionResult` containing the feature flag.
- **Exceptions**: Throws if the flag does not exist.

### `public async Task<IActionResult> Create(FeatureFlag flag)`

Creates a new feature flag.

- **Parameters**
  - `flag`: The feature flag to create.
- **Return value**: An `IActionResult` indicating success or failure.
- **Exceptions**: Throws if the flag already exists or creation fails.

### `public async Task<IActionResult> Update(FeatureFlag flag)`

Updates an existing feature flag.

- **Parameters**
  - `flag`: The feature flag to update.
- **Return value**: An `IActionResult` indicating success or failure.
- **Exceptions**: Throws if the flag does not exist or update fails.

### `public async Task<IActionResult> Enable(string key)`

Enables a feature flag.

- **Parameters**
  - `key`: The key of the feature flag to enable.
- **Return value**: An `IActionResult` indicating success or failure.
- **Exceptions**: Throws if the flag does not exist or enabling fails.

### `public async Task<IActionResult> Disable(string key)`

Disables a feature flag.

- **Parameters**
  - `key`: The key of the feature flag to disable.
- **Return value**: An `IActionResult` indicating success or failure.
- **Exceptions**: Throws if the flag does not exist or disabling fails.

### `public async Task<IActionResult> GetAuditLogs(string key)`

Retrieves the audit logs for a feature flag.

- **Parameters**
  - `key`: The key of the feature flag.
- **Return value**: An `IActionResult` containing the audit logs.
- **Exceptions**: Throws if the flag does not exist or retrieval fails.

### `public string FeatureFlagKey`

Gets or sets the key of the feature flag.

### `public string UserId`

Gets or sets the user identifier.

### `public string Email`

Gets or sets the user email.

### `public string? Country`

Gets or sets the user country.

### `public string? Tier`

Gets or sets the user tier.

### `public string? Region`

Gets or sets the user region.

### `public Dictionary<string, string>? CustomAttributes`

Gets or sets custom attributes for targeting.

## Usage

### Example 1: Evaluating a Feature Flag
