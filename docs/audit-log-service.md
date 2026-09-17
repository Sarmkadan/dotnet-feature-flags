# AuditLogService

## Overview
The `AuditLogService` is responsible for managing audit trail operations for feature flags. It provides functionality to retrieve, query, and clean up audit log entries that track all changes made to feature flags for compliance, debugging, and historical analysis purposes.

## Responsibilities

### Audit Trail Management
- Retrieves audit logs for specific feature flags
- Provides paginated access to audit log data
- Filters audit logs by user who made changes
- Retrieves recent audit log entries
- Gets the most recent change for a feature flag
- Queries audit logs within date ranges
- Cleans up old audit log entries based on retention policies

### Data Validation & Error Handling
- Validates input parameters (feature flag IDs, page numbers, page sizes, dates)
- Handles exceptions from the repository layer
- Logs errors appropriately using the injected logger
- Converts repository exceptions to domain-specific `FeatureFlagDataException`

### Compliance & Debugging Support
- Enables change history review for compliance requirements
- Supports rollback analysis by tracking old and new values
- Provides IP address and user agent tracking for security auditing
- Offers human-readable change descriptions and summaries

## API Reference

### Methods

#### `GetAuditLogsAsync(int featureFlagId)`
Retrieves all audit log entries for a specific feature flag.

**Parameters:**
- `featureFlagId`: The ID of the feature flag (must be > 0)

**Returns:**
- `Task<IEnumerable<AuditLog>>`: Collection of audit log entries

**Exceptions:**
- `ArgumentException`: If featureFlagId is not > 0
- `FeatureFlagDataException`: If repository operation fails

#### `GetAuditLogsPagedAsync(int featureFlagId, int pageNumber, int pageSize)`
Retrieves a paginated list of audit log entries for a specific feature flag.

**Parameters:**
- `featureFlagId`: The ID of the feature flag (must be > 0)
- `pageNumber`: The page number to retrieve (must be >= 1)
- `pageSize`: Number of entries per page (must be between 1 and `FeatureFlagConstants.MaxPageSize`)

**Returns:**
- `Task<IEnumerable<AuditLog>>`: Paginated collection of audit log entries

**Exceptions:**
- `ArgumentException`: If any parameter is invalid
- `FeatureFlagDataException`: If repository operation fails

#### `GetAuditLogsByUserAsync(string changedBy)`
Retrieves all audit log entries made by a specific user.

**Parameters:**
- `changedBy`: The name/user ID of the user who made changes (cannot be null or empty)

**Returns:**
- `Task<IEnumerable<AuditLog>>`: Collection of audit log entries by the user

**Exceptions:**
- `ArgumentException`: If changedBy is null or empty
- `FeatureFlagDataException`: If repository operation fails

#### `GetRecentAuditLogsAsync(int count)`
Retrieves the most recent audit log entries across all feature flags.

**Parameters:**
- `count`: Number of recent entries to retrieve (must be >= 1)

**Returns:**
- `Task<IEnumerable<AuditLog>>`: Collection of recent audit log entries ordered by date (newest first)

**Exceptions:**
- `ArgumentException`: If count is not >= 1
- `FeatureFlagDataException`: If repository operation fails

#### `GetLastChangeAsync(int featureFlagId, CancellationToken cancellationToken = default)`
Retrieves the most recent change for a specific feature flag.

**Parameters:**
- `featureFlagId`: The ID of the feature flag (must be > 0)
- `cancellationToken`: Optional token to cancel the operation

**Returns:**
- `Task<AuditLog?>`: The most recent audit log entry, or null if none exists

**Exceptions:**
- `ArgumentException`: If featureFlagId is not > 0
- `FeatureFlagDataException`: If repository operation fails

#### `GetChangeHistoryAsync(DateTime startDate, DateTime endDate)`
Retrieves all audit log entries within a specified date range.

**Parameters:**
- `startDate`: Inclusive start date of the range
- `endDate`: Inclusive end date of the range (must be >= startDate)

**Returns:**
- `Task<IEnumerable<AuditLog>>`: Collection of audit log entries within the date range

**Exceptions:**
- `ArgumentException`: If startDate is after endDate
- `FeatureFlagDataException`: If repository operation fails

#### `CleanupOldLogsAsync(int retentionDays, CancellationToken cancellationToken = default)`
Removes audit log entries older than the specified retention period.

**Parameters:**
- `retentionDays`: Number of days of audit history to retain (must be >= 1)
- `cancellationToken`: Optional token to cancel the operation

**Returns:**
- `Task`: Represents the asynchronous cleanup operation

**Exceptions:**
- `ArgumentException`: If retentionDays is not >= 1
- `FeatureFlagDataException`: If repository operation fails

## AuditLog Model

The service works with the `AuditLog` model which contains:

### Properties
- `Id`: Unique identifier of the audit log entry
- `FeatureFlagId`: ID of the feature flag that was changed
- `Action`: Type of action performed (see `AuditAction` enum)
- `ChangedBy`: User who made the change
- `ChangedAt`: UTC timestamp when the change was made
- `OldValue`: Value of the feature flag before the change
- `NewValue`: Value of the feature flag after the change
- `Description`: Human-readable description of the change
- `IpAddress`: IP address from which the change was made (nullable)
- `UserAgent`: User agent of the client that made the change (nullable)
- `FeatureFlag`: Navigation property to the associated feature flag

### Methods
- `GetSummary()`: Returns a formatted string summarizing the change
- `IsRollbackOf(AuditLog? previousLog)`: Determines if this change is a rollback compared to another entry
- `IsValid()`: Validates that the audit log has required fields
- `GetChangeDetails()`: Returns a tuple of (oldValue, newValue) for analysis

## Dependencies
- `IAuditLogRepository`: Data access layer for audit log operations
- `ILogger<AuditLogService>`: Logging infrastructure for diagnostics and error tracking

## Implementation Notes
- All methods validate their input parameters before proceeding
- Repository exceptions are caught, logged, and rethrown as `FeatureFlagDataException`
- The service implements explicit interface implementations for `GetLastChangeAsync` and `CleanupOldLogsAsync` to match the `IAuditLogService` interface
- Uses UTC timestamps consistently for all date/time operations
- Follows defensive programming practices with null checks and argument validation