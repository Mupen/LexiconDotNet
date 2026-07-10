# Configuration And Data

This file explains where secrets, config, and persisted data live.

## NewsAPI Key

The NewsAPI key must stay on the backend.

Local development:

```powershell
cd Projects\ReactNews\Backend\Api\ReactNews.Api
dotnet user-secrets set "NewsApi:ApiKey" "your-api-key"
```

Docker:

```text
NEWSAPI_KEY=your-api-key
```

Do not put the NewsAPI key in frontend code or `VITE_*` variables.

## Admin Seed

Public registration creates Reader accounts only.

Create a local Admin through user secrets:

```powershell
cd Projects\ReactNews\Backend\Api\ReactNews.Api
dotnet user-secrets set "AdminSeed:Email" "admin@example.com"
dotnet user-secrets set "AdminSeed:DisplayName" "Admin User"
dotnet user-secrets set "AdminSeed:Password" "Password123!"
```

Restart the API. If the configured email does not exist, the backend creates the Admin account.

## SQLite

Local database default path:

```text
Backend/Api/ReactNews.Api/Data/reactnews.db
```

Playwright E2E database:

```text
Backend/Api/ReactNews.Api/Data/reactnews-e2e.db
```

SQLite stores:

| Data | Table |
| --- | --- |
| Users | `Users` |
| Article snapshots | `ArticleSnapshots` |
| Saved articles | `SavedArticles` |
| Reader preferences | `ReaderPreferences` |
| Editorial articles | `EditorialArticles` |
| Migration history | `__EFMigrationsHistory` |

EF Core migrations own the schema. Startup calls `Database.Migrate()` through Infrastructure.

## Auth Cookie

- Cookie name: `ReactNews.Auth`
- Cookie is HttpOnly.
- Frontend sends cookies with `credentials: include`.
- Protected endpoints require Reader/Admin or Admin role.

## `.env.example` And `.env`

`.env.example` is a public template. It should contain example values only.

`.env` is your private local Docker config. It contains your real `NEWSAPI_KEY` and should not be committed.

## Data Ownership

| Data | Owner |
| --- | --- |
| NewsAPI key | Backend/operator |
| External article data | NewsAPI |
| Users/password hashes | Backend SQLite |
| Auth session | Backend/browser cookie |
| Saved articles | Backend SQLite |
| Preferences | Backend SQLite |
| Editorial articles | Backend SQLite |
| Article search filters | Frontend URL params |
| Request cache | TanStack Query/frontend |
| NewsAPI response cache | Backend memory cache |
