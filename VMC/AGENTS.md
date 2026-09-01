# VMC Operator HMI — Build & Run Guide

## Project Structure

```
VMC/
├── backend/           # ASP.NET Core backend (Clean Architecture)
│   ├── src/
│   │   ├── VmcHmi.Domain/          # Entities, enums, domain rules
│   │   ├── VmcHmi.Application/     # Commands, queries, DTOs, validators
│   │   ├── VmcHmi.Infrastructure/   # EF Core, repos, auth, logging
│   │   └── VmcHmi.Api/              # Controllers, middleware, Program.cs
│   └── tests/
│       ├── VmcHmi.Domain.Tests/
│       └── VmcHmi.Application.Tests/
└── frontend/           # React + TypeScript frontend
    └── src/
        ├── api/          # API client, auth, session services
        ├── auth/         # Auth context, LoginPage
        ├── features/     # 5 stage components
        ├── components/   # Shared UI components
        ├── state/        # Zustand store
        ├── types/        # TypeScript interfaces
        ├── App.tsx
        └── main.tsx
```

## Build Commands

### Backend
```bash
cd VMC/backend
dotnet build
dotnet test                         # Run unit tests
dotnet run --project src/VmcHmi.Api # Start backend (http://localhost:5000)
```

### Frontend
```bash
cd VMC/frontend
npm install
npm run dev                          # Start dev server (http://localhost:5173)
npm run build                        # Production build
```

## Docker Compose
```bash
cd VMC
cp .env.example .env
docker-compose up --build
# Backend: http://localhost:5000
# Frontend: http://localhost:80
# Swagger UI: http://localhost:5000/swagger
```

## Demo Credentials
- **Username**: `operator`
- **Password**: `Operator@123`

## Backend Version Notes
- All EF Core packages are aligned to **9.0.0** (`Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`). Do NOT mix 8.0.x EF packages with the 9.0.0 Npgsql provider — it causes a `TypeLoadException` when generating migrations.
- The `dotnet-ef` global tool must be **9.0.x** (not 8.0.11).
- Base image is `.NET 8.0` SDK/runtime; EF Core 9 targets `net8.0`, so this is compatible.
- Migrations live in `backend/src/VmcHmi.Infrastructure/Migrations` (`InitialCreate`).

### Add a new migration
```bash
cd VMC/backend
dotnet ef migrations add <Name> \
  --project src/VmcHmi.Infrastructure \
  --startup-project src/VmcHmi.Api \
  --output-dir Migrations
```

## Frontend Notes
- `npm run build` runs `tsc && vite build`. `tsconfig.json` MUST keep `"jsx": "react-jsx"` (and `"strict": true`), otherwise `.tsx` files fail with "Cannot use JSX unless the '--jsx' flag is provided".
- `verbatimModuleSyntax: true` is on → type-only imports need `import type`.

## Docker Notes
- The Docker build context and repo root is the `VMC/` folder itself. `Dockerfile.backend` references `backend/...` paths; `docker-compose.yml` uses `context: ./frontend`.
- Frontend Docker build uses `npm ci` then `npm run build` (full install, not `--only=production`, because the build runs `tsc`).

## Deployment (Render.com)
- See `DEPLOYMENT.md` for the full guide. `render.yaml` is a Render Blueprint that provisions:
  - `vmc-hmi-db` managed Postgres
  - `vmc-hmi-backend` (Docker, `Dockerfile.backend`)
  - `vmc-hmi-frontend` (static site, publish `frontend/dist`)
- Backend auto-applies EF migrations + seed on startup (`SeedData.EnsureSeededAsync` → `MigrateAsync`), so no separate migration job is needed.

## API Endpoints
| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/auth/login` | Authenticate, get JWT |
| GET | `/api/session/current` | Get current session state |
| POST | `/api/session/checklist/{itemId}/confirm` | Confirm checklist item |
| POST | `/api/session/checklist/{itemId}/unconfirm` | Unconfirm checklist item |
| POST | `/api/session/advance` | Advance to next stage |
| POST | `/api/session/operation/start` | Start operation |
| POST | `/api/session/operation/stop` | Stop operation |
