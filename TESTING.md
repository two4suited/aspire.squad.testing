# Testing Guide

## Overview

The Dog Teams application includes comprehensive testing at multiple levels:
- **Unit Tests** (56 tests)
- **Integration Tests** (13 skipped, ready to enable)
- **E2E Tests** (15 scenarios)

## Backend Testing

### Running Backend Tests

```bash
# Run all tests
cd src
dotnet test DogTeams.sln

# Run specific test project
dotnet test DogTeams.Tests/DogTeams.Tests.csproj

# Run with verbose output
dotnet test DogTeams.sln --verbosity=detailed

# Run specific test
dotnet test DogTeams.sln --filter "FullyQualifiedName~JwtTokenServiceTests.GetUserIdFromToken"

# Generate coverage report
dotnet test DogTeams.sln /p:CollectCoverage=true
```

### Backend Test Structure

**Unit Tests** (45 tests):
- `JwtTokenServiceTests.cs` - JWT token generation, validation, tampering
- `UserServiceTests.cs` - User CRUD, password hashing, email lookup
- `DomainModelTests.cs` - Entity creation, relationships, validation

**Integration Tests** (13 tests - currently skipped):
- `AuthIntegrationTests.cs` - Full auth flow with API
- `TeamsIntegrationTests.cs` - Team CRUD operations
- `OwnersIntegrationTests.cs` - Owner operations
- `DogsIntegrationTests.cs` - Dog operations

### Example: Running JWT Tests

```bash
dotnet test DogTeams.Tests/DogTeams.Tests.csproj \
  --filter "FullyQualifiedName~JwtTokenServiceTests" \
  --logger "console;verbosity=detailed"
```

## Frontend Testing

### Running Frontend Tests

```bash
cd src/DogTeams.Web/ClientApp

# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run with UI
npm test:ui

# Run specific test file
npm test -- Modal.test.tsx

# Generate coverage
npm test -- --coverage
```

### Frontend Unit Tests (11 tests)

**Component Tests**:
- `Modal.test.tsx` - Modal rendering, closing, children
- `ErrorAlert.test.tsx` - Error message display, dismiss
- `LoadingSpinner.test.tsx` - Spinner render and animation

### Frontend E2E Tests (15 scenarios)

**Authentication Tests** (5 scenarios):
```bash
npm run test:e2e -- --grep "Authentication"
```

Scenarios:
- Register and login successfully
- Login with existing account
- Logout successfully
- Redirect to login when accessing protected routes
- Show error on invalid credentials

**Team Management Tests** (3 scenarios):
```bash
npm run test:e2e -- --grep "Team Management"
```

Scenarios:
- Create a team
- View team details
- Delete a team

**Owner Management Tests** (2 scenarios):
```bash
npm run test:e2e -- --grep "Owner Management"
```

Scenarios:
- Add owner to team
- Delete owner from team

**Dog Management Tests** (2 scenarios):
```bash
npm run test:e2e -- --grep "Dog Management"
```

Scenarios:
- Add dog to owner
- Delete dog from owner

**Error Handling Tests** (2 scenarios):
```bash
npm run test:e2e -- --grep "Error Handling"
```

Scenarios:
- Handle API errors gracefully
- Show error messages on failed operations

**Complete User Journey** (1 scenario):
```bash
npm run test:e2e -- --grep "Complete User Journey"
```

Full workflow: register → create team → add owner → add dog → logout

## Running E2E Tests

### Prerequisites & Setup

**IMPORTANT: Aspire automatically handles all services - backend, frontend, database, and cache**

#### Single Command: Start Everything with Aspire

```bash
cd src
dotnet run --project DogTeams.AppHost
```

This single command starts:
- .NET API on http://localhost:5000
- React frontend on http://localhost:5173
- Cosmos DB local emulator
- Redis cache
- Aspire Dashboard on https://localhost:17048 (for monitoring)

**Wait 15-20 seconds for all services to fully initialize**

#### Verify Services Are Ready

In a separate terminal:
```bash
# Check API
curl http://localhost:5000/api/health

# Check Frontend
curl http://localhost:5173 | head -5
```

Both should respond successfully.

### Execute E2E Tests

Once Aspire is running (and services verified), open a new terminal:

```bash
cd src/DogTeams.Web/ClientApp

# Run all E2E tests
npm run test:e2e

# Run with interactive UI
npm run test:e2e:ui

# Run in debug mode (step through tests)
npm run test:e2e:debug

# Run specific test file
npx playwright test tests/app.spec.ts

# Run specific test by name
npx playwright test -g "should register and login successfully"

# Run with verbose output
npx playwright test --reporter=verbose
```

### Test Results

Expected output:
```
Running 15 tests using 1 worker

✓ [chromium] › tests/app.spec.ts:5 › Authentication Flow › should register and login successfully
✓ [chromium] › tests/app.spec.ts:14 › Authentication Flow › should login with existing account
✓ [chromium] › tests/app.spec.ts:30 › Authentication Flow › should logout successfully
...
15 passed (2m 30s)
```

### Troubleshooting E2E Tests

**Tests fail with "Connection refused"**
- Verify Aspire is running: `lsof -i :5000`
- Check both services are accessible:
  ```bash
  curl http://localhost:5000/api/health
  curl http://localhost:5173
  ```
- Restart Aspire if needed

**"Cannot find localhost:5173"**
- Frontend may still be starting - wait 20 seconds after running Aspire
- Check Aspire dashboard: https://localhost:17048

**Playwright browser issues**
```bash
# Reinstall browsers
npx playwright install --with-deps

# Clear Playwright cache
rm -rf ~/.cache/ms-playwright
npx playwright install
```

**Tests timeout or hang**
- Check Aspire logs for errors
- Verify no port conflicts: `lsof -i :5000; lsof -i :5173; lsof -i :6379`
- Restart from clean state: stop Aspire and run again

### Aspire Dashboard

While testing, you can monitor services in real-time:
- Open https://localhost:17048/login?t=<token> in browser
- Check API response times and errors
- View Redis cache performance
- Monitor database queries

### E2E Test Configuration

See `playwright.config.ts` for configuration:
- Browser: Chrome
- Base URL: http://localhost:5173
- Timeout: 30 seconds per test
- Retries: 2 on CI

## Test Results

### Current Status
- ✅ **45 Backend Unit Tests** - 100% passing
- ✅ **11 Frontend Component Tests** - 100% passing
- ✅ **15 E2E Scenarios** - All ready
- ⏸️ **13 Integration Tests** - Skipped (non-blocking)

### Expected Output

```
Backend:
Passed! - Failed: 0, Passed: 45, Skipped: 13, Total: 58

Frontend:
✓ src/components/Modal.test.tsx (4 tests)
✓ src/components/ErrorAlert.test.tsx (5 tests)
✓ src/components/LoadingSpinner.test.tsx (2 tests)
Test Files  3 passed (3)
Tests  11 passed (11)
```

## Writing New Tests

### Backend Unit Test Example

```csharp
using Xunit;
using Moq;
using FluentAssertions;

public class MyServiceTests
{
    private readonly MyService _service;

    public MyServiceTests()
    {
        _service = new MyService();
    }

    [Fact]
    public void MyMethod_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var input = "test";

        // Act
        var result = _service.MyMethod(input);

        // Assert
        result.Should().Be("expected");
    }
}
```

### Frontend Unit Test Example

```typescript
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import MyComponent from './MyComponent';

describe('MyComponent', () => {
  it('should render correctly', () => {
    render(<MyComponent />);
    expect(screen.getByText('Hello')).toBeInTheDocument();
  });
});
```

### Frontend E2E Test Example

```typescript
import { test, expect } from '@playwright/test';

test('should complete user flow', async ({ page }) => {
  // Navigate to app
  await page.goto('/');
  
  // Fill login form
  await page.fill('#email', 'test@example.com');
  await page.fill('#password', 'password');
  
  // Submit
  await page.click('button:has-text("Sign in")');
  
  // Verify redirect
  await expect(page).toHaveURL('/dashboard');
});
```

## Continuous Integration

### GitHub Actions

Tests run automatically on:
- Push to main
- Pull requests
- Scheduled daily (optional)

Configuration: `.github/workflows/ci.yml`

### Local Pre-commit Hook

Recommended to run tests before committing:

```bash
#!/bin/bash
# .git/hooks/pre-commit

echo "Running tests..."
cd src
dotnet test DogTeams.sln --no-build --no-restore || exit 1

cd DogTeams.Web/ClientApp
npm test -- --run --bail || exit 1

echo "Tests passed!"
```

## Debugging Tests

### Backend Test Debugging

```bash
# Run with detailed output
dotnet test --verbosity:detailed

# Attach debugger
dotnet test --configuration Debug
```

### Frontend Test Debugging

```bash
# Debug mode
npm run test:e2e:debug

# Watch mode for unit tests
npm test -- --watch

# VSCode debugger
node --inspect-brk ./node_modules/.bin/vitest
```

## Performance Testing

### Load Testing (Future)

```bash
# Using Artillery or K6
k6 run load-test.js
```

### Profiling

```bash
# Backend
dotnet test --collect:"XPlat Code Coverage"

# Frontend
npm run build -- --analyze
```

## Known Issues & Limitations

1. **Integration Tests Skipped** - Requires AppHostFixture setup
2. **No API Rate Limiting Tests** - To be added
3. **Mobile Testing** - Manual testing recommended
4. **Accessibility Testing** - To be added with WAVE integration

## Best Practices

✅ Write tests for new features
✅ Keep tests independent and fast
✅ Use meaningful test names
✅ Arrange-Act-Assert pattern
✅ Mock external dependencies
✅ Aim for >80% code coverage
✅ Run tests before committing
✅ Update tests when requirements change

## Troubleshooting

### Tests Fail to Run

```bash
# Clear build cache
dotnet clean
rm -rf bin obj
npm cache clean --force

# Rebuild
dotnet build
npm install
```

### Playwright Browser Issues

```bash
# Install browsers
npx playwright install

# Install dependencies
npx playwright install-deps
```

### JWT Token Expired in Tests

Tokens are automatically refreshed in auth flow tests. If tests fail:
```bash
# Clear test state
rm -rf node_modules/.playwright
npm test -- --no-cache
```
