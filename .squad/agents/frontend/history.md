# Project Context

- **Owner:** Brian Sheridan
- **Project:** aspire.squad.testing — Web app with React frontend, .NET Core backend, Cosmos DB, Redis, and authentication
- **Stack:** React, TypeScript, .NET Core Web API, Cosmos DB, Redis, .NET Aspire (aspire.dev)
- **Created:** 2026-03-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-07: React app lives at src/DogTeams.Web/ClientApp
`dotnet new react` template was unavailable; the ClientApp was scaffolded manually using Vite + React 18 + TypeScript. The folder `src/DogTeams.Web/ClientApp/` is the canonical React root. If the Aspire agent later generates a .csproj wrapping this ClientApp, it should reference the same folder without regenerating it.

### 2026-03-07: Auth uses JWT stored in localStorage
Token stored under key `dogteams_token`, full user object under `dogteams_user`. AuthContext provides `login`, `register`, `logout`. `RequireAuth` wrapper component redirects unauthenticated users to `/login`.

### 2026-03-07: API client uses relative paths + Vite proxy
`src/api/client.ts` uses relative `/api/…` paths. In dev, Vite proxies `/api` to the backend URL (read from Aspire env vars). In production, the .NET host serves the SPA and handles `/api` routing directly. `VITE_API_URL` can override the base URL.

### 2026-03-07: Domain model: User → Owner → Team; Owner → [Dogs]
The logged-in user IS an Owner. The relationship chain drives routing: dashboard shows teams, `/teams/:id` shows owners and their dogs per team.

