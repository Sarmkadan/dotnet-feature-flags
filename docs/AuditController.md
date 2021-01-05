# AuditController

The `AuditController` provides endpoints for retrieving and exporting audit logs and change history related to feature flag operations. It supports querying logs by user, date range, and exporting data in CSV format for compliance and analysis purposes.

## API

### `public AuditController()`

Initializes a new instance of the `AuditController` class.

### `public async Task<IActionResult> GetFlagAuditLog()`

Retrieves the audit log entries for a specific feature flag.

**Returns:**
- `200 OK` with the list of audit log entries if successful.
- `404 NotFound` if the flag does not exist.

**Throws:**
- `ArgumentNullException` if the flag identifier is null or empty.

---

### `public async Task<IActionResult> GetAuditLogsByUser(string userId)`

Retrieves all audit log entries associated with a specific user.

**Parameters:**
- `userId` (string): The unique identifier of the user whose audit logs are to be retrieved.

**Returns:**
- `200 OK` with the list of audit log entries if successful.
- `400 BadRequest` if the `userId` is null or empty.

**Throws:**
- No exceptions are explicitly thrown; invalid parameters result in HTTP error responses.

---

### `public async Task<IActionResult> GetAuditLogsByDateRange(DateTime startDate, DateTime endDate)`

Retrieves audit log entries within a specified date range.

**Parameters:**
- `startDate` (DateTime): The start of the date range (inclusive).
- `endDate` (DateTime): The end of the date range (inclusive).

**Returns:**
- `200 OK` with the list of audit log entries if successful.
- `400 BadRequest` if `startDate` is after `endDate`.

**Throws:**
- No exceptions are explicitly thrown; invalid date ranges result in HTTP error responses.

---
### `public async Task<IActionResult> GetChangeHistory(string flagId)`

Retrieves the detailed change history for a specific feature flag.

**Parameters:**
- `flagId` (string): The unique identifier of the feature flag.

**Returns:**
- `200 OK` with the list of change history entries if successful.
- `404 NotFound` if the flag does not exist.

**Throws:**
- `ArgumentNullException` if the `flagId` is null or empty.

---
### `public async Task<IActionResult> GetAuditSummary()`

Retrieves a summary of audit log statistics, such as total entries, recent activity, and user-specific counts.

**Returns:**
- `200 OK` with the audit summary data if successful.

**Throws:**
- No exceptions are explicitly thrown.

---
### `public async Task<IActionResult> ExportAuditLogsCsv()`

Exports all audit logs in CSV format.

**Returns:**
- `200 OK` with the CSV file content if successful.
- `204 NoContent` if no audit logs exist.

**Throws:**
- No exceptions are explicitly thrown.

## Usage

### Example 1: Retrieving audit logs by user
