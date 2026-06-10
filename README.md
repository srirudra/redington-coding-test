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

## Known limitations

- No authentication, persistence, or rate limiting — intentionally out of scope for this exercise.
- Audit records are file-based; a production system would use a centralized, queryable sink.
- Container support and CI are follow-on work.
