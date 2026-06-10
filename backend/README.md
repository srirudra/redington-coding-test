# Probability Calculator — Backend

ASP.NET Core Minimal API that calculates a result from two probabilities using one of two operations. This document is for engineers and QA who need to run, test, and validate the backend.

## What it does

Given two probabilities `A` and `B` (each in the inclusive range `[0, 1]`) and an operation, it returns a calculated probability:

- `CombinedWith`: $P(A) \times P(B)$
- `Either`: $P(A) + P(B) - P(A) \times P(B)$

The `[0, 1]` rule is enforced in two places: user-facing field validation at the API boundary, and a domain invariant inside the `Probability` value object.

## Tech stack

- .NET 9 / C# (`net9.0`)
- ASP.NET Core Minimal API
- FluentValidation for boundary validation
- Serilog for structured logging (console + rolling files)
- xUnit + `Microsoft.AspNetCore.Mvc.Testing` for tests

## Project layout

``` text
backend/
  Redington.ProbabilityCalculator.sln
  src/
    Redington.ProbabilityCalculator.Domain/        # Probability value object, calculation strategies
    Redington.ProbabilityCalculator.Application/   # CalculationService, audit logging abstraction
    Redington.ProbabilityCalculator.Api/           # Minimal API host, DTO, validation, middleware
  tests/
    Redington.ProbabilityCalculator.Domain.Tests/
    Redington.ProbabilityCalculator.Application.Tests/
    Redington.ProbabilityCalculator.Api.Tests/
```

Dependencies flow one way: `Api` → `Application` → `Domain`. Each operation is a separate strategy (`CombinedWithCalculation`, `EitherCalculation`) selected by `CalculationType`, so a new operation can be added without modifying existing ones.

## Prerequisites

- [.NET SDK 9.0](https://dotnet.microsoft.com/download)

Verify with:

```pwsh
dotnet --version
```

## Run locally

From the `backend` folder:

```pwsh
dotnet run --project src/Redington.ProbabilityCalculator.Api
```

Default endpoints (from `launchSettings.json`):

- HTTP: `http://localhost:3000`
- HTTPS: `https://localhost:3443`

To force the HTTPS profile:

```pwsh
dotnet run --project src/Redington.ProbabilityCalculator.Api --launch-profile https
```

## Configuration

All settings live in `appsettings.json` and can be overridden per environment or via environment variables (ASP.NET Core's standard hierarchical key syntax: `Section__Key`).

### CORS allowed origins

`Cors:AllowedOrigins` — the list of origins the API will accept cross-origin requests from.

Default (set in `appsettings.json`):

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5173",
    "http://localhost:4173"
  ]
}
```

These defaults cover the Vite dev server (`5173`) and preview server (`4173`) so the frontend works out of the box without any extra setup. Override for other environments:

```json
// appsettings.Production.json
"Cors": {
  "AllowedOrigins": [ "https://your-deployed-frontend.example.com" ]
}
```

Or as an environment variable:

```pwsh
$env:Cors__AllowedOrigins__0 = "https://your-deployed-frontend.example.com"
```

## API

### `POST /api/calculations`

Request body:

```json
{
  "firstProbability": 0.5,
  "secondProbability": 0.4,
  "calculationType": "CombinedWith"
}
```

- `firstProbability`, `secondProbability`: decimals in `[0, 1]`.
- `calculationType`: string enum — `CombinedWith` or `Either`.

Success response (`200 OK`):

```json
{
  "firstProbability": 0.5,
  "secondProbability": 0.4,
  "calculationType": "CombinedWith",
  "result": 0.2,
  "calculatedAtUtc": "2026-06-10T10:19:53.0535573Z"
}
```

Validation failure (`400 Bad Request`) returns an RFC 7807 `ValidationProblemDetails` with field-specific messages, for example when a probability is outside `[0, 1]` or the calculation type is unsupported.

Example with `curl`:

```pwsh
curl -X POST http://localhost:3000/api/calculations `
  -H "Content-Type: application/json" `
  -d '{"firstProbability":0.5,"secondProbability":0.4,"calculationType":"CombinedWith"}'
```

### `GET /health`

Liveness/readiness probe. Returns `200 OK` with body `Healthy` when the service is up.

## Test

Run the full suite from the `backend` folder:

```pwsh
dotnet test
```

Tests are tagged by category, so you can scope runs:

```pwsh
# Fast unit tests (Domain, Application, validators)
dotnet test --filter "Category=Unit"

# API integration tests (in-memory host via WebApplicationFactory)
dotnet test --filter "Category=Integration"
```

Coverage focus:

- Domain: `Probability` invariant and both calculation strategies.
- Application: strategy selection, result shape, and unsupported-type handling.
- Api: endpoint contract (200/400), request validation, and `/health`.

## Logging

Serilog writes to the console and to rolling daily files under a `logs/` folder relative to the working directory:

- `app-<date>.log` — operational logs (request summaries, validation rejections, errors).
- `calculations-audit-<date>.log` — calculation audit records only (one entry per successful calculation), retained for 90 files.

Routing is configured in `appsettings.json`. Audit events are tagged with an `EventType` property and split into the dedicated audit file via a conditional sink, so operational noise and the business audit trail stay separate. The `logs/` folder is git-ignored.

## Design choices

- **Layered with one-way references** keeps the domain free of framework concerns and testable in isolation.
- **Strategy per operation** favors open/closed extension over branching logic.
- **Validation split**: boundary validation returns friendly field errors; the `Probability` value object still enforces the invariant so the domain cannot be constructed in an invalid state. The validator reuses `Probability.IsValid` to avoid duplicating the rule.
- **Audit logging behind an interface** (`ICalculationAuditLogger`) decouples the Application layer from the logging backend; the current implementation emits structured events through `Microsoft.Extensions.Logging`, which Serilog routes.

## Known limitations and trade-offs

- No authentication, persistence, or rate limiting — intentionally out of scope for this exercise.
- Audit records are file-based; a production system would typically use a centralized/queryable sink.
- The operational log file is currently unbounded (no retention cap), unlike the audit file.
- CORS allowed origins default to the local Vite dev and preview ports and are configurable via `Cors:AllowedOrigins` in `appsettings.json` (see [Configuration](#configuration)); no code change is needed to add a deployed frontend origin.
