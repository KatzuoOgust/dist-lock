# AGENTS.md — AI agent instructions for DistLock

Behavioral rules for AI coding agents working in this repository.  
See also: [CONTRIBUTING.md](CONTRIBUTING.md) for the human contributor guide.

---

## Project identity

- **.NET 10** class library solution (`DistLock.slnx`)
- **Package manager / build:** `dotnet` CLI only — never use `msbuild` directly
- **Test framework:** xUnit + Moq
- **Namespace prefix:** `KatzuoOgust.DistLock` (all projects)
- **Purpose:** Provider-agnostic distributed lock abstractions for .NET

## Project layout

```
src/DistLock/         → IDistributedLock, IDistributedLockHandle, IDistributedLockProvider,
                        DistributedLockException, DistributedLockExtensions
tests/DistLock.Tests/ → xUnit tests
```

## Commands

```sh
dotnet build              # build all projects
dotnet test               # run all tests (always run before finishing)
dotnet test --no-build    # run tests without rebuilding
dotnet build --no-restore # skip restore step if dependencies unchanged
```

**Before finishing any task:** always run `dotnet test` to verify no regressions.

## Conventions

- **RootNamespace:** each csproj sets `<RootNamespace>` explicitly — never add `.Core`, `.Abstractions`, or `.Tests` as a namespace suffix in source files.
- **Test naming:** `Subject_Result[_WhenCondition]` — e.g. `TryAcquireAsync_ReturnsNull_WhenResourceIsLocked`.
- **Test doubles:** use Moq (`new Mock<T>()`); do not add NSubstitute or other mocking libraries.
- **Private fields:** `_camelCase` (enforced by `.editorconfig`).
- **Interfaces:** must start with `I` (enforced by `.editorconfig`).
- **`using` directives:** outside namespace declarations.
- **No `this.` qualification** on members.
- **XML documentation:** required for all public types and members.

## Architecture

The core library defines **interfaces only** — no implementations. Backend-specific packages (Redis, SQL, etc.) implement `IDistributedLockProvider` and register themselves in DI.

## Hard constraints

- **`AcquireAsync`** is an extension method on `IDistributedLock` (in `DistributedLockExtensions`), not a member of the interface itself.
- Do **not** add backend implementations (Redis, SQL, …) to `src/` — those belong in separate packages.
- Do **not** change `global.json` unless the task is explicitly about updating the SDK version.
- Do **not** modify `bin/` or `obj/` directories.
- Always run `dotnet test` before marking any task complete.
- Do **not** commit secrets, credentials, or connection strings.
