# Dog Teams — Architecture

> **Status:** Proposed  
> **Author:** Lead  
> **Date:** 2025-07-18  
> **Stack:** .NET 10 · .NET Aspire 9.1 (NuGet 13.1.x) · Azure Cosmos DB · Redis · React + TypeScript

---

## 1. Solution Structure

```
DogTeams/
├── DogTeams.sln
├── src/
│   ├── DogTeams.AppHost/            # Aspire orchestrator
│   │   ├── Program.cs
│   │   └── DogTeams.AppHost.csproj
│   ├── DogTeams.ServiceDefaults/    # Shared Aspire defaults (telemetry, health, resilience)
│   │   ├── Extensions.cs
│   │   └── DogTeams.ServiceDefaults.csproj
│   ├── DogTeams.Api/                # ASP.NET Core Web API + Auth server
│   │   ├── Program.cs
│   │   ├── DogTeams.Api.csproj
│   │   ├── Domain/
│   │   │   ├── Team.cs
│   │   │   ├── Owner.cs
│   │   │   └── Dog.cs
│   │   ├── Data/
│   │   │   ├── CosmosDbContext.cs
│   │   │   └── Repositories/
│   │   │       ├── ITeamRepository.cs
│   │   │       ├── TeamRepository.cs
│   │   │       ├── IOwnerRepository.cs
│   │   │       ├── OwnerRepository.cs
│   │   │       ├── IDogRepository.cs
│   │   │       └── DogRepository.cs
│   │   ├── Auth/
│   │   │   ├── JwtTokenService.cs
│   │   │   ├── IdentitySetup.cs
│   │   │   └── CosmosUserStore.cs
│   │   ├── Endpoints/
│   │   │   ├── TeamEndpoints.cs
│   │   │   ├── OwnerEndpoints.cs
│   │   │   ├── DogEndpoints.cs
│   │   │   └── AuthEndpoints.cs
│   │   └── Caching/
│   │       └── RedisCacheService.cs
│   └── DogTeams.Web/               # React + TypeScript frontend
│       ├── package.json
│       ├── tsconfig.json
│       ├── vite.config.ts
│       └── src/
│           ├── main.tsx
│           ├── App.tsx
│           ├── api/
│           │   └── client.ts
│           ├── auth/
│           │   ├── AuthContext.tsx
│           │   ├── AuthProvider.tsx
│           │   ├── useAuth.ts
│           │   └── ProtectedRoute.tsx
│           ├── features/
│           │   ├── teams/
│           │   │   ├── TeamList.tsx
│           │   │   ├── TeamDetail.tsx
│           │   │   └── TeamForm.tsx
│           │   ├── owners/
│           │   │   ├── OwnerList.tsx
│           │   │   ├── OwnerDetail.tsx
│           │   │   └── OwnerForm.tsx
│           │   └── dogs/
│           │       ├── DogList.tsx
│           │       ├── DogDetail.tsx
│           │       └── DogForm.tsx
│           ├── components/
│           │   ├── Layout.tsx
│           │   └── Nav.tsx
│           └── types/
│               └── index.ts
└── tests/
    └── DogTeams.Tests/
        ├── DogTeams.Tests.csproj
        ├── ApiTests/
        │   ├── TeamEndpointTests.cs
        │   ├── OwnerEndpointTests.cs
        │   ├── DogEndpointTests.cs
        │   └── AuthEndpointTests.cs
        └── IntegrationTests/
            └── AppHostTests.cs
```

### Project Relationships

| Project | References | Purpose |
|---|---|---|
| `DogTeams.AppHost` | Api, Web (project refs) | Aspire orchestrator — wires services, databases, caching |
| `DogTeams.ServiceDefaults` | — | Shared config: OpenTelemetry, health checks, resilience policies |
| `DogTeams.Api` | ServiceDefaults | Web API, auth server, Cosmos DB access, Redis caching |
| `DogTeams.Web` | — (standalone npm) | React SPA served by Aspire via `AddNpmApp` |
| `DogTeams.Tests` | AppHost (via Testing) | Integration tests using `DistributedApplicationTestingBuilder` |

### Target Frameworks

- **AppHost, Api, ServiceDefaults, Tests:** `net10.0`
- **Web:** Node.js (Vite + React), managed by Aspire as an npm app resource

---

## 2. Cosmos DB Data Model

### Decision: Single Container, Separate Documents

**Container:** `dog-teams` (single container)  
**Partition Key:** `/teamId`  
**Discriminator:** `type` field on every document

**Why single container?**
- All queries are team-scoped (list owners for a team, list dogs for a team)
- Single partition key means cross-entity queries within a team cost 1 physical partition read
- Cosmos DB is optimized for few containers with good partition keys, not many containers

**Why separate documents (not embedded Dogs array)?**
- Dogs have independent CRUD lifecycle — adding/removing a dog shouldn't require reading and rewriting the entire Owner document
- Unbounded arrays are a Cosmos DB anti-pattern — an owner could have many dogs
- Separate documents allow point-reads by dog ID with known partition key
- Simpler concurrency — no conflicts when two users edit different dogs for the same owner

### Document Shapes

#### Team

```json
{
  "id": "team-uuid",
  "teamId": "team-uuid",
  "type": "team",
  "name": "Northwest K9 Squad",
  "description": "Regional search and rescue team",
  "createdAt": "2025-07-18T00:00:00Z",
  "updatedAt": "2025-07-18T00:00:00Z"
}
```

> `id` and `teamId` are identical for Team documents. This is intentional — `teamId` is the partition key on every document type, and `id` is the Cosmos DB document ID.

#### Owner

```json
{
  "id": "owner-uuid",
  "teamId": "team-uuid",
  "type": "owner",
  "identityUserId": "identity-uuid",
  "displayName": "Jane Handler",
  "email": "jane@example.com",
  "role": "member",
  "createdAt": "2025-07-18T00:00:00Z",
  "updatedAt": "2025-07-18T00:00:00Z"
}
```

> `identityUserId` links to ASP.NET Core Identity. This is the bridge between auth and domain.  
> `role` supports future authorization (e.g., `admin`, `member`).

#### Dog

```json
{
  "id": "dog-uuid",
  "teamId": "team-uuid",
  "ownerId": "owner-uuid",
  "type": "dog",
  "name": "Rex",
  "breed": "German Shepherd",
  "dateOfBirth": "2021-03-15",
  "certifications": ["SAR Level 1", "Tracking"],
  "notes": "Excellent air scent work",
  "createdAt": "2025-07-18T00:00:00Z",
  "updatedAt": "2025-07-18T00:00:00Z"
}
```

> `certifications` is a bounded array (reasonable for a dog's certifications). Embedded is fine here.

#### Identity User (separate container)

```json
{
  "id": "identity-uuid",
  "userId": "identity-uuid",
  "type": "identity-user",
  "email": "jane@example.com",
  "normalizedEmail": "JANE@EXAMPLE.COM",
  "userName": "jane@example.com",
  "normalizedUserName": "JANE@EXAMPLE.COM",
  "passwordHash": "...",
  "securityStamp": "...",
  "emailConfirmed": true
}
```

**Container:** `identity`  
**Partition Key:** `/userId`

> Identity data lives in a **separate container** from domain data. Identity is an infrastructure concern — keeping it isolated prevents partition key conflicts and simplifies the domain container's query patterns.

### Container Summary

| Container | Partition Key | Document Types | Access Pattern |
|---|---|---|---|
| `dog-teams` | `/teamId` | `team`, `owner`, `dog` | All domain queries scoped by team |
| `identity` | `/userId` | `identity-user` | Auth lookups by user ID, email lookup via cross-partition query (login only) |

### Key Query Patterns

| Query | Scope | Cost |
|---|---|---|
| Get team by ID | Point read (`id` + `teamId`) | 1 RU |
| List owners for team | Single-partition query (`WHERE teamId = X AND type = 'owner'`) | Low |
| List dogs for team | Single-partition query (`WHERE teamId = X AND type = 'dog'`) | Low |
| List dogs for owner | Single-partition query (`WHERE teamId = X AND ownerId = Y AND type = 'dog'`) | Low |
| Login (find user by email) | Cross-partition query on `identity` container | Moderate (acceptable — login is infrequent) |

---

## 3. Authentication

### Decision: API as Auth Server with ASP.NET Core Identity + JWT

**Approach:** The `DogTeams.Api` project serves as both the resource server and the auth server. ASP.NET Core Identity manages user registration, password hashing, and account management. JWT bearer tokens are issued by the API and validated on every request.

**Why not an external identity provider (e.g., Auth0, Entra ID)?**
- Keeps the architecture self-contained — no external dependencies for local dev
- Aspire's emulator story for Cosmos DB and Redis means the full stack runs locally
- If external auth is needed later, the JWT validation layer stays the same — only the token issuer changes
- For a team-tracking app, self-hosted identity is proportionate to the scope

### Auth Flow

```
1. POST /api/auth/register  →  Create Identity user + Owner record
2. POST /api/auth/login     →  Validate credentials → Issue JWT (access + refresh)
3. Client stores JWT in memory (not localStorage)
4. All API requests include: Authorization: Bearer {token}
5. POST /api/auth/refresh   →  Exchange refresh token for new access token
```

### JWT Configuration

| Setting | Value | Rationale |
|---|---|---|
| Access token lifetime | 15 minutes | Short-lived, limits exposure |
| Refresh token lifetime | 7 days | Supports "remember me" without long-lived access tokens |
| Signing algorithm | HS256 | Symmetric key, appropriate for single-API-server topology |
| Issuer/Audience | Configured via Aspire service discovery URL | No hardcoded URLs |
| Claims | `sub` (identityUserId), `teamId`, `ownerId`, `role` | All authorization-relevant data in the token |

### Identity Storage

ASP.NET Core Identity backed by a **custom Cosmos DB user store** (`CosmosUserStore`). This avoids pulling in Entity Framework just for identity. The store implements `IUserStore<T>`, `IUserPasswordStore<T>`, and `IUserEmailStore<T>`.

### Authorization Model

- **Team-scoped authorization:** Middleware extracts `teamId` from the JWT and ensures the user can only access resources within their team
- **Owner-scoped writes:** An owner can only modify their own dogs; team admins can modify any resource in the team
- **Role claim:** `admin` or `member` — admin can manage owners and team settings

---

## 4. API Surface

Base path: `/api`

### Auth Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Anonymous | Register new user (creates Identity user + Owner) |
| `POST` | `/api/auth/login` | Anonymous | Authenticate, return JWT |
| `POST` | `/api/auth/refresh` | Anonymous (with refresh token) | Refresh access token |
| `POST` | `/api/auth/logout` | Bearer | Invalidate refresh token |
| `GET` | `/api/auth/me` | Bearer | Get current user profile |

### Team Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/api/teams/{teamId}` | Bearer | Get team details |
| `PUT` | `/api/teams/{teamId}` | Bearer (admin) | Update team |
| `DELETE` | `/api/teams/{teamId}` | Bearer (admin) | Delete team and all children |
| `POST` | `/api/teams` | Bearer | Create new team (creator becomes admin) |

### Owner Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/api/teams/{teamId}/owners` | Bearer | List owners in team |
| `GET` | `/api/teams/{teamId}/owners/{ownerId}` | Bearer | Get owner details |
| `PUT` | `/api/teams/{teamId}/owners/{ownerId}` | Bearer (self or admin) | Update owner |
| `DELETE` | `/api/teams/{teamId}/owners/{ownerId}` | Bearer (admin) | Remove owner from team |

### Dog Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/api/teams/{teamId}/dogs` | Bearer | List all dogs in team |
| `GET` | `/api/teams/{teamId}/owners/{ownerId}/dogs` | Bearer | List dogs for specific owner |
| `GET` | `/api/teams/{teamId}/dogs/{dogId}` | Bearer | Get dog details |
| `POST` | `/api/teams/{teamId}/dogs` | Bearer | Add dog (assigned to current owner) |
| `PUT` | `/api/teams/{teamId}/dogs/{dogId}` | Bearer (owner or admin) | Update dog |
| `DELETE` | `/api/teams/{teamId}/dogs/{dogId}` | Bearer (owner or admin) | Remove dog |

### API Design Notes

- **Minimal APIs** (not controllers) — lighter, fits Aspire conventions
- **`teamId` in URL path** — matches partition key, enforced by auth middleware
- Responses use standard HTTP status codes + problem details (RFC 9457) for errors
- All list endpoints support cursor-based pagination via `?continuationToken=`

---

## 5. Redis Usage

### Decision: Cache + Distributed Coordination (Not Sessions)

Redis serves two purposes: **read-through caching** of hot domain data and **refresh token storage**. We do not use server-side sessions — JWT is the session.

### Cache Strategy

| Cache Key Pattern | Data | TTL | Invalidation |
|---|---|---|---|
| `team:{teamId}` | Serialized Team document | 10 min | On team update/delete |
| `team:{teamId}:owners` | List of Owner summaries | 5 min | On owner add/update/remove |
| `team:{teamId}:dogs` | List of Dog summaries | 5 min | On dog add/update/remove |
| `refresh:{userId}` | Hashed refresh token | 7 days | On logout or token refresh |

### Cache Pattern: Read-Through with Explicit Invalidation

```
GET /api/teams/{teamId}
  → Check Redis for team:{teamId}
  → Cache hit: return cached
  → Cache miss: read from Cosmos DB, write to Redis, return

PUT /api/teams/{teamId}
  → Write to Cosmos DB
  → Delete Redis key team:{teamId} (invalidate)
```

### Why Not Sessions?

- JWT-based auth doesn't need server-side sessions
- Avoids Redis as a SPOF for auth — if Redis goes down, auth still works (just slower reads)
- Refresh tokens in Redis allow explicit revocation (logout)

### Redis Configuration

- Use Aspire's `AddRedis` — provides connection string injection and health checks
- Client integration: `Aspire.StackExchange.Redis` in the API project
- Output caching is **not used** — we cache at the repository layer for precise control

---

## 6. Aspire Service Wiring

### AppHost Program.cs

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator()
    .AddDatabase("dogteams-db");

var redis = builder.AddRedis("redis");

// API
var api = builder.AddProject<Projects.DogTeams_Api>("api")
    .WithReference(cosmos)
    .WithReference(redis)
    .WithExternalHttpEndpoints();

// React frontend
builder.AddNpmApp("web", "../DogTeams.Web")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

### Service Defaults (DogTeams.ServiceDefaults)

Standard Aspire service defaults — applied to the API project:

- **OpenTelemetry:** Traces, metrics, logs exported to Aspire dashboard
- **Health checks:** `/health` and `/alive` endpoints
- **Resilience:** Default HTTP resilience policies (retry, circuit breaker)
- **Service discovery:** Automatic — API URL resolved by the frontend via Aspire

### Package Matrix

| Project | Key NuGet Packages |
|---|---|
| `DogTeams.AppHost` | `Aspire.Hosting.AppHost`, `Aspire.Hosting.Azure.CosmosDB`, `Aspire.Hosting.Redis` |
| `DogTeams.ServiceDefaults` | `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.ServiceDiscovery`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime` |
| `DogTeams.Api` | `Aspire.Microsoft.Azure.Cosmos`, `Aspire.StackExchange.Redis`, `Microsoft.AspNetCore.Identity`, `Microsoft.AspNetCore.Authentication.JwtBearer` |
| `DogTeams.Tests` | `Aspire.Hosting.Testing`, `xunit`, `Microsoft.NET.Test.Sdk` |

---

## 7. Frontend Structure

### React + TypeScript + Vite

The `DogTeams.Web` project is a standard Vite React app. Aspire manages it as an `NpmApp` resource — no custom hosting needed.

### Folder Structure (detailed)

```
DogTeams.Web/
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
└── src/
    ├── main.tsx                    # Entry point, renders App
    ├── App.tsx                     # Router setup, AuthProvider wrapper
    ├── api/
    │   └── client.ts              # Axios/fetch wrapper with JWT interceptor
    ├── auth/
    │   ├── AuthContext.tsx         # React context for auth state
    │   ├── AuthProvider.tsx        # Provider: login, logout, token refresh
    │   ├── useAuth.ts             # Hook: access auth state and actions
    │   └── ProtectedRoute.tsx     # Route guard: redirects to login if unauthenticated
    ├── features/
    │   ├── teams/
    │   │   ├── TeamList.tsx        # List view of teams
    │   │   ├── TeamDetail.tsx      # Single team view with owners/dogs
    │   │   └── TeamForm.tsx        # Create/edit team form
    │   ├── owners/
    │   │   ├── OwnerList.tsx       # List owners within a team
    │   │   ├── OwnerDetail.tsx     # Owner profile with their dogs
    │   │   └── OwnerForm.tsx       # Edit owner profile
    │   └── dogs/
    │       ├── DogList.tsx         # List dogs (filterable by owner)
    │       ├── DogDetail.tsx       # Dog profile
    │       └── DogForm.tsx         # Add/edit dog
    ├── components/
    │   ├── Layout.tsx             # Shell: nav + content area
    │   └── Nav.tsx                # Navigation bar with auth-aware links
    └── types/
        └── index.ts               # Shared TypeScript interfaces (Team, Owner, Dog)
```

### Auth State Management

**Approach:** React Context + `useAuth` hook. No Redux or external state library.

```
AuthProvider
  ├── Stores: accessToken (in memory), refreshToken (httpOnly cookie or in-memory)
  ├── On mount: attempt silent refresh
  ├── Provides: user, login(), logout(), isAuthenticated
  └── Wraps: Axios interceptor for automatic token attachment + 401 refresh retry
```

**Why Context over Redux/Zustand?**
- Auth state is simple (user object + token + loading flag)
- No complex state transitions that benefit from a state machine
- Context is built-in, no additional dependency

### API Client

- **Base URL:** Injected at build time via Aspire service discovery (environment variable)
- **Interceptor:** Attaches `Authorization: Bearer {token}` to every request
- **401 handling:** Attempts token refresh, retries original request; if refresh fails, redirects to login

### Routing

| Path | Component | Auth Required |
|---|---|---|
| `/login` | LoginPage | No |
| `/register` | RegisterPage | No |
| `/teams` | TeamList | Yes |
| `/teams/:teamId` | TeamDetail | Yes |
| `/teams/:teamId/owners` | OwnerList | Yes |
| `/teams/:teamId/owners/:ownerId` | OwnerDetail | Yes |
| `/teams/:teamId/dogs` | DogList | Yes |
| `/teams/:teamId/dogs/:dogId` | DogDetail | Yes |

---

## 8. Testing Strategy

### Integration Tests with Aspire TestingBuilder

```csharp
public class AppHostTests
{
    [Fact]
    public async Task AppHost_StartsSuccessfully()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.DogTeams_AppHost>();
        
        await using var app = await builder.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}
```

### Test Categories

| Category | What | How |
|---|---|---|
| AppHost smoke tests | Aspire app starts, services are healthy | `DistributedApplicationTestingBuilder` |
| API endpoint tests | CRUD operations, auth flows | HTTP client against running API (via TestingBuilder) |
| Auth tests | Register, login, token refresh, protected routes | HTTP client with/without tokens |

### Test Infrastructure

- Cosmos DB emulator runs automatically via Aspire's `RunAsEmulator()`
- Redis runs as a container via Aspire
- No mocks for infrastructure — tests run against real emulators
- Tests are in `DogTeams.Tests`, referencing `DogTeams.AppHost` as a project

---

## Open Questions

1. **Team creation flow:** Does registering create a team automatically, or is team creation a separate step? (Current assumption: separate step — user registers, then creates or joins a team.)
2. **Invite system:** How do owners join a team? Invite link? Admin adds them? (Out of scope for v1 — assume admin creates owners.)
3. **Dog transfer:** Can a dog be transferred between owners? (Out of scope for v1.)

---

## Version Reference

| Component | Version | Notes |
|---|---|---|
| .NET | 10.0 | Preview/RC — target `net10.0` |
| .NET Aspire | 9.1 (NuGet 13.1.x) | Latest stable: 13.1.2 |
| Azure Cosmos DB SDK | 3.x (preview) | `Microsoft.Azure.Cosmos` preview package |
| Redis | Latest container image | Managed by Aspire |
| React | 19.x | With TypeScript 5.x |
| Vite | 6.x | Build tooling |
| xUnit | 2.x | Test framework |
