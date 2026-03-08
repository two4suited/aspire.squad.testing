# Decision: Seeded User E2E Tests for Debugging

**Date:** 2026-03-07  
**Author:** Bobbie (Tester)  
**Category:** Testing Pattern  
**Status:** Recommended

## Pattern

For authentication-critical workflows, create **seeded user E2E tests** alongside **registration-based E2E tests**.

### Purpose

- **Seeded tests:** Isolate the authentication endpoint (login) from registration flow
- **Registration tests:** Validate full user lifecycle (register → login → interact)
- Together: Separate concerns and enable faster debugging

### Implementation

1. **Add seed data to backend** (if not present):
   - Define test user in `UserSeedData.cs` or equivalent
   - Seed only in Development mode: `if (app.Environment.IsDevelopment())`

2. **Create focused seeded-user E2E test**:
   ```typescript
   test('should login with seeded test credentials', async ({ page }) => {
     await page.goto('/login');
     await page.fill('#email', 'test@example.com');
     await page.fill('#password', 'TestPassword123!');
     await page.click('button:has-text("Sign in")');
     
     await expect(page).toHaveURL('/'); // Verify redirect
     await expect(page.locator('body')).toContainText('Welcome,'); // Verify success state
   });
   ```

3. **Place in test suite** before registration-based tests:
   - Establishes baseline (seeded user works)
   - Registration tests build on this foundation

### Benefits

1. **Faster debugging:** No registration overhead; test fails at login if backend broken
2. **Offline verification:** Manual testers can verify credentials outside automated tests
3. **Layered validation:** Infrastructure (seed initialization) validated separately from registration logic
4. **Idempotent tests:** Seeding typically idempotent; tests can re-run without cleanup
5. **Clear separation:** Tests "auth works in general" vs "auth works + registration works"

### When to Use

✅ Use when:
- Authentication is critical to workflows
- Seed data infrastructure exists
- Development-mode testing acceptable
- Need fast feedback on login endpoint

❌ Don't use when:
- Seed data not appropriate (e.g., transaction-heavy data)
- Production environment (don't seed production)
- Registration logic itself is under test (use registration-based tests instead)

### Example Checklist

- [ ] Seed data defined and idempotent
- [ ] Seeding guarded by `IsDevelopment()` check
- [ ] Seeded test placed first in auth suite
- [ ] Comments explain credential source
- [ ] Test verifies both redirect and success state
- [ ] Error cases covered separately (invalid creds test)

## Files Affected

- `src/DogTeams.Web/ClientApp/tests/app.spec.ts` — Added seeded user test

## Related

- Pattern: Registration-based E2E tests (existing in app.spec.ts)
- Infrastructure: `UserSeedData.cs`, `InMemoryUserService.SeedTestUsers()`
