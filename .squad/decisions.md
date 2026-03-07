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

---

## 2026-03-07 Flyball Domain & Planning Decisions

### 2026-03-07T00:00:00Z: Use Linux-Based CosmosDB Emulator via RunAsPreviewEmulator()
**Date:** 2026-03-07  
**Status:** Approved  
**By:** Brian Sheridan (via Copilot / Aspire Expert)

Replace `.RunAsEmulator()` with `.RunAsPreviewEmulator()` in `DogTeams.AppHost/Program.cs` to explicitly use the Linux-based Azure Cosmos DB emulator.

**Context:**
- `Aspire.Hosting.Azure.CosmosDB` 13.1.2 exposes two emulator methods:
  - `RunAsEmulator()` → `mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest`
  - `RunAsPreviewEmulator()` → `mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview`
- Both already use Linux images in 13.1.2; `RunAsPreviewEmulator()` is the explicitly documented Linux-targeted method.
- `RunAsPreviewEmulator()` is in the **stable** 13.1.2 package but carries Aspire diagnostic `ASPIRECOSMOSDB001` (evaluation API); suppressed with `#pragma warning disable ASPIRECOSMOSDB001`.

**Implementation:**
```csharp
// DogTeams.AppHost/Program.cs
#pragma warning disable ASPIRECOSMOSDB001
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator();
#pragma warning restore ASPIRECOSMOSDB001
```

**Rationale:**
- Linux emulator is lighter weight and cross-platform (no Windows container dependency)
- `RunAsPreviewEmulator()` unlocks `WithDataExplorer()` for local development inspection
- Aligns with the project's cross-platform dev environment goals
- No package version change needed — 13.1.2 stable retained

**Constraints:**
- Do **not** change `Aspire.Hosting.Azure.CosmosDB` to a prerelease version; `RunAsPreviewEmulator()` is available in the stable 13.1.2 package
- The pragma suppression is required; placing it before the statement start is mandatory (the diagnostic is attributed to the expression root, not the method call site)

---

### 2026-03-07T00:00:00Z: Flyball Domain Design — 7 Architecture Decisions
**Date:** 2026-03-07  
**Status:** Approved  
**By:** Copilot Squad Lead  
**Requested by:** Brian

This document captures the key design decisions for adding NAFA flyball race/points tracking to the Dog Teams application. These decisions shape entity modeling, Cosmos DB partition strategy, and the points/title calculation architecture.

**Decision 1: Club is a New Top-Level Entity (Team is not renamed)**
- Add `Club` as a new top-level entity. The existing `Team` model maps to a NAFA **Racing Team** (the 4–6 dogs that race together). A Club has one or more Teams.
- Rejected: Rename `Team` → `Club` and introduce a new `RacingTeam` entity.
- Rationale: Renaming `Team` would break the existing partition key strategy (`/teamId`), require a Cosmos document migration, and invalidate all existing API routes (`/teams/...`).
- Implications: `Team` model gets `ClubId: string` and `Class: TeamClass (enum)` added. `Club` gets its own Cosmos container (`clubs`, `/clubId` partition key). Existing `/teams` endpoints unchanged. New `/clubs` endpoints added.

**Decision 2: CRN Lives on the Dog Model (Not a Separate Entity)**
- CRN (Competition Racing Number) is a property on the `Dog` document.
- Rejected: Separate `DogRegistration` entity with CRN, linking to Dog.
- Rationale: CRN is a 1:1 attribute of a dog — it is never transferred, never shared. A separate entity adds a join on every dog read with no query benefit. Cosmos doesn't support joins; denormalizing CRN to Dog document is the correct approach.
- Constraints: CRN uniqueness must be enforced at the API layer (check for existing CRN on Dog Create). CRN is not transferable — the API should reject any attempt to change CRN once set.

**Decision 3: Jump Height Measurements Are Embedded in the Dog Document**
- `JumpHeightMeasurements` is a `List<JumpHeightMeasurement>` embedded in the Dog document.
- Rationale: Measurements are always accessed with the dog (never queried independently). A dog has at most 2–3 measurements (one temporary, two matching OIRs for permanent status). Embedding is idiomatic Cosmos for small, owner-scoped sub-collections.

**Decision 4: Points Are Materialized on the Dog Document**
- `Dog.LifetimePoints` (and `Dog.LifetimeMultibreedPoints`) are **materialized integer fields** updated in-place each time a heat result is recorded.
- Rejected alternatives: (1) Compute on the fly by summing all HeatResult records on each read. (2) Background batch job that recalculates totals nightly.
- Rationale: Real-time sum aggregation across potentially thousands of heat records in Cosmos is expensive. Materializing on write is O(1) per read, O(1) per write (one document update). The materialized total is the authoritative value — auditable by comparing against heat history.
- Constraints: `DogPointsUpdateService` must be called synchronously within the heat result POST transaction. Re-submitting a heat result must be idempotent (detect existing heat ID, do not double-count). If a heat is deleted/corrected, points must be recalculated from history (rare edge case — implement as admin-only "recalculate" endpoint, not automatic).

**Decision 5: Title Progression Is Event-Driven at Result Entry (No Batch Job)**
- Title threshold checks run synchronously in `DogPointsUpdateService` each time points are added. New titles are appended to `Dog.Titles` immediately.
- Rejected: Nightly batch job that sweeps all dogs and awards titles.
- Rationale: Heat results are entered infrequently (at tournament events, not real-time). Event-driven is simpler to reason about: result saved → points updated → titles checked → Dog document persisted. Competitors want to see their title earned immediately, not the next day. No scheduler infrastructure needed. Reduces operational complexity.
- Constraints: TitleProgressionService must be idempotent: do not re-award a title already in Dog.Titles. Title crossing is one-way — no title revocation in this implementation (per NAFA rules, titles are permanent once earned).

**Decision 6: Heat and Tournament Documents Share a Cosmos Container**
- Tournament, TournamentEntry, Heat, and RaceRecord documents all live in the `tournaments` Cosmos container, partitioned by `/tournamentId`.
- Rejected: Separate containers for Heats and Tournaments.
- Rationale: Heats are always accessed in the context of a tournament (list heats for tournament, get results for tournament). Co-locating them on the same partition key enables single-partition queries with no cross-partition fan-out. Tournament Director workflows (schedule + record heats) always operate within one tournament. Cosmos pricing is per container (throughput provisioning) — fewer containers is lower cost at this scale. Type discriminator field (`type: "tournament" | "entry" | "heat" | "raceRecord"`) distinguishes documents.

**Decision 7: Regional Points Are Materialized on ClubSeasonRecord**
- Regional championship points are materialized on `ClubSeasonRecord` documents (stored in a `club-seasons` Cosmos container, partitioned by `/clubId`).
- Rationale: Same reasoning as Decision 4: aggregating placements across 10 tournaments per year is more expensive than materializing on write. `ClubSeasonRecord` is updated when tournament results are submitted (infrequent write, frequent read for standings). The 80% rule (best finishes across ≤80% of tournament weeks) requires tracking individual tournament finishes anyway — `TournamentFinish` list on `ClubSeasonRecord` is the natural source of truth.

---

### 2026-03-07T00:00:00Z: Flyball Planning Session — 5 User Decisions
**Date:** 2026-03-07  
**Status:** Approved  
**By:** Brian (via Copilot)

**Decision 1: CRN Storage — Both NAFA-Assigned and Internal App ID**
- Store both a NAFA-assigned CRN and an internal app ID on the Dog entity.
- **NafaCrn** (string, nullable): The official NAFA-assigned Competition Racing Number, entered by the user
- **Internal ID** (string): CosmosDB document ID, immutable, auto-generated
- Rationale: NAFA-assigned CRNs are the source of truth for official record-keeping. Internal app IDs provide a stable identity within the system independent of NAFA assignments. Allows retroactive import of historical NAFA data and linking to existing dogs.
- Constraints: NafaCrn must be unique (enforced at API layer). NafaCrn is immutable once set (no CRN transfers). Internal ID is the primary partition/query key.

**Decision 2: Racing Year Calendar — NAFA Year, Not Calendar Year**
- A racing year is defined by NAFA's calendar: when races are submitted/recorded to NAFA, not January–December.
- Rationale: Aligns with official NAFA record-keeping and compliance. Simplifies reporting and standings — no split accounting across calendar boundaries. Iron Dog consecutive-year tracking follows NAFA year boundaries. Regional championship points accumulate within NAFA season, not split across years.
- Constraints: Application must track NAFA season start/end dates (configurable per application instance). Historical data import must respect NAFA year boundaries.

**Decision 3: Tournament Director Role — Separate Auth Role with Elevated Permissions**
- Create a distinct Tournament Director role. Only Tournament Directors can enter race results and record heats.
- Rationale: Race result recording is sensitive — points and titles flow from recorded results. Regular club admins and team owners should not have this permission. Supports governance: TDs are vetted scorekeepers, separate from administrative roles. Prevents accidental/unauthorized result recording.
- Constraints: Club admins cannot assign race results (only TDs can). Tournament Director role is separate from Club Admin role. Audit logging required for all result entries.

**Decision 4: Historical Data — Bulk Import of Historical Points and Results Required**
- Yes — bulk import of historical points and results from NAFA records is required.
- Rationale: Dogs competing for 10+ years have existing NAFA point history. Users expect historical totals and title progression to be complete and accurate. Supports accurate Iron Dog consecutive-year determinations.
- Implementation: Plan a dedicated endpoint/job in sprint FB-7. Supports importing NAFA C.6 race records (historical raw results). Recalculates points and re-awards titles retroactively. Admin-only operation.

**Decision 5: Breed Tracking — Seeded AKC List, Hardcoded but Editable by Admins**
- Breed list is seeded with AKC-recognized breeds, hardcoded in the app, but editable by admins at runtime.
- Rationale: AKC breed groups are the baseline standard for Multibreed class eligibility. Editing capability allows admins to add non-AKC breeds (rare registries, new breed recognitions). Avoids complex breed master data management.
- Usage: Used for validating Multibreed class eligibility (≥3 breed groups among 4 running dogs). Admin panel to add/remove breeds. Breed edits do not retroactively recalculate class eligibility (immutable once race recorded).

---

