# WebhookRepository

Provides data access operations for managing webhooks and their delivery records in the feature flag system. Supports CRUD operations for webhooks, querying active or recently failed deliveries, and tracking delivery statistics.

## API

### `WebhookRepository` Constructor

Initializes a new instance of the `WebhookRepository` class with required dependencies for data access.

### `CreateAsync(Webhook webhook)`

Creates a new webhook record in the repository.

- **Parameters**
  - `webhook`: The `Webhook` instance to create. Must not be `null`.
- **Return Value**
  - A `Task<Webhook>` representing the asynchronous operation, yielding the created webhook with updated identifiers.
- **Exceptions**
  - Throws `ArgumentNullException` if `webhook` is `null`.
  - Throws if the underlying data store fails to persist the record.

### `GetByIdAsync(Guid id)`

Retrieves a webhook by its unique identifier.

- **Parameters**
  - `id`: The unique identifier of the webhook to retrieve.
- **Return Value**
  - A `Task<Webhook?>` representing the asynchronous operation, yielding the webhook if found, otherwise `null`.
- **Exceptions**
  - None expected from public API; internal data access exceptions may propagate.

### `GetActiveAsync()`

Retrieves all webhooks currently marked as active.

- **Return Value**
  - A `Task<List<Webhook>>` representing the asynchronous operation, yielding a list of active webhooks (possibly empty).
- **Exceptions**
  - None expected from public API.

### `GetByEventTypeAsync(string eventType)`

Retrieves all webhooks associated with a specific event type.

- **Parameters**
  - `eventType`: The event type to filter by. Must not be `null` or empty.
- **Return Value**
  - A `Task<List<Webhook>>` representing the asynchronous operation, yielding a list of matching webhooks (possibly empty).
- **Exceptions**
  - Throws `ArgumentException` if `eventType` is `null` or whitespace.

### `GetRecentFailuresAsync(int count = 10)`

Retrieves the most recent webhook delivery failures.

- **Parameters**
  - `count`: The maximum number of failures to return. Defaults to `10`.
- **Return Value**
  - A `Task<List<Webhook>>` representing the asynchronous operation, yielding a list of webhooks with recent failures (possibly empty).
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` is less than `0`.

### `UpdateAsync(Webhook webhook)`

Updates an existing webhook record in the repository.

- **Parameters**
  - `webhook`: The `Webhook` instance to update. Must not be `null` and must have a valid identifier.
- **Return Value**
  - A `Task<bool>` representing the asynchronous operation, yielding `true` if the update was successful, otherwise `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `webhook` is `null`.
  - Throws if the underlying data store fails to update the record.

### `DeleteAsync(Guid id)`

Deletes a webhook by its unique identifier.

- **Parameters**
  - `id`: The unique identifier of the webhook to delete.
- **Return Value**
  - A `Task<bool>` representing the asynchronous operation, yielding `true` if the deletion was successful, otherwise `false`.
- **Exceptions**
  - None expected from public API; internal data access exceptions may propagate.

### `GetCountAsync()`

Retrieves the total number of webhooks stored.

- **Return Value**
  - A `Task<int>` representing the asynchronous operation, yielding the total count.
- **Exceptions**
  - None expected from public API.

### `GetActiveCountAsync()`

Retrieves the total number of active webhooks stored.

- **Return Value**
  - A `Task<int>` representing the asynchronous operation, yielding the count of active webhooks.
- **Exceptions**
  - None expected from public API.

---

### `WebhookDeliveryRepository` Constructor

Initializes a new instance of the `WebhookDeliveryRepository` class with required dependencies for data access.

### `CreateAsync(WebhookDelivery delivery)`

Creates a new webhook delivery record in the repository.

- **Parameters**
  - `delivery`: The `WebhookDelivery` instance to create. Must not be `null`.
- **Return Value**
  - A `Task<WebhookDelivery>` representing the asynchronous operation, yielding the created delivery with updated identifiers.
- **Exceptions**
  - Throws `ArgumentNullException` if `delivery` is `null`.
  - Throws if the underlying data store fails to persist the record.

### `GetByIdAsync(Guid id)`

Retrieves a webhook delivery by its unique identifier.

- **Parameters**
  - `id`: The unique identifier of the delivery to retrieve.
- **Return Value**
  - A `Task<WebhookDelivery?>` representing the asynchronous operation, yielding the delivery if found, otherwise `null`.
- **Exceptions**
  - None expected from public API; internal data access exceptions may propagate.

### `GetByWebhookIdAsync(Guid webhookId)`

Retrieves all delivery records associated with a specific webhook.

- **Parameters**
  - `webhookId`: The unique identifier of the webhook whose deliveries are to be retrieved.
- **Return Value**
  - A `Task<List<WebhookDelivery>>` representing the asynchronous operation, yielding a list of matching deliveries (possibly empty).
- **Exceptions**
  - None expected from public API.

### `GetPendingRetriesAsync()`

Retrieves all delivery records currently marked for retry.

- **Return Value**
  - A `Task<List<WebhookDelivery>>` representing the asynchronous operation, yielding a list of pending retry deliveries (possibly empty).
- **Exceptions**
  - None expected from public API.

### `GetRecentDeliveriesAsync(int count = 10)`

Retrieves the most recent webhook deliveries.

- **Parameters**
  - `count`: The maximum number of deliveries to return. Defaults to `10`.
- **Return Value**
  - A `Task<List<WebhookDelivery>>` representing the asynchronous operation, yielding a list of recent deliveries (possibly empty).
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` is less than `0`.

### `GetDeliveryStatsAsync()`

Retrieves aggregated delivery statistics for all webhook deliveries.

- **Return Value**
  - A `Task<(int Successful, int Failed)>` representing the asynchronous operation, yielding a tuple with counts of successful and failed deliveries.
- **Exceptions**
  - None expected from public API.

### `UpdateAsync(WebhookDelivery delivery)`

Updates an existing webhook delivery record in the repository.

- **Parameters**
  - `delivery`: The `WebhookDelivery` instance to update. Must not be `null` and must have a valid identifier.
- **Return Value**
  - A `Task<bool>` representing the asynchronous operation, yielding `true` if the update was successful, otherwise `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `delivery` is `null`.
  - Throws if the underlying data store fails to update the record.

## Usage
