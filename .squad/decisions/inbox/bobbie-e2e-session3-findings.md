# E2E Test Session 3: Infrastructure Ready, Database Schema Missing

**Status:** 🟡 BLOCKER (Fixable by backend)  
**Date:** 2026-03-07  
**Reporter:** Bobbie (Tester)

## Executive Summary

E2E tests now successfully connect to the Aspire-orchestrated frontend (fixed via dynamic port discovery in Playwright config). However, all 15 tests fail because Cosmos DB emulator lacks the required schema. The API returns 404 errors for the "Teams" collection.

**Action Required:** Amos (Backend) must ensure Cosmos DB schema is initialized during Aspire startup.

---

## Issue 1: Test Infrastructure — Dynamic Port Discovery ✅ FIXED

### Problem
- Playwright hardcoded `baseURL: http://localhost:5173`
- Aspire dynamically allocates ports (actual frontend: 53074)
- Tests failed with `net::ERR_CONNECTION_REFUSED`

### Solution Implemented
Updated `src/DogTeams.Web/ClientApp/playwright.config.ts`:
```typescript
const getBaseURL = (): string => {
  if (process.env.VITE_BASE_URL) return process.env.VITE_BASE_URL;
  if (process.env.services__web__http__0) return process.env.services__web__http__0;
  if (process.env.services__web__https__0) return process.env.services__web__https__0;
  return 'http://localhost:5173';
};
```

### Result
✅ Frontend now connects properly  
✅ Tests proceed to authentication flow

---

## Issue 2: Database Schema — Cosmos Collections Missing ❌ BLOCKER

### Problem
After successful registration and login, all tests fail with Cosmos errors:
```
404 CosmosException: Collection 'Teams' not found in database 'DogTeamsDb'
RequestUri: http://localhost:52994/dbs/DogTeamsDb/colls/Teams
```

**HTTP Response:** 500 Internal Server Error when accessing `/api/teams`

### Test Failures Summary
- **Total Tests:** 15
- **Connected:** 15 ✅
- **Failed:** 15 ❌
- **Failure Reason:** All blocked by Cosmos DB 404

**Tests by Category:**
| Category | Count | Blocker |
|----------|-------|---------|
| Authentication | 5 | Teams collection missing |
| Team Management | 3 | Teams collection missing |
| Owner Management | 2 | Teams collection missing (needed for team creation) |
| Dog Management | 2 | Teams collection missing (needed for team creation) |
| Error Handling | 2 | Teams collection missing |
| Complete Journey | 1 | Teams collection missing |

### Root Cause
The Cosmos DB emulator (`RunAsPreviewEmulator()` in AppHost) doesn't auto-initialize database schema. The "Teams" collection must be created explicitly.

**Likely Issue:** Entity Framework migrations not running on startup, or EF Core not configured to auto-create schema.

### Required Fix (Backend Responsibility)
In `src/DogTeams.Api/Program.cs` or database initialization:
1. Add Entity Framework migration logic to run on Aspire startup
2. Ensure `context.Database.EnsureCreatedAsync()` or equivalent is called
3. Verify all collections exist: Teams, Owners, Dogs, Users (if domain model requires)

---

## Test Execution Details

### Command
```bash
VITE_BASE_URL=http://localhost:53074 npm run test:e2e
```

### Sample Failure Output
```
Error: expect(locator).toContainText(expected) failed
Expected: "Welcome, Test User"
Received: "❌ API error 500: Microsoft.Azure.Cosmos.CosmosException: 
           Collection 'Teams' not found in database 'DogTeamsDb'"
```

### Timeline
1. ✅ Frontend app loads at http://localhost:53074
2. ✅ Authentication form displays correctly
3. ✅ Registration submission succeeds
4. ✅ Page redirects to dashboard
5. ❌ Dashboard tries to load `/api/teams`
6. ❌ API call fails (collection not found)
7. ❌ Test assertion fails (expected welcome message, got error)

---

## Recommendations

### Immediate Action (Amos)
- [ ] Check `DogTeamsDb` database initialization in AppHost/API startup
- [ ] Verify EF Core migrations are configured
- [ ] Ensure Cosmos collections are created before API accepts requests
- [ ] Option: Add startup health check that validates schema exists

### Follow-up (Bobbie)
- [ ] Re-run E2E tests after database schema is fixed
- [ ] Verify all 15 tests pass
- [ ] Consider adding pre-flight health checks to test suite

### Documentation (Team)
- [ ] Document E2E test prerequisites (Aspire running, Cosmos initialized)
- [ ] Add troubleshooting guide: "E2E tests connect but get 404"

---

## Files Modified

- `src/DogTeams.Web/ClientApp/playwright.config.ts` — Added dynamic port discovery

## Next Session

After Amos fixes Cosmos DB initialization:
1. Restart Aspire (`dotnet run --project src/DogTeams.AppHost`)
2. Run: `cd src/DogTeams.Web/ClientApp && npm run test:e2e`
3. Expected: All 15 tests pass (or reveal API-level issues, not infrastructure)
