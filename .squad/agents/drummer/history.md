# Drummer — Learning History

## Project Context
**Project:** DogTeams — .NET 10 Aspire application for managing flyball canine sports teams  
**Stack:** .NET 10, Aspire, React 18, TypeScript, Cosmos DB (emulator), Redis (emulator)  
**Repo:** two4suited/aspire.squad.testing  
**User:** Brian Sheridan  

## Initial Assignment
**Date:** 2026-03-07  
**Role:** DevOps / Aspire Infrastructure Specialist  
**Focus:** Ensure all Aspire resources are green before E2E tests run

## Session 2026-03-07 (22:00-22:15 UTC) — Infrastructure Validation & Breed Seeding Fix

**Status:** ✅ COMPLETE - All resources GREEN and operational

### Problem Discovered
API crashed on startup with Cosmos DB "Document does not contain an id field" error during breed seeding. The 30 flyball breed reference data couldn't be persisted to Cosmos.

### Root Cause Analysis
- **Symptom:** CosmosException (400 Bad Request) when seeding breeds
- **Investigation:** Checked API logs in DCP container output
- **Root Cause:** Breed model used `System.Text.Json.Serialization.JsonPropertyName` attributes, but Cosmos SDK 3.57.1 uses Newtonsoft.Json (JSON.NET)
- **Impact:** Cosmos received Breed objects without an `id` field, violating Cosmos DB requirement for partition key serialization

### Solution Implemented
**File:** `src/DogTeams.Api/Models/Breed.cs`
- Changed import from `System.Text.Json.Serialization` → `Newtonsoft.Json`
- Updated all property decorators: `[JsonPropertyName(...)]` → `[JsonProperty(...)]`
- Ensures Cosmos SDK's JSON serializer correctly maps C# properties to Cosmos document fields

**Result:** API now boots successfully, seeds 30 breeds into `breeds` container in <100ms

### Infrastructure Status Validated

| Resource | Port | Status | Notes |
|----------|------|--------|-------|
| API | 5000 | ✅ GREEN | Listening via DCP proxy, seeds completed |
| Frontend (Vite) | 5173 | ✅ GREEN | Dev server ready on dynamic port, proxied |
| Cosmos DB | 56175 | ✅ GREEN | Emulator running, 6 containers initialized |
| Redis | 56206 | ✅ GREEN | Cache emulator operational |
| Dashboard | 17048 | ✅ GREEN | Monitoring available |

### Learnings: Aspire + Cosmos DB Best Practices

1. **JSON Serialization Compatibility:**
   - Cosmos SDK 3.57.1+ uses Newtonsoft.Json internally
   - Models must use `Newtonsoft.Json.JsonProperty` attributes, NOT `System.Text.Json.Serialization.JsonPropertyName`
   - This applies to all entities stored in Cosmos (not just Breed)

2. **Port Mapping Transparency:**
   - Aspire uses DCP (Distributed Control Plane) for service orchestration
   - Services bind to dynamic internal ports (e.g., API on 5001, Vite on 56214)
   - DCP proxy layer forwards requests from configured ports (5000, 5173) to internal services
   - Dashboard reports actual internal ports; users should target configured external ports

3. **Breed Seeding Pattern (Working):**
   - Idempotent check: Query top 1 breed to detect if already seeded
   - Batch insert: Loop through seed list with conflict handling
   - Performance: 30 breeds seed in <100ms on cold start
   - Reliability: Continues if individual breed already exists (ConflictException)

4. **Emulator Startup Timing:**
   - Cosmos emulator needs ~30 seconds from docker start to ready
   - Aspire waits for emulator before starting API (WaitFor dependency)
   - Redis emulator starts faster (<10 seconds)
   - Always allow 45-60 seconds for full orchestration startup

### Commands Used
```bash
aspire run --project src/DogTeams.AppHost
# Verify logs: /Users/brian/.aspire/cli/logs/apphost-*.log
# Check containers: docker ps
# Inspect DCP resources: /var/folders/.../T/aspire-*/
```

### Files Modified
- `src/DogTeams.Api/Models/Breed.cs` — Fixed JSON attribute mismatch

### Next Steps
- ✅ Infrastructure ready for Bobbie (Tester) to run E2E test suite
- ✅ No infrastructure blockers identified
- Recommend: Add JSON serialization check to CI/CD for model consistency

## Session 2026-03-07 (Late) — Issue #39: Enable Cosmos DB Data Explorer

### Mission: Add Data Explorer UI to local Cosmos DB development

**Status:** ✅ COMPLETE - Build verified, change committed

### Problem
Cosmos DB emulator needed Data Explorer UI configuration to improve developer experience during local development and testing.

### Solution Implemented
**File:** `src/DogTeams.AppHost/Program.cs`

Modified the Cosmos DB resource configuration to use lambda-based emulator setup:

```csharp
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithDataExplorer();
    });
```

**Key Learning:** The `RunAsPreviewEmulator()` method accepts an optional lambda parameter that receives the emulator resource builder instance. The `WithDataExplorer()` method must be called **within this lambda**, not in a fluent chain on the return value. This pattern prevents type mismatch errors with the base `IResourceBuilder<AzureCosmosDBResource>` type.

### Build Verification
✅ Full solution builds successfully with no errors or warnings
✅ All dependent projects compile (ServiceDefaults, Api, AppHost)

### Commit
- Commit: `d2e61b1` — "feat: Enable Cosmos DB Data Explorer in Aspire emulator"
- Reference: Closes #39

### Impact
- Cosmos DB Data Explorer is now available in Aspire dashboard during development
- Developers can inspect collections, documents, and seed data visually
- Simplifies local debugging of Cosmos DB operations

## Learnings
**Drummer now understands:**
- Aspire orchestration architecture (DCP + Docker)
- Cosmos DB emulator integration pattern
- JSON serialization requirements for .NET entity models
- Infrastructure validation approach for microservice stacks
- Cosmos DB Data Explorer enablement with lambda-based configuration pattern
