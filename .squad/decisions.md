# Squad Decisions

## 2026-03-07 Initial Scaffold Session

### 2026-03-07T04:33:00Z: Solution Architecture Defined
**By:** Lead  
**Status:** Approved  

Complete architecture for Dog Teams app:

**Solution Structure**
- Five projects: DogTeams.AppHost, DogTeams.Api, DogTeams.Web (React), DogTeams.ServiceDefaults, DogTeams.Tests
- Standard Aspire conventions

**Cosmos DB Data Model**
- Single `dog-teams` container with `/teamId` partition key
- Separate `identity` container with `/userId` partition key
- Team, Owner, Dog stored as separate documents with `type` discriminator
- Dogs NOT embedded in Owner documents — independent CRUD lifecycle

**Authentication**
- ASP.NET Core Identity with custom Cosmos DB user store
- JWT bearer tokens: 15-min access tokens, 7-day refresh tokens stored in Redis

**API Design**
- Minimal APIs, team-scoped REST endpoints (`/api/teams/{teamId}/...`)
- URL structure mirrors partition key for natural authorization enforcement

**Caching & Sessions**
- Redis: Read-through caching (5-10 min TTLs) + refresh token storage
- No server-side sessions

**Aspire Wiring**
- Cosmos DB emulator, Redis container, API project, React app via AddNpmApp
- Service discovery provides API URL to frontend

**Frontend**
- React 19 + TypeScript + Vite
- Auth via Context + useAuth hook
- Feature-based folder structure

**Testing**
- xUnit + DistributedApplicationTestingBuilder
- Tests run against real Cosmos emulator and Redis container (no mocks)

**Open Questions**
- Team creation flow (auto on register vs. separate step)
- Invite/join mechanism for owners
- Dog transfer between owners

---

### 2026-03-07T04:33:15Z: Stack Version Directives
**By:** Brian Sheridan (via Copilot)  
**Status:** Active  

Team stack versions:
- Use .NET 10
- Use .NET Aspire 9.1 (SDK version 13.1.x)
- Aspire.Hosting.Azure.CosmosDB may use preview versions

---

### 2026-03-07T04:33:30Z: Cosmos SDK Version Clarification
**By:** Brian Sheridan (via Copilot)  
**Status:** Active  

- `Aspire.Hosting.Azure.CosmosDB` (hosting/AppHost package) can use prerelease
- `Microsoft.Azure.Cosmos` client SDK uses current STABLE release (3.58.0)
- Rationale: Preview flag applies to Aspire hosting integration only, not client SDK

---

### 2026-03-07T04:33:45Z: Project Domain Defined
**By:** Brian Sheridan (via Copilot)  
**Status:** Active  

Core domain entities: **Teams, Owners, Dogs**
- A Team has Owners and Dogs
- Establishes data model and feature boundaries

---

### 2026-03-07T04:34:00Z: Domain Relationships
**By:** Brian Sheridan (via Copilot)  
**Status:** Active  

Foundational entity relationships:
- Owner belongs to ONE Team only (no cross-team owners)
- Dog belongs to ONE Team, via their Owner
- Authentication maps to ownership — logged-in user IS an Owner and manages their Team
- Relationship chain: User (auth) → Owner → Team ← Dogs

Drives:
- Cosmos DB partition key design
- API authorization model
- UI routing

---

### 2026-03-07T04:34:00Z: Owner → Dogs One-to-Many
**By:** Brian Sheridan (via Copilot)  
**Status:** Active  

- An Owner can have multiple Dogs
- A Dog belongs to exactly one Owner
- Full model chain: User(auth) → Owner → Team; Owner → [Dog, Dog, Dog...]

---

### 2026-03-07T04:34:15Z: DogTeams Solution Scaffolded
**By:** Aspire Agent  
**Status:** Complete  

**Solution Structure**
```
src/
  DogTeams.sln
  DogTeams.AppHost/
  DogTeams.ServiceDefaults/
  DogTeams.Api/
  DogTeams.Web/
  DogTeams.Tests/
```

**Aspire Versions**
- SDK: 13.1.2 (via template; latest stable)
- Corresponds to .NET Aspire 9.1

**Key Packages**
| Package | Version |
|---------|---------|
| Aspire.Hosting.Azure.CosmosDB | 13.1.2 |
| Aspire.Hosting.Redis | 13.1.2 |
| Aspire.Microsoft.Azure.Cosmos | 13.1.2 |
| Aspire.StackExchange.Redis | 13.1.2 |
| Aspire.Hosting.Testing | 13.1.2 |
| Microsoft.Azure.Cosmos | 3.58.0 (stable) |

**AppHost Wiring**
- `cosmos`: AddAzureCosmosDB("cosmos").RunAsEmulator()
- `redis`: AddRedis("redis") — Docker container
- `api`: References cosmos + redis with external HTTP endpoints
- `web`: References api with external HTTP endpoints
- Both respect startup order via WaitFor()

**Solution Status**: Builds with 0 errors

---

### 2026-03-07T04:34:30Z: Domain Models & Cosmos Container Strategy
**By:** Backend Agent  
**Status:** In Progress  

**Models**
- Team — top-level aggregate
- Owner — belongs to one Team
- Dog — belongs to one Owner with teamId denormalized

**Partition Key Strategy**
| Container | Partition Key | Rationale |
|-----------|---------------|-----------|
| Teams | `/id` | Top-level, low cardinality |
| Owners | `/teamId` | Team-scoped, co-located queries |
| Dogs | `/teamId` | Denormalized, efficient team-wide queries |

**API Routes**
- `/teams` — team CRUD
- `/teams/{teamId}/owners` — owner CRUD scoped to team
- `/owners/{ownerId}/dogs` — dog CRUD scoped to owner

**Authorization**
- Owner.UserId maps to authenticated user identity claim
- Policy: Owner can only manage their own dogs

**Status**: Models and stub controllers created; repositories pending implementation

---

### 2026-03-07T04:34:45Z: Frontend Scaffold Complete
**By:** Frontend Agent  
**Status:** Complete  

**Structure**
```
src/DogTeams.Web/ClientApp/
  api/              # apiFetch base + teams, owners, dogs, auth modules
  components/       # auth/, layout/, teams/, owners/, dogs/ (stubs)
  contexts/         # AuthContext.tsx
  hooks/            # useAuth, useTeam, useOwner, useDog
  pages/            # LoginPage, RegisterPage, DashboardPage, TeamPage
  types/            # Team, Owner, Dog, AuthUser interfaces
```

**Auth Approach**
- JWT stored in localStorage (`dogteams_token`, `dogteams_user`)
- AuthProvider wraps app; exposes login, register, logout
- RequireAuth component redirects unauthenticated requests to /login
- 401 responses trigger logout + redirect

**API Client Pattern**
- `apiFetch<T>(path, init)` attaches Bearer token, handles 401
- Base URL defaults to relative '' (Vite proxy) or `VITE_API_URL` override
- Vite dev server proxies `/api/*` to Aspire-injected backend URL
- Domain modules build on apiFetch

**Why**: Relative-path + Vite proxy keeps dev/prod config symmetrical; localStorage JWT consistent with .NET JWT bearer backend

---

### 2026-03-07T04:35:00Z: Test Infrastructure Setup
**By:** Tester Agent  
**Status:** Complete  

**Test Strategy**
- **Unit tests** (Unit/): Domain model construction, defaults. Fast, no external deps.
- **Integration tests** (Integration/): AppHostFixture (IAsyncLifetime) spins up full Aspire host with Cosmos emulator and Redis. Exercises HTTP endpoints via live HttpClient.

**Packages**
| Package | Version | Purpose |
|---------|---------|---------|
| Aspire.Hosting.Testing | 13.1.2 | Aspire test host |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.3 | ASP.NET testing utilities |
| FluentAssertions | 8.8.0 | Assertion syntax |

**Status**
- Unit tests: **passing**
- Integration tests: **skipped** (blockers listed below)

**Blockers for Integration Tests**
1. TeamsController — stubs need real Cosmos implementation
2. Auth controller — POST /auth/register and /auth/login not implemented
3. Authorization middleware — JWT bearer auth not configured in Program.cs
4. CosmosDB emulator — requires Docker availability

**Implementation Guidance**: Remove Skip parameter file-by-file as each API surface is implemented and verified.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
