# Project Context

- **Owner:** Brian Sheridan
- **Project:** aspire.squad.testing — Web app with React frontend, .NET Core backend, Cosmos DB, Redis, and authentication
- **Stack:** React, TypeScript, .NET Core Web API, Cosmos DB, Redis, .NET Aspire (aspire.dev)
- **Created:** 2026-03-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-07: Linux CosmosDB emulator via RunAsPreviewEmulator()

**Context:** Switched AppHost from `.RunAsEmulator()` to the Linux-based emulator.

**Findings from inspecting `Aspire.Hosting.Azure.CosmosDB` 13.1.2:**
- `RunAsEmulator()` uses `mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest`
- `RunAsPreviewEmulator()` uses `mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`
- Both methods use the Linux image in 13.1.2 (the Windows emulator was the default in earlier Aspire versions)
- `RunAsPreviewEmulator()` is explicitly documented as "the Azure Cosmos DB Linux-based emulator (preview)" and additionally supports `.WithDataExplorer()`
- `RunAsPreviewEmulator()` is in the **stable** 13.1.2 package but is marked as evaluation (`ASPIRECOSMOSDB001`); suppress with `#pragma warning disable ASPIRECOSMOSDB001`

**Pattern used:**
```csharp
#pragma warning disable ASPIRECOSMOSDB001
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator();
#pragma warning restore ASPIRECOSMOSDB001
```

**Important:** The `#pragma warning disable` must be placed BEFORE the start of the statement (not mid-chain). The Aspire diagnostic is attributed to the beginning of the expression, not the call site of the flagged method.

### 2026-03-07: Initial solution scaffold

**Packages used:**
- `Aspire.AppHost.Sdk`: 13.1.0 (via template; note: 13.1.2 is latest)
- `Aspire.Hosting.Azure.CosmosDB`: 13.1.2
- `Aspire.Hosting.Redis`: 13.1.2
- `Aspire.Microsoft.Azure.Cosmos`: 13.1.2
- `Aspire.StackExchange.Redis`: 13.1.2
- `Microsoft.Azure.Cosmos`: 3.58.0-preview.1 (latest preview as of scaffold)
- `Aspire.Hosting.Testing`: 13.1.2

**Issues encountered:**
- `dotnet new aspire` (Aspire Empty App) creates only AppHost + ServiceDefaults — Api, Web, Tests must be created separately.
- Template created a stub `AppHost.cs` with top-level statements; when we created `Program.cs`, the compiler flagged duplicate top-level statements. Fixed by deleting `AppHost.cs`.
- No `react` dotnet template available — Web project created with `dotnet new web` (ASP.NET Core Empty). React SPA setup must be done separately (Vite/CRA scaffold or bsinc-web-nextjs custom template).
- The `aspire` template pre-populated `DogTeams.Api/` with domain Models, DTOs, and Configuration (Dog, Owner, Team models) from a previous squad agent run — those files were preserved.
- AppHost SDK version in template was 13.1.0; package references installed at 13.1.2. Consider updating the SDK reference to match.
- `dotnet add ... reference` on Api reported ServiceDefaults already referenced — the template had already wired it.

**Solution structure created:**
```
src/
  DogTeams.sln
  DogTeams.AppHost/       ← Aspire orchestration entry point
  DogTeams.ServiceDefaults/  ← Shared telemetry, health, service discovery
  DogTeams.Api/           ← ASP.NET Core Web API (controllers)
  DogTeams.Web/           ← ASP.NET Core Empty (React SPA to be added)
  DogTeams.Tests/         ← xUnit with Aspire.Hosting.Testing
```
