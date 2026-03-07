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
