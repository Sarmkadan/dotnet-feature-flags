# Audit Controller

API endpoints for accessing and analyzing audit logs of feature flag changes.

## Endpoints

### Get Audit Logs for a Feature Flag

Retrieves paginated audit logs for a specific feature flag.

**HTTP Request**
```
GET /api/audit/flags/{featureFlagId}
```

**Path Parameters**
| Parameter | Type | Description |
|-----------|------|-------------|
| featureFlagId | integer | The ID of the feature flag |

**Query Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | integer | 1 | Page number for pagination |
| pageSize | integer | 20 | Number of items per page |

**Responses**
| Status Code | Description |
|-------------|-------------|
| 200 OK | Paginated list of audit logs |
| 404 Not Found | Feature flag not found |
| 500 Internal Server Error | Unexpected error |

### Get Audit Logs by User

Retrieves paginated audit logs filtered by the user who made the change.

**HTTP Request**
```
GET /api/audit/by-user/{username}
```

**Path Parameters**
| Parameter | Type | Description |
|-----------|------|-------------|
| username | string | The username of the user who made the changes |

**Query Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | integer | 1 | Page number for pagination |
| pageSize | integer | 20 | Number of items per page |

**Responses**
| Status Code | Description |
|-------------|-------------|
| 200 OK | Paginated list of audit logs |
| 500 Internal Server Error | Unexpected error |

### Get Audit Logs by Date Range

Retrieves paginated audit logs within a specified date range.

**HTTP Request**
```
GET /api/audit/by-date-range
```

**Query Parameters**
| Parameter | Type | Description |
|-----------|------|-------------|
| startDate | date-time (required) | Start of the date range (inclusive) |
| endDate | date-time (required) | End of the date range (inclusive) |
| page | integer | 1 | Page number for pagination |
| pageSize | integer | 20 | Number of items per page |

**Responses**
| Status Code | Description |
|-------------|-------------|
| 200 OK | Paginated list of audit logs |
| 400 Bad Request | Start date is after end date |
| 500 Internal Server Error | Unexpected error |

### Get Change History for a Feature Flag

Retrieves the change history for a specific feature flag showing before/after values.

**HTTP Request**
```
GET /api/audit/history/{featureFlagId}
```

**Path Parameters**
| Parameter | Type | Description |
|-----------|------|-------------|
| featureFlagId | integer | The ID of the feature flag |

**Query Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| maxEntries | integer | 50 | Maximum number of history entries to return |

**Responses**
| Status Code | Description |
|-------------|-------------|
| 200 OK | List of change history entries |
| 500 Internal Server Error | Unexpected error |

### Get Audit Summary

Retrieves a summary of all audit activity.

**HTTP Request**
```
GET /api/audit/summary
```

**Query Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| days | integer | 30 | Number of days to look back for the summary |

**Responses**
| Status Code | Description |
|-------------|-------------|
| 200 OK | Audit summary object |
| 500 Internal Server Error | Unexpected error |

**Response Body (Summary)**
```json
{
  "totalChanges": 0,
  "uniqueUsers": 0,
  "changesByAction": [
    {
      "action": "string",
      "count": 0
    }
  ],
  "changesByFlag": [
    {
      "flagId": 0,
      "count": 0
    }
  ],
  "mostActiveUsers": [
    {
      "user": "string",
      "count": 0
    }
  ]
}
```

### Export Audit Logs to CSV

Exports audit logs to CSV format.

**HTTP Request**
```
GET /api/audit/export/csv
```

**Query Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| featureFlagId | integer (optional) | null | If provided, filters logs to this feature flag |
| days | integer | 30 | Number of days to look back for the export |

**Responses**
| Status Code | Description |
|-------------|-------------|
| 200 OK | CSV file download |
| 500 Internal Server Error | Unexpected error |

**Response Headers**
- `Content-Type`: text/csv
- `Content-Disposition`: attachment; filename="audit-logs-{timestamp}.csv"

## Models

### AuditLog
Represents an audit log entry for a feature flag change.

| Property | Type | Description |
|----------|------|-------------|
| Id | integer | Unique identifier for the audit log |
| FeatureFlagId | integer | ID of the associated feature flag |
| Action | AuditAction | Type of action performed (see AuditAction enum) |
| ChangedBy | string | Username of the user who made the change |
| ChangedAt | date-time | Timestamp when the change was made |
| OldValue | string | Previous value (if applicable) |
| NewValue | string | New value (if applicable) |

### AuditAction
Enum representing the types of actions that can be audited.

| Value | Description |
|-------|-------------|
| Created | Feature flag was created |
| Updated | Feature flag was updated |
| Enabled | Feature flag was enabled |
| Disabled | Feature flag was disabled |
| RolloutChanged | Rollout percentage was changed |
| RuleAdded | A rule was added to the feature flag |
| RuleRemoved | A rule was removed from the feature flag |
| VariantUpdated | A variant was updated |
| Deleted | Feature flag was deleted |

### PaginatedApiResponse<T>
Generic wrapper for paginated API responses.

| Property | Type | Description |
|----------|------|-------------|
| Success | boolean | Indicates if the request was successful |
| Data | T[] | Array of data items |
| Pagination | PaginationInfo | Pagination metadata |

### PaginationInfo
Contains pagination metadata.

| Property | Type | Description |
|----------|------|-------------|
| PageNumber | integer | Current page number |
| PageSize | integer | Number of items per page |
| TotalCount | integer | Total number of items available |
| TotalPages | integer | Total number of pages |

### ApiResponse<T>
Standard API response wrapper.

| Property | Type | Description |
|----------|------|-------------|
| Success | boolean | Indicates if the request was successful |
| Data | T | The response data (if successful) |
| Message | string | Additional message about the response |