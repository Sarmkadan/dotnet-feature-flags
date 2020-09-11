# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2024-02-20

### Added
- Webhook integration for flag change notifications
- Custom user attributes support in conditions
- Performance monitoring metrics endpoint
- Bulk operations for flags (enable/disable/delete multiple)
- Export functionality (CSV, XML, JSON formats)
- Rate limiting middleware with configurable thresholds
- Request correlation ID for distributed tracing
- Advanced search with fuzzy matching
- Flag dependency resolution
- Canary deployment patterns in examples

### Changed
- Improved rule evaluation performance with caching
- Enhanced audit log details with change diffs
- Optimized database queries with better indexing
- Upgraded to .NET 10 with latest C# 13 features
- Updated Docker image to Alpine-based for smaller footprint

### Fixed
- Bug in percentage rollout distribution for edge cases
- Concurrent access issues in cache layer
- Null reference exception in variant allocation
- Database migration rollback support
- Memory leak in long-running evaluations

### Deprecated
- Legacy condition operators (will be removed in 2.0)

### Security
- Added input validation on all API endpoints
- Implemented CSRF protection
- Enhanced SQL injection prevention
- Added rate limiting per IP address
- Improved secrets handling in configuration

## [1.1.0] - 2024-01-15

### Added
- A/B test variant allocation with conversion tracking
- Gradual rollout strategy with daily percentage increments
- Rule-based evaluation with AND/OR logic
- Comprehensive audit logging with change history
- Pagination support for large datasets
- Caching layer with configurable TTL
- Search and filtering capabilities
- Admin dashboard endpoints
- Health check endpoint
- OpenAPI/Swagger documentation
- Docker and Docker Compose support
- Comprehensive documentation and examples
- Integration with Application Insights

### Changed
- Refactored FeatureFlagService for better testability
- Improved database schema with proper relationships
- Enhanced error messages with actionable details
- Optimized consistent hashing algorithm

### Fixed
- Issue with rule priority not being respected
- Condition evaluation with special characters
- Memory usage with large flag sets
- Concurrent request handling

## [1.0.0] - 2023-12-01

### Added
- Core feature flag evaluation engine
- Percentage-based rollout with consistent hashing
- User targeting with conditions
- EF Core integration with SQL Server
- REST API for flag management
- Repository pattern with generic base
- Dependency injection configuration
- Unit test suite
- Project documentation
- MIT License

### Features
- Create, read, update, delete feature flags
- Enable/disable flags in real-time
- Evaluate flags for users with context
- Support for custom user attributes
- Audit logging with compliance tracking
- Type-safe C# implementation
- Production-ready error handling

---

## Version History Details

### v0.1.0 (Unreleased)

Initial project setup with:
- Solution structure
- Project templates
- Git initialization
- Basic documentation

---

## Roadmap

### v1.3.0 (Planned)
- [ ] Redis caching integration
- [ ] Multi-tenant support
- [ ] Advanced analytics dashboard
- [ ] Flag versioning and rollback
- [ ] Custom operators via plugins
- [ ] GraphQL API support

### v2.0.0 (Planned)
- [ ] Breaking changes cleanup
- [ ] Native SDKs for popular frameworks
- [ ] Real-time flag updates via WebSockets
- [ ] Advanced targeting with machine learning
- [ ] Managed cloud hosting option
- [ ] Performance dashboard and insights

---

## Migration Guides

### Upgrading from 1.1.0 to 1.2.0

No breaking changes. Simply update the NuGet package:

```bash
dotnet add package FeatureFlags --version 1.2.0
```

Run database migrations:

```bash
dotnet ef database update
```

### Upgrading from 1.0.0 to 1.1.0

1. Update package
2. Run migrations: `dotnet ef database update`
3. No code changes required for basic usage
4. To use A/B testing, create flags with `RolloutType.ABTest`

---

## Support

For issues with specific versions, please reference the version number in your GitHub issue.

- **Current**: v1.2.0
- **LTS**: v1.0.0 (security fixes only)
- **EOL**: None yet

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
