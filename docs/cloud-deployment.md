# Cloud Deployment Guide

How the Probability Calculator would be deployed, scaled, and updated in the
cloud. The brief calls for global availability, high usage, integration with
other internal tools, and a 99.99% uptime target; this guide explains how the
current container-based design meets that path without overengineering the
coding-test implementation.

Audience: engineers and reviewers. Scope: deployment topology, recommended
managed services (Azure primary, AWS alternative), Terraform structure, scaling,
and operational caveats. This guide describes a target design — no cloud
infrastructure is provisioned in this repository.

## Current deployable artifacts

The app already ships as two independent containers, which are the unit of cloud
deployment:

- **Backend** — ASP.NET Core API. Multi-stage [backend/Dockerfile](../backend/Dockerfile)
  produces an `aspnet:9.0` runtime image listening on port `8080`, running as
  `Production`, with a `/health` endpoint used as a liveness/readiness probe.
- **Frontend** — static SPA served by nginx ([frontend/Dockerfile](../frontend/Dockerfile),
  [frontend/nginx.conf](../frontend/nginx.conf)). nginx serves the built assets
  and reverse-proxies `/api/*` to the backend, so the browser uses a single
  origin.

Two properties make this straightforward to host:

- **The API is stateless.** Calculations need no server-side session, so the
  backend scales horizontally behind a load balancer.
- **Same-origin by design.** Routing `/api` through the edge avoids CORS in
  production. CORS origins remain configurable (`Cors:AllowedOrigins`) for
  split-origin deployments.

## Recommended topology

```text
            Internet
               |
        [ CDN / edge ]            static SPA assets
               |
        [ HTTPS ingress ]----> /api/* ----> [ API instances (N) ]
               |                                   |
        [ static frontend ]                 [ central logging /
                                              audit sink ]
```

The frontend is static and belongs on a CDN-backed host. The API is a
horizontally scalable container service behind managed ingress with TLS.

## Azure deployment (primary)

| Concern | Service | Why |
| --- | --- | --- |
| Edge/routing | **Azure Front Door** | Single public origin: terminates TLS, serves the SPA globally, and routes `/api/*` to the API |
| Frontend hosting | **Blob Static Website** behind Front Door (or **Azure Static Web Apps** for a simpler all-in-one) | Cheap static asset storage on a global CDN |
| API | **Azure Container Apps** | Serverless containers, scale-to-N, revisions for safe rollout, managed ingress + health probes |
| Container images | **Azure Container Registry** | Private image storage feeding Container Apps |
| Logs/metrics | **Azure Monitor + Log Analytics / Application Insights** | Central, queryable replacement for file-based audit logs |
| Secrets/config | **Container Apps env vars + Key Vault** | `Cors:AllowedOrigins`, app settings without code changes |

Recommended default: put **Azure Front Door** in front, serving static assets and
routing `/api/*` to Container Apps. This keeps the production single-origin shape
that mirrors the local nginx proxy, so no CORS configuration is needed. **Azure
Static Web Apps** is a simpler alternative that bundles static hosting and an
edge, but routing the API through one Front Door origin gives more control as the
system grows.

Why Container Apps over alternatives: it runs the existing container directly,
maps cleanly to the `/health` probe, autoscales on HTTP load (including to zero
for non-prod), and provides revision-based blue/green rollout without managing a
Kubernetes cluster. AKS is available if the platform later standardizes on
Kubernetes, but it adds operational cost not justified by this workload.

## AWS deployment (alternative)

| Concern | Service |
| --- | --- |
| Frontend | **S3 + CloudFront** |
| API | **ECS on Fargate** behind an Application Load Balancer |
| Container images | **Amazon ECR** |
| Logs/metrics | **CloudWatch Logs + Metrics** |
| Secrets/config | **SSM Parameter Store / Secrets Manager** |

The shape is identical: CDN-fronted static SPA, load-balanced stateless API
containers, central logging, ALB health checks pointed at `/health`.

## Terraform structure

Provision with Terraform for repeatable, reviewable infrastructure. Suggested
layout (not yet present in the repo):

```text
infra/
  modules/
    frontend/      # static hosting + CDN
    api/           # container service, ingress, autoscale rules
    registry/      # container registry
    observability/ # logging, metrics, alerts
  envs/
    staging/
    production/
```

Conventions:

- One module per concern; compose them per environment under `envs/`.
- Keep environments identical in shape, differing only in variables (instance
  counts, scale thresholds, domains).
- Store Terraform state remotely with locking (Azure Storage backend, or S3 +
  DynamoDB on AWS).
- Pass image tags in as variables so CI can deploy a specific build.

Provider focus: target the Azure provider (`azurerm`) first to match the primary
recommendation; the AWS provider path mirrors the same module boundaries.

## Updates and CI/CD

A pipeline (e.g. GitHub Actions) should, on each change:

```text
restore + build (API)  ->  unit + integration tests (API)
install + lint (UI)    ->  unit/component tests (UI)
build + push images    ->  deploy to staging  ->  promote to production
```

Rollout and rollback:

- Deploy to **staging first**, then promote after checks pass.
- Use **revisions (Container Apps) or blue/green (ECS)** so a bad release is
  switched away from, not hot-patched.
- Health probes gate traffic; failed probes block promotion.

This mirrors the local guarantee already in [docker-compose.yml](../docker-compose.yml),
where the frontend waits for the backend `/health` check before starting.

## Scaling and high availability

- **Horizontal API scaling**: stateless containers scale on CPU/HTTP load; size
  minimum replicas to absorb baseline traffic and burst above it.
- **Multi-instance + multi-zone**: run ≥2 API replicas across availability zones
  behind the load balancer to remove single points of failure.
- **Global reach**: serve the SPA from a CDN; add API regional deployments with
  latency/geo routing only if usage genuinely spans regions (defer until needed).
- **99.99% target**: requires multi-instance redundancy, health-gated rolling or
  blue/green deploys, automated rollback, and alerting — all compatible with the
  topology above. Uptime also depends on the chosen services' own SLAs, which
  must be composed and validated, not assumed.

## Monitoring, logging, troubleshooting

- Replace the file-based audit log with **central structured logging** (App
  Insights / CloudWatch). The audit logger sits behind `ICalculationAuditLogger`,
  so this is an implementation swap, not a redesign.
- Capture: request rate, latency, error rate (4xx/5xx), validation-failure rate,
  calculation volume by type, and health status.
- Alert on: elevated 5xx, latency regression, failed health checks, resource
  saturation, and abnormal traffic.
- Add **correlation IDs** so a frontend request can be traced through API logs
  and infrastructure traces.

## Known limitations and caveats

- **No infrastructure is provisioned here.** This is a design guide; the `infra/`
  Terraform and CI deploy stages are described, not implemented.
- **File-based audit logging does not survive horizontal scaling.** The local
  volume in Compose is per-host; production must use a central sink before
  scaling the API beyond one instance, or audit records will be fragmented.
- **No authentication or authorization.** Out of scope per the brief. A globally
  exposed API would need at minimum network restrictions or an auth layer before
  real rollout.
- **No persistence or rate limiting.** Acceptable for a pure calculation service,
  but add rate limiting at the edge before exposing it to high external volume.
- **Container build reproducibility trade-off.** The frontend image installs deps
  without the lockfile to work around an npm optional-dependencies bug
  (npm/cli#4828); strict `npm ci` reproducibility in the image is deferred until
  the lockfile is regenerated on Linux. See [frontend/README.md](../frontend/README.md).
- **99.99% is a target, not a guarantee.** Achieving it depends on composed
  service SLAs, tested failover, and operational maturity beyond this exercise.
- **Cost vs. complexity.** Recommendations favor managed services (Container
  Apps, Static Web Apps) over Kubernetes to keep operational cost low for the
  current scale; revisit if platform standardization or scale changes the trade-off.
