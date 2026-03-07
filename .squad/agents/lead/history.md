# Project Context

- **Owner:** Brian Sheridan
- **Project:** aspire.squad.testing — Web app with React frontend, .NET Core backend, Cosmos DB, Redis, and authentication
- **Stack:** React, TypeScript, .NET Core Web API, Cosmos DB, Redis, .NET Aspire (aspire.dev)
- **Created:** 2026-03-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

- **2025-07-18:** .NET Aspire 9.1 maps to NuGet package version 13.1.x (latest 13.1.2). The product version and NuGet version diverge — always reference both.
- **2025-07-18:** Designed single-container Cosmos DB model (`dog-teams`) with `/teamId` partition key. Team, Owner, Dog are separate documents differentiated by `type` discriminator. Identity data lives in a separate `identity` container with `/userId` partition key.
- **2025-07-18:** Chose Dogs as separate documents (not embedded in Owner) — unbounded arrays are a Cosmos anti-pattern, and dogs have independent CRUD lifecycle.
- **2025-07-18:** Auth approach: ASP.NET Core Identity with custom Cosmos DB user store + JWT bearer tokens. API serves as its own auth server. No external identity provider for v1.
- **2025-07-18:** Redis is for read-through caching and refresh token storage only — no server-side sessions. JWT is the session.
- **2025-07-18:** Frontend auth state managed via React Context + `useAuth` hook — no Redux/Zustand needed for this scope.
- **2025-07-18:** Full architecture proposal written to `ARCHITECTURE.md`. Open questions captured: team creation flow, invite system, dog transfers.
