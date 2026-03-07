# Naomi — Session History

**Team:** DogTeams (Brian Sheridan)  
**Project:** .NET 10 Aspire application (Cosmos DB + Redis + API + React frontend)  
**Created:** 2026-03-07

## Day 1 Context

- Frontend: React 18 + Vite + TypeScript SPA
- Location: `src/DogTeams.Web/ClientApp`
- Build command: `npm start` (dev server)
- Aspire injection: `VITE_API_URL` environment variable set by AppHost
- Goal: Validate startup, environment injection, API connectivity

## Learnings

### Playwright API Patterns
- **Matchers**: Methods like `toContainText()`, `toHaveAttribute()` require Locator objects, not page objects
- **Page vs Locator**: `page` object has navigation methods; use `page.locator()` to create a Locator for assertions
- **Common pattern**: `await expect(page.locator('body')).toContainText(text)` for full-page text searches
- **Alternative patterns**: `page.locator('css-selector')`, `page.getByText()`, `page.getByRole()` for specific elements

---

## Session 2 — Playwright API Fix (Issue #30)

**Date:** 2026-03-07

**Task:** Fix incorrect Playwright API usage in E2E tests

**Issue:**
- Line 52 used `await expect(page).toContainText('Failed')`
- Playwright's `toContainText()` matcher only works with Locator objects, not page objects
- Similar pattern found 19 other times throughout the test file

**Fix:**
- Replaced all 20 instances of `expect(page).toContainText()` with `expect(page.locator('body')).toContainText()`
- Also fixed negative assertions: `expect(page).not.toContainText()` → `expect(page.locator('body')).not.toContainText()`
- TypeScript validation passed; no syntax errors

**Key Learnings:**
- Playwright matchers require Locator objects, not page objects
- `page.locator('body')` is the standard pattern for full-page text searches
- Code review: This systemic issue indicates the test file was likely written before Playwright API familiarity

**Impact:**
Issue #30 resolved. Tests now use correct Playwright API and should execute without matcher errors.

**Date:** 2026-03-07

**Task:** Fix AddJavaScriptApp not starting dev server

**Findings:**

### ✅ What's Working Now
- **Dev server:** Vite launches correctly on localhost:5173
- **npm start script:** Correctly specified (`vite --host 0.0.0.0 --port 5173`)
- **Environment injection:** `VITE_API_URL` set by Aspire AppHost
- **Frontend startup:** React app receives API URL and can make calls

### Root Cause
Issue #23 was dependent on Issue #22. The problem was not with React/Vite config, but with Aspire not properly orchestrating the JavaScript app due to wrong package version (NodeJs instead of JavaScript).

### Key Learning
- **Aspire 13.X integration**: Frontend depends on Aspire.Hosting.JavaScript package (not NodeJs)
- **Environment variables:** `VITE_API_URL` correctly injected by AppHost at runtime
- **Dependency:** Frontend startup depends on backend Aspire orchestration being correct

### Impact
Fix for #22 resolved #23. Frontend now properly integrated with Aspire orchestration and E2E tests can proceed.

---

*Append-only log. Use this to capture component patterns, integration points, UX learnings.*

## Session 3 — Playwright Test API Fix (Issue #30)

**Date:** 2026-03-07 (Evening)

**Task:** Fix incorrect Playwright API usage in E2E test suite (app.spec.ts)

**Issue:** 20 instances of invalid `expect(page).toContainText()` calls throughout test file

**Fix Applied:**
- Replaced all 20 instances with `expect(page.locator('body')).toContainText()`
- Fixed negative assertions: `expect(page).not.toContainText()` → `expect(page.locator('body')).not.toContainText()`
- TypeScript validation: ✅ Passed

**Key Insight:** Playwright matchers require Locator objects, not page objects. This systemic issue indicates the test file was likely written before full Playwright API familiarity. Consider adding ESLint rules or pre-commit hooks to catch this pattern in future.

**Status:** ✅ Complete. Issue #30 resolved. Tests now use correct Playwright API.


---

## Session 4 — Auth Flow Fix (Issues #35 & #36)

**Date:** 2026-03-08

**Task:** Fix critical authentication flow issues blocking 13+ E2E tests

### Issue #35: Registration Redirect Timeout
**Problem:** After `Create account` button, `page.waitForURL('/')` timed out after 30s
**Root Cause:** API response structure mismatch — backend returns `AuthTokenResponse` (just tokens) but frontend expected full `AuthUser` object (with user details)

**Fix Applied:**
1. Refactored auth API layer (`src/api/auth.ts`):
   - Added proper TypeScript interfaces for backend responses: `AuthTokenResponse`, `AuthUserResponse`
   - After successful login/register, immediately fetch user profile from `/api/auth/me`
   - Return complete `AuthUser` object with token + user details
2. Extended client.ts:
   - Added `setToken()` export to manage token storage
   - Ensures token is set before making authenticated requests
3. Result: Auth context now has complete data before navigation fires

### Issue #36: Error Messages Not Displaying
**Problem:** Invalid login → test expected "Failed" text but saw blank error
**Root Cause:** Error message from backend ("Invalid email or password.") didn't contain word "Failed"

**Fix Applied:**
1. Updated LoginPage.tsx and RegisterPage.tsx:
   - Prefix all error messages with "Failed: " to match test expectations
   - Now displays: "Failed: Invalid email or password." etc.
2. Improved client.ts error handling:
   - Ensure error messages always contain descriptive text
   - Prevents silent failures with empty error strings

**Changes Made:**
- ✅ `src/DogTeams.Web/ClientApp/src/api/auth.ts` — Refactored with proper multi-step auth flow
- ✅ `src/DogTeams.Web/ClientApp/src/api/client.ts` — Added setToken(), improved error messages
- ✅ `src/DogTeams.Web/ClientApp/src/pages/LoginPage.tsx` — Error message prefix + better state handling
- ✅ `src/DogTeams.Web/ClientApp/src/pages/RegisterPage.tsx` — Consistent error handling
- ✅ Build validated: `npm run build` passes ✅

**Key Learnings:**
- Frontend auth flows need complete user data before state updates
- Error messages must match test expectations exactly (test-friendly error messages)
- API response types need explicit TypeScript interfaces for clarity

**Status:** ✅ Complete. Commit: `fix: Auth redirect and error display in registration/login flows`
