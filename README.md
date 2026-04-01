# DistLock

<div align="center">
  <img src="./logo.png" alt="DistLock logo" width="200" height="200">
</div>

[![CI](https://github.com/KatzuoOgust/dist-lock/actions/workflows/ci.yml/badge.svg)](https://github.com/KatzuoOgust/dist-lock/actions/workflows/ci.yml)
[![CodeQL](https://github.com/KatzuoOgust/dist-lock/actions/workflows/codeql.yml/badge.svg)](https://github.com/KatzuoOgust/dist-lock/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)

> Lightweight distributed-lock abstractions and utilities for .NET 10.

`KatzuoOgust.DistLock` provides a clean, provider-agnostic API for acquiring and releasing distributed locks — letting you plug in any backend (Redis, SQL, etcd, …) while keeping application code free from infrastructure details.

---

## Features

- **Provider-agnostic design** — implement `IDistributedLockProvider` once, swap backends freely
- **Flexible API** — execute work safely, acquire handles directly, or try without throwing
- **Built on abstractions** — clean interfaces for testability and decoupling
- **Production-ready** — xUnit test suite, EditorConfig linting, CI/CD pipeline

---

## Package

**`KatzuoOgust.DistLock`** contains:

- Core interfaces — `IDistributedLock`, `IDistributedLockHandle`, `IDistributedLockProvider`
- `DistributedLockException`
- Extension methods — `AcquireAsync`, `ExecuteWithLockAsync`, `TryExecuteWithLockAsync`

---

## Quick start

```csharp
using KatzuoOgust.DistLock;

// Obtain a lock for a resource (provider supplied by your DI container)
IDistributedLock @lock = provider.CreateLock("orders:42");

// Execute work exclusively — throws DistributedLockException if the lock
// can't be acquired within the wait window
await @lock.ExecuteWithLockAsync(
    async ct => await ProcessOrderAsync(42, ct),
    expiry: TimeSpan.FromSeconds(30),
    wait:   TimeSpan.FromSeconds(5));

// Fire-and-forget style — returns false instead of throwing
bool ran = await @lock.TryExecuteWithLockAsync(
    async ct => await ProcessOrderAsync(42, ct),
    expiry: TimeSpan.FromSeconds(30));

// Lower-level: acquire the handle directly
await using IDistributedLockHandle handle = await @lock.AcquireAsync(
    expiry: TimeSpan.FromSeconds(30),
    wait:   TimeSpan.FromSeconds(5));
```

Implement `IDistributedLockProvider` (and the related interfaces) for your chosen backend and register it with your DI container.

---

## Requirements

- .NET 10 SDK (`global.json` pins the exact version)

---

## Build & test

```sh
dotnet build
dotnet test
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for the full contributor workflow.
