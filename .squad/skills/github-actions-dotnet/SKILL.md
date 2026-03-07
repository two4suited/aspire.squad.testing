# Skill: GitHub Actions CI Pipeline for .NET Projects

**Category**: DevOps / GitHub Actions  
**Difficulty**: Intermediate  
**Time to Implement**: 30-45 minutes  
**Last Updated**: 2026-03-07  

## Overview

This skill demonstrates how to create a production-grade GitHub Actions CI pipeline for .NET projects that:
- Builds on multiple branches (push + PR)
- Runs tests with filtering and coverage collection
- Publishes results back to GitHub
- Integrates with branch protection rules

## Prerequisites

- GitHub repository with .NET solution
- `.sln` file at repository root or known path
- xUnit test project (or similar .NET test framework)
- `.github/workflows/` directory writable

## Pattern: .NET Build + Test Workflow

### Basic Structure

```yaml
name: Build and Test

on:
  pull_request:
    branches: [main]
  push:
    branches: [main]

permissions:
  contents: read
  checks: write

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    name: Build and Test (.NET 10)

    steps:
      - uses: actions/checkout@v4
      
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - run: dotnet restore
      
      - run: dotnet build --configuration Release --no-restore
      
      - run: dotnet test --configuration Release --no-build
      
      - uses: actions/upload-artifact@v4
        if: always()
        with:
          name: test-results
          path: '**/TestResults/*.trx'
```

## Key Decisions & Rationale

### 1. **Setup .NET Explicit Version**
```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'
```
- **Why**: Ensures reproducible builds; pins to major version, patches automatically
- **Alt**: Omit version to use runner default (less reliable)

### 2. **Release Configuration**
```yaml
run: dotnet build --configuration Release --no-restore
```
- **Why**: Matches production runtime; catches compiler optimizations issues
- **Trade-off**: Longer build time; accept for quality gate

### 3. **Test Result Publishing**
```yaml
- uses: EnricoMi/publish-unit-test-result-action@v2
  with:
    files: '**/TestResults/*.trx'
    check_name: Test Results
```
- **Why**: Shows test results in PR checks UI instead of just logs
- **Requirements**: Must generate TRX files (`--logger "trx;LogFileName=test-results.trx"`)

### 4. **Code Coverage Collection**
```yaml
run: dotnet test ... --collect:"XPlat Code Coverage"
```
- **Why**: XPlat format works on all runners (Windows/Linux/macOS)
- **Library**: Requires Coverlet.collector package in test project

### 5. **Test Filtering Strategy**

#### By Namespace (Recommended)
```bash
dotnet test --filter "FullyQualifiedName!~DogTeams.Tests.Integration"
```
- Use when: Tests organized by folder/namespace
- Advantage: Simple, declarative
- How: Tests in `DogTeams.Tests.Integration` namespace are excluded

#### By Trait (Alternative)
```bash
dotnet test --filter "Category!=Integration"
```
- Use when: xUnit Trait attribute applied to tests
- Requires: `[Trait("Category", "Integration")]` on test class
- More fine-grained control

#### By Project (Fallback)
```bash
dotnet test DogTeams.Tests/DogTeams.Tests.csproj
```
- Use when: Tests split across multiple projects
- Less flexible; pick specific test projects to run

## Common Pitfalls & Solutions

| Problem | Solution | Why |
|---------|----------|-----|
| "dotnet command not found" | Check dotnet-version syntax; use '10.0.x' not '10' | Runner default may be old |
| Test results not published | Generate TRX format; check path in `files:` | Default console output not parseable |
| Build succeeds but tests fail silently | Use `if: always()` on upload step | Default is `if: success()` |
| Integration tests timeout in CI | Use filter to exclude; document why | CI environment missing dependencies |
| Cache misses on dependency restore | Don't cache .dotnet; runner reuses SDK | NuGet cache is ephemeral per job |

## Branch Protection Integration

After CI pipeline runs successfully, configure GitHub branch rules:

```bash
gh api repos/{owner}/{repo}/branches/main/protection \
  -f required_status_checks='{"strict":true,"contexts":["Build and Test (.NET 10)"]}' \
  -f enforce_admins=true \
  -f required_pull_request_reviews='{"required_approving_review_count":1}' \
  -f allow_force_pushes=false \
  -f allow_deletions=false
```

**Note**: Job name in status checks must match workflow step name exactly.

## Test Exclusion Scenarios

### Scenario 1: Integration Tests Require Infrastructure
**Problem**: Tests fail because AppHost, Cosmos DB, Redis unavailable  
**Solution**:
```yaml
- run: dotnet test --filter "FullyQualifiedName!~Integration"
```
Document in CONTRIBUTING.md that integration tests require local setup.

### Scenario 2: Flaky Tests in CI
**Problem**: Tests pass locally, fail randomly in CI  
**Solution**:
```yaml
- run: dotnet test --logger "console;verbosity=detailed" --collect:"XPlat Code Coverage"
```
Add retry logic or use `.Skip("Pending fix")` attribute.

### Scenario 3: Long-Running E2E Tests
**Problem**: CI timeout; tests run > 10 minutes  
**Solution**: Run in separate job with extended timeout
```yaml
jobs:
  unit-tests:
    runs-on: ubuntu-latest
    timeout-minutes: 15
    
  e2e-tests:
    runs-on: ubuntu-latest
    timeout-minutes: 60
    needs: unit-tests  # Block E2E until unit tests pass
```

## Troubleshooting Checklist

- [ ] .sln file path correct?
- [ ] .NET version available on runner?
- [ ] Test project has xUnit/test SDK installed?
- [ ] TRX logger configured if publishing results?
- [ ] Filter syntax valid? (Test `--filter` locally first)
- [ ] Job name matches status check context?
- [ ] Permissions sufficient (contents: read, checks: write)?
- [ ] Artifacts retained for debugging (retention-days set)?

## Real-World Example: aspire.squad.testing

Full working example available at:  
`.github/workflows/squad-ci.yml` and `.github/workflows/branch-protection.yml`

Key learnings:
- Namespace filtering works reliably for mixed unit/integration suites
- Test result publishing requires EnricoMi/publish-unit-test-result-action
- Branch protection rules require manual configuration if workflow lacks admin permissions
- Document integration test exclusions in CONTRIBUTING.md for developer awareness

## References

- [GitHub Actions: .NET](https://github.com/actions/setup-dotnet)
- [dotnet test filtering](https://github.com/microsoft/vstest/blob/main/docs/filter.md)
- [xUnit Traits](https://xunit.net/docs/getting-started/xunit.org)
- [Coverlet for code coverage](https://github.com/coverlet-coverage/coverlet)
