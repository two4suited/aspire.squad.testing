# Setup Guide

## System Requirements

### Minimum Requirements
- **.NET 10 SDK** or later - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** - [Download](https://nodejs.org)
- **Git** - [Download](https://git-scm.com)

### Optional (for local Cosmos DB/Redis)
- **Docker** & **Docker Compose** - [Download](https://www.docker.com)

## Local Development Setup

### 1. Clone Repository

```bash
git clone https://github.com/two4suited/aspire.squad.testing.git
cd aspire.squad.testing
```

### 2. Backend Setup

#### Install Dependencies
```bash
cd src
dotnet restore
```

#### Verify Build
```bash
dotnet build
```

#### Configure Environment

Create `.env` file in `src/DogTeams.AppHost/`:

```env
# JWT Configuration
JWT_SECRET_KEY=your-super-secret-key-minimum-32-characters-long
JWT_ISSUER=DogTeamsAPI
JWT_AUDIENCE=DogTeamsApp

# Cosmos DB
COSMOSDB_CONNECTIONSTRING=AccountEndpoint=https://localhost:8081/;AccountKey=your-key
COSMOSDB_DATABASE=dogteams

# Redis
REDIS_CONNECTIONSTRING=localhost:6379

# CORS
CORS_ORIGINS=http://localhost:5173,http://localhost:3000

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

#### Start Backend

```bash
# Option 1: Using Aspire Dashboard
dotnet run --project DogTeams.AppHost

# Option 2: Standard run
cd DogTeams.Api
dotnet run

# Option 3: Watch mode (auto-restart on changes)
dotnet watch run
```

Backend will be available at:
- API: http://localhost:5000/api
- Swagger: http://localhost:5000/swagger (if enabled)

### 3. Frontend Setup

#### Install Dependencies
```bash
cd src/DogTeams.Web/ClientApp
npm install
```

#### Configure Environment

Create `.env` file in `src/DogTeams.Web/ClientApp/`:

```env
VITE_API_URL=http://localhost:5000/api
VITE_APP_NAME=Dog Teams
VITE_APP_VERSION=1.0.0
```

#### Development Server
```bash
npm run dev
```

Frontend available at: http://localhost:5173

#### Build for Production
```bash
npm run build
```

Output in `dist/` directory.

### 4. Local Database & Cache Setup

#### Option A: Docker Compose (Recommended)

Create `docker-compose.yml` in project root:

```yaml
version: '3.8'

services:
  cosmosdb:
    image: mcr.microsoft.com/cosmosdb/linux/latest
    ports:
      - "8081:8081"
    environment:
      COSMOS_DB_CONN_STR: AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPSQcjKF0a50XlO97IHlkMqXog==

  redis:
    image: redis:alpine
    ports:
      - "6379:6379"
    command: redis-server --appendonly yes
```

Start services:
```bash
docker-compose up -d
```

Stop services:
```bash
docker-compose down
```

#### Option B: Manual Installation

**Cosmos DB Emulator:**
- Download: https://aka.ms/cosmosdb-emulator
- Run emulator
- Connection string: `AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPSQcjKF0a50XlO97IHlkMqXog==`

**Redis:**
```bash
# macOS
brew install redis
redis-server

# Windows (WSL)
wsl
sudo apt-get install redis-server
redis-server

# Docker
docker run -d -p 6379:6379 redis:alpine
```

## Running the Application

### Simplified: Using Aspire (Recommended)

Aspire automatically orchestrates all services with a single command:

```bash
cd src
dotnet run --project DogTeams.AppHost
```

This starts everything:
- ✅ Backend API (http://localhost:5000)
- ✅ Frontend (http://localhost:5173)
- ✅ Cosmos DB Emulator
- ✅ Redis Cache
- ✅ Aspire Dashboard (https://localhost:17048)

**Wait 15-20 seconds for initialization, then:**

```bash
# Terminal 2: Verify services
curl http://localhost:5000/api/health
curl http://localhost:5173

# Terminal 3: Run tests
cd src/DogTeams.Web/ClientApp
npm run test:e2e
```

### Alternative: Manual Service Management (Advanced)

If you prefer to run services separately:

```bash
# Terminal 1: Backend only
cd src/DogTeams.Api
dotnet run

# Terminal 2: Frontend only
cd src/DogTeams.Web/ClientApp
npm start

# Terminal 3: Tests
npm run test:e2e
```

**Note:** Aspire is simpler and recommended for most development.

## Running Tests

```bash
# All backend tests
cd src
dotnet test DogTeams.sln

# All frontend tests
cd src/DogTeams.Web/ClientApp
npm test

# E2E tests (requires both services running)
npm run test:e2e
```

## Troubleshooting

### Backend Build Fails

```bash
# Clear cache
dotnet clean
rm -rf bin/ obj/

# Restore packages
dotnet restore

# Rebuild
dotnet build
```

### Port Already in Use

```bash
# Backend (5000)
netstat -ano | findstr :5000  # Windows
lsof -i :5000  # macOS/Linux
kill -9 <PID>

# Frontend (5173)
netstat -ano | findstr :5173
kill -9 <PID>
```

### Cosmos DB Connection Issues

```bash
# Verify emulator is running
curl https://localhost:8081/_explorer/index.html

# Check connection string
echo "AccountEndpoint=https://localhost:8081/;AccountKey=..."
```

### Redis Connection Issues

```bash
# Test connection
redis-cli ping
# Should return: PONG

# Check running services
docker ps | grep redis
```

### Frontend Build Issues

```bash
# Clear node modules
rm -rf node_modules package-lock.json

# Reinstall
npm install

# Rebuild
npm run build
```

### Token Expiration Issues

```bash
# In app, logout and login again
# Refresh tokens are automatically handled by auth service
```

## Environment Variables

### Backend (`src/DogTeams.AppHost/`)

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Development | Environment mode |
| `JWT_SECRET_KEY` | (required) | JWT signing key (min 32 chars) |
| `JWT_ISSUER` | DogTeamsAPI | Token issuer |
| `JWT_AUDIENCE` | DogTeamsApp | Token audience |
| `COSMOSDB_CONNECTIONSTRING` | localhost:8081 | Cosmos DB endpoint |
| `COSMOSDB_DATABASE` | dogteams | Database name |
| `REDIS_CONNECTIONSTRING` | localhost:6379 | Redis endpoint |
| `CORS_ORIGINS` | http://localhost:5173 | Allowed CORS origins |

### Frontend (`src/DogTeams.Web/ClientApp/`)

| Variable | Default | Description |
|----------|---------|-------------|
| `VITE_API_URL` | http://localhost:5000/api | Backend API endpoint |
| `VITE_APP_NAME` | Dog Teams | Application name |
| `VITE_APP_VERSION` | 1.0.0 | Version number |

## Development Workflow

### Branch Strategy

```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Make changes and commit
git add .
git commit -m "feat: description of changes"

# Push to GitHub
git push origin feature/your-feature-name

# Create Pull Request on GitHub

# After review, merge to main
git checkout main
git merge feature/your-feature-name
```

### Code Standards

- **C#**: Follow Microsoft's C# coding conventions
- **TypeScript/React**: Use Prettier for formatting
- **Comments**: Only for complex logic
- **Tests**: Aim for >80% coverage

### Pre-commit Checklist

- [ ] Code builds without errors
- [ ] All tests pass
- [ ] No console warnings/errors
- [ ] Code follows style guide
- [ ] Documentation updated

## Useful Commands

```bash
# Clean build
cd src && dotnet clean && dotnet build

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Format code
dotnet format

# Watch for file changes
dotnet watch run

# NPM tasks
npm run dev        # Development server
npm run build      # Production build
npm test           # Run unit tests
npm run test:e2e   # Run E2E tests
npm run test:ui    # UI test runner
npm run lint       # Run linter
npm run type-check # TypeScript check
```

## Production Deployment

See [DEPLOYMENT.md](./DEPLOYMENT.md) for production setup instructions.

## Getting Help

1. Check [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)
2. Review [API.md](./API.md) for endpoint details
3. See [TESTING.md](./TESTING.md) for test information
4. Check GitHub Issues for known problems
5. Open a new issue with details
