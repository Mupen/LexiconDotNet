$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Building backend..."
# Build with one MSBuild node and disabled build servers for deterministic
# verification across the multi-project backend. Single-node builds avoid stale
# build-server state and make failures easier to reproduce from command output.
dotnet build (Join-Path $root "Backend\Api\ReactNews.Api\ReactNews.Api.csproj") --disable-build-servers -m:1
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Building backend tests..."
dotnet build (Join-Path $root "Backend\UnitTests\ReactNews.UnitTests\ReactNews.UnitTests.csproj") --no-restore --disable-build-servers -m:1
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Running backend tests..."
dotnet test (Join-Path $root "Backend\UnitTests\ReactNews.UnitTests\ReactNews.UnitTests.csproj") --no-build --no-restore --disable-build-servers
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Building frontend..."
Push-Location (Join-Path $root "Frontend\Public")
try {
    Write-Host "Running frontend tests..."
    npm run test
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    npm run build
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host "Running frontend E2E tests..."
    npm run test:e2e
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
finally {
    Pop-Location
}

Write-Host "ReactNews verification passed."
