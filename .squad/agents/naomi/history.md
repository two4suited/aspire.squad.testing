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
