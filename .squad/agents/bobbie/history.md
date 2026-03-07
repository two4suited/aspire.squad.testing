# Bobbie — Session History

**Team:** DogTeams (Brian Sheridan)  
**Project:** .NET 10 Aspire application (Cosmos DB + Redis + API + React frontend)  
**Created:** 2026-03-07

## Day 1 Context

- Test frameworks: xUnit (backend), Jest/Vitest (frontend)
- Priority tests: Aspire orchestration integration tests, API connectivity, frontend startup
- Goal: Validate that all services start and connect correctly

## Learnings

(To be populated as work progresses)

---

*Append-only log. Use this to capture test patterns, coverage metrics, quality learnings.*

## Session 1 — E2E Test Blocker Analysis (Issue #20)

**Date:** 2026-03-07

**Task:** Analyze E2E test failures

**Findings:**

### ✅ What's Working
- **Test framework:** Playwright 1.58.2 correctly configured
- **Frontend startup:** Vite running on port 5173
- **Test structure:** 15 tests across 5 suites, helpers properly abstracted
- **Configuration:** baseURL correctly set to http://localhost:5173

### ❌ What's Broken
- **API accessibility:** Tests timeout waiting for registration API on port 5000
- **Root cause:** Aspire orchestration not properly exposing API to frontend

### Blocker Chain Identified
```
#22 (Aspire package version) → #23 (JS app orchestration) → #20 (E2E tests)
```

### Key Learning
- **E2E test dependency:** All tests depend on API connectivity for authentication
- **Infrastructure blocker:** Tests can't proceed until Aspire correctly orchestrates both frontend and backend
- **Test hardening needed:** Future work should add pre-flight health checks for both frontend and API

### Verification Plan
1. Merge fixes for #22 and #23
2. Re-run E2E tests via `npm run test:e2e`
3. Expect all 15 tests to pass once API is accessible
4. Add pre-flight checks for production-like behavior

### Impact
Blockers #22 and #23 resolved. E2E tests should now pass on re-run. No changes to test code needed.

---

*Append-only log. Use this to capture test patterns, coverage metrics, quality learnings.*

## Session 2 — E2E Test Failure Documentation (Issue #29, #30)

**Date:** 2026-03-07

**Task:** Run E2E tests and document all failures

**Findings:**

### Test Results Summary
- **Total tests:** 15
- **Passed:** 1 (redirect to login without auth)
- **Failed:** 14
- **Execution time:** 6.7 minutes

### ✅ What's Working
- **Aspire AppHost:** Successfully starts on port 17048
- **Frontend:** Vite dev server running correctly on port 5173
- **API processes:** Running (multiple instances detected)
- **Test framework:** Playwright 1.58.2 configured correctly
- **One passing test:** "should redirect to login when accessing protected routes without auth"

### ❌ Critical Issues Found

#### Issue #29: Port 5000 Conflict (13 test failures)

**Root Cause:** Port 5000 is occupied by macOS ControlCenter service (AirPlay/Continuity), causing Aspire to assign random high ports to the API (57514, 62973, 63118, etc.).

**Impact:**
- Frontend hardcoded to call `http://localhost:5000/api` (AppHost Program.cs line 22)
- API is inaccessible from frontend
- All authentication flows timeout after 30 seconds
- 13 tests fail at `page.waitForURL('/')` after registration/login

**Affected test categories:**
- Authentication Flow: 3 failures
- Team Management: 3 failures
- Owner Management: 2 failures
- Dog Management: 2 failures
- Error Handling: 2 failures
- Complete User Journey: 1 failure

**Recommended fix:**
1. Use Aspire service discovery instead of hardcoded port
2. Configure API to use alternative port (e.g., 5001) in launchSettings.json
3. Add pre-flight health checks in tests

#### Issue #30: Playwright API Misuse (1 test failure)

**Root Cause:** Test code incorrectly uses `expect(page).toContainText()` instead of `expect(page.locator(...)).toContainText()`.

**Location:** `tests/app.spec.ts:52`

**Fix:** Change to `expect(page.locator('body')).toContainText('Failed')`

### Key Learnings

1. **Port conflicts are common on developer machines:** macOS uses port 5000 for AirPlay by default. This is a widespread issue that affects local development.

2. **Aspire port assignment:** When a configured port is unavailable, Aspire silently assigns random ports. This breaks hardcoded URLs.

3. **Service discovery is critical:** Production-grade Aspire apps should use service discovery, not hardcoded ports, for inter-service communication.

4. **Pre-flight checks needed:** E2E tests should verify API availability before running test suites. Current tests have no health check mechanism.

5. **Multiple API instances:** Found 6 running DogTeams.Api processes, suggesting previous test runs didn't clean up properly. Need better process management.

### Test Infrastructure Assessment

**Strengths:**
- Good test coverage across user journeys
- Well-organized test helpers
- Proper authentication flow testing

**Weaknesses:**
- No API health checks before test execution
- Hardcoded ports instead of dynamic discovery
- No retry logic for transient failures
- Missing cleanup of stale processes

### Next Steps

1. Team should prioritize fixing Issue #29 (blocker for all E2E tests)
2. Quick fix for Issue #30 (single line change)
3. Consider adding:
   - Test setup script to check port availability
   - Dynamic service discovery in tests
   - Process cleanup in test teardown
   - API health check polling before test execution

### Issues Created

- **#29:** [E2E TEST] Port 5000 conflict causes 13 test failures — API inaccessible
- **#30:** [E2E TEST] Incorrect Playwright API usage — toContainText on page object

---

*Testing complete. All failures documented and tracked.*

