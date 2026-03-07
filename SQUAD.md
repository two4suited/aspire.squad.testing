# Squad Way of Operating

## Development Process

This document outlines the standard operating procedure for the Dog Teams AI Squad. All team members should follow this workflow.

## Issue-Driven Development

### Every Task Starts with a GitHub Issue

1. **Create Issue Before Working**
   - Create a GitHub issue describing the problem/feature
   - Add appropriate labels (bug, feature, documentation, etc.)
   - Include acceptance criteria and reproducible steps for bugs
   - Get clarity on requirements before starting

2. **Work on the Issue**
   - Create a branch or work directly (if small fix)
   - Implement the solution
   - Follow code style and best practices
   - Write tests where applicable

3. **Close Issue with Evidence**
   - Commit changes with clear message
   - Link commit to issue: "Fixes issue #X"
   - Add comment to issue documenting what was done:
     - Changes made
     - Testing performed
     - Any side effects or dependencies
   - Close the issue

### Commit Message Format

```
type: short description

Longer explanation if needed (2-3 sentences).
Include what was changed and why.

Fixes issue #X

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

**Types:**
- `fix:` - Bug fixes
- `feat:` - New features
- `docs:` - Documentation
- `refactor:` - Code restructuring without changing behavior
- `test:` - Test additions/improvements
- `chore:` - Build scripts, dependencies, etc.

### Example Issue Workflow

```
1. Create Issue #42: "BUG: Frontend not loading"
   - Describe the problem
   - Add reproduction steps
   - Add acceptance criteria

2. Work on the fix
   - Investigate root cause
   - Make code changes
   - Test thoroughly

3. Commit:
   git commit -m "fix: frontend loading issue due to missing API endpoint
   
   Added missing /api/health endpoint that frontend expects on startup.
   Tests pass. Verified in browser.
   
   Fixes issue #42
   
   Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"

4. Close issue with comment:
   "Fixed in commit [hash]. Frontend now loads successfully and connects to API."
```

## Testing & Verification

### Before Closing Any Issue

- [ ] Code builds without errors
- [ ] All tests pass (existing + new)
- [ ] Feature works as described in acceptance criteria
- [ ] No console warnings or errors
- [ ] Documentation updated if needed

### Test Command Reference

```bash
# Backend tests
cd src && dotnet test DogTeams.sln

# Frontend unit tests
cd src/DogTeams.Web/ClientApp && npm test

# E2E tests (requires Aspire running)
cd src && dotnet run --project DogTeams.AppHost
# In new terminal:
cd src/DogTeams.Web/ClientApp && npm run test:e2e
```

## Running the Application

### Quick Start (Aspire Recommended)

```bash
cd src
dotnet run --project DogTeams.AppHost
```

This starts:
- ✅ Backend API (localhost:5000)
- ✅ Frontend (localhost:5173)
- ✅ Cosmos DB Emulator
- ✅ Redis Cache
- ✅ Aspire Dashboard (localhost:17048)

**Wait 20 seconds** for all services to initialize.

### Verify Services

```bash
# In new terminal
curl http://localhost:5000/api/health
curl http://localhost:5173 | head -5
```

## Common Patterns & Solutions

### Package Version Issues

**Problem:** New package feature not working
**Solution:** Always check aspire.dev or official docs for version compatibility

Example:
- ❌ Aspire.Hosting.NodeJs v9.5.2 with Aspire 13.1.2
- ✅ Aspire.Hosting.JavaScript v13.1.2 with Aspire 13.1.2

### Frontend Not Starting

**Problem:** Tests fail with "Connection refused" on port 5173
**Solution:** Ensure npm start script points to vite, not old build output

```json
{
  "scripts": {
    "start": "vite --host 0.0.0.0 --port 5173",
    "dev": "vite --host 0.0.0.0 --port 5173"
  }
}
```

### Aspire React Configuration

**Wrong:** Wrap React app in .NET ASP.NET project
**Right:** Use `AddJavaScriptApp()` in AppHost to run npm start directly

```csharp
// ✅ Correct
builder.AddJavaScriptApp("web", "../DogTeams.Web/ClientApp", "start")
    .WithReference(api)
    .WithExternalHttpEndpoints();

// ❌ Wrong
builder.AddProject<Projects.DogTeams_Web>("web")  // .NET wrapper
    .WithReference(api);
```

## Documentation Standards

### When to Update Docs

1. **Feature Added** → Update README.md and relevant guide
2. **Setup Changed** → Update SETUP.md
3. **API Endpoint Added** → Update API.md
4. **Architecture Changed** → Update ARCHITECTURE.md
5. **Test Strategy Changed** → Update TESTING.md
6. **Deployment Changed** → Update DEPLOYMENT.md

### Documentation Files

- **README.md** - Project overview, quick start, features
- **SETUP.md** - Local development setup
- **API.md** - Endpoint documentation with examples
- **TESTING.md** - How to run tests, test structure
- **ARCHITECTURE.md** - System design and patterns
- **DEPLOYMENT.md** - Production deployment guide
- **SQUAD.md** - This file - way of operating

## Code Review Checklist

When reviewing code or testing a feature:

- [ ] Issue number referenced in commit
- [ ] Code builds and tests pass
- [ ] Feature works as described in issue
- [ ] No breaking changes to existing features
- [ ] Documentation updated if needed
- [ ] Commit message clear and descriptive
- [ ] Co-author credit included in commit

## Troubleshooting Guide

### Port Already in Use

```bash
# Find process on port
lsof -i :5000  # API
lsof -i :5173  # Frontend
lsof -i :6379  # Redis

# Kill process
kill -9 <PID>
```

### Clean Build

```bash
# Backend
cd src && dotnet clean && dotnet build

# Frontend
cd src/DogTeams.Web/ClientApp && rm -rf node_modules && npm install
```

### Aspire Dashboard Not Accessible

- Verify: https://localhost:17048 (use https, not http)
- Check token: Look in console output for login URL
- Browser may warn about self-signed cert - proceed anyway

## Sprint Workflow

### Creating a Sprint

1. Create GitHub milestone (e.g., "Sprint 3")
2. Add issues to milestone
3. Document sprint goals in issue description

### Tracking Sprint Progress

- Use GitHub issue labels: sprint-1, sprint-2, etc.
- Mark in-progress work in issue comments
- Close issues with evidence of completion
- Update milestone progress

### End of Sprint

- Ensure all issues are closed or documented
- Run full test suite: `dotnet test && npm test && npm run test:e2e`
- Create summary comment in sprint milestone
- Document any blockers or future work

## Performance Guidelines

### When Adding Features

1. **Do not break existing tests**
   - If test fails, investigate and fix before committing

2. **Measure performance**
   - Document baseline metrics
   - Verify no significant regression
   - Example: API response time, bundle size

3. **Cache appropriately**
   - Use Redis for frequently accessed data
   - Set appropriate TTLs (see ARCHITECTURE.md)

4. **Monitor RU consumption** (Cosmos DB)
   - Single-partition queries: ~1 RU
   - Cross-partition: 10-100+ RU
   - Aim for 60%+ RU reduction with caching

## Release Checklist

Before marking work complete:

- [ ] All tests passing (unit, integration, E2E)
- [ ] No console warnings/errors
- [ ] Documentation updated
- [ ] Performance acceptable
- [ ] Security implications reviewed
- [ ] Code follows style guide
- [ ] Issue closed with summary

## Getting Help

1. **Check existing documentation** - README, SETUP, API, TESTING guides
2. **Review similar past issues** - Look at closed issues for patterns
3. **Check TROUBLESHOOTING.md** - Common problems and solutions
4. **Ask in issue comments** - Describe what you've tried

## Squad Core Values

- **Ship working code** - Tests must pass before closing issues
- **Document decisions** - Commit messages and issue comments explain why
- **Iterate quickly** - Create issue → implement → test → close
- **No broken builds** - Always verify tests pass locally before pushing
- **Continuous improvement** - Update guides and processes based on learnings
