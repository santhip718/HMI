# VMC Operator HMI — Project Specification
**Primeform Labs Technical Assignment — Startup & Operation Workflow**

Stack: **React (frontend) + ASP.NET Core (Clean Architecture, backend) + PostgreSQL (persistence)**

---

## 1. Requirement Analysis

The assignment asks for a responsive, single-operator HMI that guides a machine operator through a fixed
sequence after power-on, one stage at a time, before allowing a simulated operation start/stop.

Non-negotiable constraints from the brief:
- Single user role — **operator only**, no admin, no multi-user features.
- One stage visible at a time; no unrelated menus, no extra navigation.
- Progression is gated: a stage cannot be left until every item in it is confirmed.
- Simple persistence is required (state must survive a refresh / reconnect).
- Must end up hosted at a live URL with demo login credentials if access is restricted.

Because "no admin" is explicit, authentication exists only to identify *the* operator session (single role,
single scope) — not for authorization tiers.

---

## 2. Candidate Mock Scenario (assumed data)

Per Section 1 of the brief ("define one mock scenario, preload the values"), the following is invented and
preloaded — no order-creation flow is implemented.

| Field | Value |
|---|---|
| Operation | CNC Milling — Aluminum Mounting Bracket |
| Quantity | 25 units |
| Material | Aluminum 6061-T6 |
| Drawing Revision | Rev C |
| CNC Program | O1042, Rev 3 |
| Fixture | Custom fixture plate FX-118, mounted on Vise Station 2 |
| Work Offset | G54 |
| Required Tools | T01 — Face Mill 50mm · T02 — End Mill 10mm (4-flute) · T03 — Drill 8mm |
| Workpiece Orientation | Datum face against fixed jaw, pocket face up |
| Clamping Instruction | Torque clamp to 25 Nm, confirm zero gap at datum face |

This scenario is seeded into the database on first run (a seed/migration), not entered by the operator.

---

## 3. Workflow (state machine)

```
POWER ON
   │
   ▼
MACHINE_CHECKS  ──(all items confirmed)──▶  TOOLS
   │                                          │
   │                                          ▼
   │                                    (all tools confirmed)
   │                                          │
   │                                          ▼
   │                                     WORKPIECE ──(all confirmed)──▶ READY_REVIEW
   │                                                                         │
   │                                                                         ▼
   │                                                              (operator proceeds)
   │                                                                         │
   │                                                                         ▼
   └────────────────────────────────────────────────────────────────▶  OPERATION
                                                                     READY ⇄ RUNNING ⇄ STOPPED
```

Each stage transition and each confirm/start/stop action is persisted immediately, so a page refresh resumes
at the exact stage and item state — this satisfies "simple persistence" without over-engineering it into a
full audit/event system (out of scope for this assignment, but the schema leaves room for it).

---

## 4. Architecture Overview

### 4.1 Backend — ASP.NET Core, Clean Architecture

```
/backend
 ├─ src/
 │   ├─ VmcHmi.Domain/            // Entities, enums, value objects, domain rules — no dependencies
 │   ├─ VmcHmi.Application/       // Use cases (CQRS-style), interfaces, DTOs, validators
 │   ├─ VmcHmi.Infrastructure/    // EF Core + Npgsql, repositories, auth (JWT), logging setup
 │   └─ VmcHmi.Api/               // Controllers, middleware, DI composition root, Program.cs
 └─ tests/
     ├─ VmcHmi.Application.Tests/
     └─ VmcHmi.Api.Tests/
```

**Dependency rule:** Domain has zero references. Application references Domain only. Infrastructure
references Application (implements its interfaces). Api references Application + Infrastructure (composition
only, at startup).

**Domain layer**
- Entities: `MachineSession`, `ChecklistItem` (stage: MachineCheck/Tool/Workpiece), `Tool`, `OperationRun`.
- Enums: `StageType { MachineChecks, Tools, Workpiece, ReadyReview, Operation }`,
  `OperationStatus { Ready, Running, Stopped }`.
- Invariant enforced in-domain: a stage cannot be marked complete while any of its `ChecklistItem`s are
  unconfirmed; `OperationRun` cannot transition to `Running` unless the session's current stage is
  `ReadyReview` or beyond.

**Application layer**
- Use cases as request/handler pairs (MediatR-style), e.g. `ConfirmChecklistItemCommand`,
  `AdvanceStageCommand`, `StartOperationCommand`, `StopOperationCommand`, `GetCurrentStateQuery`.
- FluentValidation validators guard input at this boundary.
- `IMachineSessionRepository`, `ICurrentUserService`, `IAppLogger<T>` interfaces defined here,
  implemented in Infrastructure.

**Infrastructure layer**
- EF Core `DbContext` (Npgsql provider) + migrations.
- Repository implementations.
- `Serilog` configured here (console + file sink; structured logging with request correlation ID).
- JWT token generation/validation, password hashing (`BCrypt.Net` or ASP.NET Identity's hasher).

**Api layer**
- Thin controllers calling Application handlers only (no business logic in controllers).
- Middleware: global exception handler → problem-details JSON, request logging middleware, JWT auth
  middleware, CORS policy scoped to the frontend origin.
- `Program.cs` wires DI, Serilog, EF Core, JWT, Swagger (dev only).

### 4.2 Authentication (single role — operator only)

- One role: `Operator`. No admin role, no role-based authorization branches — authentication exists purely
  to gate access to the session, per the brief's single-user assumption.
- Flow: `POST /api/auth/login` (username/password) → JWT (short-lived access token) returned →
  attached as `Authorization: Bearer <token>` on all subsequent calls.
- Demo credential seeded on startup (documented at the bottom of this file) so reviewers can log in
  immediately.
- Passwords hashed at rest; JWT signing key from configuration/environment (never hardcoded).

### 4.3 Logging

- **Serilog** in Infrastructure, sinks: Console (structured JSON) + rolling file.
- Logged: authentication attempts, every stage transition, every confirm action, every start/stop, and all
  unhandled exceptions (via the global exception middleware) with a correlation/request ID.
- No sensitive data (passwords, tokens) ever logged.

### 4.4 Database — PostgreSQL

```
users
 ├─ id (uuid, pk)
 ├─ username (unique)
 ├─ password_hash
 └─ created_at

machine_sessions
 ├─ id (uuid, pk)
 ├─ user_id (fk -> users)
 ├─ current_stage (enum: MachineChecks | Tools | Workpiece | ReadyReview | Operation)
 ├─ operation_status (enum: Ready | Running | Stopped, nullable until Operation stage)
 ├─ created_at
 └─ updated_at

checklist_items
 ├─ id (uuid, pk)
 ├─ session_id (fk -> machine_sessions)
 ├─ stage (enum: MachineChecks | Tools | Workpiece)
 ├─ label (text)              -- e.g. "E-stop released", "T02 End Mill 10mm inserted"
 ├─ sort_order (int)
 ├─ is_confirmed (bool)
 └─ confirmed_at (timestamp, nullable)

operation_runs
 ├─ id (uuid, pk)
 ├─ session_id (fk -> machine_sessions)
 ├─ status (enum: Ready | Running | Stopped)
 ├─ started_at (nullable)
 └─ stopped_at (nullable)
```

`checklist_items` are seeded per new session from the fixed mock scenario (Section 2) — the seed data is
static configuration, not something the operator authors.

### 4.4.1 Shared Server, Dedicated Schema

This project connects to the **same PostgreSQL server** already used by the lab project, but lives in its
own **schema** so the two applications never collide on table names or migrations.

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=learning_portal;Username=postgres;Password=Saraswathi@99;SearchPath=hmi"
}
```

- **Server / Database**: reuse `learning_portal` on `localhost:5432` — no new database is created.
- **Schema**: `hmi` — a dedicated schema inside `learning_portal` holding every table listed in Section 4.4
  (`users`, `machine_sessions`, `checklist_items`, `operation_runs`), fully isolated from the lab project's
  own tables/schema (typically `public`).
- **EF Core setup**: configure the schema once in `DbContext.OnModelCreating`:
  ```csharp
  modelBuilder.HasDefaultSchema("hmi");
  ```
  All migrations then generate under `hmi.*` automatically — no need to prefix every table manually.
- **Migration bootstrap**: `CREATE SCHEMA IF NOT EXISTS hmi;` runs once (either as the first EF Core
  migration or a startup check) before the first `dotnet ef database update`.
- **Security note**: the credentials above are development-only. Before this goes anywhere shared (a repo,
  a deployed environment), move the password out of `appsettings.json` into `appsettings.Development.json`
  (git-ignored) or environment variables / user-secrets, and rotate it if it was ever committed.

### 4.5 Frontend — React

```
/frontend
 ├─ src/
 │   ├─ api/            // axios/fetch client, auth token interceptor
 │   ├─ auth/            // login page, auth context, protected route wrapper
 │   ├─ features/
 │   │   ├─ machineChecks/
 │   │   ├─ tools/
 │   │   ├─ workpiece/
 │   │   ├─ readyReview/
 │   │   └─ operation/
 │   ├─ components/      // shared: StageHeader, ChecklistItemCard, PrimaryActionButton, StatusBadge
 │   ├─ state/            // React Context or Zustand store holding: currentStage, items, operationStatus
 │   ├─ App.tsx           // renders exactly ONE active stage component based on server state
 │   └─ main.tsx
 └─ index.html
```

Design principles carried into the frontend to match the brief:
- **One stage rendered at a time** — no sidebar/tab navigation; the visible component is driven directly by
  `currentStage` from the backend, not client-side routing choices.
- Large touch-friendly confirm buttons/status indicators (this is a shop-floor HMI, not a desktop admin UI).
- "Next" is disabled until every checklist item in the active stage is confirmed (mirrors the domain
  invariant — client-side is UX only, the server re-validates).
- On load, fetch current session state so a refresh resumes exactly where the operator left off.

---

## 5. API Surface

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/auth/login` | Authenticate operator, return JWT |
| GET | `/api/session/current` | Get current stage, checklist items, operation status |
| POST | `/api/session/checklist/{itemId}/confirm` | Confirm one checklist item |
| POST | `/api/session/advance` | Advance to next stage (server validates all items confirmed) |
| POST | `/api/session/operation/start` | READY → RUNNING |
| POST | `/api/session/operation/stop` | RUNNING → STOPPED |

All endpoints except `/api/auth/login` require a valid JWT.

---

## 6. Deployment Plan (to satisfy "live URL" requirement)

| Component | Suggested host | Notes |
|---|---|---|
| React frontend | Vercel or Netlify | Static build, env var for API base URL |
| ASP.NET backend | Render or Railway (Docker) | Dockerfile using `mcr.microsoft.com/dotnet/aspnet` runtime image |
| PostgreSQL | Neon or Supabase (managed Postgres) | Free tier sufficient for a review-only deployment |
| Secrets | Host-level environment variables | JWT signing key, DB connection string — never committed |

CORS on the backend must allow the deployed frontend origin. A `Dockerfile` + `docker-compose.yml` (API +
Postgres) should also be included locally so the project runs with a single `docker-compose up` for local
review.

---

## 7. Demo Credentials (to include with submission)

```
Username: operator
Password: <set at seed time, e.g. Operator@123>
```
(Document the actual seeded password once implemented; never leave a placeholder in the real submission.)

---

## 8. Build Order (suggested milestones)

1. Domain layer: entities, enums, invariants + unit tests.
2. Application layer: commands/queries, validators + unit tests.
3. Infrastructure: EF Core DbContext, PostgreSQL migrations, seed data, Serilog, JWT/auth.
4. Api layer: controllers, middleware, Swagger, CORS.
5. React: auth flow, state fetch, five stage components, shared UI kit.
6. Wire frontend to backend end-to-end; manual pass through the full POWER ON → RUNNING flow.
7. Dockerize both apps; deploy backend + Postgres + frontend; verify the live URL end-to-end.
8. Write the submission email with the link and demo credentials.

---

## 9. Naming Conventions

These are the baseline conventions for this project. **Once the lab project is shared, this section will be
revised to match its actual conventions exactly** — the intent is consistency across both codebases, not two
different house styles.

### 9.1 Backend (.NET / C#)

| Element | Convention | Example |
|---|---|---|
| Namespace | `VmcHmi.<Layer>[.<SubFolder>]` | `VmcHmi.Application.Sessions` |
| Project (folder) | `VmcHmi.<Layer>` | `VmcHmi.Infrastructure` |
| Class / Record | PascalCase | `MachineSession`, `ChecklistItem` |
| Interface | `I` + PascalCase | `IMachineSessionRepository` |
| Method | PascalCase, verb-first | `ConfirmChecklistItemAsync` |
| Async method | suffix `Async` | `GetCurrentStateAsync` |
| Private field | `_camelCase` | `_dbContext` |
| Local variable / parameter | camelCase | `sessionId`, `itemId` |
| Constant | PascalCase | `DefaultSchema` |
| Command (write use case) | `<Verb><Noun>Command` | `AdvanceStageCommand` |
| Query (read use case) | `Get<Noun>Query` | `GetCurrentStateQuery` |
| Handler | `<CommandOrQueryName>Handler` | `AdvanceStageCommandHandler` |
| DTO out of API | `<Noun>Response` | `SessionStateResponse` |
| DTO into API | `<Noun>Request` | `ConfirmItemRequest` |
| Controller | `<Noun>Controller` (plural resource) | `SessionController` |
| Enum | PascalCase, singular type name | `StageType.MachineChecks` |
| Database table | snake_case, plural | `checklist_items` |
| Database column | snake_case | `is_confirmed`, `created_at` |
| Schema | lowercase, short | `hmi` |
| Migration file | `<Timestamp>_<PascalCaseDescription>` | `20260830_InitialHmiSchema` |
| Config keys (appsettings) | PascalCase, colon-nested | `Jwt:SigningKey` |
| Environment variable override | UPPER_SNAKE with `__` nesting | `JWT__SIGNINGKEY` |

### 9.2 Frontend (React / TypeScript)

| Element | Convention | Example |
|---|---|---|
| Component file | PascalCase, `.tsx` | `MachineChecksStage.tsx` |
| Component name | PascalCase, matches filename | `MachineChecksStage` |
| Hook file / name | `use` + camelCase | `useSessionState.ts` |
| Non-component module | camelCase, `.ts` | `apiClient.ts` |
| Feature folder | camelCase, matches domain concept | `features/machineChecks/` |
| Context | PascalCase + `Context` suffix | `AuthContext.tsx` |
| Store / state slice | camelCase + `Store`/`Slice` suffix | `sessionStore.ts` |
| Props type | `<ComponentName>Props` | `MachineChecksStageProps` |
| Type / interface (data shape) | PascalCase | `ChecklistItem`, `SessionState` |
| Function / variable | camelCase | `confirmItem`, `currentStage` |
| Boolean variable | `is`/`has`/`can` prefix | `isConfirmed`, `canAdvance` |
| Constant (module-level, fixed) | UPPER_SNAKE_CASE | `API_BASE_URL` |
| Event handler prop/function | `handle` + PascalCase event | `handleConfirmClick` |
| CSS class (if not Tailwind/CSS-in-JS) | kebab-case, BEM-ish | `stage-header__title` |
| Environment variable (Vite) | `VITE_` prefix, UPPER_SNAKE | `VITE_API_BASE_URL` |
| Test file | same name + `.test.tsx` | `MachineChecksStage.test.tsx` |

## 10. Aligning with the Existing Lab Project

Once the lab project's frontend and backend are shared, this spec should be revisited to:
- Match its actual folder/layer structure (in case it isn't a strict Clean Architecture split).
- Match its actual naming patterns where they differ from Section 9 above (e.g. if it uses a different DTO
  suffixing style, a different auth approach, or a different React state-management library).
- Reuse any shared building blocks it already has (base repository, JWT setup, logging config, API client
  wrapper) instead of re-implementing them for the HMI.
- Confirm the `hmi` schema naming doesn't collide with anything the lab project already reserves.

## 11. Explicitly Out of Scope

- Order creation/acceptance workflow.
- Admin role or any multi-operator management.
- Additional menus, settings screens, or historical run reporting (schema allows it later, UI does not
  expose it now).
