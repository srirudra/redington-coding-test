# REDINGTON Probability Calculator

A C# and React coding test project that calculates a result from two probabilities using one of two operations:

- `CombinedWith`: $P(A) \times P(B)$
- `Either`: $P(A) + P(B) - P(A) \times P(B)$

## Architecture

| Layer | Technology |
|---|---|
| Frontend | React 18 + TypeScript, Vite, Vitest |
| Backend | ASP.NET Core 9 Minimal API, FluentValidation, Serilog |
| Domain | `Probability` value object, strategy per operation |

The `[0, 1]` invariant is enforced at the API boundary (field validation) and inside the `Probability` value object (domain invariant). Each operation is an independent strategy; adding a new one does not modify existing code.

For deeper detail see [backend/README.md](backend/README.md) and [frontend/README.md](frontend/README.md).

## Prerequisites

- [.NET SDK 9.0](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/) — version pinned in `.nvmrc`; use `nvm use` if you have nvm installed

## Quick start

```pwsh
npm install   # installs root deps and frontend deps (via postinstall)
npm start     # starts API on http://localhost:3000 and UI on http://localhost:5173
```

Both processes run concurrently; killing either stops the other.

## Run with Docker

Requires Docker Desktop (or a Docker Engine with Compose v2). From the repo root:

```pwsh
npm run docker      # build and start both containers
npm run docker:down # stop and remove them
```

This builds and runs two containers with no extra configuration:

- **UI** — nginx serving the built SPA at `http://localhost:8080`. API calls go to the same origin (`/api/...`) and nginx reverse-proxies them to the backend, so no CORS setup is needed.
- **API** — also published at `http://localhost:3000` for direct access (e.g. `curl`).

The frontend waits for the backend health check to pass before starting. Audit and operational logs persist in a named `backend-logs` volume. Separate Dockerfiles let the frontend and backend be built, scaled, and deployed independently in the cloud.

## Commands

| Command | What it does |
|---|---|
| `npm start` | Run API + UI concurrently |
| `npm run start:api` | Run API only |
| `npm run start:ui` | Run UI only |
| `npm test` | Run all backend and frontend tests |
| `npm run test:api` | Backend tests only |
| `npm run test:ui` | Frontend tests only |
| `npm run test:coverage` | Coverage for both; prints a text summary for the API |
| `npm run build` | Production build of API and UI |
| `npm run docker` | Build and run both containers via Docker Compose |
| `npm run docker:down` | Stop and remove the containers |

## Known limitations

- No authentication, persistence, or rate limiting — intentionally out of scope for this exercise.
- Audit records are file-based (persisted to a Docker volume when containerised); a production system would use a centralized, queryable sink.
- CI is follow-on work.
