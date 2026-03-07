### 2026-03-07T20:52:29Z: User directive

**By:** brian (via Copilot)

**What:** Coordinator should not pause at milestones or ask for permission to continue. Keep the work pipeline moving autonomously—spawn follow-up agents immediately when work is identified, don't wait for user confirmation between work items.

**Why:** User preference for continuous autonomous execution. Mirrors Ralph (Work Monitor) behavior. Eliminates friction between work phases.

**Scope:** All sessions going forward. Coordinator assumes user wants continuous pipeline unless explicitly told to idle/stop.

**Exception:** Still respect explicit user input (e.g., "Ralph, idle", "stop", direct questions requiring answers). Just don't pause between identified work items.
