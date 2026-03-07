### 2026-03-07T17:00:31Z: User directive on Aspire integration approach

**By:** Brian Sheridan (via Copilot)

**What:** For React+Aspire integration issues (like the port 5000 conflict), consult aspire.dev documentation and current official examples as the source of truth before trying custom solutions.

**Why:** Aspire has official patterns and best practices for running React/Vite with Aspire orchestration. Using these patterns ensures consistency and avoids reinventing wheels or hitting undocumented edge cases. This is especially important for environment variable injection and service discovery configuration.

**Applies to:** Issue #29 (port 5000 conflict) and any future Aspire/frontend integration work.
