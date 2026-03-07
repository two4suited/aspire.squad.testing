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

(To be populated as work progresses)

---

*Append-only log. Use this to capture component patterns, integration points, UX learnings.*

## Session 1 — React Startup Fix (Issue #23)

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
