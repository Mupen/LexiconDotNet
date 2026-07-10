$ErrorActionPreference = "Stop"

# What: Resolve the ReactNews project root from this script location.
# How: this script lives in Frontend/Public/e2e, so walking three folders up
# reaches Projects/ReactNews.
# Why: Playwright starts this script from the frontend folder, but the backend
# project and test database live outside the frontend folder.
$reactNewsRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$apiProject = Join-Path $reactNewsRoot "Backend\Api\ReactNews.Api\ReactNews.Api.csproj"
$dataFolder = Join-Path $reactNewsRoot "Backend\Api\ReactNews.Api\Data"
$databasePath = Join-Path $dataFolder "reactnews-e2e.db"

# What: Ensure the API Data folder exists before SQLite tries to create a file.
# How: New-Item with -Force creates the folder only when missing.
# Why: a predictable file path lets the E2E run use its own database instead of
# reusing the normal local development database.
New-Item -ItemType Directory -Force -Path $dataFolder | Out-Null

# What: Remove old E2E database files from previous runs.
# How: Delete only the known e2e database and SQLite sidecar files inside the
# resolved Data folder.
# Why: each E2E run should start from clean state so registration/admin/editorial
# tests are repeatable and do not fail because a previous user already exists.
foreach ($path in @($databasePath, "$databasePath-shm", "$databasePath-wal")) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

# What: Provide runtime configuration for the API process.
# How: ASP.NET Core maps double-underscore environment variable names to nested
# configuration keys such as ConnectionStrings:ReactNews and AdminSeed:Email.
# Why: Playwright needs a seeded admin account and an isolated database without
# requiring user-secrets or editing appsettings.Development.json.
$env:ConnectionStrings__ReactNews = "Data Source=$databasePath"
$env:AdminSeed__Email = "admin-e2e@example.com"
$env:AdminSeed__DisplayName = "E2E Admin"
$env:AdminSeed__Password = "Password123!"
$env:NewsApi__ApiKey = "playwright-does-not-call-newsapi"

# What: Start the real ReactNews API.
# How: dotnet run hosts the API on a test-only port. This script intentionally
# stays attached to dotnet run so Playwright can stop the process when tests end.
# Why: E2E tests should exercise the real backend pipeline, migrations, auth
# cookies, and SQLite persistence rather than mocked HTTP calls.
dotnet run --project $apiProject --urls "http://127.0.0.1:5227"
