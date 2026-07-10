# Architecture

ReactNews has one React frontend and a clean-architecture-style ASP.NET Core backend.

## Project Shape

```text
Projects/ReactNews
  Backend/
    Api/ReactNews.Api
    Application/ReactNews.Application
    Domain/ReactNews.Domain
    Infrastructure/ReactNews.Infrastructure
    UnitTests/ReactNews.UnitTests
  Frontend/Public
  Docs
  Start.ps1
  Verify.ps1
```

## Dependency Direction

```text
Api -> Application -> Domain
Infrastructure -> Application
Infrastructure -> Domain
UnitTests -> all backend projects
Frontend -> API over HTTP
```

Domain is the center. It does not know about ASP.NET Core, EF Core, NewsAPI, React, Docker, or browser concepts.

## Backend Layers

| Layer | Responsibility |
| --- | --- |
| API | Controllers, routes, auth cookies, role authorization, CORS, result mapping |
| Application | Use cases, validation, contracts, interfaces, mapping |
| Domain | Entities and enums such as users, articles, saved articles, preferences, editorial articles |
| Infrastructure | EF Core/SQLite, migrations, NewsAPI client, cache, concrete stores |

## Main Flows

Public news:

1. React route `/news` calls frontend hook `useNewsSearch`.
2. Frontend calls `GET /api/articles`.
3. API controller calls Application use case.
4. Application uses `INewsProvider`.
5. Infrastructure checks cache, calls NewsAPI if needed, maps data, and remembers snapshots.
6. API returns JSON to the frontend.

Auth:

1. User registers or logs in.
2. Backend validates credentials.
3. Backend sends HttpOnly auth cookie.
4. Later protected requests use that cookie.

Editorial:

1. Admin creates/publishes content at `/editorial`.
2. Backend stores editorial articles in SQLite.
3. Published articles appear publicly at `/editorial-feed`.

## Ownership Rules

Backend owns:

- NewsAPI key
- users/password hashes
- auth cookies and roles
- saved articles
- preferences
- editorial content
- SQLite data
- NewsAPI integration

Frontend owns:

- routes
- forms
- temporary UI state
- URL search params
- TanStack Query request/cache state

The browser never receives the NewsAPI key or password hashes.
