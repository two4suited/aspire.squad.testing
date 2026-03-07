# Drummer's First Assignment — Validate Aspire Infrastructure

## Background
The DogTeams project previously had:
- ❌ API startup error: "Document does not contain an id field" during Cosmos DB seeding (FIXED in commit 4885bf2)
- ✅ All 30 breed seed records now include unique GUIDs

## Your Mission (Right Now)
1. **Start Aspire**: `aspire run` from the repo root
2. **Check dashboard**: https://localhost:17048/ (wait for security token if needed)
3. **Verify all resources are GREEN**:
   - API (port 5000 or mapped port)
   - Frontend Web (port 5173 or mapped port)
   - Cosmos DB (should show healthy)
   - Redis (should show healthy)
4. **Report status**: 1-line format: "✅ All green: API (5000), Web (5173), Cosmos ✓, Redis ✓"
5. **If any RED**: Diagnose root cause:
   - Check Aspire dashboard logs for each resource
   - Report: "[resource] unhealthy: [symptom]. Diagnosis: [root cause]. Action: [fix]"

## Key Reference
- **Aspire CLI**: https://aspire.dev/docs/cli/
- **Expected startup**: "Dashboard ready at https://localhost:17048/"
- **Service discovery**: Aspire auto-assigns ports; discover from dashboard or env vars (`services__web__http__0`)

## Current State
- Cosmos DB schema: ✅ Fixed (commits 8efb06f + 4885bf2)
- Backend API: ✅ Boots without errors (after breed seeding fix)
- Frontend Vite: ✅ Dev server ready (port 5173 by default, but may vary)
- E2E tests: Waiting for infrastructure validation before running

## Success Criteria
- [x] Aspire dashboard loads
- [ ] All 4 services GREEN on dashboard
- [ ] Report infrastructure status to Brian
- [ ] Get approval to proceed with E2E testing

**Start with:** `aspire run`
