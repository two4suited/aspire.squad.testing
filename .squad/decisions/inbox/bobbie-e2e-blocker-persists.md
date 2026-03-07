# E2E Test Blocker Status: Schema Initialization Not Applied

**Date:** 2026-03-07  
**Reporter:** Bobbie (Tester)  
**Priority:** 🔴 Critical — Blocks All E2E Tests  

## Test Results

**Run Date:** 2026-03-07 @ 12:30 PM  
**Result:** 1/15 tests passed (14 failed)  
**Test Duration:** 6.8 minutes  

## Root Cause: Stale API Process

The schema initialization fix (commit `8efb06f`) was implemented and built successfully, but the currently running API processes were started **before** the code changes were deployed.

### Timeline Evidence

1. **Current API Process:** Started at 11:45 AM (PID 83001, 82703)
2. **Schema Fix Committed:** 12:33 PM (commit `8efb06f`)
3. **API DLL Last Built:** 12:33 PM (`DogTeams.Api.dll` timestamp)
4. **Tests Executed:** 12:30 PM with stale API

### Verification

- ✅ Code exists: `CosmosDbInitializer.cs` created
- ✅ Code registered: `Program.cs` lines 77-82 invoke initialization
- ✅ Code built: DLL timestamp matches commit time
- ❌ Code running: API process predates the fix

## Failure Pattern

All 14 failed tests exhibit identical behavior:

1. ✅ Frontend loads successfully (Vite on port 56174)
2. ✅ User fills registration form
3. ✅ Clicks "Create account" button
4. ❌ Button enters "Creating account…" state (disabled)
5. ❌ API hangs on `/api/auth/register` endpoint (no response after 30+ seconds)
6. ❌ Test times out after 30 seconds waiting for navigation to dashboard

**Direct API Test:** `curl` to registration endpoint hangs indefinitely — confirms API-level blocker, not test infrastructure issue.

## Tests Passing

**1 test passed:**  
- "should redirect to login when accessing protected routes without auth" (no API dependency)

## Required Action

**Owner:** Holden (Lead) or team member with Aspire access  
**Action:** Restart Aspire AppHost to load updated API code with schema initialization  

### Steps

1. Stop current Aspire AppHost process (PID 95179)
2. Restart: `dotnet run --project src/DogTeams.AppHost` from repo root
3. Wait for all services to initialize (Cosmos, Redis, API, frontend)
4. Verify API startup logs show schema initialization messages
5. Re-run E2E tests: `cd src/DogTeams.Web/ClientApp && VITE_BASE_URL=http://localhost:<vite-port> npm run test:e2e`

### Expected Outcome After Restart

- Cosmos DB schema initialization logs in API startup
- Database collections created: `Teams`, `Owners`, `Dogs`, `Breeds`, `Users`, identity containers
- Breed reference data seeded (174 breeds)
- Registration API responds with 201 Created
- All 15 E2E tests pass ✅

## Test Infrastructure Status

✅ **Playwright Configuration:** Working correctly (dynamic port discovery from Aspire)  
✅ **Frontend:** Vite dev server running and accessible  
✅ **Test Helpers:** Registration/login flows well-structured  
✅ **Aspire Orchestration:** Services starting correctly  

The only blocker is loading the schema initialization code into the running API.

## Learnings

**Critical Pattern for Aspire Development:**  
Code changes to API require **full Aspire restart** to take effect. Unlike traditional development where hot reload might apply, Aspire manages process lifecycle independently. After committing backend changes, always restart AppHost to ensure running processes reflect the latest code.

---

*This issue should be resolved within 5 minutes once Aspire is restarted.*
