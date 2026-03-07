# FB-1 Test Coverage Report

**Agent:** Tester  
**Sprint:** FB-1  
**Date:** 2026-03-07  

## What Is Tested

### Unit Tests (pass immediately)

**JumpHeightCalculationTests** — 5 tests, all passing
- `JumpHeight_ForDog_14Inch_WithersIs8Inches` — withers=14 → jump=8 (normal case)
- `JumpHeight_ForDog_12Inch_Withers_Is6Inches` — withers=12 → jump=7 (min floor applied)
- `JumpHeight_ForDog_20Inch_Withers_Is14Inches` — withers=20 → jump=14 (at max)
- `JumpHeight_ForDog_22Inch_Withers_Is14Inches` — withers=22 → jump=14 (max cap applied)
- `JumpHeight_RoundsDown` — withers=13.75 → jump=7 (floor, not round)

These tests use an inline helper encoding the spec: `floor(withers - 6)`, clamped `[7, 14]`.  
**TODO:** When Backend implements the production `JumpHeightCalculator` (or method on `Dog`), replace the inline helper with a call to the real method.

### Unit Tests (pending Backend — wrapped in `#if false`)

**ClubModelTests** — 2 tests
- `Club_DefaultValues_AreCorrect` — Id is non-empty Guid string, type="club", CreatedAt is set
- `Club_NafaClubNumber_IsNullable` — NafaClubNumber defaults to null

**DogFlyballFieldTests** — 3 tests
- `Dog_FlyballDefaults_AreCorrect` — LifetimePoints=0, MultibreedLifetimePoints=0, MeasurementType=None
- `Dog_NafaCrn_IsNullableByDefault` — NafaCrn defaults to null
- `Dog_WithersHeight_IsNullableByDefault` — WithersHeightInches defaults to null

**BreedModelTests** — 1 test
- `Breed_DefaultValues_AreCorrect` — IsActive=true, type="breed", CreatedAt is set

**Activation:** Remove `#if false` / `#endif` wrapping in `DomainModelTests.cs` once Backend adds `Club`, `Breed`, and extended `Dog` flyball fields to `DogTeams.Api.Models`.

### Integration Tests (skipped — pending controllers)

**ClubsIntegrationTests** — 3 tests, all skipped
- `GetAllClubs_ReturnsOk` — GET /api/clubs → 200
- `CreateClub_ReturnsCreated` — POST /api/clubs → 201 + Location header
- `GetClub_ById_ReturnsClub` — POST then GET by Location → 200, body contains name

**BreedsIntegrationTests** — 2 tests, all skipped
- `GetAllBreeds_ReturnsSeedData` — GET /api/breeds → 200, body contains "Border Collie" and "Australian Shepherd"
- `GetBreed_ById_ReturnsBreed` — list, then GET by id → 200, body contains id

**Activation:** Remove `Skip` parameter from `[Fact]` once `ClubsController` and `BreedsController` are implemented.

## What Is Left for Later

- **Jump height tests against production code** — inline helper must be replaced with call to Backend's implementation (any sprint after Backend lands)
- **Dog extended field tests** — un-gate `#if false` block when Backend adds NafaCrn, WithersHeightInches, MeasurementType, LifetimePoints, etc.
- **MeasurementType enum tests** — enum transitions (None→Temporary→Permanent) and expiry logic are not yet tested
- **JumpHeightMeasurement embed tests** — `Dog.JumpHeightMeasurements` list (Decision 3) not yet tested
- **BreedSeedData integration test** — verifies specific breeds in seed list (~30 entries); `GetAllBreeds_ReturnsSeedData` only spot-checks 2 breeds
- **CRN uniqueness enforcement test** — API-layer uniqueness check (per Decision 2) needs integration test
- **Club → Team relationship test** — `Team.ClubId` field (per architecture Decision 1) not yet tested
