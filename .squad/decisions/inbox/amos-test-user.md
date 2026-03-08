# Test User Seeding for Manual Testing

**Date:** 2026-03-07  
**Author:** Amos (Backend)  
**Status:** ✅ COMPLETE

## Summary

A test user has been seeded into the authentication system for Brian's manual testing. The implementation uses the existing `InMemoryUserService` and seeds the user automatically during API startup in development mode.

## Test User Credentials

```
Email:    test@example.com
Password: TestPassword123!
Name:     Test User
```

## How to Login

1. Start the Aspire application: `dotnet run --project src/DogTeams.AppHost`
2. Navigate to the frontend (typically http://localhost:5173)
3. Use the credentials above to login via the auth endpoint
4. The API will issue JWT tokens stored in Redis for session management

## Implementation Details

### Files Created
- **`src/DogTeams.Api/Auth/UserSeedData.cs`** — Static seed data class containing test user credentials and factory method for creating User objects with hashed passwords

### Files Modified
- **`src/DogTeams.Api/Auth/UserService.cs`** — Added `SeedTestUsers()` static method to `InMemoryUserService` that idempotently populates the in-memory user dictionary during initialization
- **`src/DogTeams.Api/Program.cs`** — Added seeding call in development environment (lines 95–101) that runs after Cosmos DB initialization

### Design Decisions

1. **In-Memory Storage** — The system currently uses `InMemoryUserService` (not Cosmos DB). The test user is seeded into memory during startup, so it will persist for the duration of the application runtime but will be lost on restart.

2. **Idempotent Seeding** — The `SeedTestUsers()` method checks if a user with the same email already exists before inserting. This allows safe re-runs and prevents duplicate user creation.

3. **Development-Only** — Seeding only occurs in development mode (`app.Environment.IsDevelopment()`). Production deployments will not automatically create test users.

4. **BCrypt Password Hashing** — Passwords are hashed using BCrypt (EnhancedHashPassword) to match the hashing strategy used in the register endpoint, ensuring credentials are verified correctly during login.

## Verification

To verify the test user works:

1. Start the API with `dotnet run --project src/DogTeams.Api`
2. POST to `/api/auth/login` with:
   ```json
   {
     "email": "test@example.com",
     "password": "TestPassword123!"
   }
   ```
3. Expect a 200 OK response with access token and refresh token

## Future Considerations

- **Cosmos DB Migration** — When the system migrates from `InMemoryUserService` to a Cosmos DB-backed store, the seeding mechanism should be updated to write directly to the identity container (already defined in `CosmosDbContext`)
- **Environment Configuration** — Test user credentials could be externalized to `appsettings.Development.json` for easier customization per environment
- **Multi-User Seeding** — Additional test users can be added to `UserSeedData.GetAll()` as needed for testing different roles or scenarios

## Related Work

- `CosmosDbInitializer` — Established the pattern for seeding reference data during startup (Breeds container)
- `AuthController` — Endpoints for register, login, logout, and token refresh
- `JwtTokenService` — Token generation and validation

---

**Test user is ready for manual testing. No further action required.**
