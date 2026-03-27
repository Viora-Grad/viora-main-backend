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

## CI

GitHub Actions workflow: `.github/workflows/DotnetDockerCI.yml`

Current CI job:

- Triggers on push/PR to `main`
- Builds using Docker Compose

## Project Structure

## Notes

- Current API bootstrap is minimal (`Controllers` + `OpenAPI` + `HTTPS redirection`).
- Add environment-specific configuration in `Viora.Api/appsettings.*.json` as needed.