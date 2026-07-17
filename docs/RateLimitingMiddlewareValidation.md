# RateLimitingMiddlewareValidation

Utility class for validating configuration settings used by the rate-limiting middleware in the `dotnet-feature-flags` project. It ensures that rate-limiting parameters such as retry counts and wait durations are within acceptable bounds to prevent misconfiguration that could degrade service performance or stability.

## API

### `Validate`
