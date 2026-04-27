# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-05-21
### Fixed
- Fix percentage rollout inconsistency across application restarts
- Added regression test for the fix

## [2.0.1] - 2026-07-21
### Security
- Added input validation and length limits
- Added request timeout configuration
- Added security policy and vulnerability reporting

## [2.0.0] - 2026-07-14

### Added
- Add feature experimentation with metrics collection and analysis
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

---

## [1.0.0] - 2025-05-07

### Added
- NuGet packaging configuration with full metadata
- CodeQL security scanning workflow
- Dependabot configuration for NuGet and GitHub Actions
- Complete documentation suite: API reference, getting started guide, FAQ, deployment guide
- Performance benchmarks table with throughput and latency measurements
- Cross-references and integration examples with redis-cache-patterns

### Changed
- Promoted project to stable 1.0.0 release
- Hardened all API endpoints with input validation
- Rate limiting applied per IP address to prevent abuse
- Improved secrets handling via environment variable overrides

### Fixed
- Null reference in variant allocation when all percentages sum to exactly 100
- Concurrent cache write race condition under high load

---

## [0.9.0] - 2025-04-30

### Added
- Docker and Docker Compose support with Alpine-based image
- OpenAPI/Swagger documentation on all controller endpoints
- Example projects: BasicEvaluation, UserTargeting, ABTesting, AdvancedScenarios, MiddlewareIntegration, TestingAndMonitoring
- Phase2 dependency injection extensions for optional services
- Health check endpoint

### Changed
- Restructured examples directory with standalone runnable files
- Improved Dockerfile multi-stage build for smaller production image

### Fixed
- Docker Compose SQL Server volume mount path on Linux

---

## [0.8.0] - 2025-04-16

### Changed
- Optimized rule evaluation with short-circuit logic for AND conditions
- Improved audit log change diffs to include old and new values side by side
- Optimized database queries with explicit eager loading for rules and conditions
- Enhanced error messages with actionable resolution hints

### Fixed
- Rule priority ordering: lower `Priority` value now correctly evaluated first
- Percentage rollout distribution off-by-one at 0% and 100% boundaries
- Condition evaluation failing for attribute values containing commas
- Memory usage spike when evaluating flags with large variant sets

---

## [0.7.0] - 2025-04-02

### Added
- CSV and XML export formatters (`CsvExporter`, `XmlExporter`)
- CLI argument parser for command-line flag evaluation and management
- `PerformanceMonitor` utility for measuring evaluation latency
- `AuditLogCleanupWorker` background job for automatic log retention enforcement
- `SearchQueryBuilder` for dynamic flag search with multiple filter criteria

### Changed
- `AuditLogService` now records old and new field values on every change
- CLI `--export` command supports `--format csv|xml|json` and `--output` path

---

## [0.6.0] - 2025-03-19

### Added
- Webhook integration: register endpoints that receive payloads on flag changes
- `WebhookService` and `WebhookRepository` with HMAC-SHA256 signature verification
- `EventSystem` for internal publish/subscribe between services
- Custom user attributes via `UserContext.SetCustomAttribute`
- Admin controller endpoints for bulk enable, disable, and delete operations

### Changed
- `FeatureFlagService` now fires internal events on create, update, and delete
- Webhook delivery retries up to 3 times with exponential backoff

### Fixed
- Flag key uniqueness constraint not enforced at service layer

---

## [0.5.0] - 2025-03-05

### Added
- In-memory caching layer (`CacheService`) with configurable TTL
- `PaginationHelper` for consistent page/size handling across all list endpoints
- `RequestLoggingMiddleware` with correlation ID injection
- `FeatureFlagOptions` configuration binding from `appsettings.json`
- `FeatureFlags__*` environment variable overrides for all options

### Changed
- All list endpoints now return paginated responses with `totalCount`, `pageNumber`, and `pageSize`
- Cache enabled by default with a 5-minute TTL; disable via `FeatureFlags:EnableCache=false`

### Fixed
- Search returning duplicate results when flags matched multiple conditions

---

## [0.4.0] - 2025-02-19

### Added
- `GradualRolloutSchedulerService` and `GradualRolloutSchedulerWorker` for time-based percentage increments
- `RateLimitingMiddleware` with configurable request-per-window thresholds
- `AuthenticationMiddleware` with API key validation
- `ErrorHandlingMiddleware` returning structured `ApiResponse` on unhandled exceptions

### Changed
- Background workers registered as hosted services via `IHostedService`
- Error responses standardized to `{ success, error, statusCode }` envelope

### Fixed
- Gradual rollout scheduler not advancing percentage when host restarted mid-schedule

---

## [0.3.0] - 2025-02-05

### Added
- A/B testing with `ABTestVariant` model and percentage-based allocation
- `GetVariantAsync` method on `IFeatureFlagService`
- `AuditLogService` and `AuditLogRepository` for change history
- `AuditController` with paginated log retrieval and user-scoped queries
- Dependency injection extensions (`DependencyInjectionExtensions`) for one-call service registration
- Audit log retention policy (`CleanupOldLogsAsync`)

### Changed
- `FeatureFlagDbContext` extended with `AuditLogs` and `Variants` `DbSet`s
- Variant allocation uses the same consistent hash as percentage rollout for reproducibility

### Fixed
- A/B allocation producing different variants for the same user across app restarts

---

## [0.2.0] - 2025-01-22

### Added
- Percentage-based rollout with MurmurHash3 consistent hashing (`PercentageRolloutService`)
- `UserContext` model with standard attributes: `UserId`, `Email`, `Tier`, `Country`, `Region`
- `RuleEvaluationService` for evaluating targeting rules against user context
- `Condition` model supporting operators: `Equals`, `NotEquals`, `Contains`, `In`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`
- `Rule` model with AND/OR `ConditionLogic` and integer `Priority`
- `RolloutType` enum: `Boolean`, `Percentage`, `RulesBased`, `ABTest`

### Changed
- `FeatureFlagService.IsEnabledAsync` now routes evaluation through rollout type strategy

### Fixed
- Case-sensitive attribute matching now documented; exact-match semantics preserved

---

## [0.1.0] - 2025-01-08

### Added
- Initial project structure: solution, `src/FeatureFlags`, `src/FeatureFlags.Tests`
- `FeatureFlag` entity with key, display name, description, enabled state, created/modified dates
- EF Core integration with SQL Server (`FeatureFlagDbContext`)
- `FeatureFlagRepository` implementing generic `IRepository<T>` pattern
- `FeatureFlagService` with create, read, update, delete, enable, and disable operations
- `FeatureFlagController` REST API (GET, POST, PUT, DELETE, enable, disable)
- `DatabaseSeeder` with example seed data
- Repository pattern interfaces (`IRepository`, `IFeatureFlagRepository`)
- Unit test project with xunit, Moq, and FluentAssertions
- MIT License

---

## Credits

All development by [Vladyslav Zaiets](https://sarmkadan.com)

See individual commits for detailed contributions.

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

For more information:
- [README](README.md)
- [Getting Started](docs/getting-started.md)
- [API Reference](docs/api-reference.md)
- [GitHub Releases](https://github.com/Sarmkadan/dotnet-feature-flags/releases)
