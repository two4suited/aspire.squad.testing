# Dog Teams — Flyball Race/Points Tracking Sprint Plan

**Phase:** 2 — NAFA Flyball Feature Layer  
**Prerequisite:** Phase 1 Sprints 1–2 complete (CosmosDB repositories, API endpoints, Auth)  
**Requested by:** Brian  
**Lead:** Copilot Squad Lead

> ⚠️ **Team Note:** Sprint FB-8 (User Manual & Screenshots) requires a **Docs agent**. Current team has no documentation specialist. Recommend adding before FB-8 begins.

---

## Context & Scope

This plan extends the existing Dog Teams app (Team → Owner → Dog CRUD) with NAFA Flyball
race/points tracking. The existing foundation provides the data layer, auth, and API scaffolding
that flyball features build directly on top of.

### What Already Exists
- `Team`, `Owner`, `Dog` models (stub CRUD, no flyball fields)
- CosmosDB with `/teamId` partition key (`dog-teams` container)
- Redis caching infrastructure (planned)
- React frontend scaffolded (not yet wired to API)
- Auth system planned (Sprint 1 prerequisite)

### What Is Net-New
- Club entity (NAFA-sanctioned organization above Teams)
- Dog flyball registration (CRN, jump height measurements, age eligibility)
- Tournament/Event management
- Heat recording (dogs, times, pass infractions, clean run flags)
- Points calculation engine (time → per-dog points table)
- Title progression tracker (20 pts → FD … 100,000 pts → FGDCh-G)
- Multibreed title track (parallel to Regular)
- Iron Dog award tracking (10 consecutive years)
- Regional Championship points (Club-level, best-of-year finishes)
- NAFA Championship tracking (6 fastest times from regional champions)

---

## Key Design Decisions

> See also: `.squad/decisions/inbox/lead-flyball-domain.md`

| Decision | Choice | Rationale |
|---|---|---|
| Existing `Team` vs new `Club` | `Club` is a **new top-level entity**. `Team` remains as the NAFA Racing Team (4–6 dogs that run together). A Club has one or more Teams. | Avoids breaking the existing partition strategy. Aligns with NAFA: a Club (org w/ NAFA #) fields one or more racing Teams. |
| CRN placement | Property on `Dog` model (not a separate entity) | CRN is a 1:1 attribute of a dog. Separate entity adds cost with no query benefit. |
| Jump height measurements | Sub-collection on `Dog` (list of `JumpHeightMeasurement` records) | Measurements are always accessed with the dog document. Max 2–3 items. Embed is correct. |
| Points calculation | **Materialized on Dog document** (`LifetimePoints`, `LifetimeMultibreedPoints`) updated at result entry time | This is a CRUD+calculation app. On-the-fly recalculation across thousands of heat records is expensive. Materialized is correct for read performance. |
| Title progression | **Event-driven at result entry** (no batch job) | When a `HeatResult` is saved, the API synchronously checks if a new title threshold is crossed and records it. Simple, auditable, no scheduler needed for MVP. |
| Cosmos partition for new entities | `Club`: `/clubId` container. `Tournament`: `/tournamentId` container. `HeatResult`: `/tournamentId` partition (heats always accessed in tournament context). | Keeps partition hot-spots manageable. Tournament and heat data is always queried together. |
| Points table storage | Hardcoded as a static lookup in `PointsCalculationService` | The NAFA points table is rules-defined, not user-editable. Static lookup is correct here. |
| Regional/NAFA Championship | Materialized `ClubSeasonRecord` documents, recalculated on tournament result entry | Championship standings change only when results are submitted. Real-time recalc unnecessary. |

---

## Entity Model Summary (New + Extended)

```
Club  (new top-level, /clubId partition)
  ├── NafaClubNumber: string
  ├── Name, Region, Captain
  └── Teams: List<string> (team IDs — cross-reference)

Team  (existing — extended)
  ├── ClubId: string           ← NEW: links Team to Club
  ├── Class: enum              ← NEW: Regular | Multibreed | Veterans | Open
  └── ... (existing fields)

Dog  (existing — extended)
  ├── Crn: string                            ← NEW
  ├── IsMixedBreed: bool                     ← NEW
  ├── WittersHeightInches: decimal           ← NEW
  ├── JumpHeightMeasurements: List<JumpHeightMeasurement>  ← NEW
  ├── LifetimePoints: int                    ← NEW (materialized)
  ├── LifetimeMultibreedPoints: int          ← NEW (materialized)
  ├── Titles: List<DogTitle>                 ← NEW (earned titles with date)
  ├── EarnedIronDog: bool                    ← NEW
  └── ... (existing fields)

Tournament  (new, /tournamentId partition)
  ├── NafaSanctionNumber: string
  ├── HostClubId: string
  ├── Date, Location, Region
  ├── Classes: List<TeamClass>
  └── Status: enum  (Upcoming | Active | ResultsSubmitted)

TournamentEntry  (new, /tournamentId partition)
  ├── TournamentId, TeamId, ClubId
  ├── Class: TeamClass
  ├── SeededTime: decimal
  └── Dogs: List<string>  (dog IDs on timesheet — max 6)

Heat  (new, /tournamentId partition)
  ├── TournamentId, Round, HeatNumber, Lane
  ├── TeamId
  ├── FinishTime: decimal?
  ├── IsClean: bool
  ├── Result: enum  (Win | Loss | ByePass | Breakout | Forfeit)
  ├── DogRuns: List<DogRunResult>
  └── PassInfractions: List<PassInfraction>

DogRunResult  (embedded in Heat)
  ├── DogId, Position (1–4)
  ├── CompletedCleanRun: bool
  └── Notes: string?

RaceRecord  (new, /tournamentId partition)
  ├── TournamentId, TeamId
  ├── Class, Round
  ├── HeatsWon, HeatsLost
  └── BestTime: decimal?

ClubSeasonRecord  (new, /clubId partition)
  ├── ClubId, SeasonYear, Region, Class
  ├── TournamentFinishes: List<TournamentFinish>
  ├── RegionalPoints: int  (materialized)
  └── FastestTimes: List<decimal>  (top 4 for tiebreaker)
```

---

## Sprint FB-1: Domain Extension — Club, CRN & Jump Heights

**Goal:** Add Club as a top-level entity; extend Dog with flyball registration fields.  
**Owner agents:** Backend, Tester  
**Blocks:** All other flyball sprints

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| FB-1.1 | Create `Club` model, DTOs, `IClubRepository`, `ClubRepository` | Backend | Cosmos container: `clubs` with `/clubId` partition key. Fields: Id, NafaClubNumber, Name, Region, Captain, TeamIds. Wire in CosmosDbContext. |
| FB-1.2 | Implement `ClubsController` (CRUD) | Backend | POST /clubs, GET /clubs/{id}, PUT /clubs/{id}, DELETE /clubs/{id}. Auth-scoped: only club admin can mutate. |
| FB-1.3 | Extend `Team` model with `ClubId` and `Class` | Backend | Add `ClubId: string`, `Class: TeamClass (enum)`. Migration: existing Team documents get `Class = Regular` as default. Update DTOs and TeamRepository queries. |
| FB-1.4 | Extend `Dog` model with flyball fields | Backend | Add `Crn`, `IsMixedBreed`, `WithersHeightInches`, `JumpHeightMeasurements` (embedded list), `LifetimePoints`, `LifetimeMultibreedPoints`, `Titles` (list), `EarnedIronDog`. Update DTOs. |
| FB-1.5 | Add `JumpHeightCalculationService` | Backend | Static method: `(decimal withersHeight) → int jumpHeightInches`. Rules: jumpHeight = withers - 6, min 7", max 14". Round to nearest integer. |
| FB-1.6 | Unit tests: Club CRUD, Dog extension fields, jump height calculation | Tester | Test ClubRepository CRUD, new Dog field validation (CRN uniqueness check, age ≥ 15 months), JumpHeightCalculationService edge cases (min/max/rounding). |

**Sprint Dependencies:** Phase 1 Sprint 1–2 (repositories, CosmosDbContext, auth)  
**Exit Criteria:**
- `Club` documents created/read/updated/deleted in Cosmos ✓
- `Dog` documents include CRN, withers height, jump height measurements ✓
- `Team` documents include ClubId and Class ✓
- Jump height calculation returns correct values for boundary cases ✓
- Unit tests pass ✓

---

## Sprint FB-2: Tournament & Heat Models

**Goal:** Define Tournament, Entry, Heat, and Run entities; implement their repositories and Cosmos wiring.  
**Owner agents:** Backend, Tester  
**Blocks:** FB-3 (points), FB-4 (API endpoints)

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| FB-2.1 | Create `Tournament`, `TournamentEntry` models + DTOs | Backend | Cosmos container: `tournaments` with `/tournamentId` partition key. Tournament has NAFA sanction #, date, location, region, status. TournamentEntry links Team + Tournament, stores timesheet dogs (max 6), seeded time. |
| FB-2.2 | Implement `ITournamentRepository`, `TournamentRepository` | Backend | CRUD + query by date/region/status. Also query "entries by team". |
| FB-2.3 | Create `Heat`, `DogRunResult`, `PassInfraction`, `RaceRecord` models + DTOs | Backend | Heat stored in `tournaments` container (same partition as Tournament for co-location). DogRunResult and PassInfraction embedded in Heat. RaceRecord aggregates heats-won/lost per team per tournament round. |
| FB-2.4 | Implement `IHeatRepository`, `HeatRepository` | Backend | Create heat, update heat, query heats by tournament+round, query heats by team+tournament. |
| FB-2.5 | Validate time-sheet rules at API layer | Backend | Validators: max 6 dogs on timesheet, max 4 active + 2 alternates, dog must have CRN, dog must be 15+ months old, dog double-listing rule (max 2 timesheets/day). Return 400 with structured errors on violation. |
| FB-2.6 | Unit tests: Tournament/Heat models, validation rules | Tester | Test tournament CRUD, timesheet validator (6-dog max, age check, double-listing), heat result shapes, DogRunResult embedded correctly. |

**Sprint Dependencies:** FB-1 (Club/Dog extensions needed for validation)  
**Exit Criteria:**
- Tournament documents create/read in Cosmos ✓
- TournamentEntry rejects >6 dogs, under-age dogs, dogs without CRN ✓
- Heat documents create/update in Cosmos, co-partitioned with Tournament ✓
- All validation rules tested ✓

---

## Sprint FB-3: Points Calculation Engine

**Goal:** Implement the NAFA points table, title progression, Iron Dog tracking, and materialized points update logic.  
**Owner agents:** Backend, Tester  
**Blocks:** FB-4 (results entry triggers this), FB-5 (regional points uses this)

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| FB-3.1 | Implement `PointsTableService` | Backend | Encodes the NAFA time-to-points table as a static lookup. Input: team finish time (decimal seconds). Output: int points (0–25). Document the full table from the rulebook. Rules: <24s = 25 pts, 24-28s = graduated scale, must be clean run. Returns 0 for non-clean runs. |
| FB-3.2 | Implement `TitleProgressionService` | Backend | Given lifetime points + class (Regular/Multibreed), returns the current earned title and the next threshold. Regular track: FD(20)→FDX(100)→FDCh(500)→FDCh-S(1000)→FDCh-G(2500)→FM(5000)→FMX(10000)→FMCh(15000)→FMCH-S(20000)→FMCH-G(30000)→FGDCh(40000)→FGDCh-S(50000-90000)→FGDCh-G(100000). Multibreed track: MBD(20)→MBDX(100)→MBDCh(500)→MBDCh-S(1000)→MBDCh-G(2500)→MBM(5000)→MBMX(10000)→MBMCh(15000)→MB ONYX(20000)→MBGDCh(30000). |
| FB-3.3 | Implement `IronDogTrackingService` | Backend | Query Dog's heat history: identify distinct calendar years where dog earned ≥1 NAFA point. If 10 consecutive years found, set `EarnedIronDog = true` on Dog document. Run on each points update. |
| FB-3.4 | Implement `DogPointsUpdateService` | Backend | Orchestrator called after a Heat is marked complete. For each DogRunResult in the heat: if clean run, call PointsTableService with heat team time, add to Dog.LifetimePoints (or LifetimeMultibreedPoints for Multibreed class). Call TitleProgressionService, record any newly-crossed title in Dog.Titles with date. Call IronDogTrackingService. Persist updated Dog document. |
| FB-3.5 | Unit tests: points calculation, title progression, Iron Dog | Tester | Test PointsTableService for every threshold boundary (23.99s = 25pts, 28.01s = 0pts). Test TitleProgressionService for each title crossing. Test IronDogTrackingService with 9 vs 10 years, non-consecutive years. Test DogPointsUpdateService for dirty run (0 pts), breakout (0 pts), clean run (correct pts). |
| FB-3.6 | Integration test: heat submission → points materialized | Tester | Submit a heat result via API, verify Dog.LifetimePoints is updated and title is recorded if threshold crossed. Test idempotency: re-submitting same heat does not double-count points. |

**Sprint Dependencies:** FB-2 (Heat/DogRunResult models), FB-1 (Dog model with LifetimePoints)  
**Exit Criteria:**
- Points table covers all time ranges with correct values ✓
- All title thresholds correctly detected for both tracks ✓
- Iron Dog correctly requires 10 *consecutive* calendar years ✓
- Dog document materialized points match sum of all clean run results ✓
- No double-counting on repeated heat submission ✓

---

## Sprint FB-4: Race & Results API Endpoints

**Goal:** Expose all tournament, heat entry, and points query endpoints. Connect results entry to points engine.  
**Owner agents:** Backend, Tester  
**Note:** This sprint is the "wire-up" sprint — most logic lives in FB-2/FB-3 services.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| FB-4.1 | Implement `TournamentsController` | Backend | POST /tournaments, GET /tournaments (filter: region, date range, status), GET /tournaments/{id}, PUT /tournaments/{id}, DELETE /tournaments/{id}. Require auth. |
| FB-4.2 | Implement `TournamentEntriesController` | Backend | POST /tournaments/{id}/entries (submit timesheet), GET /tournaments/{id}/entries, GET /tournaments/{id}/entries/{entryId}, DELETE (withdraw). Validates timesheet rules (from FB-2.5). |
| FB-4.3 | Implement `HeatsController` | Backend | POST /tournaments/{id}/heats (record heat result), GET /tournaments/{id}/heats (list all heats for tournament), GET /tournaments/{id}/heats/{heatId}. On POST: trigger `DogPointsUpdateService` (from FB-3.4). Return updated dog points in response. |
| FB-4.4 | Implement `DogsController` points/title endpoints | Backend | GET /dogs/{id}/points — returns LifetimePoints, LifetimeMultibreedPoints, all titles with dates, next title threshold, Iron Dog status. GET /dogs/{id}/heat-history — paginated list of heats dog participated in with per-run clean flag. |
| FB-4.5 | Add Redis caching for dog points/titles | Backend | Cache dog points profile (5-min TTL). Invalidate on DogPointsUpdateService write. Cache tournament list (10-min TTL). |
| FB-4.6 | API integration tests: tournament lifecycle + heat results | Tester | End-to-end: create tournament → enter team → submit heat result → verify dog points updated → verify title triggered at threshold. Test breakout rule (time < seeded = loss regardless of clean). Test pass infraction marks run as dirty. |

**Sprint Dependencies:** FB-2, FB-3 (all calculation logic must exist before wiring)  
**Exit Criteria:**
- Full tournament lifecycle testable via curl/Postman ✓
- Heat result submission updates Dog.LifetimePoints ✓
- Dog points endpoint returns correct accumulated totals ✓
- Breakout correctly results in loss with no points awarded ✓
- Integration tests pass ✓

---

## Sprint FB-5: Regional & Championship Points

**Goal:** Implement club-level regional points accumulation and NAFA Championship tracking.  
**Owner agents:** Backend, Tester  
**Priority:** MVP-adjacent — needed for complete feature set, but not blocking basic race tracking.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| FB-5.1 | Create `ClubSeasonRecord` model + repository | Backend | Cosmos: `club-seasons` container, `/clubId` partition. Tracks per-season, per-region, per-class: list of tournament finishes (placement, time), accumulated regional points, top-4 fastest times for tiebreaker. |
| FB-5.2 | Implement `RegionalPointsService` | Backend | On tournament result submission (top-3 placements confirmed): award 3/2/1 points to clubs. Respect max 30 regional points/year cap. Apply 80% rule: count only best finishes from ≤80% of tournament weeks (max 10 tournaments). Update `ClubSeasonRecord`. |
| FB-5.3 | Implement `RegionalChampionshipService` | Backend | End-of-year calculation: per region, per class (Regular + Multibreed), rank clubs by regional points. Tiebreaker: average of 4 fastest best-placement times. Produce `RegionalChampion` record. |
| FB-5.4 | Implement `NafaChampionshipService` | Backend | From regional champions: require minimum 6 tournaments entered. Rank by 6 fastest times from the year. Produce `NafaChampion` record. |
| FB-5.5 | Regional standings endpoints | Backend | GET /regions/{region}/standings?year={year}&class={class} — returns ranked clubs with regional points. GET /regions/{region}/champion?year={year} — returns champion record. GET /nafa/champion?year={year}&class={class} — NAFA champion. |
| FB-5.6 | Unit + integration tests: regional points, championship rules | Tester | Test 80% tournament cap (11 tournaments → only best 10 count). Test 30-point season cap. Test tiebreaker: two clubs at same points, faster average time wins. Test minimum 6-tournament rule for NAFA Championship. |

**Sprint Dependencies:** FB-4 (tournament results must flow before regional points can be awarded)  
**Exit Criteria:**
- Regional points update correctly after tournament result entry ✓
- 80% tournament participation cap enforced ✓
- 30-point season cap enforced ✓
- Tiebreaker by fastest-4 times works ✓
- NAFA Championship requires ≥6 tournaments ✓

---

## Sprint FB-6: Frontend — Tournament & Points UI

**Goal:** Build React UI for tournament management, heat scorekeeper entry, dog profile with points/titles, and club standings.  
**Owner agents:** Frontend, Tester  
**Priority:** MVP — without UI, the feature set is only testable via API.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| FB-6.1 | API client modules: tournaments, heats, clubs | Frontend | Add `tournaments.ts`, `heats.ts`, `clubs.ts` to API client layer. Mirror existing pattern from `teams.ts`, `dogs.ts`. |
| FB-6.2 | Club management pages (list, create, detail) | Frontend | `/clubs` — list user's clubs with NAFA number. `/clubs/:id` — club detail: name, region, teams list, season record summary. Create/edit club form. |
| FB-6.3 | Tournament management pages | Frontend | `/tournaments` — list upcoming/active/completed tournaments. `/tournaments/new` — create tournament form (sanction #, date, location, class). `/tournaments/:id` — tournament detail: entries, heats, results summary. |
| FB-6.4 | Timesheet / entry UI | Frontend | `/tournaments/:id/enter` — enter team + select dogs from roster (max 6). Validate age/CRN client-side before submit. Show seeded time input. |
| FB-6.5 | Heat scorekeeper entry UI | Frontend | `/tournaments/:id/heats/new` — heat entry form: round, heat #, lane, team, 4 dog run results (clean/dirty per dog), pass infractions, finish time. On submit, show updated dog points inline. |
| FB-6.6 | Dog profile — points & titles panel | Frontend | Extend dog detail page with a collapsible "NAFA Points" panel: lifetime points (Regular + Multibreed), earned titles (with dates), next title + points to go, Iron Dog badge. |
| FB-6.7 | Club standings / leaderboard page | Frontend | `/regions/:region/standings` — regional standings table sorted by points. Show tiebreaker times. Link to club detail. |
| FB-6.8 | E2E tests: tournament lifecycle | Tester | Playwright: create tournament → enter team → submit heat result → verify dog points updated in profile → verify title appears at threshold. |

**Sprint Dependencies:** FB-4 (API endpoints must be live), Phase 1 Sprint 3 (base frontend wired to API)  
**Exit Criteria:**
- Scorekeeper can enter heat result in <60 seconds via UI ✓
- Dog points/titles visible in dog profile immediately after heat entry ✓
- Club standings page renders with correct regional points ✓
- E2E tests pass ✓

---

## Sprint FB-7: Reporting, Edge Cases & Polish

**Goal:** Close out edge cases, add export/reporting, validate full NAFA rulebook compliance.  
**Owner agents:** Backend, Frontend, Tester  
**Priority:** Nice-to-have — completes the feature set but not blocking core workflows.

| # | Work Item | Agent | Notes |
|---|-----------|-------|-------|
| FB-7.1 | Tournament results report (Form C.6 analog) | Backend + Frontend | Endpoint: GET /tournaments/{id}/results-report — returns structured JSON suitable for submission. Frontend: render as printable HTML table. |
| FB-7.2 | Jump height form (Form C.9 analog) | Frontend | Per-tournament, per-team: list of dogs with current jump heights. Allow OIR measurement entry (two matching measurements → permanent). Flag temporary measurements (dogs 15–24 months). |
| FB-7.3 | FGDCh-S plate tracking (10K increments above 50K) | Backend | Extend TitleProgressionService: FGDCh-S awarded at 50K, 60K, 70K, 80K, 90K (one plate per 10K). Track plate count on DogTitle record. |
| FB-7.4 | Multibreed class breed diversity validation | Backend | TournamentEntry validator: for Multibreed class, at least 3 different breed groups represented in the 4 running dogs. Return structured error if not met. |
| FB-7.5 | Veterans/Open class jump height flexibility | Backend | For Veterans and Open class heats, jump height is handler-declared (not withers-calculated). Add `DeclaredJumpHeight` to DogRunResult for these classes. |
| FB-7.6 | Dog double-listing audit endpoint | Backend | GET /tournaments/{id}/double-listing-audit — returns any dog appearing on >2 timesheets that day (rule violation). Used by tournament director. |
| FB-7.7 | Points history export (CSV) | Backend | GET /dogs/{id}/points-export — returns CSV of all heat results (date, tournament, time, points earned, running total). Supports title certificate preparation. |
| FB-7.8 | Full rulebook compliance regression tests | Tester | Systematic test of each NAFA rule: breakout detection, pass infraction = dirty run, double-listing limit, age minimum, jump height min/max, Iron Dog consecutive year gap detection, Multibreed breed diversity rule, Veterans declared jump height. |

**Sprint Dependencies:** FB-5 (regional points), FB-6 (frontend)  
**Exit Criteria:**
- All NAFA rules from rulebook represented by at least one automated test ✓
- Double-listing audit catches violations ✓
- FGDCh-S plates tracked correctly at 10K increments ✓
- Multibreed breed diversity enforced ✓
- CSV export works end-to-end ✓

---

## Critical Path

```
Phase 1: Sprint 1 (Cosmos/Auth/Redis) 
    ↓
Phase 1: Sprint 2 (API endpoints — Team/Owner/Dog CRUD)
    ↓
FB-1: Club + Dog extension
    ↓
FB-2: Tournament + Heat models
    ↓
FB-3: Points calculation engine     ← Can parallelize FB-3 with FB-2 after FB-1
    ↓
FB-4: Race/Results API endpoints
    ↓
FB-5: Regional/Championship         ← Can start after FB-4
FB-6: Frontend UI                   ← Can start after FB-4 + Phase 1 Sprint 3
    ↓
FB-7: Polish/edge cases/reporting
```

### Parallelization Opportunities

| Sprints | Can Run In Parallel? | Condition |
|---|---|---|
| FB-2 + FB-3 (early) | ✓ Yes | Start FB-3 service stubs after FB-1 completes; FB-3 only needs Dog model shape |
| FB-5 + FB-6 | ✓ Yes | Both unblock after FB-4 |
| FB-7.1–7.3 + FB-7.4–7.8 | ✓ Yes | All independent polish items |

---

## MVP Definition

**Flyball MVP = FB-1 through FB-4 + FB-6 core (FB-6.1–6.6)**

Delivers:
- Dogs with CRN, jump heights, age eligibility
- Tournament creation and team entry
- Heat result recording by scorekeeper
- Points calculated and materialized on Dog profile
- Titles awarded automatically on threshold crossing
- Dog points profile visible in UI

**Full Feature Set = All FB sprints complete**

---

## Cosmos DB Changes Summary

| Container | Partition Key | New/Changed |
|---|---|---|
| `dog-teams` | `/teamId` | Existing — Dog and Team documents extended (new fields) |
| `clubs` | `/clubId` | New |
| `tournaments` | `/tournamentId` | New — Tournament, TournamentEntry, Heat, RaceRecord all stored here |
| `club-seasons` | `/clubId` | New |

---

## Agent Assignments Summary

| Agent | Primary Sprints | Key Responsibilities |
|---|---|---|
| **Backend** | FB-1, FB-2, FB-3, FB-4, FB-5, FB-7 | All C# models, repos, services, controllers, calculation engines |
| **Frontend** | FB-6, FB-7 (reports/forms) | All React pages, API client modules, scorekeeper UI |
| **Aspire** | FB-1 (Cosmos containers), FB-4 (caching) | Wire new Cosmos containers in AppHost, Redis cache invalidation |
| **Tester** | All FB sprints | Unit tests for all calculation services, integration tests for full lifecycle, E2E for UI flows |

---

## Backlog / Post-Flyball MVP

- **PDF certificate generation** for title achievements (FD, FDCh, etc.)
- **Email notifications** on title threshold crossings
- **Bulk heat import** from CSV (EJS export format)
- **Tournament seeding automation** (time-based bracket generation)
- **Public-facing leaderboard** (no-auth standings page)
- **Historical data import** (retroactive points from NAFA records)
- **Mobile scorekeeper app** (simplified heat entry for ringside)
- **NAFA data sync** (official submission format for Form C.6, C.9)

---

## User Decisions

Captured from Brian's planning session (2026-03-07):

1. **CRN Storage:** Both — store real NAFA-assigned CRN AND generate an internal app ID. Dog entity will have both `NafaCrn` (nullable, user-entered) and internal CosmosDB id.

2. **Racing Year Calendar:** NAFA calendar — racing year = when races are submitted/recorded to NAFA, not Jan-Dec.

3. **Tournament Director Auth:** Separate Tournament Director role with elevated permissions. Club admins and regular users cannot enter race results.

4. **Historical Data:** Yes — bulk import of historical points and results required. Plan for an import endpoint/job in FB-7.

5. **Breed Tracking:** Seeded AKC breed list hardcoded in app, but editable by admins. Used for Multibreed class eligibility.
