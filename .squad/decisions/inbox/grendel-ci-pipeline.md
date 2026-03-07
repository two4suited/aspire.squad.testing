# Decision: GitHub Actions CI Pipeline for aspire.squad.testing

**Date**: 2026-03-07  
**Owner**: Grendel (DevOps)  
**Status**: Implemented  

## Context

The aspire.squad.testing repository needed a GitHub Actions CI pipeline to:
- Build .NET solution on every push and PR
- Run unit tests with quality gates
- Protect main branch with required checks
- Provide clear feedback to developers on test failures

## Decision

Implemented a two-workflow CI system:

### 1. **Build and Test Workflow** (squad-ci.yml)
- **Trigger**: Push to [main, dev, insider] + PRs to [main, dev, preview, insider]
- **Job Name**: "Build and Test (.NET 10)"
- **Steps**:
  1. Checkout code
  2. Setup .NET 10.0.x via actions/setup-dotnet@v4
  3. Restore dependencies with `dotnet restore`
  4. Build solution (Release config) with `dotnet build`
  5. Run unit tests with xUnit (integration tests filtered out)
  6. Collect code coverage via Coverlet
  7. Upload test results as artifacts
  8. Publish results to PR checks using EnricoMi/publish-unit-test-result-action@v2

- **Integration Test Strategy**: Excluded by namespace filter `--filter "FullyQualifiedName!~DogTeams.Tests.Integration"`
  - **Rationale**: Integration tests fail due to AppHost/Cosmos DB infrastructure not available in CI (blocker chain per decisions.md: PR #22 → #23 → #20)
  - **Future**: Will re-enable after infrastructure issues resolved

### 2. **Branch Protection Workflow** (branch-protection.yml)
- Configures GitHub branch rules for `main` via GitHub CLI
- Requires status check: "Build and Test (.NET 10)"
- Requires 1 approval on PRs
- Prevents force pushes and deletions
- Note: Requires elevated repository permissions to run

## Rationale

- **xUnit as test framework**: Already configured in DogTeams.Tests.csproj; 77 unit tests passing
- **Namespace filtering**: More reliable than trait-based filtering for Aspire integration tests
- **Release build**: Ensures production-like builds are tested
- **Code coverage collection**: Supports future quality gates and reporting
- **Two-workflow approach**: Separates CI logic from branch protection config (which may require manual setup depending on GitHub org permissions)

## Trade-offs

| Choice | Alternative | Why Selected |
|--------|-------------|-------------|
| Skip integration tests in CI | Run all tests, allow failures | Integration tests require AppHost running; blocker chain in decisions.md prevents fixing now |
| namespace filter | xUnit traits | Simpler to implement; tests already organized in folders |
| Release build | Debug build | Catches compiler optimizations; matches production runtime |

## Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| Integration tests always skipped in CI | Document rationale in history.md; add issue link in PR comment |
| Branch protection workflow may fail (permissions) | Documented in workflow; requires GitHub org admin to manually set branch rules if workflow fails |
| Test result publishing fails silently | `if: always()` ensures artifact upload always runs; fallback to viewing logs if publishing fails |

## Success Criteria (Met)

- ✅ Pipeline runs on every push/PR
- ✅ All unit tests gated (77/77 passing)
- ✅ Build in Release configuration
- ✅ Merge blocked until CI passes (via status check requirement)
- ✅ Clear failure messages via test result publishing
- ✅ Did not modify application code
- ✅ Respected team dependencies (integration test blocker chain)

## Future Enhancements

1. **Re-enable integration tests** after PR #22, #23, and #20 are merged
2. **Add frontend tests** (React/Vitest) when JavaScript test infrastructure is ready
3. **Add E2E tests** (Playwright) after API reliability established
4. **Configure code coverage thresholds** once baseline established
5. **Add artifact retention policies** for long-term test history

## References

- Team Decisions: /Users/brian/Source/aspire.squad.testing/.squad/decisions.md
- Bobbie's Test Analysis: Issue #20 (E2E Test Failure Analysis)
- Amos's Package Fix: PR #22 (Aspire.Hosting.NodeJs → Aspire.Hosting.JavaScript)
- Naomi's Startup Fix: PR #23 (React dev server startup)
