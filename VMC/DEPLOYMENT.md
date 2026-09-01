# Deploying VMC Operator HMI to Render.com

This app can be deployed to Render using the included `render.yaml` blueprint.
It provisions three resources: a managed PostgreSQL database, the ASP.NET Core
backend (Docker), and the React frontend (static site).

## Prerequisites

- A free [Render.com](https://render.com) account.
- The project pushed to a GitHub (or GitLab) repository.
- The repo root must contain `render.yaml`, `Dockerfile.backend`,
  `frontend/`, and `backend/` (i.e. the whole `VMC` folder contents at the repo root).

## Option A — Blueprint (recommended, one-click)

1. Push this project to a GitHub repo.
2. In Render: **New -> Blueprint**.
3. Select the repo. Render reads `render.yaml` and provisions:
   - `vmc-hmi-db` — managed PostgreSQL (schema/db: `hmi`)
   - `vmc-hmi-backend` — API service (auto-applies DB migrations on boot)
   - `vmc-hmi-frontend` — static site
4. Click **Apply** and wait for all services to deploy.
5. Open the deployed frontend URL (e.g. `https://vmc-hmi-frontend-xxxx.onrender.com`).

> If your Render service names are auto-suffixed (e.g. `vmc-hmi-backend-abc123`),
> the `FrontendUrl` and `VITE_API_BASE_URL` hard-coded values below will be stale.
> Update them in Render (Environment) to the actual URLs.

## Option B — Manual services

1. **Database**: New -> PostgreSQL. Note the internal connection string.
2. **Backend**: New -> Web Service -> Docker. Configure:
   - Build/root directory = repo root, Dockerfile = `Dockerfile.backend`
   - Env vars:
     - `ASPNETCORE_ENVIRONMENT=Production`
     - `ASPNETCORE_URLS=http://+:80`
     - `JWT__SIGNINGKEY=<long random string>`
     - `ConnectionStrings__DefaultConnection=<Postgres internal connection string>`
     - `FrontendUrl=https://<your-frontend-url>.onrender.com`
   - The backend runs `MigrateAsync()` at startup, so migrations/seeds apply automatically.
3. **Frontend**: New -> Static Site. Configure:
   - Build command: `cd frontend && npm ci && npm run build`
   - Publish directory: `frontend/dist`
   - Env var (build time): `VITE_API_BASE_URL=https://<your-backend-url>.onrender.com`
4. Add a `/*` header with `Cache-Control: no-cache` on the static site to avoid stale SPA loads.

## After deploy

- Login with demo credentials:
  - Username: `operator`
  - Password: `Operator@123`
- API docs (Swagger) at `https://<backend-url>/swagger`

## Security notes (demo)

- The Postgres `ipAllowList` is open to `0.0.0.0/0` and the JWT key is auto-generated.
  For a production rollout, restrict the DB IP allow list, rotate the JWT key,
  and enable Render's built-in authentication on the static site.
