# Decision: Playwright API Fix (Issue #30)

**Date:** 2026-03-07  
**Owner:** Naomi (Frontend)  
**Status:** Complete

## Summary

Fixed systemic Playwright API misuse in E2E test suite. All `expect(page).toContainText()` calls were invalid; Playwright matchers only work with Locator objects.

## Discovery

- Issue #30 reported line 52 had `await expect(page).toContainText('Failed')`
- Audit revealed 20 total instances across entire test file
- Root cause: Test file written without Playwright API matchers correctly understood

## Solution

Replaced all instances with `await expect(page.locator('body')).toContainText(text)`, maintaining test intent while using valid API.

## Technical Details

### Invalid Pattern
```typescript
await expect(page).toContainText('Text');  // ❌ page object not supported
```

### Valid Pattern
```typescript
await expect(page.locator('body')).toContainText('Text');  // ✅ Locator required
```

### Why This Matters
- Playwright matchers (toContainText, toHaveAttribute, etc.) require Locator objects
- Page objects are for navigation (goto, click, fill) and page-level methods
- Using page.locator() creates a Locator for assertion chain

## Changes
- 20 toContainText replacements
- 22 lines modified in app.spec.ts
- TypeScript validation: ✅ Passed

## Team Knowledge
- Frontend developers should understand Playwright Locator vs Page object distinction
- E2E tests should use page.locator() for all element assertions
- Consider adding ESLint rule or pre-commit hook to catch this pattern
