# Azure DevOps CI/CD Guide

How the Probability Calculator would be built, tested, and deployed with Azure
DevOps Pipelines. The design favors **separate API and UI pipelines** so each
side gets fast, independent feedback and can deploy on its own cadence, while
avoiding redundant builds.

Audience: engineers and reviewers. Scope: pipeline topology, stages, triggers,
and caveats. This guide describes a target setup — no pipeline YAML is committed
in this repository yet. It complements [cloud-deployment.md](cloud-deployment.md),
which covers the runtime topology these pipelines deploy to.

## Why two pipelines

The backend (.NET) and frontend (React) are independent deployable units
([backend Dockerfile](../backend/Dockerfile), [frontend Dockerfile](../frontend/Dockerfile),
wired locally by [docker-compose.yml](../docker-compose.yml); separate toolchains).
Splitting the pipelines delivers:

- **Faster feedback** — a UI change does not wait on the .NET build/test cycle,
  and vice versa.
- **Independent deploys** — each artifact ships on its own cadence without
  coupling release timing.
- **Fewer wasted builds** — path filters mean a change to one side does not
  trigger the other's pipeline.

A small shared validation pipeline still guards cross-cutting concerns (see
below), but the heavy build/test/deploy work is split.

## Pipeline topology

```text
                 ┌─────────────────────────────┐
 push / PR  ───▶ │ path filter: backend/**      │ ──▶ API pipeline
                 └─────────────────────────────┘
                 ┌─────────────────────────────┐
 push / PR  ───▶ │ path filter: frontend/**     │ ──▶ UI pipeline
                 └─────────────────────────────┘
```

Each pipeline triggers only on changes to its own path, so a backend-only commit
builds only the API pipeline. Both run on pull requests as required checks, so a
failing build, test, or integration check **blocks the merge into `main`**.

**Build reuse rule:** compile and test once per pipeline run, publish one
immutable image tag, and promote that same tag through staging and production.
Do not rebuild per environment.

## API pipeline (`backend/**`)

Stages, mapped to existing commands:

| Stage | Command | Purpose |
| --- | --- | --- |
| Restore/build | `dotnet build … -c Release` | Compile once; fail fast on errors |
| Unit tests | `dotnet test --no-build -c Release --filter "Category=Unit"` | Fast domain/application/validator feedback |
| Integration tests | `dotnet test --no-build -c Release --filter "Category=Integration"` | In-memory API contract tests (`WebApplicationFactory`) |
| Package | `docker build` (backend) | Produce the runtime image once |
| Publish | push image to Azure Container Registry | Tag with the build ID for traceable deploys |

The `--no-build` flag reuses the compile output from the build stage, so the test
stages do not recompile. The unit/integration split still surfaces fast unit
failures before the heavier integration suite runs.

## UI pipeline (`frontend/**`)

| Stage | Command | Purpose |
| --- | --- | --- |
| Install | `npm ci` | Deterministic dependency install |
| Lint | `npm run lint` | ESLint static checks |
| Build | `npm run build` | Type-check (`tsc -b`) and produce the production bundle |
| Unit/component tests | `npm run test` | Vitest + Testing Library |
| Package | `docker build` (frontend) | Build the nginx-served image |
| Publish | push image to Azure Container Registry | Tag with the build ID |

## End-to-end tests (planned)

End-to-end tests with **Playwright are not yet implemented**. When added, they
fit as a dedicated stage that runs the composed stack
([docker-compose.yml](../docker-compose.yml)) and drives the real UI against the
real API:

```text
build images ──▶ docker compose up ──▶ playwright e2e ──▶ tear down
```

Positioned as a **required PR check**, this gives feedback when the
frontend/backend integration breaks *before* a merge into `main` or a deploy —
catching contract drift that unit tests on either side cannot. Until then,
backend integration tests (`Category=Integration`) provide partial cross-boundary
coverage at the API level.

## Deploy stages

After image publish, deployment promotes through environments:

```text
publish image ──▶ deploy staging ──▶ smoke checks ──▶ deploy production
```

- **Staging first**, then promote to production after checks pass.
- Use **Container Apps revisions** (or blue/green) so a bad release is switched
  away from rather than hot-patched; `/health` gates traffic.
- Deploys are parameterized by **image tag** so a specific build is promoted, not
  rebuilt. See [cloud-deployment.md](cloud-deployment.md) for the target services.

## Triggers and branch policy

- **PR validation**: both pipelines (and, when added, the Playwright stage) run as
  **required status checks** on PRs targeting `main`.
- **Path filters**: each pipeline triggers only on its own folder to cut redundant
  builds.
- **Main builds**: a merge to `main` runs the affected pipeline through publish
  and the deploy stages.

## Known limitations and caveats

- **No pipeline YAML is committed.** This is a design guide; the Azure DevOps
  `azure-pipelines.yml` definitions are described, not implemented.
- **Playwright e2e is not implemented.** The stage is planned; today only
  unit/component tests and backend integration tests run.
- **`npm ci` vs. the container build.** The UI test/lint stages use `npm ci`
  (locked versions), but the container image build installs without the lockfile
  to work around an npm optional-dependencies bug (npm/cli#4828) — so the image
  may resolve newer compatible patches. See [frontend/README.md](../frontend/README.md).
- **Shared-change blind spot.** Path filters mean a change touching only shared
  root files (e.g. `docker-compose.yml`) may not trigger either app pipeline; a
  lightweight root/validation pipeline should cover those paths.
- **Secrets and registry access** (ACR credentials, environment configuration)
  must be supplied via Azure DevOps service connections and variable groups; they
  are out of scope for this guide.
- **Azure DevOps assumed.** The same stages map to GitHub Actions if the team
  standardizes there; only the YAML syntax and trigger model differ.
