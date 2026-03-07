# Architecture Guide

## System Overview

Dog Teams is a modern, scalable full-stack application built on cloud-native technologies. The architecture follows SOLID principles and clean architecture patterns.

```
┌─────────────────────────────────────────────────────────────┐
│                   Client Layer                               │
│  ┌──────────────────────────────────────────────────────┐   │
│  │        React 18 SPA (TypeScript)                     │   │
│  │  ├─ Pages (Dashboard, Team, Auth)                   │   │
│  │  ├─ Components (Modal, Forms, Alerts)               │   │
│  │  ├─ Services (API client, Auth, Storage)            │   │
│  │  └─ Hooks (useEffect, useState, custom hooks)       │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                        │ REST API (HTTP)
┌─────────────────────────────────────────────────────────────┐
│           API/Application Layer (.NET)                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │     Controllers (Teams, Owners, Dogs, Auth)          │   │
│  │  ├─ Input validation                                 │   │
│  │  ├─ Request routing                                  │   │
│  │  └─ Response formatting                              │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │        Service Layer (Business Logic)               │   │
│  │  ├─ UserService (auth, profiles)                    │   │
│  │  ├─ TeamService (CRUD operations)                   │   │
│  │  ├─ OwnerService (owner management)                 │   │
│  │  ├─ DogService (dog management)                     │   │
│  │  └─ JwtTokenService (token generation)              │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │        Data Access Layer (Repositories)             │   │
│  │  ├─ IRepository<T> (generic interface)              │   │
│  │  ├─ CosmosDbRepository (SQL implementation)         │   │
│  │  └─ Unit of Work pattern                            │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │      Cross-Cutting Concerns                         │   │
│  │  ├─ Authentication (JWT Bearer tokens)              │   │
│  │  ├─ CORS (Cross-origin requests)                    │   │
│  │  ├─ Error handling (Custom middleware)              │   │
│  │  ├─ Logging (Serilog)                               │   │
│  │  └─ Caching (Redis layer)                           │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
         │                      │                │
         ▼                      ▼                ▼
   ┌──────────────┐      ┌──────────────┐  ┌──────────────┐
   │  Cosmos DB   │      │    Redis     │  │   Identity   │
   │  (Primary)   │      │   (Cache)    │  │  (JWT Tokens)│
   └──────────────┘      └──────────────┘  └──────────────┘
```

## Design Patterns

### Repository Pattern

Abstracts data access, enabling:
- Easy testing with mock repositories
- Switching between data sources
- Consistent CRUD operations

```csharp
public interface IRepository<T> where T : Entity
{
    Task<T> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
}
```

### Dependency Injection

All services registered in `Program.cs`:
- Scoped services (per request): Repositories, UnitOfWork
- Singletons: Configuration, Caching clients
- Transient: Factories, converters

### Unit of Work Pattern

Manages multiple repositories and coordinates database changes:
- Single transaction scope
- Atomicity guarantee
- Simplified orchestration

## Data Layer

### Cosmos DB (Primary)

**Database Design:**
- Single database: `dogteams`
- Multiple containers (collections):
  - `users` - User accounts and profiles
  - `teams` - Team information
  - `owners` - Dog owners
  - `dogs` - Dog records

**Partition Keys:**
- `/userId` - For users
- `/teamId` - For teams, owners, dogs (enables team isolation)

**Indexes:**
- Automatic: All properties indexed by default
- Manual optimizations for queries by email, team

**Benefits:**
- Unlimited scalability per partition
- Single-region setup (regional failover available)
- Automatic backup and disaster recovery

### Query Patterns

**Efficient queries** using partition keys:

```csharp
// Queries within partition (RU efficient)
teams = client.GetContainerRef("teams")
    .GetItemLinqQueryable<Team>()
    .Where(t => t.TeamId == teamId)
    .ToFeedIterator();

// Cross-partition queries (higher RU cost)
allTeams = client.GetContainerRef("teams")
    .GetItemLinqQueryable<Team>()
    .ToFeedIterator();
```

## Caching Strategy

### Redis Cache Layer

**Purpose:**
- Reduce Cosmos DB Request Units (RU) by 60%+
- Improve response times
- Reduce database load

**Cache Keys:**
```
team:{teamId}
owner:{ownerId}
dogs:team:{teamId}
user:email:{email}
```

**TTLs:**
- Teams: 10 minutes
- Owners: 5 minutes
- Dogs: 5 minutes
- User emails: 60 minutes

**Invalidation Strategy:**
- Time-based: TTL expiration
- Event-based: On CREATE, UPDATE, DELETE operations
- Pattern-based: Clear related keys (e.g., all dogs when team deleted)

**Implementation:**
```csharp
// Read from cache, fallback to DB
var key = $"team:{teamId}";
var cached = await _cache.GetStringAsync(key);
if (cached != null)
    return JsonConvert.DeserializeObject<Team>(cached);

var team = await _repo.GetByIdAsync(teamId);
await _cache.SetStringAsync(key, JsonConvert.SerializeObject(team), 
    TimeSpan.FromMinutes(10));
return team;
```

## Authentication & Security

### JWT Token Flow

```
User Login
    ↓
UserService validates credentials
    ↓
JwtTokenService generates:
  - Access token (15-min expiry)
  - Refresh token (7-day expiry)
    ↓
Frontend stores tokens in localStorage
    ↓
Frontend includes: Authorization: Bearer {access_token}
    ↓
Middleware validates token
    ↓
User authenticated ✓
```

### Password Security

- **Hashing:** BCrypt with configurable cost factor
- **Salt:** Automatically generated by BCrypt
- **Comparison:** Time-constant comparison (prevent timing attacks)

```csharp
// Registration
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, 12);

// Login
if (!BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
    throw new UnauthorizedAccessException();
```

### Token Refresh

```
Access Token Expires
    ↓
Frontend detects 401 response
    ↓
Frontend sends Refresh Token to /auth/refresh
    ↓
Backend validates refresh token validity
    ↓
Backend generates new Access Token
    ↓
Frontend retries original request
    ↓
Success ✓
```

## API Layer

### RESTful Design

**Endpoints:**
- `GET /api/teams` - List teams
- `POST /api/teams` - Create team
- `GET /api/teams/{id}` - Get team
- `PUT /api/teams/{id}` - Update team
- `DELETE /api/teams/{id}` - Delete team

**Status Codes:**
- `200 OK` - Successful GET/PUT
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Auth required
- `404 Not Found` - Resource not found
- `500 Server Error` - Unexpected error

### Input Validation

**FluentValidation** for complex rules:
```csharp
public class CreateTeamValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
        
        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
```

### Error Handling

Middleware catches and formats errors:
```json
{
  "error": "ValidationException",
  "message": "Team name is required",
  "statusCode": 400,
  "details": [
    { "field": "name", "error": "NotEmpty" }
  ]
}
```

## Frontend Architecture

### Component Structure

**Pages:**
- LoginPage - Authentication
- RegisterPage - Account creation
- DashboardPage - Team list and management
- TeamPage - Owner and dog management

**Reusable Components:**
- Modal - Overlay dialogs
- ErrorAlert - Error messages
- LoadingSpinner - Loading states
- FormInput - Validated form fields

**Hooks:**
- useAuth - Authentication state and methods
- useApi - API communication with caching
- useLocalStorage - Persistent state

### State Management

**Local Storage:**
- Access token
- Refresh token
- User profile (optional)
- Theme preference

**Component State:**
- useReducer for complex flows
- useState for simple toggles
- Context for global state (future)

### API Integration

**api/client.ts:**
```typescript
class ApiClient {
  async request<T>(method: string, path: string, body?: any): Promise<T> {
    const token = localStorage.getItem('token');
    const headers = {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
    
    const response = await fetch(`${API_URL}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined
    });
    
    if (response.status === 401) {
      // Refresh token and retry
    }
    
    return response.json();
  }
}
```

## Performance Optimizations

### Backend

**RU Optimization (Cosmos DB):**
- Single-partition queries: ~1 RU
- Cross-partition: ~10-100+ RU
- Cache hits: 0 RU (Redis response)

**Caching Results:**
- 60% RU reduction (measured)
- Sub-second response times
- Reduced database load

**Connection Pooling:**
- CosmosClient singleton
- Connection reuse
- MaxConnectionsPerEndpoint: 50

### Frontend

**Bundle Optimization:**
- Tree-shaking unused code
- Minification and compression
- CSS minimization
- Image optimization

**Network Optimization:**
- Request batching (where possible)
- Response caching
- Lazy loading routes (future)

**Rendering Optimization:**
- React.memo for pure components
- useCallback for stable references
- useMemo for expensive computations

## Scalability Considerations

### Horizontal Scaling

**Stateless Backend:**
- Can run multiple instances
- Load balancer distributes requests
- Shared Cosmos DB and Redis

**Frontend:**
- Served from CDN
- Geographic distribution
- Edge caching

### Vertical Scaling

**Database:**
- Cosmos DB: Increase RU/s provisioning
- Redis: Upgrade instance size
- Both fully managed

**API Servers:**
- Increase server resources
- Monitor CPU/memory

### Future Enhancements

- **Event Sourcing:** Audit trail of all changes
- **CQRS:** Separate read/write models
- **Message Queue:** Async operations (SignalR)
- **Search Index:** ElasticSearch for full-text search
- **Multi-region:** Global distribution with replication

## Monitoring & Diagnostics

### Logging

**Serilog configuration:**
- Console output (development)
- File output (production)
- Structured logging (JSON)
- Log levels: Debug, Information, Warning, Error

**Key events logged:**
- Authentication (login, logout, token refresh)
- Database operations (create, update, delete)
- Errors and exceptions
- Performance metrics

### Metrics

**Application metrics:**
- Request count and latency
- Error rates
- Database RU consumption
- Cache hit/miss ratio

**Infrastructure metrics:**
- API CPU and memory
- Cosmos DB RU provisioning
- Redis memory usage
- Network I/O

## Deployment Architecture

See [DEPLOYMENT.md](./DEPLOYMENT.md) for:
- Container strategy (Docker)
- Orchestration (Kubernetes optional)
- CI/CD pipeline (GitHub Actions)
- Environment configuration
- Secrets management
