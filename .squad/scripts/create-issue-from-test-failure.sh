#!/bin/bash
# create-issue-from-test-failure.sh
# 
# Helper script to create GitHub issues from E2E test failures.
# Automated by Bobbie to capture test failures in the project backlog.
#
# Usage:
#   ./create-issue-from-test-failure.sh \
#     --test-name "Test Name" \
#     --root-cause "database|api|frontend|framework" \
#     --failure-logs "failure log output" \
#     --responsible-agent "amos|naomi|bobbie"
#
# Example:
#   ./create-issue-from-test-failure.sh \
#     --test-name "should create team with valid data" \
#     --root-cause "api" \
#     --failure-logs "500 Internal Server Error" \
#     --responsible-agent "amos"

set -euo pipefail

# Parse arguments
TEST_NAME=""
ROOT_CAUSE=""
FAILURE_LOGS=""
RESPONSIBLE_AGENT=""
DRY_RUN=false

while [[ $# -gt 0 ]]; do
  case $1 in
    --test-name)
      TEST_NAME="$2"
      shift 2
      ;;
    --root-cause)
      ROOT_CAUSE="$2"
      shift 2
      ;;
    --failure-logs)
      FAILURE_LOGS="$2"
      shift 2
      ;;
    --responsible-agent)
      RESPONSIBLE_AGENT="$2"
      shift 2
      ;;
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    *)
      echo "Unknown option: $1"
      echo "Usage: $0 --test-name NAME --root-cause CAUSE --failure-logs LOGS --responsible-agent AGENT [--dry-run]"
      exit 1
      ;;
  esac
done

# Validate required arguments
if [[ -z "$TEST_NAME" || -z "$ROOT_CAUSE" || -z "$FAILURE_LOGS" || -z "$RESPONSIBLE_AGENT" ]]; then
  echo "Error: All required arguments must be provided"
  echo "Usage: $0 --test-name NAME --root-cause CAUSE --failure-logs LOGS --responsible-agent AGENT [--dry-run]"
  exit 1
fi

# Map responsible agent to assignee
case "$RESPONSIBLE_AGENT" in
  amos)
    ASSIGNEE="amos"
    SQUAD_LABEL="squad:amos"
    ;;
  naomi)
    ASSIGNEE="naomi"
    SQUAD_LABEL="squad:naomi"
    ;;
  bobbie)
    ASSIGNEE="bobbie"
    SQUAD_LABEL="squad:bobbie"
    ;;
  *)
    echo "Error: Unknown agent '$RESPONSIBLE_AGENT'. Must be: amos, naomi, or bobbie"
    exit 1
    ;;
esac

# Create issue title
ISSUE_TITLE="E2E Test Failure: $TEST_NAME"

# Create issue body with root cause analysis
ISSUE_BODY=$(cat <<EOF
## Test Failure Report

**Test:** \`$TEST_NAME\`  
**Root Cause:** $ROOT_CAUSE  
**Responsible Agent:** squad:$RESPONSIBLE_AGENT

### Failure Logs

\`\`\`
$FAILURE_LOGS
\`\`\`

### Root Cause Analysis

- **Category:** $ROOT_CAUSE
- **Assigned to:** @$ASSIGNEE

### Next Steps

1. Investigate the root cause
2. Create a fix
3. Re-run E2E tests to verify
4. Close this issue once resolved

---
*Automatically created by Bobbie E2E test runner*
EOF
)

# Set labels
LABELS="bug,testing,e2e,$SQUAD_LABEL"

# Prepare gh command
GH_CMD="gh issue create"
GH_CMD="$GH_CMD --title \"$ISSUE_TITLE\""
GH_CMD="$GH_CMD --body \"$ISSUE_BODY\""
GH_CMD="$GH_CMD --label \"$LABELS\""
if [[ -n "$ASSIGNEE" ]]; then
  GH_CMD="$GH_CMD --assignee \"$ASSIGNEE\""
fi

# Execute or dry-run
if [[ "$DRY_RUN" == true ]]; then
  echo "=== DRY RUN MODE ==="
  echo "Would execute:"
  echo "$GH_CMD"
  echo ""
  echo "Issue Details:"
  echo "  Title: $ISSUE_TITLE"
  echo "  Root Cause: $ROOT_CAUSE"
  echo "  Labels: $LABELS"
  echo "  Assignee: $ASSIGNEE"
  echo ""
  echo "DRY RUN: No issues created"
  exit 0
else
  echo "Creating GitHub issue..."
  eval "$GH_CMD"
  echo "Issue created successfully"
fi
