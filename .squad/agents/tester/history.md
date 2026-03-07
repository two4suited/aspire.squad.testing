# Project Context

- **Owner:** Brian Sheridan
- **Project:** aspire.squad.testing — Web app with React frontend, .NET Core backend, Cosmos DB, Redis, and authentication
- **Stack:** React, TypeScript, .NET Core Web API, Cosmos DB, Redis, .NET Aspire (aspire.dev)
- **Created:** 2026-03-07

## Learnings

### 2026-03-07: Test project setup for DogTeams

- Test project lives at `src/DogTeams.Tests/`, targets `net10.0`.
- References: `DogTeams.AppHost` (for Aspire integration) and `DogTeams.Api` (for domain model access).
- Packages: `Aspire.Hosting.Testing 13.1.2`, `Microsoft.AspNetCore.Mvc.Testing 10.0.3`, `FluentAssertions 8.8.0`.
- `DistributedApplication` is in `Aspire.Hosting` namespace — must import both `Aspire.Hosting` and `Aspire.Hosting.Testing` in `AppHostFixture.cs`.
- Integration tests skip with `[Fact(Skip = "...")]` until API controllers are fully implemented (controllers currently return TODO stubs).
- Domain model unit tests use real types from `DogTeams.Api.Models` — models use `string` (not `Guid`) IDs for Cosmos DB compatibility.
- Auth integration tests are written but skipped — no auth controller exists in the Api yet.

### 2026-03-07: FB-1 new domain model tests

- When Backend types don't exist yet, use `#if false` / `#endif` to wrap model-dependent test classes. Code is syntactically valid but excluded from compilation. Removing the guards activates the tests immediately once Backend lands.
- Pure math tests (e.g., jump height calculation) need no `#if false` — write them with an inline helper that mirrors the spec formula. These run and pass before Backend implements the production method. Add a TODO comment to replace the helper with the real call later.
- Integration test files for new endpoints (ClubsIntegrationTests, BreedsIntegrationTests) can be created with all tests skipped via `[Fact(Skip = "...")]` — this documents intent and creates the fixture wiring without blocking CI.
- The `#if false` approach is preferable to TODO comments when the unresolved types are numerous, because it lets the compiler validate the test logic once types exist.
- File-scoped `namespace` declarations (`namespace Foo.Bar;`) apply to the entire file — never repeat them inside `#if false` blocks in the same file.

