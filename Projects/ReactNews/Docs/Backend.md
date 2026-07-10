# Backend

The backend lives in `Backend` and is split into five projects.

```text
Backend/Api/ReactNews.Api
Backend/Application/ReactNews.Application
Backend/Domain/ReactNews.Domain
Backend/Infrastructure/ReactNews.Infrastructure
Backend/UnitTests/ReactNews.UnitTests
```

## API

Purpose: HTTP boundary.

Contains:

- controllers
- cookie auth setup
- role authorization
- CORS
- exception/result mapping
- startup composition in `Program.cs`

The API should stay thin. It should not contain NewsAPI HTTP logic, EF Core store logic, or business rules.

## Application

Purpose: use cases and rules.

Contains:

- auth use cases
- article/source use cases
- saved article use cases
- reader preference use cases
- editorial use cases
- request/response contracts
- interfaces implemented by Infrastructure
- `Result<T>` and validation errors

Application decides what the system should do. It does not know how SQLite or NewsAPI work internally.

## Domain

Purpose: core business objects.

Current domain concepts:

- `Article`
- `Source`
- `SavedArticle`
- `ReaderPreferences`
- `EditorialArticle`
- `EditorialArticleStatus`
- `User`
- `UserRole`

Domain should stay framework-independent.

## Infrastructure

Purpose: technical implementations.

Contains:

- `ReactNewsDbContext`
- EF Core migrations
- SQLite persistence records
- EF stores
- NewsAPI client
- NewsAPI DTOs
- memory cache wrapper
- admin seed helper

Current stores:

- `EfArticleSnapshotStore`
- `EfSavedArticleStore`
- `EfReaderPreferencesStore`
- `EfEditorialArticleStore`
- `EfUserStore`

## Tests

Backend tests live in:

```text
Backend/UnitTests/ReactNews.UnitTests
```

They cover application use cases, API integration, EF stores, NewsAPI client behavior, cache behavior, auth, roles, saved articles, preferences, editorial flow, and project gap checks.

Current backend test count: `95`.
