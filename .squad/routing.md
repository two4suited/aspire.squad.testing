# Work Routing Rules

## By Domain

| Domain | Primary | Secondary | Notes |
|--------|---------|-----------|-------|
| Architecture & scope | Holden | — | All major decisions route to Lead for review |
| API endpoints, databases, services | Amos | Holden | Backend owns implementation; Lead reviews |
| React components, TypeScript, UI | Naomi | Holden | Frontend owns UI; Lead reviews large changes |
| Testing, QA, integration | Bobbie | Holden | Tester owns test strategy; Lead signs off |
| Aspire orchestration, DevOps | Drummer | Amos | Infrastructure specialist owns runtime health; Ask Amos for backend diagnostics |
| DevOps, infrastructure setup | Holden | Drummer | Architecture decisions with Drummer implementing |
| Docs, changelogs | Scribe | — | Scribe maintains via orchestration log |

## By Signal

| Signal | Router |
|--------|--------|
| "Holden, [task]" | Route to Holden |
| "Amos, [task]" | Route to Amos |
| Setup, validation, deployment | Holden (Lead) |
| Issues labeled `squad:*` | Assigned agent |
| Untriaged issues | Holden (triage) |

## Reviewer Gates

- **Holden** (Lead): approves architectural decisions, major code changes, deployment validation
- **Bobbie** (Tester): approves test coverage, integration tests before merge

---

*Routing rules are context-specific. Adjust as needed per session.*
