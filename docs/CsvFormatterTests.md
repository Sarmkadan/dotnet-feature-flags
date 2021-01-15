# CsvFormatterTests

Unit tests for CSV and XML serialization/deserialization logic in the `dotnet-feature-flags` project. These tests validate the correct formatting and parsing of feature flag data and audit logs in both CSV and XML formats, ensuring round-trip consistency and proper handling of edge cases like quoted values and invalid XML.

## API

### `ExportFeatureFlags_ProducesValidCsv`
Validates that the `ExportFeatureFlags` method generates a properly formatted CSV output containing all feature flags with correct headers and data rows.

### `ExportAuditLogs_ProducesValidCsv`
Validates that the `ExportAuditLogs` method generates a properly formatted CSV output containing all audit log entries with correct headers and data rows.

### `ParseCsv_ReadsValidCsv`
Validates that the CSV parser correctly reads and deserializes a well-formed CSV input into the appropriate data structures.

### `ParseCsv_HandlesQuotedValues`
Validates that the CSV parser correctly handles quoted values, including those containing commas, newlines, or other special characters.

### `ExportFeatureFlags_ProducesValidXml`
Validates that the `ExportFeatureFlags` method generates a properly formatted XML output containing all feature flags with correct structure and data.

### `ExportAuditLogs_ProducesValidXml`
Validates that the `ExportAuditLogs` method generates a properly formatted XML output containing all audit log entries with correct structure and data.

### `ParseXml_ReadsValidXml`
Validates that the XML parser correctly reads and deserializes a well-formed XML input into the appropriate data structures.

### `ParseXml_HandlesInvalidXml`
Validates that the XML parser gracefully handles malformed or invalid XML input by throwing appropriate exceptions.

### `CreateOptions_ReturnsValidOptions`
Validates that the `CreateOptions` method returns a properly configured set of options for CSV/ XML processing with all required fields set.

### `CreateCompactOptions_ReturnsValidOptions`
Validates that the `CreateCompactOptions` method returns a properly configured set of compact options for CSV/XML processing with minimal required fields set.

### `FeatureFlagConverter_SerializesCorrectly`
Validates that the `FeatureFlagConverter` correctly serializes and deserializes feature flag objects to/from CSV/XML formats.

## Usage
