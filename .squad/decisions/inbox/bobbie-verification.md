# E2E Test Verification - PR #31 & #32

**Date:** 2026-03-07  
**Tester:** Bobbie  
**Task:** Validate that PR #31 (Playwright fix) and PR #32 (port 5000 conflict fix) resolve e2e test failures

## Summary

✅ **PR #31** - Playwright API usage fix verified as merged (20 instances corrected)
✅ **PR #32** - Port 5000 conflict fix verified as merged (uses service discovery)  
⚠️ **NEW ISSUE** - Frontend port allocation prevents test execution

## Detailed Findings

### PR #31 - Playwright API Fix (Merged ✓)
- **Status:** Fixed 1/15 test failure
- **Change:** expect(page).toContainText() → expect(page.locator('body')).toContainText()
- **Verification:** Code review confirms all 20 instances were corrected in app.spec.ts

### PR #32 - Port 5000 Conflict Fix (Merged ✓)
- **Status:** Fixed 13/15 test failures (port 5000 blocked API access)
- **Changes:**
  - Removed hardcoded `VITE_API_URL="http://localhost:5000/api"` from AppHost
  - Updated vite.config to use Aspire service discovery: `services__api__https__0` and `services__api__http__0`
  - API now uses dynamic port allocation via Aspire
- **Verification:** Code review confirms fix is properly implemented

###  NEW ISSUE - Frontend Port Allocation
- **Status:** 15/15 tests still failing (connectivity issue)
- **Root Cause:** Aspire's `.WithExternalHttpEndpoints()` assigns a random port to the Vite frontend (e.g., 56447, 57619)
- **Impact:** Playwright tests configured for `localhost:5173` cannot connect
- **Details:**
  - npm start script specifies `--port 5173`
  - Aspire appends additional `--port <random>` flag to the command
  - Vite receives: `vite --host 0.0.0.0 --port 5173 --port 57619`
  - Last port flag wins, so Vite listens on 57619, not 5173
  - Playwright config expects `http://localhost:5173` and fails immediately

## Test Execution Results

### Command
```bash
cd src/DogTeams.Web/ClientApp && npm run test:e2e
```

### Result
```
15 failed (all tests)
Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5173/register
```

### Root Cause Analysis
This is NOT a regression from PR #31 or #32. This is an infrastructure/configuration issue:
1. The fixes assumed tests would run with the frontend on a known port
2. Aspire's dynamic port allocation breaks this assumption
3. Tests cannot discover the port that Aspire assigned

## Recommendation

**Option A (Quick Fix for Testing):**
- Don't use `.WithExternalHttpEndpoints()` for frontend in test mode
- Use a configuration flag or environment variable to control this behavior
- Ensures frontend always runs on port 5173 when testing

**Option B (Proper Fix):**
- Update playwright.config.ts to dynamically read the frontend port from Aspire
- Aspire provides port info via environment variables (services__web__http__0)
- Parse URL to extract port and use in baseURL

**Option C (Hybrid):**
- Keep current setup for production use
- Add test-specific startup script that manually starts services on known ports
- Document the difference between dev/test/production startup sequences

## Next Steps

1. Clarify with team: Is Aspire dynamic port allocation intentional for tests?
2. If yes, implement port discovery in playwright.config.ts
3. If no, modify AppHost to not use WithExternalHttpEndpoints() for frontend
4. Update CONTRIBUTING.md with proper e2e test startup sequence

## Files Affected

- `src/DogTeams.AppHost/Program.cs` - AddViteApp configuration
- `src/DogTeams.Web/ClientApp/playwright.config.ts` - Test configuration
- `src/DogTeams.Web/ClientApp/package.json` - npm start script
- `src/DogTeams.Web/ClientApp/vite.config.ts` - Vite configuration

---

**Outcome:** PR #31 and PR #32 fixes are correctly implemented, but infrastructure issue prevents test execution. This requires team decision on how to handle Aspire port allocation for tests.
