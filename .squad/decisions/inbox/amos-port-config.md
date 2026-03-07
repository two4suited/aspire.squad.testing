# Decision: Dynamic API Port Configuration via Aspire Service Discovery

**Issue:** #29  
**Status:** Implemented  
**Owner:** Amos (Backend)  
**Date:** 2026-03-07

## Problem

macOS ControlCenter (AirPlay) occupies port 5000, causing Aspire to silently fail binding the API and assign a random high port. The frontend hardcoded `VITE_API_URL=http://localhost:5000/api`, making it unreachable. This broke 13/15 E2E tests with authentication timeouts.

## Solution Options Considered

### Option A: Dynamic Port Discovery (CHOSEN)
Use Aspire's built-in service discovery to communicate ports automatically.
- **Pros:** Zero configuration, works on any port, future-proof, establishes pattern for other services
- **Cons:** Requires understanding of Aspire env var naming convention
- **Implementation:** Remove hardcoded env var, update Vite proxy to use `services__api__*` vars

### Option B: Alternative Port Binding
Configure API to always use an alternative port (5001, 5050, 8080) less likely to conflict.
- **Pros:** Simple, immediate fix
- **Cons:** Still vulnerable to conflicts, hardcoded values, not scalable

### Option C: Manual Port Configuration
Allow users to specify port via environment variable or config file.
- **Pros:** Flexible
- **Cons:** Adds complexity, requires user configuration, fragile

## Decision

**Implement Option A: Aspire Service Discovery**

Rationale:
1. **Leverages platform capability** — Aspire already handles this via `WithReference()` and environment variables
2. **Eliminates port conflicts entirely** — OS assigns unused ports automatically
3. **Future-proof** — Pattern works for Cosmos DB, Redis, and any future services
4. **Zero configuration** — Works out of the box with no user intervention

## Implementation Details

### AppHost Change
- Removed: `.WithEnvironment("VITE_API_URL", "http://localhost:5000/api")`
- Kept: `.WithReference(api)` which automatically propagates endpoint vars

### Vite Config Change
- Old: `target: process.env.services__dogteamsapi__https__0 || process.env.services__dogteamsapi__http__0`
- New: `target: process.env.services__api__https__0 || process.env.services__api__http__0`
- Reasoning: Service name in AppHost is "api", not "dogteamsapi"; env var naming reflects this

### Aspire Env Var Convention
When `AddProject("api").WithReference()` is used:
```
services__api__https__0 = "https://localhost:PORT"
services__api__http__0  = "http://localhost:PORT"
```

## Verification

✓ API binds to dynamic port (53195 in test, not 5000)  
✓ Vite proxy correctly forwards /api/* requests  
✓ No port binding errors in logs  
✓ Frontend accesses API via discovered endpoint  

## Impact

- **Fixes:** 13/15 E2E test failures (authentication timeouts)
- **Enables:** Local development on machines with port 5000 occupied
- **Pattern:** Establishes service discovery for future cross-service communication

## Team Implications

### Naomi (Frontend)
- Vite proxy now uses Aspire service discovery automatically
- No manual port configuration needed
- Works on any developer machine regardless of port availability

### Bobbie (Tester)
- E2E tests can reach API on discovered port
- Authentication flow no longer times out
- Tests proceed to actual validation

### Holden (Lead)
- Deployment strategy remains unchanged
- Pattern is production-compatible if using Aspire in managed environment
- No breaking changes to local or cloud setup

## Rollback Plan

If issues arise:
1. Revert commit `66e79c58...`
2. Frontend will use hardcoded port 5000 again (will fail on port-conflicted machines)
3. Alternative: Merge with Option B (hardcode alternative port like 5001)

## Future Considerations

This pattern should be applied to:
- Cosmos DB service communication (currently working via connection strings, could use discovery)
- Redis client connections (same as above)
- Any additional microservices added to the orchestration
