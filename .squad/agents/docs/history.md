# Docs — History & Learnings

## Project Context
**Project:** Dog Teams — NAFA flyball race tracking web application
**User:** Brian
**Stack:** React 18 + TypeScript (Vite), .NET 10 Web API, Azure Cosmos DB (Linux emulator), Redis, .NET Aspire 9.1
**Repo root:** /Users/brian/Source/aspire.squad.testing
**Solution:** src/DogTeams.sln (5 projects: AppHost, ServiceDefaults, Api, Web, Tests)

## Domain Summary
- **Clubs** have NAFA club numbers and field one or more Teams
- **Teams** have 4–6 dogs + handlers per race
- **Dogs** have CRN (both NAFA-assigned NafaCrn and internal app ID), withers height, jump height measurement (Temporary/Permanent)
- **Tournaments** contain Heats → HeatResults per dog → RaceRecords
- **Points** are materialized on Dog.LifetimePoints, updated synchronously on result entry
- **Titles** (FD → FGDCh-G for Regular; MBD → MBGDCh for Multibreed) are auto-awarded at thresholds
- **Iron Dog** = 1+ point in 10 consecutive racing years (NAFA calendar)
- **Regional points** = 1st/2nd/3rd placement per tournament, max 30/year, 80% tournament week rule
- **Tournament Director** role required to enter race results (elevated permissions)
- **Historical import** required — bulk import endpoint planned in FB-7

## Sprint Assignments
- **FB-8**: Full user manual with annotated screenshots (depends on FB-6 UI complete)

## Learnings
- Joined team 2026-03-07 to own FB-8 documentation sprint
