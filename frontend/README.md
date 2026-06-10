# Probability Calculator — Frontend

React + TypeScript single-page app for the Probability Calculator. It collects two
probabilities and an operation, calls the backend API, and displays the result.

## What it does

- Enter two probabilities (each in the inclusive range `[0, 1]`).
- Choose an operation:
  - **Combined with**: `P(A) × P(B)`
  - **Either**: `P(A) + P(B) − P(A) × P(B)`
- Calculate and display the result returned by the backend.

Probability validation runs in two layers: immediate client-side checks for fast
feedback, and the backend remains the source of truth — its field-specific
validation errors are surfaced against the relevant input.

## Tech stack

- React 18 + TypeScript
- Vite 5 (dev server and build)
- Vitest + Testing Library (unit and component tests)
- ESLint

## Prerequisites

- Node.js 20.19+ or 22.12+ recommended (developed on Node 21).
- The backend API running locally (see [../backend/README.md](../backend/README.md)).

## Setup

```pwsh
npm install
```

## Configuration

The API base URL is read from `VITE_API_BASE_URL` and defaults to
`http://localhost:3000` (the backend's local HTTP endpoint). To override it,
copy `.env.example` to `.env` and edit the value:

```pwsh
Copy-Item .env.example .env
```

The backend's CORS policy allows the Vite dev origin `http://localhost:5173` and
the preview origin `http://localhost:4173`.

## Run locally

```pwsh
npm run dev
```

The dev server starts on `http://localhost:5173`. Start the backend first so
calculations succeed.

## Test

```pwsh
npm run test          # run once
npm run test:watch    # watch mode
npm run test:coverage # with coverage
```

## Build

```pwsh
npm run build     # type-check and produce a production bundle in dist/
npm run preview   # serve the production build locally
```

## Project layout

```text
frontend/
  index.html
  src/
    api/
      calculationApi.ts   # fetch client + typed errors
      types.ts            # request/response contract types
    components/
      CalculatorForm.tsx  # form, validation wiring, result display
    config.ts             # API base URL resolution
    validation.ts         # client-side probability validation
    App.tsx               # page layout
```

## API contract

`POST /api/calculations` with body:

```json
{
  "firstProbability": 0.5,
  "secondProbability": 0.4,
  "calculationType": "CombinedWith"
}
```

`calculationType` is the string enum `CombinedWith` or `Either`. A `200` response
returns the calculation `result`; a `400` response returns an RFC 7807
`ValidationProblemDetails` with field-specific messages.
