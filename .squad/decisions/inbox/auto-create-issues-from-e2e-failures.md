# Decision: Auto-Create GitHub Issues from E2E Test Failures

**Date:** 2026-03-07  
**User Directive:** "I am not really seeing issues get created when bugs are hit on e2e testing those should be created in github issues"

## Problem

E2E test failures are being logged and reported to the team, but **no GitHub issues are being created**. This means:
- Failures aren't visible in the project backlog
- Cannot assign failures to team members via GitHub labels
- Difficult to track which failures have been addressed
- Work is tracked in decisions/inbox but not in the canonical issue tracker

## Solution

Establish automated workflow where:

1. **Bobbie runs E2E tests** → Reports results
2. **For each test failure:**
   - Parse failure root cause (database, API, frontend, test framework)
   - Determine severity and category
   - Create GitHub issue with:
     - Title: `E2E Test Failure: [Test Name]`
     - Body: Root cause analysis, failure logs, reproducible steps
     - Labels: `bug`, `testing`, `e2e`, `squad:[responsible-agent]` (if known)
     - Assignee: Based on root cause (squad:amos for API, squad:naomi for frontend, squad:bobbie for test framework)

3. **Link decision to GitHub issue** in `.squad/decisions.md`

## Implementation Notes

- Use `gh issue create` CLI (already available)
- Bobbie creates issues with detailed failure analysis
- If multiple tests fail for same root cause, create one issue (link as duplicates)
- Scribe updates `.squad/decisions.md` to reference issue numbers
- Ralph can then process backlog via GitHub API

## Acceptance Criteria

- ✅ E2E test failure → GitHub issue created within 1 minute
- ✅ Issue includes root cause analysis
- ✅ Issue labeled with responsible team member
- ✅ Decision log references issue URL

---
**Status:** Ready for implementation  
**Owner:** Bobbie (test execution + issue creation)
