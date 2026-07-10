# ReactNews

ReactNews is a React/Vite news website with an ASP.NET Core backend. It started from a React news-page assignment and has been expanded into a full-stack project with backend proxying, authentication, persistence, admin editorial tools, and automated tests.

The important design rule is:

> The backend owns secrets, identity, permissions, persistence, and external API calls. The frontend owns presentation, interaction state, routing, and browser experience.

## What The App Does

- Shows public NewsAPI-backed headlines and search results.
- Keeps the NewsAPI key on the backend, never in browser code.
- Provides article detail pages with dynamic routing at `/article/:articleId`.
- Stores article snapshots in SQLite so detail pages can load remembered articles.
- Supports Reader accounts with cookie-based login/register/logout.
- Supports Admin accounts through backend `AdminSeed` configuration.
- Lets Readers save articles for later.
- Lets Readers manage preferences such as theme, font size, compact cards, and preferred categories.
- Provides profile/account management: edit display name, change password, and delete account.
- Provides an Admin editorial workspace for ReactNews-owned articles.
- Lets Admins create, publish, and archive editorial articles.
- Shows published editorial articles publicly at `/editorial-feed`.

## Tech Stack

- Frontend: React, Vite, JavaScript
- Routing: React Router
- Frontend server state: TanStack Query
- Article table view: TanStack Table
- Backend: ASP.NET Core controller API
- Backend architecture: API, Application, Domain, Infrastructure
- Authentication: ASP.NET Core cookie authentication
- Persistence: EF Core migrations with SQLite
- External API: NewsAPI, called only from the backend
- Cache: ASP.NET Core `IMemoryCache`
- Tests: xUnit, WebApplicationFactory, Vitest, React Testing Library, Playwright
- Docker: API container plus Nginx frontend container

## Requirements Covered

The original assignment requirements are covered:

- React news page
- NewsAPI as article source
- JSON data rendered visually
- NewsAPI key kept out of public source/browser code
- Browse/search news articles
- Dynamic article detail route
- Frontend user interaction
- State management for search/display state
- Browser-safe preferences
- Responsive layout
- Optional .NET backend proxy

ReactNews goes beyond the assignment with login, roles, persistence, saved articles, admin editorial publishing, migrations, Docker, and automated tests.

## Setup

Create a free NewsAPI key at:

```text
https://newsapi.org
```

For local backend development, store the key with .NET user secrets:

```powershell
cd Projects\ReactNews\Backend\Api\ReactNews.Api
dotnet user-secrets set "NewsApi:ApiKey" "your-api-key"
```

To create an Admin account for the editorial workspace, set admin seed values:

```powershell
cd Projects\ReactNews\Backend\Api\ReactNews.Api
dotnet user-secrets set "AdminSeed:Email" "admin@example.com"
dotnet user-secrets set "AdminSeed:DisplayName" "Admin User"
dotnet user-secrets set "AdminSeed:Password" "Password123!"
```

For Docker, copy the example environment file and put your real key in `.env`:

```powershell
cd Projects\ReactNews
copy .env.example .env
```

Then edit `.env`:

```text
NEWSAPI_KEY=your-api-key
```

## Run Locally

From `Projects\ReactNews`:

```powershell
.\Start.ps1
```

Open the frontend:

```text
http://localhost:5173
```

API health check:

```text
http://localhost:5217/api/health
```

`Start.ps1` keeps the PowerShell window open while the app runs. Press Enter in that window to stop both the backend and frontend.

## Verify

From `Projects\ReactNews`:

```powershell
.\Verify.ps1
```

Current expected verification:

```text
95 backend tests passing
31 frontend unit tests passing
3 Playwright E2E tests passing
frontend production build passing
ReactNews verification passed.
```

The Playwright tests start their own test API and frontend on separate ports, use a separate SQLite E2E database, and stop the processes after the tests finish.

## Main API Shape

```text
GET    /api/health
GET    /api/articles
GET    /api/articles/{articleId}
GET    /api/sources

POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/logout
GET    /api/auth/me
PUT    /api/auth/profile
PUT    /api/auth/password
DELETE /api/auth/account

GET    /api/saved-articles
POST   /api/saved-articles/{articleId}
DELETE /api/saved-articles/{articleId}

GET    /api/reader-preferences
PUT    /api/reader-preferences

GET    /api/editorial/articles
GET    /api/editorial/articles/{id}
POST   /api/editorial/articles
PUT    /api/editorial/articles/{id}
POST   /api/editorial/articles/{id}/publish
POST   /api/editorial/articles/{id}/archive

GET    /api/public/editorial/articles
GET    /api/public/editorial/articles/{id}
```

## Documentation

Current documentation lives in `Docs`.

| Need | File |
| --- | --- |
| Run, verify, stop, and troubleshoot the project | [Docs/Runbook.md](Docs/Runbook.md) |
| Understand architecture and request flow | [Docs/Architecture.md](Docs/Architecture.md) |
| Understand backend structure | [Docs/Backend.md](Docs/Backend.md) |
| Understand frontend structure | [Docs/FrontendStructure.md](Docs/FrontendStructure.md) |
| See current product features | [Docs/Features.md](Docs/Features.md) |
| See API endpoints and auth rules | [Docs/ApiReference.md](Docs/ApiReference.md) |
| Understand tests and verification | [Docs/Testing.md](Docs/Testing.md) |
| Understand secrets, config, cache, persistence, and data ownership | [Docs/ConfigurationAndData.md](Docs/ConfigurationAndData.md) |
| Short frontend overview | [Docs/Frontend.md](Docs/Frontend.md) |

## Submission Notes

Before submitting or pushing publicly:

- Do not commit `.env`.
- Do not commit real API keys or passwords.
- Keep `.env.example` as the public template.
- Keep user secrets local to the machine.
- Run `.\Verify.ps1`.

## Development Disclosure

This project was written by Daniel Henriksen with ChatGPT used as a collaborative development and documentation tool.
