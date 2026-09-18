# CLAUDE.md

ASP.NET Core (net10.0, C#) feature flag engine exposed as a REST API: percentage rollouts, rule-based targeting, A/B variants, EF Core + SQL Server persistence with audit trail.

## Build

```bash
dotnet restore
dotnet build                     # Debug
dotnet build -c Release          # Release (CI uses this)
make build / make build-rel      # Makefile wrappers
dotnet run --project src/FeatureFlags/FeatureFlags.csproj   # or: make run / make watch
```

SDK pinned in `global.json` (10.0.100, rollForward latestMinor). CI matrix also builds on 8.0.x.

## Test

```bash
dotnet test                                              # all test projects
dotnet test --no-build --logger "console;verbosity=normal"   # make test
dotnet test src/FeatureFlags.Tests                       # main test project
dotnet test tests/dotnet-feature-flags.Tests             # older test project
make test-coverage                                       # coverlet /p:CollectCoverage=true
```

Stack: xUnit 2.9, FluentAssertions 7, Moq, MockQueryable.Moq. Tests are `[Fact]`/`[Theory]` methods in classes named `<Type>Tests.cs`.

## Lint / format

```bash
dotnet format                                        # make format
dotnet build /p:EnforceCodeStyleInBuild=true         # make lint
```

Style from `.editorconfig`: 4-space indent, LF, final newline, Allman braces (`csharp_new_line_before_open_brace = all`), braces preferred. `Directory.Build.props`: `Nullable` and `ImplicitUsings` enabled, `LangVersion latest`, warnings not treated as errors.

## Layout and entry points

- `dotnet-feature-flags.sln` - 3 projects: `src/FeatureFlags`, `src/FeatureFlags.Tests`, `tests/dotnet-feature-flags.Tests`.
- `src/FeatureFlags/Program.cs` - minimal hosting; registers controllers, Swagger (dev only), `FeatureFlagDbContext` (SQL Server via `ConnectionStrings:DefaultConnection`, fails fast if missing), calls `AddFeatureFlagServices`, runs `Database.MigrateAsync()` at startup.
- `src/FeatureFlags/Configuration/` - `DependencyInjectionExtensions.AddFeatureFlagServices` (core: repositories + services), `Phase2DependencyInjectionExtensions.AddPhase2Services/UsePhase2Middleware` (caching, webhooks, event bus, rate limiting, auth, hosted workers; opt-in, not wired in Program.cs).
- `src/FeatureFlags/Controllers/` - FeatureFlag, Admin, Audit, Health, Webhook.
- `src/FeatureFlags/Services/` - business logic behind `I*Service` interfaces (`FeatureFlagService`, `RuleEvaluationService`, `PercentageRolloutService`, `AuditLogService`, `FlagEvaluationLogService` (singleton, buffers logs), `GradualRolloutSchedulerService`).
- `src/FeatureFlags/Repository/` - EF Core data access behind `IRepository`, `IFeatureFlagRepository`, `IAuditLogRepository`.
- `src/FeatureFlags/Models/`, `Enums/`, `Data/` (DbContext, seeder), `Middleware/`, `Caching/`, `Events/`, `Integration/`, `BackgroundJobs/`, `CLI/`, `Formatters/`, `Utilities/`.
- `benchmarks/` - BenchmarkDotNet project. `examples/` - standalone usage samples. `docs/` - per-type reference docs; `docs/ARCHITECTURE.md` is the maintained architecture doc (root `ARCHITECTURE.md` is a pointer).
- Docker: `Dockerfile`, `docker-compose.yml` (API :5000, SQL Server :1433); `make docker-up`.
- EF migrations: `make migrate`, `make migrate-add NAME=X` (`dotnet ef ... --project src/FeatureFlags`).
- CI: `.github/workflows/ci.yml` (restore, build Release, test), plus codeql, docker, nuget-publish, release.

## Conventions

- Namespaces follow folders: `FeatureFlags.<Folder>`. Interfaces are `I<Name>` next to their implementation.
- Each type is split into partials/companion files by concern: `<Type>.cs`, `<Type>Extensions.cs`, `<Type>Validation.cs`, `<Type>JsonExtensions.cs`, `<Type>HelperExtensions.cs`. Follow this pattern when adding behavior instead of growing the main file.
- Files start with `#nullable enable` and the author header comment block (see `Program.cs`).
- Async methods end in `Async`; services are scoped unless they hold cross-request state.
- Logging via `Microsoft.Extensions.Logging` (`ILogger<T>`); Serilog.AspNetCore is referenced but Program.cs uses console/debug providers.
- Mutations to flags must write an audit log entry.
- Do not commit stray files at repo root (there are leftover artifacts like `);`, `}`, `path/`, `commit.msg`, `.aider.*`; do not extend that set).
