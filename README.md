# viora-main-backend

Backend service for the Viora project, built with ASP.NET Core on **.NET 10**.

## Overview

This repository contains a layered backend structure with:

- `Viora.Api` – HTTP API host
- `Viora.Application` – application use cases and handlers
- `Viora.Domain` – core domain abstractions and models
- `Viora.Infrastructure` – infrastructure concerns (e.g., EF Core repositories)

## Tech Stack

- **.NET 10**
- **ASP.NET Core Web API**
- **MediatR**
- **Entity Framework Core**
- **OpenAPI** (built-in ASP.NET Core OpenAPI support)
- **Docker / Docker Compose**
- **GitHub Actions** CI (Docker Compose build on `main` pushes/PRs)

## Prerequisites

Install:

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for containerized run)

## Run Locally

From the repository root:

Default local URLs (from launch settings):

- `http://localhost:5185`
- `https://localhost:7208`

## OpenAPI

In development, OpenAPI is enabled.  
After running the API, you can access the OpenAPI document at:

- `https://localhost:7208/openapi/v1.json`
- or `http://localhost:5185/openapi/v1.json`

## Run with Docker

Build and run via Docker Compose:

> The API container exposes ports `8080` (HTTP) and `8081` (HTTPS) internally.

## Webhook Tunneling (Local Development)

Payment-gateway webhooks (e.g. Kashier) are **inbound** server-to-server calls, so the
gateway must reach your machine over a public URL — `localhost` is not routable from the
internet. For local testing we expose the running API through a tunnel.

We use **[cloudflared](https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/)**
(native binary, no account needed for quick tunnels). VS Dev Tunnels also work but their
integration does not attach to the Docker Compose launch target.

### Install

```powershell
winget install --id Cloudflare.cloudflared -e
```

Open a new terminal so it is on `PATH`, then verify:

```powershell
cloudflared --version
```

### Run the tunnel

Start the API (Docker Compose publishes it on host port `8080`), then in a separate terminal:

```powershell
cloudflared tunnel --url http://localhost:8080
```

- Leave this process running — it **is** the tunnel; closing the terminal drops it.
- If you get a `502`, use `http://127.0.0.1:8080` instead (avoids the `localhost` → IPv6 `::1` gotcha).

cloudflared prints the public base URL in its startup banner:

```
+----------------------------------------------------------+
|  Your quick Tunnel has been created! Visit it at ...:    |
|  https://<random-words>.trycloudflare.com                |
+----------------------------------------------------------+
```

Webhook endpoints are then reachable at, e.g.:

```
https://<random-words>.trycloudflare.com/webhooks/kashier/subscription
https://<random-words>.trycloudflare.com/webhooks/kashier/addon
```

### Notes

- Quick-tunnel URLs are **ephemeral** — a new random `*.trycloudflare.com` is issued each run.
  Put the base in configuration (`Payments:WebhookBaseUrl`), not code, and paste the fresh URL
  each session. For a stable URL, create a free Cloudflare account and a named tunnel.
- The tunnel is **only** for inbound gateway webhooks. To try your own endpoints, open Scalar
  directly at `http://localhost:8080/scalar/v1` — going through the https tunnel triggers
  browser CORS/mixed-content blocks ("Failed to fetch").
- The webhook must verify the gateway signature regardless — a public tunnel URL is reachable
  by anyone, so it is not a security boundary.

## CI

GitHub Actions workflow: `.github/workflows/DotnetDockerCI.yml`

Current CI job:

- Triggers on push/PR to `main`
- Builds using Docker Compose

## Project Structure

## Notes
