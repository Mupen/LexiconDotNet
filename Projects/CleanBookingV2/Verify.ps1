try {
    $ErrorActionPreference = "Stop"

    $projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
    $testProject = Join-Path $projectRoot "UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj"
    $frontendRoot = Join-Path $projectRoot "Frontend\Public"
    $packageJson = Join-Path $frontendRoot "package.json"

    if (-not (Test-Path -LiteralPath $testProject)) {
        throw "Test project was not found: $testProject"
    }

    if (-not (Test-Path -LiteralPath $packageJson)) {
        throw "Frontend package.json was not found: $packageJson"
    }

    Write-Host "Restoring backend test project..."
    dotnet restore $testProject --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE."
    }

    Write-Host ""
    Write-Host "Building backend test project without restore..."
    dotnet build $testProject --no-restore --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE."
    }

    Write-Host ""
    Write-Host "Running backend tests without restore or build..."
    dotnet test $testProject --no-restore --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE."
    }

    Write-Host ""
    Write-Host "Building frontend..."
    Push-Location $frontendRoot
    try {
        npm run build
        if ($LASTEXITCODE -ne 0) {
            throw "npm run build failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }

    Write-Host ""
    Write-Host "CleanBookingV2 verification completed."
}
catch {
    Write-Host ""
    Write-Host "Verification failed:" -ForegroundColor Red
    Write-Host $_
}
finally {
    Write-Host ""
    Read-Host "Press Enter to close"
}