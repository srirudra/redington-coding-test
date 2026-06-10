# syntax=docker/dockerfile:1

# --- Build stage: produce the static SPA bundle ---
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
# The committed lockfile can omit platform-specific optional deps (npm
# optional-dependencies bug, npm/cli#4828); with the lock present, neither
# `npm ci` nor `npm install` installs the native Rollup binary for this image's
# musl libc. Removing the lock lets npm resolve the correct binary from scratch.
RUN rm -f package-lock.json && npm install --no-audit --no-fund

COPY . .

# Build with a relative API base URL so the browser calls the same origin
# (/api/...). nginx then reverse-proxies those calls to the backend, which
# avoids CORS and any host-specific configuration.
ARG VITE_API_BASE_URL=""
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# --- Runtime stage: serve static assets and proxy the API ---
FROM nginx:1.27-alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
