# Dog Teams — Sprint Plan

## Current State Summary
- **Solution:** Builds clean (0 errors)
- **5 Projects:** AppHost, ServiceDefaults, Api, Web, Tests
- **Domain Models:** Team, Owner, Dog + DTOs exist ✓
- **Controllers:** Stubbed (return empty/404) — CosmosDB TODO
- **Frontend:** React/Vite scaffolded in ClientApp, routes defined, AuthContext in place
- **Web Project:** Minimal "Hello World" — doesn't serve React app yet
- **Data Layer:** No repositories, no CosmosDbContext
- **Auth:** No JWT/Identity configured in Program.cs
- **Caching:** No Redis service wired
- **Tests:** Infrastructure exists; integration tests skipped pending implementation
- **Templates:** WeatherForecast files (unused) should be deleted

---

## Sprint 1: Foundation — Data & Auth Services
**Goal:** Implement Cosmos DB repositories, JWT auth, and Redis caching to enable API testing.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| 1.1 | Implement CosmosDbContext & partition strategy | Backend | Create CosmosDbContext with `dog-teams` container (`/teamId` partition), `identity` container (`/userId` partition). Wire in Program.cs. |
| 1.2 | Implement Team, Owner, Dog repositories | Backend | Create ITeamRepository, TeamRepository, IOwnerRepository, OwnerRepository, IDogRepository, DogRepository with full CRUD. |
| 1.3 | Wire JWT auth + Identity in Program.cs | Backend | Add ASP.NET Core Identity, configure JWT bearer auth, add JwtTokenService, wire Cosmos user store. Configure auth middleware. |
| 1.4 | Implement Redis refresh token storage | Backend | Create RefreshTokenService to store/validate refresh tokens in Redis with 7-day expiry. Wire in Program.cs. |
| 1.5 | Remove WeatherForecast template files | Backend | Delete WeatherForecast.cs, WeatherForecastController.cs from Api. |
| 1.6 | Write data layer unit tests | Tester | Domain entity construction, repository null checks, configuration validation. Unblock integration tests. |

**Sprint Dependencies:** None — foundational work.  
**Exit Criteria:** 
- CosmosDbContext runs against local emulator ✓
- All repositories CRUD-testable ✓
- JWT flow (register → login → token → refresh) works ✓
- Unit tests pass ✓

---

## Sprint 2: API Implementation — Controllers & Endpoints
**Goal:** Implement all endpoint logic (Team, Owner, Dog, Auth CRUD) backed by repositories. Enable integration tests.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| 2.1 | Implement TeamsController endpoints | Backend | GetAll (query by userId), GetById, Create, Update, Delete. Add authorization (owner-scoped). |
| 2.2 | Implement OwnersController endpoints | Backend | GetAll (team-scoped), GetById, Create, Update, Delete. Enforce team association. |
| 2.3 | Implement DogsController endpoints | Backend | GetAll (owner-scoped), GetById, Create, Update, Delete. Denormalize teamId on Dog documents. |
| 2.4 | Implement AuthController (register, login, refresh) | Backend | POST /auth/register, /auth/login, /auth/refresh. Return JWT tokens. Validate credentials. |
| 2.5 | Add caching layer to hot reads (teams, dogs) | Backend | Decorate repository methods with Redis caching (5–10 min TTL). Invalidate on write. |
| 2.6 | Write API integration tests — remove [Skip] | Tester | Full HTTP request cycles for all endpoints. Assert status codes, response shapes, authorization boundaries. Target: 80%+ coverage. |

**Sprint Dependencies:** Sprint 1 (repositories, auth, caching required).  
**Exit Criteria:**
- All endpoints return correct HTTP status codes ✓
- Authorization enforced (401 on missing token, 403 on cross-team access) ✓
- Integration tests running (not skipped) ✓
- Postman/curl calls against live API work ✓

---

## Sprint 3: Frontend Integration — Pages & API Client
**Goal:** Connect React frontend to backend API. Implement login, team list, CRUD forms.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| 3.1 | Implement API client modules (teams, owners, dogs, auth) | Frontend | Build apiFetch wrapper with JWT token attachment, 401 handling, retry logic. Implement domain API modules (teams.ts, owners.ts, dogs.ts, auth.ts). |
| 3.2 | Implement LoginPage & RegisterPage | Frontend | Form validation, error display, redirect on success, persist JWT token to localStorage. |
| 3.3 | Implement DashboardPage (team list & create team) | Frontend | List user's teams, fetch via API. Add "Create Team" button → modal/form → POST /teams. |
| 3.4 | Implement TeamPage (owners, dogs, CRUD forms) | Frontend | Show team details, list owners/dogs, inline edit forms, delete buttons. Link owners → dogs lists. |
| 3.5 | Implement AuthContext hooks (useAuth, useTeam, useOwner, useDog) | Frontend | Custom hooks for token management, team/owner/dog CRUD. Encapsulate API calls. |
| 3.6 | Write e2e smoke tests (auth flow) | Tester | Cypress or Playwright: login → see teams → create team → navigate to team page. |

**Sprint Dependencies:** Sprint 2 (working API required).  
**Exit Criteria:**
- Login/register flow end-to-end ✓
- Team list renders with data from API ✓
- Create team → appears in list ✓
- Owner & dog CRUD forms functional ✓
- Token persists across page reload ✓

---

## Sprint 4: Web Project Wiring & Deployment
**Goal:** Serve React app from DogTeams.Web project. Set up env-based configuration. Prepare for containerization.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| 4.1 | Wire React app serving in DogTeams.Web Program.cs | Backend | Use app.MapStaticAssets() + SPA fallback routing. OR: Map /api proxy to Api project, serve static from ClientApp dist. |
| 4.2 | Set up environment-based API URL configuration | Frontend | Add VITE_API_URL env var (dev: localhost:5001, prod: from AppSettings). Update apiFetch to use it. |
| 4.3 | Configure Aspire Web project to serve React in dev | Aspire | Test AppHost: npm dev + .NET run together. Verify hot reload works. |
| 4.4 | Build & minify React app for production | Frontend | npm run build outputs to DogTeams.Web wwwroot. CI pipeline runs this. |
| 4.5 | Add Dockerfile for Api & Web projects | Aspire | Multi-stage build: restore, build, runtime stages. Validate against local Cosmos/Redis. |
| 4.6 | Write deployment docs (local, staging, prod) | Tester/Docs | README: how to run locally, env vars, database setup, testing checklist. |

**Sprint Dependencies:** Sprint 3 (frontend complete).  
**Exit Criteria:**
- `dotnet run` from AppHost serves both API and React UI ✓
- Prod build (npm run build) outputs static files ✓
- Dockerfile builds & runs locally ✓
- E2E flow works via single endpoint (no CORS needed) ✓

---

## Sprint 5: Polish, Performance & Observability
**Goal:** Error handling, logging, monitoring, performance optimization.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| 5.1 | Implement error handling middleware (Api) | Backend | Centralized exception → problem detail responses (RFC 7807). Log all errors. |
| 5.2 | Add structured logging (Serilog) | Backend | Log all requests, errors, Cosmos operations. Configure for local + cloud output. |
| 5.3 | Implement resilience policies (Polly) | Backend | Retry logic for Cosmos transients, circuit breaker for Redis. |
| 5.4 | Add OpenTelemetry metrics & traces | Aspire | Wire ActivitySource in Api, auto-instrument Http/Redis. Validate in local dashboard. |
| 5.5 | Add error boundaries + toast notifications (Frontend) | Frontend | Catch React errors, show friendly messages. Toast for validation errors, successes. |
| 5.6 | Performance: optimize bundle size, lazy load routes | Frontend | Code splitting by feature. Analyze bundle with rollup-plugin-visualizer. Target <200KB gzip. |

**Sprint Dependencies:** Sprint 4 (deployment pipeline available).  
**Exit Criteria:**
- All exceptions caught, logged, returned as JSON ✓
- OpenTelemetry traces visible in diagnostic UI ✓
- Frontend gracefully handles API errors ✓
- Bundle size <200KB gzip ✓

---

## Sprint 6: Security & Testing Hardening
**Goal:** Authentication/authorization audit, security best practices, comprehensive test coverage.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| 6.1 | Audit authorization policy (team-scoped, owner-scoped) | Backend | Ensure all endpoints validate ownership/team membership. Test cross-team access denied. |
| 6.2 | Implement rate limiting (Api) | Backend | Per-IP rate limit for auth endpoints. Add middleware. |
| 6.3 | Add CSRF protection (if using cookies) | Backend | If session cookies: add CSRF middleware. (JWT bypasses this, but document.) |
| 6.4 | Write integration test for security scenarios | Tester | 401/403 on missing token, invalid token, cross-team access. SQL injection / XSS injection tests. |
| 6.5 | Implement HTTPS redirect + security headers | Backend | Add X-Frame-Options, X-Content-Type-Options, X-XSS-Protection headers. Enforce HTTPS in prod. |
| 6.6 | Add secrets management for Cosmos/Redis connection strings | Aspire | Use Azure Key Vault in prod, local.settings.json in dev. Document secret rotation. |

**Sprint Dependencies:** Sprint 5 (monitoring in place).  
**Exit Criteria:**
- All auth endpoints test 401/403 boundaries ✓
- No sensitive data in logs ✓
- Rate limiting works ✓
- Security headers present in all responses ✓

---

## Sprint 7: Documentation & Knowledge Transfer
**Goal:** Complete runbooks, API docs, frontend architecture guide.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| 7.1 | Write API OpenAPI spec (Swagger) | Backend | Auto-generate from controllers, add descriptions, examples. Publish via /swagger-ui. |
| 7.2 | Write frontend component architecture guide | Frontend | Document folder structure, hooks pattern, AuthContext usage, state management. |
| 7.3 | Write Cosmos DB data model guide | Backend | Explain partition strategy, document types, query patterns, indexing. |
| 7.4 | Write local development setup guide (README) | Aspire | Prerequisites (Docker, .NET 10, Node), steps to run AppHost, troubleshooting. |
| 7.5 | Write deployment & CI/CD runbook | Aspire | GitHub Actions workflow for build, test, push image, deploy to staging/prod. |
| 7.6 | Create decision log for future maintainers | Backend | Archive decisions.md, explain why patterns chosen (Cosmos partition key, JWT refresh tokens, React context). |

**Sprint Dependencies:** None — can run in parallel with Sprint 6.  
**Exit Criteria:**
- README sufficient for new dev to get running in <30 min ✓
- API docs auto-generated, no manual duplication ✓
- Architecture decisions documented ✓

---

## Backlog / Future

### High Priority
- **Invite/join mechanism:** Allow owners to invite other users to team (email, link sharing)
- **Dog transfer between owners:** Owner A → Owner B reassignment
- **Team deletion:** Cascade delete all owners/dogs
- **Soft delete support:** Archive teams/owners instead of hard delete
- **Audit logging:** Track who created/modified each entity, timestamps
- **Search:** Full-text search for teams, owners, dogs by name/description
- **Pagination:** Add skip/take to list endpoints; frontend infinite scroll

### Medium Priority
- **Bulk operations:** Import/export CSV of dogs, teams
- **File uploads:** Dog photos, owner documents (blob storage)
- **Notifications:** Email on invite, ownership transfer
- **Mobile app:** React Native version of web client
- **GraphQL API:** Alternative to REST (optional layer)
- **Tenancy isolation:** Multi-tenant support (roadmap)

### Low Priority (Post-MVP)
- **AI features:** Dog breed classifier, health recommendations
- **Integrations:** Slack bot, Calendar sync for vet appointments
- **Analytics:** Usage dashboards, team metrics
- **Accessibility:** WCAG AA compliance audit
- **Internationalization:** i18n for multiple languages
- **Custom branding:** White-label for partners

---

## Critical Path Summary

```
Sprint 1 (Data/Auth/Cache)
    ↓
Sprint 2 (API Endpoints)
    ↓
Sprint 3 (Frontend Pages)
    ↓
Sprint 4 (Deployment)
    ↓
Sprints 5-7 (Polish/Docs)
```

**Minimum Viable Product (MVP):** Complete Sprints 1–4
- Functional CRUD UI backed by CosmosDB ✓
- JWT authentication ✓
- Deployed & runnable ✓

**Production-Ready:** Complete Sprints 1–6
- Security audit ✓
- Monitoring/logging ✓
- Comprehensive tests ✓

**Mature Product:** All sprints + backlog items
- Rich features (invites, transfers, search) ✓
- Multi-tenant ready ✓
- Mobile support ✓

---

## Agent Assignments

- **Backend:** Aspire/C# development (Cosmos, auth, caching, API)
- **Frontend:** React/TypeScript development (pages, components, hooks, styling)
- **Aspire:** AppHost, project wiring, containerization, deployment config
- **Tester:** Unit/integration/E2E test writing, security testing, performance validation

