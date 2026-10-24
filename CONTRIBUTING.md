# Contributing to dotnet-feature-flags

Thanks for helping out. This guide covers building, testing, code style, and the PR process.

## Prerequisites

- .NET 10.0 SDK (pinned in `global.json`, `rollForward: latestMinor`)
- Git
- Docker (optional, only for `make docker-up`)

## Clone and build

```bash
git clone https://github.com/sarmkadan/dotnet-feature-flags.git
cd dotnet-feature-flags
dotnet restore
dotnet build                 # Debug
dotnet build -c Release      # Release, same as CI
```

`make build` and `make build-rel` wrap these. The solution is `dotnet-feature-flags.sln`.

## Run tests

```bash
dotnet test                                                  # all test projects
dotnet test src/FeatureFlags.Tests                           # main test project
dotnet test tests/dotnet-feature-flags.Tests                 # older test project
make test-coverage                                           # coverage via coverlet
```

Tests use xUnit, FluentAssertions, Moq, and MockQueryable.Moq. Name test classes `<Type>Tests.cs` and mark methods with `[Fact]` or `[Theory]`.

## Coding conventions

- C#, `LangVersion latest`, `Nullable` and `ImplicitUsings` enabled (see `Directory.Build.props`).
- 4-space indent, LF line endings, final newline. Braces on their own line (Allman) and always used.
- Follow `.editorconfig`. Run `make format` (`dotnet format`) before committing and `make lint` to check analyzers.
- Namespaces follow folders (`FeatureFlags.<Folder>`). Interfaces are `I<Name>` next to their implementation.
- Split a type into partial/companion files by concern (`<Type>.cs`, `<Type>Extensions.cs`, `<Type>Validation.cs`) instead of growing one file.
- Async methods end in `Async`.
- Every flag mutation must write an audit log entry.
- Add unit tests for new behavior.
- Warnings do not fail the build (`TreatWarningsAsErrors` is `false`), so treat new warnings as bugs and fix them.

## Branches and pull requests

1. Fork the repository, or branch from `main` if you have write access.
2. Name the branch after the change, e.g. `feature/rule-targeting-fix`.
3. Keep each PR small and focused on one change. Split unrelated work into separate PRs.
4. Describe what changed and why, and how you tested it. Link related issues.
5. CI must pass. It builds in Release and runs tests on .NET 8.0.x and 10.0.x.
6. Get one approving review before merging.

Do not commit stray files (scratch files, editor history, generated artifacts) to the repository root.

## Reporting issues

Open an issue on GitHub with:
- A clear, descriptive title.
- Steps to reproduce (for bugs).
- Relevant system information (OS, .NET SDK version).

## License

Contributions are licensed under the MIT License. See `LICENSE`.
