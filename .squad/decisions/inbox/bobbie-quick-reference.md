# Quick Reference: Issue Creation from Test Failures

**For Bobbie's use during test runs**

## When a Test Fails

1. **Get test name** — from Playwright output
2. **Get error message** — last 2-3 lines of failure log
3. **Pick root cause** — use quick table below
4. **Run command** — copy & paste, fill in blanks

## Root Cause Quick Picker

```
If error mentions:        | Use root cause: | Agent:
--------------------------|-----------------|-------
"Collection...not found"  | database        | amos
"404" or "500" on /api    | api             | amos
Port, service, Aspire     | infrastructure  | amos
Selectors, rendering      | frontend        | naomi
Timeout, async, Promise   | framework       | bobbie
```

## Command Template

```bash
./.squad/scripts/create-issue-from-test-failure.sh \
  --test-name "[TEST_NAME]" \
  --root-cause "[database|api|frontend|framework|infrastructure]" \
  --failure-logs "[ERROR_MESSAGE]" \
  --responsible-agent "[amos|naomi|bobbie]"
```

## Real Example

Test fails: "should create team with valid data"
Error: "500 Internal Server Error on POST /api/teams"
Root cause: API issue → responsible agent: amos

```bash
./.squad/scripts/create-issue-from-test-failure.sh \
  --test-name "should create team with valid data" \
  --root-cause "api" \
  --failure-logs "500 Internal Server Error on POST /api/teams" \
  --responsible-agent "amos"
```

## What Gets Created

- **Issue Title:** `E2E Test Failure: should create team with valid data`
- **Labels:** `bug`, `testing`, `e2e`, `squad:amos`
- **Assignee:** @amos
- **Body:** Includes your failure logs + next steps

---
*Keep this handy during test runs!*
