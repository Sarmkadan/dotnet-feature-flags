# API Reference

Complete REST API documentation for dotnet-feature-flags.

## Base URL

```
http://localhost:5000/api
```

## Common Response Format

All endpoints return responses in this format:

```json
{
  "success": true,
  "data": {},
  "message": "Operation completed successfully",
  "statusCode": 200
}
```

## Error Responses

```json
{
  "success": false,
  "data": null,
  "message": "Error description",
  "statusCode": 400
}
```

Common status codes:
- `200`: Success
- `201`: Created
- `400`: Bad Request
- `401`: Unauthorized
- `403`: Forbidden
- `404`: Not Found
- `500`: Internal Server Error

## Feature Flag Endpoints

### Create Feature Flag

Creates a new feature flag.

```http
POST /featureflag
Content-Type: application/json
```

**Request Body:**

```json
{
  "key": "string (required, unique)",
  "displayName": "string (required)",
  "description": "string (optional)",
  "isEnabled": "boolean",
  "rolloutType": "integer (0=Percentage, 1=RulesBased, 2=ABTest, 3=Full, 4=None)",
  "percentageRollout": "integer (0-100)",
  "rules": [
    {
      "name": "string",
      "priority": "integer",
      "conditionLogic": "string (AND/OR)",
      "conditions": [
        {
          "attribute": "string",
          "operator": "integer",
          "value": "string"
        }
      ]
    }
  ],
  "variants": [
    {
      "name": "string",
      "description": "string",
      "allocationPercentage": "integer"
    }
  ]
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "key": "new-checkout",
    "displayName": "New Checkout Flow",
    "description": "Redesigned checkout",
    "isEnabled": true,
    "rolloutType": 0,
    "percentageRollout": 25,
    "createdDate": "2024-02-20T10:30:00Z",
    "createdBy": "admin@company.com"
  },
  "success": true,
  "statusCode": 201
}
```

**Status Codes:**
- `201 Created`: Flag created successfully
- `400 Bad Request`: Invalid request body
- `409 Conflict`: Key already exists

---

### Get All Feature Flags

Retrieves a paginated list of all feature flags.

```http
GET /featureflag?pageNumber=1&pageSize=20&isEnabled=true&sortBy=createdDate&sortOrder=desc
```

**Query Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | integer | 1 | Page number (1-based) |
| pageSize | integer | 20 | Results per page (max 100) |
| isEnabled | boolean | null | Filter by enabled status |
| sortBy | string | "createdDate" | Sort field |
| sortOrder | string | "desc" | Sort order (asc/desc) |

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": "guid-1",
      "key": "dark-mode",
      "displayName": "Dark Mode",
      "isEnabled": true,
      "percentageRollout": 50,
      "createdDate": "2024-02-15T10:00:00Z"
    },
    {
      "id": "guid-2",
      "key": "new-checkout",
      "displayName": "New Checkout Flow",
      "isEnabled": true,
      "percentageRollout": 25,
      "createdDate": "2024-02-20T10:30:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 45,
  "statusCode": 200
}
```

---

### Get Feature Flag by Key

Retrieves a single feature flag by its key.

```http
GET /featureflag/{key}
```

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| key | string | Yes | The feature flag key |

**Response:**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "key": "new-checkout",
    "displayName": "New Checkout Flow",
    "description": "Redesigned checkout",
    "isEnabled": true,
    "rolloutType": 0,
    "percentageRollout": 25,
    "rules": [],
    "variants": [],
    "createdDate": "2024-02-20T10:30:00Z",
    "modifiedDate": "2024-02-20T14:00:00Z",
    "createdBy": "admin@company.com",
    "modifiedBy": "admin@company.com"
  },
  "statusCode": 200
}
```

**Status Codes:**
- `200 OK`: Flag found
- `404 Not Found`: Flag not found

---

### Get Feature Flag by ID

```http
GET /featureflag/{id}
```

Same response as "Get by Key" but uses GUID identifier.

---

### Update Feature Flag

Updates an existing feature flag.

```http
PUT /featureflag/{id}
Content-Type: application/json
```

**Request Body:** (all fields optional)

```json
{
  "displayName": "string",
  "description": "string",
  "percentageRollout": "integer",
  "rolloutType": "integer",
  "rules": [],
  "variants": []
}
```

**Response:** Updated flag object

**Status Codes:**
- `200 OK`: Updated successfully
- `404 Not Found`: Flag not found
- `400 Bad Request`: Invalid update

---

### Enable Feature Flag

Enables a feature flag.

```http
POST /featureflag/{id}/enable
```

**Response:**

```json
{
  "success": true,
  "data": { /* flag object */ },
  "message": "Feature flag enabled",
  "statusCode": 200
}
```

---

### Disable Feature Flag

Disables a feature flag.

```http
POST /featureflag/{id}/disable
```

**Response:** Same as enable endpoint

---

### Delete Feature Flag

Deletes a feature flag permanently.

```http
DELETE /featureflag/{id}
```

**Response:**

```json
{
  "success": true,
  "message": "Feature flag deleted",
  "statusCode": 200
}
```

---

## Evaluation Endpoints

### Evaluate Feature Flag

Evaluates if a feature flag is enabled for a user.

```http
POST /featureflag/evaluate
Content-Type: application/json
```

**Request Body:**

```json
{
  "featureFlagKey": "string (required)",
  "userId": "string (required)",
  "email": "string (optional)",
  "country": "string (optional)",
  "tier": "string (optional)",
  "region": "string (optional)",
  "customAttributes": {
    "key1": "value1",
    "key2": "value2"
  }
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "isEnabled": true,
    "evaluationTime": 2.5,
    "evaluatedRules": 2,
    "matchedRule": "Premium Users",
    "details": "Matched rule: Premium Users"
  },
  "statusCode": 200
}
```

---

### Get A/B Test Variant

Gets the assigned variant for an A/B test.

```http
POST /featureflag/variant
Content-Type: application/json
```

**Request Body:**

```json
{
  "featureFlagKey": "string (required)",
  "userId": "string (required)",
  "email": "string (optional)"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "variant": "Treatment",
    "allocationPercentage": 50,
    "description": "New checkout design"
  },
  "statusCode": 200
}
```

---

## Audit Endpoints

### Get Audit Logs for Flag

Retrieves audit logs for a specific feature flag.

```http
GET /featureflag/{id}/audit?pageNumber=1&pageSize=50&action=Updated&sortOrder=desc
```

**Query Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | integer | 1 | Page number |
| pageSize | integer | 50 | Results per page |
| action | string | null | Filter by action |
| sortOrder | string | "desc" | Sort order |

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "featureFlagId": "guid",
      "action": "Enabled",
      "changedBy": "admin@company.com",
      "timestamp": "2024-02-20T14:15:00Z",
      "details": "Feature flag was enabled",
      "oldValue": "false",
      "newValue": "true"
    },
    {
      "id": "guid",
      "featureFlagId": "guid",
      "action": "Created",
      "changedBy": "admin@company.com",
      "timestamp": "2024-02-20T10:30:00Z",
      "details": "Feature flag created",
      "oldValue": null,
      "newValue": "new-checkout"
    }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 15,
  "statusCode": 200
}
```

---

### Get All Audit Logs

```http
GET /audit
```

Retrieves all audit logs across all flags with pagination.

---

## Search Endpoints

### Search Feature Flags

```http
GET /featureflag/search?q=checkout&createdBy=admin@company.com&page=1&size=20
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| q | string | Search term (key, name, or description) |
| createdBy | string | Filter by creator email |
| isEnabled | boolean | Filter by enabled status |
| page | integer | Page number |
| size | integer | Page size |

**Response:** Paginated flag list

---

## Admin Endpoints

### Export Flags

Exports all feature flags in specified format.

```http
GET /admin/export?format=csv
```

**Query Parameters:**

| Parameter | Type | Options |
|-----------|------|---------|
| format | string | csv, json, xml |

**Response:** File download

---

### Bulk Update

Updates multiple flags at once.

```http
POST /admin/bulk-update
Content-Type: application/json
```

**Request Body:**

```json
{
  "flagIds": ["guid-1", "guid-2"],
  "updates": {
    "percentageRollout": 50
  }
}
```

---

### Cleanup Old Audit Logs

Removes audit logs older than specified days.

```http
POST /admin/cleanup-audit-logs?retentionDays=365
```

**Status Codes:**
- `200 OK`: Cleanup completed
- `204 No Content`: No logs to clean

---

## Condition Operators

When creating rules, use these operator values:

| Operator | Value | Description |
|----------|-------|-------------|
| Equals | 0 | Exact string match (case-insensitive) |
| NotEquals | 1 | Not equal |
| Contains | 2 | String contains |
| StartsWith | 3 | String starts with |
| EndsWith | 4 | String ends with |
| GreaterThan | 5 | Numeric comparison > |
| LessThan | 6 | Numeric comparison < |
| In | 7 | Value in comma-separated list |

---

## Rollout Types

When creating flags, use these rollout type values:

| Type | Value | Description |
|------|-------|-------------|
| Percentage | 0 | Percentage-based rollout |
| RulesBased | 1 | Condition-based targeting |
| ABTest | 2 | A/B testing with variants |
| Full | 3 | 100% rollout to all users |
| None | 4 | 0% rollout (disabled) |

---

## Audit Actions

Possible audit action values:

- Created
- Enabled
- Disabled
- Updated
- RolloutChanged
- RuleAdded
- RuleRemoved
- VariantUpdated
- Deleted

---

## Rate Limiting

API has the following rate limits:

- **Standard**: 1000 requests/minute per IP
- **Evaluation**: 10,000 requests/minute per IP

Exceeded limits return `429 Too Many Requests`.

---

## Authentication

Currently, the API is open. For production, implement authentication:

```csharp
// In Program.cs
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options => { /* configure */ });

app.UseAuthentication();
app.UseAuthorization();
```

---

## Examples

### Complete Workflow

```bash
# Create a flag
curl -X POST http://localhost:5000/api/featureflag \
  -H "Content-Type: application/json" \
  -d '{"key":"test","displayName":"Test","percentageRollout":50}'

# Evaluate for a user
curl -X POST http://localhost:5000/api/featureflag/evaluate \
  -H "Content-Type: application/json" \
  -d '{"featureFlagKey":"test","userId":"user1"}'

# Get audit logs
curl http://localhost:5000/api/featureflag/550e8400-e29b-41d4-a716-446655440000/audit

# Update the flag
curl -X PUT http://localhost:5000/api/featureflag/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{"percentageRollout":75}'

# Disable the flag
curl -X POST http://localhost:5000/api/featureflag/550e8400-e29b-41d4-a716-446655440000/disable
```

---

## Pagination

List endpoints support pagination with these parameters:

- `pageNumber` (integer): 1-based page number, default 1
- `pageSize` (integer): Results per page, default 20, max 100

Response includes:

```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 45,
  "totalPages": 3
}
```

---

## Filtering & Sorting

### Supported Sort Fields

- `createdDate` (default)
- `modifiedDate`
- `displayName`
- `key`
- `percentageRollout`

### Sort Order

- `asc`: Ascending (oldest first)
- `desc`: Descending (newest first)

---

For more examples, see the [examples directory](../examples/).
