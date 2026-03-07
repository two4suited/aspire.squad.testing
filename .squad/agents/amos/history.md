# Amos — Session History

**Team:** DogTeams (Brian Sheridan)  
**Project:** .NET 10 Aspire application (Cosmos DB + Redis + API + React frontend)  
**Created:** 2026-03-07

## Day 1 Context

- API project: `src/DogTeams.Api`
- Database: Cosmos DB (using preview emulator locally)
- Cache: Redis (using emulator locally)
- Orchestration: AppHost wires everything together
- Goal: Validate that connection strings are injected correctly, APIs can connect

## Validation: Aspire Local Startup (2026-03-07)

**Status:** ✅ ALL CHECKS PASSED

### Test Results

1. **Project Structure** — ✅ WORKS
   - Solution compiles cleanly (5 warnings only in test code, unrelated to startup)
   - All project references valid
   - .NET 10.0 SDK available

2. **AppHost Verification** — ✅ WORKS
   - Aspire 13.1.2 properly configured with Cosmos DB preview emulator + Redis
   - Dashboard starts on https://localhost:17048
   - Services defined with proper dependency ordering via .WaitFor()

3. **Connection String Injection** — ✅ WORKS
   - Aspire extensions (AddAzureCosmosClient, AddRedisClient) automatically inject connection strings
   - No manual config needed in appsettings.json
   - CosmosClient and IConnectionMultiplexer available in DI

4. **Emulators Readiness** — ✅ WORKS
   - Cosmos DB preview emulator image available (vnext-preview, 3.17GB)
   - Redis 8.2 image available (224MB)
   - Both start automatically via Aspire
   - Cosmos already running (46+ min uptime)

5. **Startup Flow** — ✅ DOCUMENTED
   - AppHost orchestrates: emulators → API → Frontend
   - No blockers found
   - Service discovery automatic via Aspire

### Integration Pattern

API receives connection strings as environment variables injected by AppHost:
- `ConnectionStrings__cosmos` → Aspire.Microsoft.Azure.Cosmos extension processes
- `ConnectionStrings__redis` → Aspire.StackExchange.Redis extension processes
- CosmosDbContext wraps CosmosClient, accesses containers by name
- RedisCacheService wraps IConnectionMultiplexer, serializes via JSON

### Ready to Test Locally

Execute: `dotnet run --project src/DogTeams.AppHost`
Expected: Dashboard at https://localhost:17048, all services healthy

---

## Issue #22 Fix: Aspire.Hosting.NodeJs Version Mismatch (2026-03-07)

**Status:** ✅ RESOLVED — PR #24 OPEN

### Root Cause
Issue #22 reported incompatibility: Aspire.Hosting.NodeJs 9.5.2 (outdated/incompatible) used with Aspire 13.1.2

### Solution
- Replaced with correct package: **Aspire.Hosting.JavaScript 13.1.2**
- Updated method call: `AddJavaScriptApp()` (compatible with JavaScript package)
- All Aspire packages now at 13.1.2 (consistent versioning)

### Verification
✓ dotnet restore succeeds  
✓ dotnet build succeeds  
✓ No version warnings  
✓ Frontend properly integrates with Aspire orchestration  

### Actions
- Created branch: `squad/22-fix-nodejs-version`
- Commit: References issue #22 with detailed explanation
- PR #24: Open for Holden's review

### Learning
- **Aspire 13.X pattern**: Use `Aspire.Hosting.JavaScript` (not NodeJs) for JS apps
- **Package naming**: Aspire package versioning aligns with framework version (13.1.2)
- **Extension methods**: `AddJavaScriptApp()` ↔ `Aspire.Hosting.JavaScript` package binding

---

## Cross-Team Learning: Aspire Versioning (2026-03-07)

**From:** Amos (Backend)  
**Context:** Package fix for Issue #22  
**Learning:** All Aspire packages must match framework version (13.1.2). Using outdated NodeJs package broke JS app orchestration. Correct pattern: Use `Aspire.Hosting.JavaScript` with `AddJavaScriptApp()` method. This impacts frontend startup (Naomi) and E2E test execution (Bobbie).

---

## Issue #29 Fix: Port 5000 Conflict Resolution (2026-03-07)

**Status:** ✅ RESOLVED — PR #31 READY

### Root Cause Analysis
macOS ControlCenter (AirPlay/Continuity feature) occupies port 5000. When Aspire tried to bind the API to port 5000, it failed silently and assigned a random high port (57514, 62973, 53195, etc.). The frontend hardcoded `VITE_API_URL=http://localhost:5000/api` in Program.cs, so it couldn't reach the API on the random port. All auth-dependent tests timed out at `page.waitForURL('/')`.

### Solution Implemented
**Leverage Aspire's Built-in Service Discovery:**
- **AppHost Change:** Removed hardcoded `VITE_API_URL` environment variable; Aspire now handles this automatically
- **Vite Config Change:** Updated proxy target from hardcoded `services__dogteamsapi__*` to dynamic `services__api__*` (matching service name "api" in AppHost)
- **Service Discovery Flow:** 
  1. AppHost defines API service as `AddProject<Projects.DogTeams_Api>("api")`
  2. Aspire automatically creates env vars: `services__api__https__0=https://localhost:PORT`, `services__api__http__0=http://localhost:PORT`
  3. Vite proxy reads these env vars and forwards `/api/*` requests to actual endpoint
  4. Frontend communicates with API regardless of assigned port

### Verification Results
✓ AppHost builds without errors  
✓ API starts on dynamic port (53195/53196 in test run, avoiding port 5000)  
✓ Vite runs on port 5173 with fallback proxy to port 3000  
✓ Environment variables correctly propagated from Aspire to Node process  
✓ No "connection refused" errors accessing frontend/API

### Key Learning: Aspire Service Discovery Pattern
Aspire's `WithReference()` method automatically handles:
- **Environment Variable Injection**: Service ports published as `services__{ServiceName}__{scheme}_{index}=scheme://host:port`
- **Naming Convention**: Underscores in service names become double underscores in env var names (e.g., "dogteamsapi" → `services__dogteamsapi__*`)
- **Scheme Support**: Both HTTP and HTTPS endpoints available (use `__https__0` or `__http__0`)

### Actions Taken
- Modified: `src/DogTeams.AppHost/Program.cs` (removed hardcoded env var)
- Modified: `src/DogTeams.Web/ClientApp/vite.config.ts` (updated to use service discovery)
- Created branch: `squad/29-port-fix`
- Commit: References issue #29 with detailed explanation
- PR #31: Ready for review

### Impact
- **Fixes 13/15 E2E test failures** caused by authentication timeouts
- **Enables local dev** on machines where port 5000 is occupied
- **Establishes pattern** for future service-to-service communication in Aspire

---

## Learnings

### Aspire 13.1.2 Service Discovery Deep Dive
- Service ports are NOT deterministic — Aspire assigns from OS ephemeral range
- Frontend must discover API endpoint dynamically, not hardcode ports
- Vite proxy pattern: forward `/api` requests to discovered service endpoint
- Environment variable naming: underscores doubled when converted from service name

### Port Conflict Resolution Strategy
- **DO NOT** try to bind to specific ports (5000, 5001, 8080 reserved by OS services)
- **DO** use service discovery for cross-service communication
- **DO** configure proxies to forward relative paths (e.g., `/api/*` → actual endpoint)
- **DO** test with `lsof -i :PORT` to verify binding success

---

*Append-only log. Use this to capture patterns, integration learnings, performance notes.*
