# CleanBookingV2

CleanBookingV2 is a bed and breakfast booking application with an ASP.NET Core API, SQLite persistence, and a React/Vite frontend.

The codebase is organized as a practical Clean Architecture example: domain rules live in the backend, application use cases coordinate workflows, infrastructure owns EF Core persistence, and the frontend stays responsible for user interaction.

The frontend holds working state. The backend owns authoritative state.

## Quick Start

From `Projects/CleanBookingV2`:

```powershell
.\Start.ps1
```

This starts:

- API and Swagger: `http://localhost:5217/swagger`
- React app: `http://localhost:5173`

Logs are written to:

- API: `Api\CleanBookingV2.Api\Logs\api-dev.log`
- Frontend: `Frontend\Public\logs\frontend-dev.log`

## What The Application Does

- Lists seeded rooms and parking spaces.
- Searches available rooms and parking spaces for a date range.
- Creates, updates, and cancels bookings.
- Prevents overlapping active room bookings.
- Prevents overlapping active parking assignments.
- Recalculates booking price on the backend from room price and nights.
- Keeps cancelled bookings as history without blocking future availability.
- Exposes booking policy rules to the frontend from `GET /api/booking-policy`.
- Stores only UI preferences in browser storage.

## Documentation

Use this README as the entry point. All project documentation lives directly in `Docs`; there are no nested documentation folders.

| Need | File |
| --- | --- |
| Understand how the code is structured and how requests flow through it | [Docs/Architecture.md](Docs/Architecture.md) |
| Understand supported features, business rules, and expected failures | [Docs/Features.md](Docs/Features.md) |
| Understand technologies, folders, configuration, persistence, Docker, and replacement options | [Docs/TechStack.md](Docs/TechStack.md) |
| Run, build, test, troubleshoot, and operate the project locally | [Docs/Runbook.md](Docs/Runbook.md) |
| Call the HTTP API and understand request/response shapes | [Docs/ApiReference.md](Docs/ApiReference.md) |
| Manually verify API, frontend, build, and Docker behavior | [Docs/ManualTestChecklist.md](Docs/ManualTestChecklist.md) |
| Map this implementation back to the assignment requirements | [Docs/Assignment.md](Docs/Assignment.md) |

## Verification

Frontend production build:

```powershell
cd Projects\CleanBookingV2\Frontend\Public
npm run build
```

Backend tests:

```powershell
dotnet restore Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --verbosity normal
dotnet build Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --no-restore --verbosity normal
dotnet test Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --no-restore --no-build --verbosity normal
```

Or run both checks from `Projects\CleanBookingV2`:

```powershell
.\Verify.ps1
```

Current known verification result:

- Backend tests: 24 passed, 0 failed.
- Frontend build: passed from `Projects\CleanBookingV2\Frontend\Public`.

If `dotnet test` hangs at restore, run restore directly first, then run test with `--no-restore`. See the runbook troubleshooting section.

## Production Notes

This project is suitable for local learning and demo use. Before treating it as production software, add authentication and authorization, review database migration strategy, and enforce booking overlap rules with database-level locking or constraints where the database supports it.
