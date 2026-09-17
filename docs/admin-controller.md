# Admin Controller API Documentation

This document describes the administrative endpoints exposed by the `AdminController` in the Feature Flags application.

## Base URL

All endpoints are prefixed with `/api/admin`.

## Endpoints

### Webhook Management

#### Register a Webhook
- **URL**: `POST /api/admin/webhooks`
- **Description**: Registers a new webhook endpoint to receive feature flag events.
- **Request Body**:
  ```json
  {
    "url": "string (required)",
    "description": "string (optional)",
    "eventTypes": "Integration.WebhookEventType (optional)",
    "featureFlagKey": "string (optional)",
    "secret": "string (optional)"
  }
  ```
- **Responses**:
  - `201 Created`: Webhook successfully registered
  - `400 Bad Request`: Invalid webhook URL or registration failed
- **Authorization**: Requires authenticated user (extracts user ID from "sub" claim)

#### Get All Webhooks
- **URL**: `GET /api/admin/webhooks`
- **Description**: Retrieves list of all active webhooks.
- **Responses**:
  - `200 OK`: Returns array of webhook objects
  - `500 Internal Server Error`: Failed to retrieve webhooks

#### Delete a Webhook
- **URL**: `DELETE /api/admin/webhooks/{webhookId}`
- **Description**: Deletes a webhook by its ID.
- **Path Parameters**:
  - `webhookId`: integer (required) - The ID of the webhook to delete
- **Responses**:
  - `204 No Content`: Webhook successfully deleted
  - `404 Not Found`: Webhook not found

### Data Export/Import

#### Export to CSV
- **URL**: `GET /api/admin/export/csv`
- **Description**: Exports all feature flags to CSV format for backup or analysis.
- **Query Parameters**:
  - `includeRules`: boolean (optional, default: false) - Whether to include rule details in export
- **Responses**:
  - `200 OK`: Returns CSV file with name `feature-flags-{timestamp}.csv`
  - `400 Bad Request`: Invalid request
  - `500 Internal Server Error`: Export failed

#### Export to XML
- **URL**: `GET /api/admin/export/xml`
- **Description**: Exports all feature flags to XML format for system integration.
- **Responses**:
  - `200 OK`: Returns XML file with name `feature-flags-{timestamp}.xml`
  - `400 Bad Request`: Invalid request
  - `500 Internal Server Error`: Export failed

#### Import from CSV
- **URL**: `POST /api/admin/import/csv`
- **Description**: Imports feature flags from CSV file.
- **Request Body**: multipart/form-data with file field
- **Responses**:
  - `200 OK`: Returns count of imported flags and the flags data
  - `400 Bad Request`: No file provided or import failed
  - `500 Internal Server Error`: Import failed

### System Operations

#### Clear Cache
- **URL**: `POST /api/admin/cache/clear`
- **Description**: Clears the feature flag cache to force fresh database load.
- **Responses**:
  - `204 No Content`: Cache successfully cleared
  - `500 Internal Server Error`: Failed to clear cache

#### Health Check
- **URL**: `GET /api/admin/health`
- **Description**: Gets system health status and basic information.
- **Responses**:
  - `200 OK`: Returns health status object with:
    - `status`: string ("healthy")
    - `timestamp`: datetime (UTC)
    - `version`: string ("1.0.0")
    - `uptime`: datetime (UTC)

#### Get Statistics
- **URL**: `GET /api/admin/stats`
- **Description**: Gets system statistics and metrics about feature flags.
- **Responses**:
  - `200 OK`: Returns statistics object with:
    - `totalFlags`: integer
    - `enabledFlags`: integer
    - `disabledFlags`: integer
    - `percentRolloutCount`: integer
    - `rulesBasedCount`: integer
    - `abTestCount`: integer
  - `500 Internal Server Error`: Failed to retrieve statistics

#### Get Stale Flags
- **URL**: `GET /api/admin/flags/stale`
- **Description**: Gets feature flags that haven't been modified within the specified time window.
- **Query Parameters**:
  - `days`: integer (optional, default: 30) - Number of days to consider for staleness
- **Responses**:
  - `200 OK`: Returns array of stale feature flag objects
  - `500 Internal Server Error`: Failed to retrieve stale flags

## Error Responses

Common error responses across endpoints:
- `400 Bad Request`: Invalid input or request format
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Unexpected server error

## Authentication

Most endpoints require authentication. The controller extracts the user ID from the "sub" claim in the authenticated user's token. If no authentication is present, it defaults to "system" for webhook registration and "unknown" for cache clearing operations.

## Dependencies

The AdminController depends on the following services:
- `IWebhookService`: For webhook management operations
- `IFeatureFlagService`: For feature flag data operations
- `ICacheService`: For cache management operations
- `ILogger<AdminController>`: For logging operations