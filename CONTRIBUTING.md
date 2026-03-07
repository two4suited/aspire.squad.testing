# Contributing Guide

## Local Development Setup

### Prerequisites

- .NET 10 SDK ([download](https://dotnet.microsoft.com/download))
- Node.js 18+ ([download](https://nodejs.org/))
- Docker & Docker Compose (for local services)

### Initial Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/two4suited/aspire.squad.testing.git
   cd aspire.squad.testing
   ```

2. **Backend setup**
   ```bash
   cd src
   dotnet restore
   dotnet build
   ```

3. **Frontend setup**
   ```bash
   cd DogTeams.Web/ClientApp
   npm install
   npm run build
   ```

## Running Tests Locally

### All Tests
```bash
cd src
dotnet test DogTeams.sln
```

### Specific Test Project
```bash
cd src
dotnet test DogTeams.Tests/DogTeams.Tests.csproj -v detailed
```

### Watch Mode (for development)
```bash
cd src
dotnet watch test DogTeams.Tests/DogTeams.Tests.csproj
```

### With Code Coverage
```bash
cd src
dotnet test DogTeams.sln /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

## CI Pipeline Overview

The GitHub Actions CI pipeline runs automatically on:
- **Push to**: `main`, `dev`, `insider`
- **Pull Requests to**: `main`, `dev`, `preview`, `insider`

### Pipeline Steps

1. **Checkout** - Clone repository code
2. **Setup .NET 10** - Install .NET 10 SDK
3. **Restore** - `dotnet restore src/DogTeams.sln`
4. **Build** - `dotnet build src/DogTeams.sln --configuration Release`
5. **Test** - `dotnet test src/DogTeams.sln --configuration Release`
6. **Upload Results** - Store test artifacts for 7 days

### Status Checks

- **Check Name**: `Build and Test (.NET 10)`
- **Required**: Must pass before merging to `main`
- **Review Approval**: Requires at least 1 approval

## Troubleshooting Pipeline Failures

### Build Failures

**Error**: `dotnet build` fails with MSBuild errors
- Check .NET version: `dotnet --version` (should be 10.0.x)
- Clean and rebuild: `dotnet clean && dotnet restore && dotnet build`
- Check for missing dependencies in .csproj files

**Error**: Package restore fails
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Try restore again: `dotnet restore`

### Test Failures

**Error**: Tests fail in CI but pass locally
- Check .NET version matches (CI uses 10.0.x)
- Ensure all dependencies restored: `dotnet restore`
- Run with detailed logging: `dotnet test --logger:"console;verbosity=detailed"`

**Error**: Test timeout
- Check for infinite loops or blocked I/O
- Increase timeout: `dotnet test --logger:"console;verbosity=detailed" --configuration Release`

### Common Issues

| Issue | Solution |
|-------|----------|
| "The SDK 'dotnet' cannot be found" | Install .NET 10 SDK locally |
| "Project file not found" | Ensure running from correct directory |
| "Test project not found" | Check DogTeams.Tests.csproj exists |
| "Incompatible NuGet packages" | Run `dotnet restore --force` |

## Code Quality

The project uses:
- **xUnit** for unit testing (See `DogTeams.Tests/`)
- **Moq** for mocking
- **FluentAssertions** for readable test assertions
- **Code Coverage** collected via Coverlet

## Making a Pull Request

1. **Create a feature branch**: `git checkout -b feature/your-feature`
2. **Make changes and test locally**: `dotnet test`
3. **Push to GitHub**: `git push origin feature/your-feature`
4. **Open a Pull Request** to `main`
5. **Wait for CI to pass** (automatic status check)
6. **Request review** from team members
7. **Merge** once approved and CI passes

## Branch Protection Rules

The `main` branch is protected with:
- ✅ Required CI status check (`Build and Test (.NET 10)`)
- ✅ Requires 1 approval on pull requests
- ✅ Dismisses stale reviews on new commits
- ✅ Prevents force pushes and deletions

## Questions?

- Check [API.md](./API.md) for endpoint documentation
- Check [TESTING.md](./TESTING.md) for detailed test guides
- Ask in team discussions or open an issue

---

**Last Updated**: 2026-03-07
