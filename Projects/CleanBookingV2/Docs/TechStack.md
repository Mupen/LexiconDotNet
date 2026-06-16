# Tech Stack And Project Layout

Purpose: explain the technologies, folders, configuration, persistence, Docker setup, and practical replacement options.

This document explains the technology choices and where the code lives.

## Backend

Technology:

- .NET `10.0`
- ASP.NET Core Web API
- EF Core with SQLite
- Swagger/OpenAPI for local API exploration
- xUnit for unit tests

Main projects:

```text
Api\CleanBookingV2.Api
Application\CleanBookingV2.Application
Domain\CleanBookingV2.Domain
Infrastructure\CleanBookingV2.Infrastructure
UnitTests\CleanBookingV2.UnitTests
```

Developer scripts:

```text
Start.ps1
Verify.ps1
```

### API Project

Path: `Api\CleanBookingV2.Api`

Responsibilities:

- HTTP routing
- JSON serialization
- Swagger setup
- CORS setup
- dependency injection composition
- database migration on local/demo startup
- mapping application results to HTTP problem details

### Domain Project

Path: `Domain\CleanBookingV2.Domain`

Responsibilities:

- entities
- value objects
- enums
- booking policy constants
- result/error primitives
- business invariants that should not depend on infrastructure

### Application Project

Path: `Application\CleanBookingV2.Application`

Responsibilities:

- use cases
- queries
- application services
- request models
- read models
- repository/query interfaces

This project depends on the domain project, but not on EF Core or ASP.NET Core.

### Infrastructure Project

Path: `Infrastructure\CleanBookingV2.Infrastructure`

Responsibilities:

- EF Core `DbContext`
- SQLite configuration
- migrations
- repository implementations
- read-query implementations
- transaction wrapper
- system GUID generator

### Unit Tests

Path: `UnitTests\CleanBookingV2.UnitTests`

Responsibilities:

- test domain rules
- test date overlap behavior
- test booking use cases with in-memory test doubles

## Frontend

Technology:

- React `19`
- Vite `8`
- plain CSS
- browser `fetch`
- local storage for display preferences

Path:

```text
Frontend\Public
```

Important folders:

```text
src\api
src\components
src\hooks
src\storage
public\rooms
```

The frontend is intentionally small. It has no global state library because the current app state is simple enough for React state and a custom data-loading hook.

## Persistence

Local API database:

```text
Api\CleanBookingV2.Api\Data\cleanbookingv2.db
```

Docker database:

```text
/app/Data/cleanbookingv2.db
```

The Docker database is stored in the `cleanbooking_data` volume.

Seeded data is configured in:

```text
Infrastructure\CleanBookingV2.Infrastructure\Persistence\CleanBookingV2DbContext.cs
```

## Configuration

API connection string:

```text
Api\CleanBookingV2.Api\appsettings.json
```

Frontend API base URL:

```text
VITE_API_BASE_URL
```

If `VITE_API_BASE_URL` is not set, the frontend defaults to:

```text
http://localhost:5217
```

## Logs

Local developer startup logs:

```text
Api\CleanBookingV2.Api\Logs\api-dev.log
Frontend\Public\logs\frontend-dev.log
```

These logs are produced by `Start.ps1`. They are not business data.

## Docker

Backend Dockerfile:

```text
Dockerfile
```

Frontend Dockerfile:

```text
Frontend\Public\Dockerfile
```

Compose file:

```text
docker-compose.yml
```

Docker Compose builds the API and frontend separately. The frontend image serves the production Vite build through nginx.

## Why This Stack

ASP.NET Core and EF Core fit the assignment because the important backend concerns are HTTP APIs, persistence, validation, and domain rules.

SQLite keeps local setup simple. It avoids requiring a database server for a learning/demo project.

React/Vite keeps the frontend fast to run and simple to build. The project does not need a larger frontend framework because routing, data loading, and state are limited.

Docker Compose gives a repeatable full-stack run path without replacing the normal local developer workflow.

`Verify.ps1` gives a repeatable local verification path for the project. It restores the backend test project, runs tests with `--no-restore`, then builds the Vite frontend from `Frontend\Public`.

## Practical Replacement Examples

When the project grows, consider these replacements:

| Current choice | Replacement when needed | Reason |
| --- | --- | --- |
| SQLite | PostgreSQL or SQL Server | Better production concurrency and stronger constraints |
| Startup migrations | reviewed SQL migration deployment | Avoids schema mutation from every app instance |
| App-layer overlap checks only | database locking or exclusion constraints | Stronger protection against race conditions |
| React local component state | reducer or state library | Useful if booking workflows become much more complex |
| no authentication | ASP.NET Core Identity or external identity provider | Required before exposing booking management publicly |
