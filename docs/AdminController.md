# AdminController

The `AdminController` is a REST API controller that provides administrative operations for managing feature flags, webhooks, cache, health status, and data import/export in the dotnet-feature-flags system. It exposes endpoints for webhook registration, health checks, cache management, and data serialization/deserialization.

## API

### `AdminController`
Initializes a new instance of the `AdminController` class.

### `RegisterWebhook`
Registers a new webhook endpoint for receiving feature flag change notifications.

- **Parameters**: None
- **Return value**: `Task<IActionResult>` – HTTP 201 Created on success, HTTP 400 Bad Request if validation fails, HTTP 500 Internal Server Error on failure.
- **Throws**: May throw if the underlying webhook service fails to persist the registration.

### `GetWebhooks`
Retrieves the list of registered webhook endpoints.

- **Parameters**: None
- **Return value**: `Task<IActionResult>` – HTTP 200 OK with the list of webhooks on success, HTTP 500 Internal Server Error on failure.

### `DeleteWebhook`
Removes a registered webhook endpoint by its identifier.

- **Parameters**: None
- **Return value**: `Task<IActionResult>` – HTTP 204 No Content on success, HTTP 404 Not Found if the webhook does not exist, HTTP 500 Internal Server Error on failure.
- **Throws**: May throw if the identifier is malformed or the deletion operation fails.

### `ExportCsv`
Exports all feature flags as a CSV file.

- **Parameters**: None
- **Return value**: `Task<IActionResult>` – HTTP 200 OK with a CSV file attachment on success, HTTP 500 Internal Server Error on failure.

### `ExportXml`
Exports all feature flags as an XML file.

- **Parameters**: None
- **Return value**: `Task<IActionResult>` – HTTP 200 OK with an XML file attachment on success, HTTP 500 Internal Server Error on failure.

### `ImportCsv`
Imports feature flags from a CSV file.

- **Parameters**: None
- **Return value**: `Task<IActionResult>` – HTTP 200 OK on successful import, HTTP 400 Bad Request if the file is invalid, HTTP 500 Internal Server Error on failure.
- **Throws**: May throw if the file cannot be read or parsed.

### `ClearCache`
Clears the in-memory cache of feature flags.

- **Parameters**: None
- **Return value**: `Task<IActionResult>` – HTTP 200 OK on success, HTTP 500 Internal Server Error on failure.

### `GetHealth`
Checks the health status of the feature flag service.

- **Parameters**: None
- **Return value**: `IActionResult` – HTTP 200 OK if healthy, HTTP 503 Service Unavailable otherwise.

### `GetStats`
Retrieves usage statistics for feature flags.

- **Parameters**: None
- **Return value**: `Task<IActionResult>` – HTTP 200 OK with statistics on success, HTTP 500 Internal Server Error on failure.

### `Url`
Gets the base URL for the webhook endpoint.

- **Type**: `string`
- **Remarks**: Read-only property representing the configured base URL.

### `Description`
Gets or sets an optional description for the webhook.

- **Type**: `string?`
- **Remarks**: Optional metadata for the webhook; may be null.

### `EventTypes`
Gets or sets the types of events the webhook should receive.

- **Type**: `Integration.WebhookEventType?`
- **Remarks**: May be null to indicate all event types.

### `FeatureFlagKey`
Gets or sets the specific feature flag key the webhook is subscribed to.

- **Type**: `string?`
- **Remarks**: May be null to subscribe to all feature flags.

### `Secret`
Gets or sets the secret used to sign webhook payloads.

- **Type**: `string?`
- **Remarks**: Optional; if set, payloads will be signed with this value.

## Usage

### Registering a Webhook
