# WebhookServiceTests

Unit tests for the `WebhookService` class, verifying webhook registration, retrieval, updating, deletion, and event triggering functionality. The test suite ensures proper handling of valid and invalid inputs, including edge cases for URL validation, feature flag key filtering, and retry mechanisms for failed deliveries.

## API

### `WebhookServiceTests`

Constructor for the test class. Initializes the test environment with required services and dependencies for webhook testing.

### `RegisterWebhookAsync_WithValidUrl_CreatesWebhook`

Verifies that a webhook with a valid URL is successfully registered. Ensures the service creates the webhook entry and persists it.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

**Throws:**
- May throw exceptions from underlying services if registration fails unexpectedly.

### `RegisterWebhookAsync_WithInvalidUrl_ThrowsArgumentException`

Ensures that attempting to register a webhook with an invalid URL throws an `ArgumentException`. Validates input validation logic.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

**Throws:**
- `ArgumentException`: When the provided URL is invalid.

### `GetWebhookAsync_WithValidId_ReturnsWebhook`

Confirms that retrieving a webhook by a valid ID returns the expected webhook entity.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

**Throws:**
- May throw exceptions if the ID is malformed or the storage layer fails.

### `GetWebhookAsync_WithInvalidId_ReturnsNull`

Validates that retrieving a webhook with an invalid ID returns `null` instead of throwing.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

### `GetActiveWebhooksAsync_WithMatchingEventType_ReturnsWebhooks`

Checks that active webhooks matching a specific event type are returned correctly.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

### `GetActiveWebhooksAsync_WithNoneMatching_ReturnsEmpty`

Ensures that no webhooks are returned when no active webhooks match the requested event type.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

### `UpdateWebhookAsync_WithValidData_UpdatesWebhook`

Verifies that updating a webhook with valid data successfully modifies the existing webhook entry.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

**Throws:**
- May throw exceptions if the update operation fails due to concurrency or data integrity issues.

### `UpdateWebhookAsync_WithNonexistentId_ReturnsFalse`

Ensures that attempting to update a non-existent webhook returns `false` instead of throwing.

**Parameters:**
- None

**Return value:**
- `Task<bool>`: Returns `false` when the webhook ID does not exist.

### `DeleteWebhookAsync_WithValidId_DeletesWebhook`

Confirms that deleting a webhook with a valid ID successfully removes it from storage.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

**Throws:**
- May throw exceptions if the ID is invalid or the deletion fails due to constraints.

### `DeleteWebhookAsync_WithNonexistentId_ReturnsFalse`

Validates that attempting to delete a non-existent webhook returns `false` instead of throwing.

**Parameters:**
- None

**Return value:**
- `Task<bool>`: Returns `false` when the webhook ID does not exist.

### `TriggerWebhooksAsync_WithMatchingEventType_CallsActiveWebhooks`

Ensures that triggering webhooks for a matching event type invokes the correct active webhooks.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

### `RetryFailedDeliveriesAsync_ProcessesRetries`

Verifies that retrying failed deliveries processes the queue and attempts redelivery.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

### `GetActiveWebhooksAsync_FiltersInactiveWebhooks`

Confirms that inactive webhooks are excluded from the results of active webhook queries.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

### `RegisterWebhookAsync_WithFeatureFlagKey_CreatesWebhookWithFilter`

Ensures that registering a webhook with a feature flag key creates the webhook with the appropriate filter applied.

**Parameters:**
- None

**Return value:**
- `Task`: Completes when the operation finishes.

## Usage

### Example 1: Registering and Retrieving a Webhook
