# Team Decisions

## 2026-03-07

### Squad Init

- **Status:** Complete
- **Roster:** Holden (Lead), Amos (Backend), Naomi (Frontend), Bobbie (Tester), Scribe (silent), Ralph (monitor)
- **Universe:** The Expanse (character names drawn from this universe)
- **Next:** Local setup validation and deployment assessment

### Local Setup Validation

- **Status:** Complete
- **Scope:** Aspire orchestration, API integration, frontend integration, build, deployment
- **Findings:** All working correctly. Setup is production-ready architecturally.
- **Deployment:** Requires managed Azure services (Cosmos DB, Redis Cache)
- **Owner:** Holden (Lead)

### Package Fix: Aspire.Hosting.NodeJs Version Mismatch (Issue #22)

- **Status:** Resolved, PR #24 opened
- **Issue:** Aspire.Hosting.NodeJs 9.5.2 incompatible with Aspire 13.1.2
- **Fix:** Replace with Aspire.Hosting.JavaScript 13.1.2, update method call to AddJavaScriptApp()
- **Learning:** Aspire 13.X uses JavaScript package for JS app orchestration
- **Owner:** Amos (Backend)

### React Dev Server Startup (Issue #23)

- **Status:** Resolved, PR #25 opened (depends on #22)
- **Issue:** AddJavaScriptApp not starting dev server (root cause: wrong package in #22)
- **Fix:** After #22 merged, Vite launches correctly on port 5173
- **Verification:** Environment variable VITE_API_URL injected by Aspire
- **Owner:** Naomi (Frontend)

### E2E Test Failure Analysis (Issue #20)

- **Status:** Blocker identified, decision recorded
- **Root Cause:** API not accessible during test execution (depends on #22 and #23)
- **Blocker Chain:** #22 (packages) → #23 (orchestration) → #20 (E2E tests)
- **Test Infrastructure:** 15 tests, Playwright 1.58.2, all depend on auth API
- **Next:** Re-run E2E tests after #22 and #23 merged
- **Owner:** Bobbie (Tester)

---

*Append-only log. Do NOT edit existing entries.*

## Session 2026-03-07 (Evening) — Naomi Playwright Fix & Holden Audit Complete

### Naomi: Playwright API Fix (Issue #30)

- **Status:** ✅ Complete
- **Fix:** Replaced all 20 instances of `expect(page).toContainText()` with `expect(page.locator('body')).toContainText()`
- **Root Cause:** Playwright matchers require Locator objects, not page objects
- **Key Learning:** E2E test file was written without complete Playwright API understanding
- **Recommendation:** Add ESLint rule or pre-commit hook to catch this pattern

### Holden: Aspire Setup & Frontend Configuration Audit

- **Status:** ✅ Complete & Ready for Development
- **Verdict:** All major components correctly configured for local development
- **Findings:**
  - Aspire orchestration: ✅ Dynamic port allocation, service discovery working
  - API integration: ✅ Service references properly wired, no hardcoded ports
  - Frontend registration: ✅ Using AddViteApp() with Aspire.Hosting.JavaScript 13.1.2
  - Environment injection: ✅ VITE_API_URL properly injected and used
  - Port allocation: ✅ No conflicts detected; no macOS port 5000 issues
  - E2E tests: ✅ Properly configured to work with Aspire orchestration

**Minor Finding:** Vite config specifies `port: 3000` but start script uses `--port 5173`. The script wins; recommendation to update config for consistency (clarity fix, not functional).

**Teams Ready:** Amos (Backend), Naomi (Frontend), Bobbie (Tester) can all proceed with development.

---

*Append-only log. Do NOT edit existing entries.*

## E2E Test Blocker — Cosmos DB Schema

**Date:** 2026-03-07  
**Status:** 🔴 Blocker — Routed to Amos (Backend)  
**Impact:** All 15 E2E tests fail due to missing database schema  

### The Blocker

Cosmos DB collections are not initialized when AppHost starts. Entity Framework has not created the necessary collections ("Teams", etc.), so all tests fail at the first database query with a 404 CosmosException.

### Test Execution Results

- **Total tests:** 15
- **Tests passed:** 0/15
- **Infrastructure status:** ✅ Aspire, frontend, API all working
- **Root cause:** Database schema missing

### Evidence

All failures follow the same pattern:
1. User registration succeeds (API returns 201 Created)
2. Test navigates to dashboard
3. API queries Cosmos DB for user's teams
4. Query fails: `Collection 'Teams' not found in database 'DogTeamsDb'` (404)

### Fix Required

Backend team (Amos) must implement Cosmos DB schema initialization on AppHost startup. Options:
1. Call `context.Database.EnsureCreatedAsync()` during DbContext initialization
2. Run Entity Framework migrations during AppHost startup
3. Apply schema in `AppStartup` handler within Aspire configuration

### Expected Outcome

Once schema initialization is implemented, all 15 E2E tests should pass on re-run.

### File Changes

- `src/DogTeams.Web/ClientApp/playwright.config.ts` — Fixed Playwright port discovery to use Aspire environment variables

---

*Append-only log. Do NOT edit existing entries.*
