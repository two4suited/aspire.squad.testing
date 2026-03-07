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

