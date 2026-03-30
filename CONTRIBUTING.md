# Contributing to DistLock

Thank you for your interest in contributing! The typical journey is:
**fork → branch → change → test → open a PR**.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (exact version pinned in `global.json`)
- Any editor with EditorConfig support (`.editorconfig` is included)

---

## Local setup

```sh
git clone https://github.com/<you>/dist-lock.git
cd dist-lock
dotnet restore
dotnet build
dotnet test
```

No external services or environment variables are required to build and test.

---

## Project layout

```
src/
  DistLock/         all source (interfaces, exception, extensions)  namespace KatzuoOgust.DistLock
tests/
  DistLock.Tests/   xUnit tests                                     namespace KatzuoOgust.DistLock
```

---

## Conventions

- **Test naming:** `Subject_Result[_WhenCondition]` — e.g. `AcquireAsync_ThrowsException_WhenTimeoutExpires`.
- **Namespace prefix:** `KatzuoOgust.DistLock` (set via `<RootNamespace>` in each csproj; do not add `Core` or `Abstractions` as a suffix).
- **Test doubles:** use Moq (`new Mock<T>()`); do not add other mocking libraries.
- **Formatting:** enforced by `.editorconfig`. Run `dotnet build` — Roslyn analyser warnings are emitted for style violations.

---

## Pull request process

1. All tests must pass (`dotnet test`).
2. Keep each PR focused — one feature or fix per PR.
3. Update or add XML doc comments for any public API you add or change.
4. Do **not** modify auto-generated files or `global.json` unless updating the SDK version is the explicit goal.

---

## What not to contribute

- Backend-specific lock implementations (Redis, SQL, etc.) — those belong in separate packages.
- Changes to `tests/Tests/bin/` or `obj/` — these are build artefacts and are gitignored.
