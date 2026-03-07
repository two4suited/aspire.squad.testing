# Project Context

- **Owner:** Brian Sheridan
- **Project:** aspire.squad.testing — Web app with React frontend, .NET Core backend, Cosmos DB, Redis, and authentication
- **Stack:** React, TypeScript, .NET Core Web API, Cosmos DB, Redis, .NET Aspire (aspire.dev)
- **Created:** 2026-03-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-07: Created domain models and API structure for Dog Teams

Scaffolded `src/DogTeams.Api/` with full domain layer:

- **Models** (`Models/`): `Team`, `Owner`, `Dog` — plain C# classes with Guid-based IDs and `DateTime CreatedAt`.
- **DTOs** (`DTOs/`): Request/Response records for all three entities (`TeamDTOs.cs`, `OwnerDTOs.cs`, `DogDTOs.cs`).
- **Controllers** (`Controllers/`): `TeamsController` (CRUD at `/teams`), `OwnersController` (scoped to `/teams/{teamId}/owners`), `DogsController` (scoped to `/owners/{ownerId}/dogs`) — all stubbed with `// TODO: implement with CosmosDB`.
- **Configuration** (`Configuration/CosmosDbOptions.cs`): Typed options for Cosmos containers.
- **Program.cs**: Updated to add `builder.AddServiceDefaults()`, `Configure<CosmosDbOptions>`, and `app.MapDefaultEndpoints()`.
- **DogTeams.Api.csproj**: Added `ProjectReference` to `DogTeams.ServiceDefaults`.

## Learnings
