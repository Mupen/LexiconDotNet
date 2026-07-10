# Runbook

Use this when you need to configure, run, verify, or stop ReactNews.

## Prerequisites

- .NET SDK
- Node.js/npm
- NewsAPI key from `https://newsapi.org`

## Configure NewsAPI

```powershell
cd Projects\ReactNews\Backend\Api\ReactNews.Api
dotnet user-secrets set "NewsApi:ApiKey" "your-api-key"
```

## Configure Admin

```powershell
cd Projects\ReactNews\Backend\Api\ReactNews.Api
dotnet user-secrets set "AdminSeed:Email" "admin@example.com"
dotnet user-secrets set "AdminSeed:DisplayName" "Admin User"
dotnet user-secrets set "AdminSeed:Password" "Password123!"
```

Restart the API after changing user secrets.

## Install Frontend Packages

```powershell
cd Projects\ReactNews\Frontend\Public
npm install
```

If Playwright browsers are missing:

```powershell
npx playwright install chromium
```

## Run Locally

```powershell
cd Projects\ReactNews
.\Start.ps1
```

Open:

```text
http://localhost:5173
```

API health:

```text
http://localhost:5217/api/health
```

Press Enter in the `Start.ps1` window to stop backend and frontend.

## Verify Everything

```powershell
cd Projects\ReactNews
.\Verify.ps1
```

Expected:

```text
95 backend tests passing
31 frontend unit tests passing
3 Playwright E2E tests passing
frontend build passing
ReactNews verification passed.
```

## Docker

```powershell
cd Projects\ReactNews
copy .env.example .env
```

Edit `.env` and set:

```text
NEWSAPI_KEY=your-api-key
```

Run:

```powershell
docker compose up --build
```

Stop:

```powershell
docker compose down
```

## Troubleshooting

API key missing:

- Set `NewsApi:ApiKey` with user secrets.
- For Docker, set `NEWSAPI_KEY` in `.env`.

Admin login fails:

- Check `AdminSeed` user secrets.
- Restart the API after setting them.

Frontend cannot call backend:

- Check API is on `http://localhost:5217`.
- Check frontend is on `http://localhost:5173`.

Article detail is missing:

- Load headlines/search first.
- Article detail depends on remembered snapshots.

Stop leftovers:

```powershell
dotnet build-server shutdown
Get-Process dotnet -ErrorAction SilentlyContinue
Get-NetTCPConnection -LocalPort 5217,5173,5227,5174 -ErrorAction SilentlyContinue
```
