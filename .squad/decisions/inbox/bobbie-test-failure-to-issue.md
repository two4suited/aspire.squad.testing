# Bobbie Integration Guide: E2E Test Failures → GitHub Issues

**Date:** 2026-03-07  
**Status:** Ready for implementation  
**Owner:** Bobbie

## Overview

This guide instructs Bobbie how to automatically create GitHub issues when E2E test failures occur. The workflow uses `.squad/scripts/create-issue-from-test-failure.sh` to capture failures in the project backlog immediately after they're detected.

## Quick Start

After running E2E tests, for each failure:

```bash
./.squad/scripts/create-issue-from-test-failure.sh \
  --test-name "Test Name" \
  --root-cause "database|api|frontend|framework" \
  --failure-logs "failure output" \
  --responsible-agent "amos|naomi|bobbie"
```

## Failure Root Cause Mapping

Use this table to categorize failures and route to the right team member:

| Root Cause | Category | Responsible Agent | Example |
|-----------|----------|------------------|---------|
| `database` | Cosmos DB schema, collections, queries | amos | "Collection 'Teams' not found in database" |
| `api` | HTTP 500, validation, business logic | amos | "500 Internal Server Error on registration" |
| `frontend` | React components, rendering, UI state | naomi | "Button click not triggering form submit" |
| `framework` | Playwright, test infra, async timing | bobbie | "Timeout waiting for element" |
| `infrastructure` | Aspire orchestration, services, ports | amos | "Service discovery failure" |

## Step-by-Step Process

### 1. **Run E2E Tests**
```bash
npm run test:e2e
```

### 2. **Parse Failure Output**
For each failure, extract:
- Test name (from test file/describe block)
- Error type (what failed)
- Failure logs (last 5-10 lines of output)

### 3. **Determine Root Cause**
Analyze the error:
- Does it mention "Collection" or "database"? → `database`
- Does it mention "HTTP 500" or API routes? → `api`
- Does it mention component rendering, DOM, or selectors? → `frontend`
- Does it mention timeouts, promises, or async? → `framework`
- Does it mention ports, services, or Aspire? → `infrastructure`

### 4. **Identify Responsible Agent**
Based on root cause, assign to:
- API/Database/Infrastructure → `amos`
- Frontend/UI → `naomi`
- Test Framework/Playwright → `bobbie`

### 5. **Create GitHub Issue**
```bash
./.squad/scripts/create-issue-from-test-failure.sh \
  --test-name "should register user with valid email" \
  --root-cause "api" \
  --failure-logs "500 Internal Server Error" \
  --responsible-agent "amos"
```

### 6. **Log in Session Notes**
Add to Bobbie's session history:
```
**Test Run [DATE]**
- Failure: [Test Name] → Issue #[issue_number]
- Root Cause: [category]
- Status: Created
```

## Example Issue Creations

### Example 1: Database Collection Missing
```bash
./.squad/scripts/create-issue-from-test-failure.sh \
  --test-name "should list teams on dashboard" \
  --root-cause "database" \
  --failure-logs "Collection 'Teams' not found in database 'DogTeamsDb'" \
  --responsible-agent "amos"
```
**Result:** Issue created, assigned to @amos, labeled `squad:amos`, `bug`, `testing`, `e2e`

### Example 2: Frontend Rendering Issue
```bash
./.squad/scripts/create-issue-from-test-failure.sh \
  --test-name "should display error message on form failure" \
  --root-cause "frontend" \
  --failure-logs "Timeout waiting for '.error-message' element" \
  --responsible-agent "naomi"
```
**Result:** Issue created, assigned to @naomi, labeled `squad:naomi`, `bug`, `testing`, `e2e`

### Example 3: Test Framework Timeout
```bash
./.squad/scripts/create-issue-from-test-failure.sh \
  --test-name "should handle auth flow within timeout" \
  --root-cause "framework" \
  --failure-logs "Test timeout after 30 seconds" \
  --responsible-agent "bobbie"
```
**Result:** Issue created, assigned to @bobbie, labeled `squad:bobbie`, `bug`, `testing`, `e2e`

## Dry-Run Mode (Testing)

To test the script without creating issues:

```bash
./.squad/scripts/create-issue-from-test-failure.sh \
  --test-name "sample test" \
  --root-cause "api" \
  --failure-logs "sample error" \
  --responsible-agent "amos" \
  --dry-run
```

**Output:** Shows what issue would be created without executing `gh issue create`

## Issue Labels & Assignees

### Labels Applied Automatically
- `bug` — Defect found in E2E testing
- `testing` — Related to testing infrastructure
- `e2e` — End-to-end test scope
- `squad:[agent]` — Responsible team member (e.g., `squad:amos`)

### Assignees (by agent)
- `amos` — Backend, API, database
- `naomi` — Frontend, React, UI
- `bobbie` — Test framework, Playwright

## Troubleshooting

### Script Fails: "gh: command not found"
**Solution:** Install GitHub CLI: https://cli.github.com/

### Script Fails: "Not authorized"
**Solution:** Authenticate with `gh auth login`

### Issues Not Appearing
**Solution:** Verify you're in the correct repository directory and have push access

### Agent Name Not Recognized
**Solution:** Use only `amos`, `naomi`, or `bobbie`

## Notes for Bobbie

1. **One issue per unique failure** — If the same failure occurs twice (e.g., flaky test), link as duplicate instead of creating a new issue.

2. **Include reproduction steps** — The failure logs should contain enough context for the assigned agent to reproduce the issue.

3. **Link to decision log** — After creating an issue, add the issue number to `.squad/decisions.md` with a reference.

4. **Monitor issue status** — Check if issues are being addressed. If an issue isn't resolved after 24 hours, escalate to Ralph.

---

**Related Documentation:**
- `.squad/decisions/inbox/auto-create-issues-from-e2e-failures.md` — Decision rationale
- `.squad/scripts/create-issue-from-test-failure.sh` — Implementation script
- `.squad/agents/bobbie/history.md` — Bobbie's session log
