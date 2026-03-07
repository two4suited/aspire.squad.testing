# Grendel — DevOps History

## Sprint 1: CI Pipeline Implementation

### Completed
- ✅ Created GitHub Actions CI pipeline (squad-ci.yml)
- ✅ Configured branch protection workflow (branch-protection.yml)
- ✅ Added CONTRIBUTING.md with local dev and CI troubleshooting guide
- ✅ Integrated test result publishing to PR checks
- ✅ Unit test filtering to exclude flaky integration tests pending infrastructure

### Learnings

#### .NET Toolchain
- **Version**: .NET 10.0.x
- **Target Framework**: net10.0 (multi-project solution)
- **Solution Structure**: DogTeams.sln with 5 projects (AppHost, ServiceDefaults, Api, Web, Tests)

#### Test Configuration
- **Framework**: xUnit 2.9.3
- **Supporting Libraries**: Moq 4.20.70, FluentAssertions 8.8.0, Microsoft.AspNetCore.Mvc.Testing
- **Coverage Tool**: Coverlet 6.0.4 (XPlat format)
- **Test Organization**: 
  - Unit tests: 77 passing
  - Integration tests: 30 tests (currently blocked pending Aspire infrastructure fixes per team decisions)
  - Skip approach: Filter by namespace `DogTeams.Tests.Integration` using `--filter "FullyQualifiedName!~DogTeams.Tests.Integration"`

#### Pipeline Strategy
- **Triggers**: Push to [main, dev, insider] + PRs to [main, dev, preview, insider]
- **Job Name**: "Build and Test (.NET 10)" (used as status check context)
- **Steps**: Checkout → Setup .NET 10 → Restore → Build (Release) → Test (Unit) → Upload artifacts → Publish results
- **Test Exclusion Rationale**: Integration tests currently fail due to AppHost startup issues (blocker chain: PR #22 → #23 → #20 per decisions.md)

#### Branch Protection Rules
- Status check: "Build and Test (.NET 10)" must pass
- Requires 1 approval on PRs (non-admin)
- Dismiss stale reviews on new commits
- Prevent force pushes and direct deletions

### Team Dependencies Met
- ✅ Blocked integration tests pending Bobbie's test infrastructure fix (issue #20)
- ✅ Respected Amos's package fix requirements (PR #22)
- ✅ Respected Naomi's React startup fix (PR #23)
- ✅ Did NOT modify application code (per Grendel boundaries)
- ✅ Did NOT make architectural decisions (per Grendel boundaries)

---

*Append-only log. Do NOT edit existing entries.*
