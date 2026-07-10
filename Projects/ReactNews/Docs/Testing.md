# Testing

Run all verification from `Projects/ReactNews`:

```powershell
.\Verify.ps1
```

The verifier:

1. Builds backend.
2. Builds backend tests.
3. Runs backend xUnit tests.
4. Runs frontend Vitest tests.
5. Builds frontend.
6. Runs Playwright E2E tests.

Current expected result:

```text
95 backend tests passing
31 frontend unit tests passing
3 Playwright E2E tests passing
ReactNews verification passed.
```

## Backend Tests

Path:

```text
Backend/UnitTests/ReactNews.UnitTests
```

Coverage:

- application use cases
- auth/register/login/current-user/profile/password/delete
- role authorization
- saved article isolation per user
- reader preference isolation per user
- editorial article use cases
- public editorial article endpoints
- API integration through `WebApplicationFactory`
- EF Core SQLite stores
- NewsAPI client mapping
- cache behavior

## Frontend Unit Tests

Path:

```text
Frontend/Public/src
```

Run directly:

```powershell
cd Projects\ReactNews\Frontend\Public
npm run test
```

Coverage:

- API client behavior
- auth hook
- saved article hook
- preference hook
- editorial hook
- app route protection/navigation
- core article/navigation components

Vitest ignores `e2e/**` because Playwright owns browser tests.

## Playwright E2E

Run directly:

```powershell
cd Projects\ReactNews\Frontend\Public
npm run test:e2e
```

Playwright starts:

- API on `http://127.0.0.1:5227`
- frontend on `http://127.0.0.1:5174`
- isolated SQLite E2E database
- seeded Admin account

E2E scenarios:

- guest opens app and public editorial feed
- reader registers, edits profile, changes password, deletes account
- admin publishes editorial article and guest reads it

## Build Stability

`Verify.ps1` uses stable backend build flags:

```text
--disable-build-servers
-m:1
```

This avoids stale build-server state and keeps backend verification output deterministic.
