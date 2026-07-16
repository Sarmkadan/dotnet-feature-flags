# Architecture

This document describes how the solution is actually laid out and why. If it disagrees with the code, the code wins - and please fix this file.

## Overview

`dotnet-feature-flags` is an ASP.NET Core (net10.0, C#) feature flag engine exposed as a REST API. It evaluates flags against a user context using three mechanisms that can be combined per flag:

- **Percentage rollout** - consistent-hash bucketing of `(UserId + flag key)` into 0-99, so a given user always lands in the same bucket (`PercentageRolloutService`, `HashingUtilities`).
- **Rule-based targeting** - prioritized rules made of conditions (`Equals`, `Contains`, `GreaterThan`, `In`, ...) with AND/OR logic (`RuleEvaluationService`).
- **A/B variants** - variant allocation with conversion tracking (`ABTestVariant`, `FeatureFlagService.GetVariantAsync`).

Persistence is EF Core against SQL Server; every mutation of a flag is written to an audit trail.

## Solution layout

```
src/FeatureFlags/            main web API project
  Controllers/               FeatureFlag, Admin, Audit, Health
  Services/                  business logic (interfaces + implementations)
  Repository/                EF Core data access behind interfaces
  Models/                    entities + validation/extension partials
  Enums/                     RolloutType, ConditionOperator, AuditAction
  Data/                      FeatureFlagDbContext, DatabaseSeeder
  Configuration/             DI extensions + FeatureFlagOptions
  Middleware/                error handling, request logging, rate limiting, auth
  Caching/                   ICacheService (in-memory + Redis-backed)
  Events/                    in-process IEventBus / IEventSubscriber
  Integration/               webhooks + HttpApiClient
  BackgroundJobs/            hosted workers (audit cleanup, rollout scheduler, ...)
  CLI/                       CliArgumentParser for tooling scenarios
  Formatters/                CSV/XML exporters, custom JSON converter
  Utilities/                 hashing, pagination, search query builder, etc.
src/FeatureFlags.Tests/      xUnit tests (formatters, caching, utilities, ...)
tests/dotnet-feature-flags.Tests/   older test project (models, services)
benchmarks/                  BenchmarkDotNet project
examples/                    standalone usage samples
docs/                        per-type reference docs + this file
```

## Runtime composition

`Program.cs` builds a standard minimal-hosting pipeline:

1. Console/Debug logging.
2. Controllers + Swagger (Swagger UI only in Development).
3. `FeatureFlagDbContext` on SQL Server via `ConnectionStrings:DefaultConnection` (throws at startup if missing - fail fast beats a half-alive app).
4. `AddFeatureFlagServices(configuration)` - the core DI module.
5. `Database.MigrateAsync()` at startup, then HTTPS redirection, authorization, controller mapping.

### DI modules

Registration is split into two extension methods in `Configuration/`:

- **`AddFeatureFlagServices`** (used by `Program.cs`): repositories (`IFeatureFlagRepository`, `IAuditLogRepository`) and services (`IFeatureFlagService`, `IRuleEvaluationService`, `IPercentageRolloutService`, `IAuditLogService`) as scoped; `IFlagEvaluationLogService` as a singleton because it buffers evaluation logs in a `ConcurrentQueue` across requests; `FeatureFlagOptions` bound from the `FeatureFlags` section.
- **`AddPhase2Services` / `UsePhase2Middleware`** (`Phase2DependencyInjectionExtensions`): caching, webhooks, the event bus, rate limiting/auth options, and hosted workers. Cache provider is chosen by config: `Cache:Provider = distributed` selects Redis (`DistributedCacheService`), anything else the in-memory `InMemoryCacheService`. **Note:** `Program.cs` does not currently call `AddPhase2Services` or `UsePhase2Middleware` - the phase-2 surface builds and is unit-tested, but is opt-in for hosts that wire it up. See "Known limitations".

Rationale for two modules: the core evaluation path has no dependency on Redis, webhooks, or background workers, so a consumer can host the evaluator with just the core module and a connection string. The split also keeps the optional (and operationally heavier) pieces behind one explicit call instead of a soup of feature toggles inside a single `AddX`.

## Data flow: a flag evaluation

```
POST /api/featureflag/evaluate
  -> FeatureFlagController
    -> IFeatureFlagService.IsEnabledAsync(key, UserContext)
      -> IFeatureFlagRepository.GetByKeyAsync (EF Core, eager-loads rules/variants as needed)
      -> disabled flag? return false immediately
      -> RolloutType switch:
           Percentage  -> IPercentageRolloutService (consistent hash bucket vs PercentageRollout)
           RulesBased  -> IRuleEvaluationService (rules by priority, conditions with AND/OR)
           ABTest/Full/None handled accordingly
      -> IFlagEvaluationLogService records the evaluation (in-memory queue)
  <- ApiResponse<T> envelope
```

Mutations (`Create/Update/Enable/Disable`) go through `FeatureFlagService`, which validates, persists via the repository, and writes an `AuditLog` row (who, what, old/new values). `AuditLogCleanupWorker` enforces retention in the background.

## Key design decisions

- **Consistent hashing over random rollout.** Deterministic bucketing means a user's flag state does not flap between requests or servers and needs no sticky state. Trade-off: distribution is only as uniform as the hash, and changing the flag key reshuffles everyone.
- **Repository layer over direct DbContext use in services.** Services stay testable without an EF provider and query intent gets a name (`GetRecentlyModifiedAsync`, `GetWithRulesAsync`). Trade-off: some pass-through boilerplate; complex ad-hoc queries need a new repository method rather than composing LINQ at the call site.
- **Rich domain models.** Entities like `FeatureFlag`, `Rule`, `Condition` carry their own validation (`IsValid()`) and small behaviors (`Condition.Evaluate`, `RolloutStrategy.GetCurrentPercentage`) instead of anemic DTOs plus a validator layer. Kept deliberately free of persistence concerns so they are usable in the examples/CLI without a database.
- **Audit-by-service, not by EF interceptor.** Audit rows are written explicitly in `FeatureFlagService` alongside the mutation. Less magic and the reason for a change (`Reason` field) can be captured; the cost is that a mutation bypassing the service layer would not be audited.
- **`Result`/`ApiResponse` envelopes instead of exceptions for expected failures.** Exceptions (`FeatureFlagException`, `ValidationException`, `ConfigurationException`) are reserved for genuinely exceptional states; `ErrorHandlingMiddleware` maps them to consistent HTTP error payloads.
- **Startup migration (`MigrateAsync`).** Convenient for a single-instance deployment (the docker-compose setup); not safe for multi-instance rollouts where two nodes could race migrations - run migrations out-of-band in that scenario.

## Extension points

- **New condition operators**: add to `ConditionOperator` and handle it in `Condition.Evaluate` / `RuleEvaluationService`.
- **Alternate rollout strategy**: implement `IPercentageRolloutService`.
- **Cache backend**: implement `ICacheService`; selection is config-driven in `AddPhase2Services`.
- **Reacting to flag changes**: subscribe via `IEventBus`/`IEventSubscriber` (in-process) or register webhooks (`IWebhookService`) for out-of-process consumers, with delivery retry handled by `WebhookRetryWorker`.
- **Export formats**: `Formatters/` follows a simple exporter shape (`CsvExporter`, `XmlExporter`).

## Known limitations

- `AddPhase2Services`/`UsePhase2Middleware` are not called from `Program.cs`, so middleware (rate limiting, API-key auth, request logging), caching, webhooks, and background workers are inert in the default host until wired up.
- `IFlagEvaluationLogService` keeps evaluation logs in memory only - they are lost on restart and unbounded growth is capped only by trimming logic, not persisted.
- Two test projects exist (`src/FeatureFlags.Tests` and `tests/dotnet-feature-flags.Tests`) with overlapping scope; they should eventually be merged.
- The custom `AuthenticationMiddleware` is API-key based, not integrated with ASP.NET Core authentication schemes, so `[Authorize]` attributes do not interact with it.
- SQL Server is the only persistence target configured; no provider abstraction beyond what EF Core gives for free.
