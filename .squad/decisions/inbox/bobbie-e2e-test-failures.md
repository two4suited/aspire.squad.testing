# E2E Test Failure Patterns — Port Conflict Root Cause

**Author:** Bobbie (Tester)  
**Date:** 2026-03-07  
**Status:** Blocker Identified  
**Related Issues:** #29, #30

## Summary

E2E test suite (15 tests) has **14 failures**: 13 due to infrastructure port conflict, 1 due to test code bug. The primary blocker is port 5000 conflict with macOS system services.

## Pattern Analysis

### Failure Pattern 1: Port Conflict (13 tests, 87%)

**Symptom:** All tests timeout after 30 seconds waiting for navigation to "/" after registration/login.

**Root Cause:** macOS ControlCenter service occupies port 5000 (AirPlay/Continuity), forcing Aspire to assign random high ports to API. Frontend hardcoded to `http://localhost:5000/api`, making API unreachable.

**Affected flows:**
- Authentication (3 tests)
- Team Management (3 tests)
- Owner Management (2 tests)
- Dog Management (2 tests)
- Error Handling (2 tests)
- Complete User Journey (1 test)

### Failure Pattern 2: Incorrect API Usage (1 test, 7%)

**Symptom:** TypeError when calling `expect(page).toContainText()`.

**Root Cause:** Playwright API misuse — `toContainText()` requires a Locator, not a Page object.

**Affected test:** Authentication Flow › should show error on invalid credentials

## Architectural Issue

**Current implementation (broken):**
```csharp
// AppHost/Program.cs:22
builder.AddViteApp("web", "../DogTeams.Web/ClientApp", "start")
    .WithEnvironment("VITE_API_URL", "http://localhost:5000/api");  // ❌ Hardcoded port
```

**Problem:** Hardcoded port assumption breaks when:
- Port is already in use (macOS AirPlay, other services)
- Multiple developers run on different ports
- CI/CD environments use dynamic port assignment
- Docker/containerized deployments

## Recommended Decision

**Adopt Aspire service discovery for inter-service communication.**

### Option 1: Use Aspire Service References (Recommended)

```csharp
// AppHost/Program.cs
var api = builder.AddProject<Projects.DogTeams_Api>("api")
    .WithExternalHttpEndpoints();

builder.AddViteApp("web", "../DogTeams.Web/ClientApp", "start")
    .WithReference(api)  // ✅ Dynamic discovery
    .WaitFor(api);
```

Frontend would access API via service name:
```typescript
// Use service name instead of hardcoded URL
const API_URL = import.meta.env.VITE_API_URL || 'http://api/api';
```

### Option 2: Configure Non-Conflicting Port

```json
// API/Properties/launchSettings.json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5050"  // ✅ Different port
    }
  }
}
```

```csharp
// AppHost/Program.cs:22
builder.AddViteApp("web", "../DogTeams.Web/ClientApp", "start")
    .WithEnvironment("VITE_API_URL", "http://localhost:5050/api");
```

**Trade-offs:**
- Option 1: Best practice, production-ready, resilient
- Option 2: Quick fix, still brittle, requires manual port management

## Testing Hardening Recommendations

1. **Pre-flight health checks:** Verify API accessibility before running tests
   ```typescript
   test.beforeAll(async () => {
     await waitForApiHealth('http://localhost:5000/api/health', 30000);
   });
   ```

2. **Dynamic port discovery:** Tests should read API URL from environment or service discovery

3. **Process cleanup:** Add teardown to stop stale API processes

4. **Retry logic:** Add retry for transient connection failures

5. **CI/CD considerations:** Ensure port availability checks in CI pipeline

## Impact Assessment

**Severity:** Blocker  
**Scope:** All E2E tests (93% failure rate)  
**Developer friction:** High — port 5000 conflict is common on macOS  
**Production risk:** High — hardcoded ports will fail in real deployments

## Next Actions

1. Team lead should decide between Option 1 (service discovery) vs Option 2 (port change)
2. Assign Issue #29 to backend engineer (Amos) for infrastructure fix
3. Assign Issue #30 to frontend engineer (Naomi) for test code fix (1-line change)
4. Re-run E2E tests after fixes to verify 15/15 passing

---

**Decision Required:** Should we use Aspire service discovery (Option 1) or change port (Option 2)?

**Recommendation:** Option 1 (service discovery) for long-term maintainability and production readiness.
