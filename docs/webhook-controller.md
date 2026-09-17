# Webhook Controller

The `WebhookController` handles incoming webhook payloads from external systems, validating and processing them securely.

## Endpoint

### Receive Webhook

**URL**: `POST /api/webhooks/receive/{webhookId}`

**Description**: Receives and validates an incoming webhook payload from an external system. The payload is validated for size, HMAC signature (if configured), and JSON structure before processing.

#### Path Parameters

| Parameter | Type   | Description                        |
|-----------|--------|------------------------------------|
| webhookId | integer| The ID of the webhook configuration|

#### Request Headers

| Header                 | Required | Description                                                                 |
|------------------------|----------|-----------------------------------------------------------------------------|
| Content-Type           | Yes      | Must be `application/json`                                                  |
| Content-Length         | No       | If provided, used to check payload size before reading the body             |
| X-Hub-Signature-256    | Conditional | Required if the webhook has a secret configured. Value must be `sha256=<hex>`|

#### Request Body

The body should be a JSON object representing the webhook payload. The exact structure depends on the sending system, but it must be valid JSON and within the size limit.

#### Responses

| Status Code | Description                                                                 |
|-------------|-----------------------------------------------------------------------------|
| 200 OK      | Webhook received and validated successfully. Returns JSON: `{ status: "received", eventType: "<event-type>" }` |
| 400 Bad Request | Invalid payload format, missing required headers, or webhook is inactive. |
| 401 Unauthorized | Missing or invalid signature header (when secret is configured).          |
| 413 Payload Too Large | Payload exceeds the maximum allowed size (1 MB).                      |
| 404 Not Found | Webhook with the specified ID does not exist.                             |
| 500 Internal Server Error | Unexpected error occurred while processing the webhook.              |

#### Security Features

1. **Size Limitation**: The controller enforces a maximum payload size of 1 MB to prevent DoS attacks via excessive memory allocation.
2. **HMAC Validation**: If the webhook configuration includes a secret, the controller validates the `X-Hub-Signature-256` header using HMAC-SHA256.
3. **Streaming Read**: The request body is read with a size limit to avoid loading excessively large payloads into memory.

#### Notes

- The controller uses dependency injection for `ILogger<WebhookController>` and `IWebhookService`.
- If the `Content-Length` header is missing, the controller enables buffering and reads the stream with a size limit.
- After reading the payload, the request body position is reset to allow subsequent reads if needed.
- The controller currently acknowledges receipt of the webhook but does not process the payload further (marked with `TODO`).