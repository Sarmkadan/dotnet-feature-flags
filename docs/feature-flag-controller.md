# Feature flag controller API

`FeatureFlagController` exposes evaluation and management operations under the route:

```text
/api/FeatureFlag
```

Examples below use ASP.NET Core's default web JSON conventions (camel-case property names). All endpoints can return `500 Internal Server Error` with a plain-text error message if a service operation fails. The controller does not declare authorization requirements; the application's surrounding middleware and configuration may add them.

## Endpoint summary

| Method | Route | Purpose | Success |
| --- | --- | --- | --- |
| `POST` | `/api/FeatureFlag/evaluate` | Evaluate one flag for a user | `200 OK` |
| `POST` | `/api/FeatureFlag/variant` | Resolve one flag's A/B-test variant | `200 OK` |
| `GET` | `/api/FeatureFlag` | Get all flags | `200 OK` |
| `GET` | `/api/FeatureFlag/{key}` | Get a flag by key | `200 OK` |
| `POST` | `/api/FeatureFlag` | Create a flag | `201 Created` |
| `PUT` | `/api/FeatureFlag/{id}` | Update a flag | `204 No Content` |
| `POST` | `/api/FeatureFlag/{id}/enable` | Enable a flag | `204 No Content` |
| `POST` | `/api/FeatureFlag/{id}/disable` | Disable a flag | `204 No Content` |
| `GET` | `/api/FeatureFlag/{id}/audit` | Get a flag's audit logs | `200 OK` |
| `POST` | `/api/FeatureFlag/evaluate/all` | Evaluate all active flags | `200 OK` |
| `GET` | `/api/FeatureFlag/evaluate/all` | Evaluate all active flags, with optional HTTP caching | `200 OK` or `304 Not Modified` |

## Single-flag evaluation

### Evaluate a flag

```http
POST /api/FeatureFlag/evaluate
Content-Type: application/json
```

Request body:

```json
{
  "featureFlagKey": "new-checkout",
  "userId": "user-123",
  "email": "user@example.com",
  "country": "US",
  "tier": "premium",
  "region": "west",
  "customAttributes": {
    "companySize": "large"
  }
}
```

`featureFlagKey`, `userId`, and `email` must be non-empty. The other properties are optional; omitted custom attributes are treated as an empty dictionary.

Success response:

```http
HTTP/1.1 200 OK
Content-Type: application/json

{"enabled":true}
```

The endpoint returns `400 Bad Request` with `Feature flag key is required` for a missing or empty key, or `User context must have userId and email` when the user context is invalid.

### Get a variant

```http
POST /api/FeatureFlag/variant
Content-Type: application/json
```

This endpoint accepts the same request body and validation rules as `/evaluate`.

```http
HTTP/1.1 200 OK
Content-Type: application/json

{"variant":"treatment-a"}
```

`variant` can be `null` when the service does not resolve a variant.

## Flag management

### Get all flags

```http
GET /api/FeatureFlag
```

Returns `200 OK` with the collection returned by `IFeatureFlagService.GetAllFeatureFlagsAsync()`.

### Get a flag by key

```http
GET /api/FeatureFlag/new-checkout
```

Returns `200 OK` with the feature flag. If no flag has the supplied key, it returns:

```http
HTTP/1.1 404 Not Found

Feature flag 'new-checkout' not found
```

### Create a flag

```http
POST /api/FeatureFlag
Content-Type: application/json

{
  "key": "new-checkout",
  "displayName": "New checkout",
  "description": "Enables the redesigned checkout",
  "isEnabled": false,
  "rolloutType": 0,
  "percentageRollout": 25,
  "rules": [],
  "variants": []
}
```

The body is a `FeatureFlag`. It must pass `FeatureFlag.IsValid()`, including a non-blank key and display name and valid rollout configuration. Invalid input returns `400 Bad Request` with `Invalid feature flag configuration`.

On success, the endpoint returns `201 Created`, the created flag in the response body, and a `Location` header pointing to `GET /api/FeatureFlag/{key}`. The acting user is `User.Identity.Name`, or `System` when no identity name is available.

### Update a flag

```http
PUT /api/FeatureFlag/42
Content-Type: application/json

{
  "id": 42,
  "key": "new-checkout",
  "displayName": "New checkout",
  "description": "Updated description",
  "isEnabled": true,
  "rolloutType": 0,
  "percentageRollout": 50,
  "rules": [],
  "variants": []
}
```

The body ID must equal the integer `{id}` in the route. A mismatch returns `400 Bad Request` with `Invalid feature flag`. Success returns `204 No Content`. The controller passes the authenticated identity name, or `System`, to the service as the actor.

### Enable or disable a flag

```http
POST /api/FeatureFlag/42/enable
POST /api/FeatureFlag/42/disable
```

Both routes take an integer flag ID and return `204 No Content` on success. They use the authenticated identity name, or `System`, as the actor.

### Get audit logs

```http
GET /api/FeatureFlag/42/audit
```

Returns `200 OK` with the audit-log collection for the integer flag ID.

## Bulk evaluation

Bulk endpoints evaluate all active flags and return an object whose `results` properties are feature flag keys.

Each result contains:

| Property | Type | Description |
| --- | --- | --- |
| `enabled` | boolean | Whether the flag is enabled for the user. |
| `variant` | string or null | A/B-test variant; requested with `includeVariants`. |
| `reason` | string or null | Evaluation reason; requested with `includeReasons`. Possible reasons include `PercentageRollout`, `RulesBased`, `ABTest`, `Full`, `FlagDisabled`, and `FlagNotFound`. |
| `percentage` | integer or null | Rollout percentage when applicable and reasons are requested. |

### Bulk evaluation with POST

```http
POST /api/FeatureFlag/evaluate/all
Content-Type: application/json

{
  "userContext": {
    "userId": "user-123",
    "email": "user@example.com",
    "country": "US",
    "tier": "premium",
    "region": "west",
    "customAttributes": {
      "companySize": "large"
    }
  },
  "includeVariants": true,
  "includeReasons": true,
  "includeETag": true
}
```

`userContext` is required, and its `userId` and `email` must be non-blank. The three `include...` options default to `false`. Invalid user context returns `400 Bad Request` with `User context must have userId and email`.

Example response:

```json
{
  "results": {
    "new-checkout": {
      "enabled": true,
      "variant": "treatment-a",
      "reason": "ABTest",
      "percentage": null
    }
  },
  "eTag": "configuration-hash"
}
```

When `includeETag` is false, the response object's `eTag` value is `null`. This POST endpoint includes the value in the JSON body only; it does not implement `If-None-Match` handling.

### Bulk evaluation with GET

```http
GET /api/FeatureFlag/evaluate/all?userId=user-123&email=user%40example.com&country=US&includeVariants=true&includeReasons=true&includeETag=true
```

Query parameters:

| Parameter | Required | Default | Description |
| --- | --- | --- | --- |
| `userId` | Yes | — | User identifier. |
| `email` | Yes | — | User email. |
| `country` | No | `null` | User country. |
| `tier` | No | `null` | User tier. |
| `region` | No | `null` | User region. |
| `includeVariants` | No | `false` | Include variant data. |
| `includeReasons` | No | `false` | Include reasons and applicable rollout percentages. |
| `includeETag` | No | `false` | Return an ETag and enable conditional requests. |

The GET form does not accept custom attributes. Its response has the same JSON shape as the POST form.

When `includeETag=true`, the response includes both an `ETag` HTTP header and the `eTag` JSON property. A subsequent request can send that value in `If-None-Match`:

```http
GET /api/FeatureFlag/evaluate/all?userId=user-123&email=user%40example.com&includeETag=true
If-None-Match: configuration-hash
```

If it exactly matches the current value, the endpoint returns `304 Not Modified` without evaluating the flags. ETag comparison is a literal string comparison, so clients should resend the header value exactly as received.

## Validation and error behavior

Because the controller has `[ApiController]`, ASP.NET Core can reject model-binding and required-member failures before an action executes. In addition to those framework-generated responses, the actions explicitly return the `400`, `404`, `304`, and `500` responses described above. Service exceptions are logged and converted to endpoint-specific plain-text `500` responses; exception details are not returned to the client.
