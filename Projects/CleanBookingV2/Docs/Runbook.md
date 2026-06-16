# Runbook

Purpose: provide the practical commands for running, building, testing, troubleshooting, and operating the project locally.

This file explains how to run, build, test, and troubleshoot CleanBookingV2 from a developer workstation.

## Start Everything

From `Projects/CleanBookingV2`:

```powershell
.\Start.ps1
```

The script opens one terminal for the API and one terminal for the React frontend.

Runtime URLs:

- API Swagger: `http://localhost:5217/swagger`
- React app: `http://localhost:5173`

Runtime logs:

- API log: `Api\CleanBookingV2.Api\Logs\api-dev.log`
- Frontend log: `Frontend\Public\logs\frontend-dev.log`

## Run Backend Manually

From the repository root:

```powershell
dotnet restore Projects\CleanBookingV2\Api\CleanBookingV2.Api\CleanBookingV2.Api.csproj
dotnet run --project Projects\CleanBookingV2\Api\CleanBookingV2.Api\CleanBookingV2.Api.csproj --launch-profile http
```

The API uses SQLite by default:

```text
Projects\CleanBookingV2\Api\CleanBookingV2.Api\Data\cleanbookingv2.db
```

The API applies EF Core migrations on startup for local/demo convenience.

## Run Frontend Manually

From `Projects/CleanBookingV2/Frontend/Public`:

```powershell
npm install
npm run dev
```

The frontend uses `VITE_API_BASE_URL` when configured. Without it, it calls `http://localhost:5217`.

## Build

Backend:

```powershell
dotnet build Projects\CleanBookingV2\Api\CleanBookingV2.Api\CleanBookingV2.Api.csproj
```

Frontend:

```powershell
cd Projects\CleanBookingV2\Frontend\Public
npm run build
```

## Test

Run all project verification from `Projects/CleanBookingV2`:

```powershell
.\Verify.ps1
```

Manual backend unit test commands:


```powershell
dotnet restore Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --verbosity normal
dotnet build Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --no-restore --verbosity normal
dotnet test Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --no-restore --no-build --verbosity normal
```

The tests cover domain booking rules, date overlap behavior, and booking use cases.

Expected current result:

```text
Test summary: total: 24, failed: 0, succeeded: 24, skipped: 0
```

## Docker

From `Projects/CleanBookingV2`:

```powershell
docker compose up --build
```

Stop containers:

```powershell
docker compose down
```

Reset the Docker SQLite volume:

```powershell
docker compose down -v
```

Docker exposes:

- API: `http://localhost:5217`
- Frontend: `http://localhost:5173`

## Troubleshooting

If `dotnet test` or `dotnet restore` hangs at `Determining projects to restore...`, first shut down .NET build servers:

```powershell
dotnet build-server shutdown
```

Then stop any remaining stuck `dotnet` processes:

```powershell
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force
```

Run restore directly:

```powershell
dotnet restore Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --verbosity normal
```

If restore succeeds, run tests without restore:

```powershell
dotnet build Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --no-restore --verbosity normal
dotnet test Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --no-restore --no-build --verbosity normal
```

If `dotnet build-server shutdown` reports a stale NuGet lock file, remove only the exact lock file it reports, then retry restore.

If the frontend cannot reach the API:

- Confirm the API is running at `http://localhost:5217`.
- Confirm CORS allows the frontend origin in `Api\CleanBookingV2.Api\Program.cs`.
- Confirm `VITE_API_BASE_URL` is either unset for local defaults or set to the API URL.

If availability results look stale:

- Refresh the frontend data.
- Check that cancelled bookings have status `Cancelled`.
- Remember that frontend availability is only a snapshot; create/update requests always re-check availability on the backend.
