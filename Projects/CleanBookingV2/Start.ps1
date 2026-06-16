$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$frontendRoot = Join-Path $projectRoot "Frontend\Public"
$apiProject = Join-Path $projectRoot "Api\CleanBookingV2.Api\CleanBookingV2.Api.csproj"
$apiLogRoot = Join-Path $projectRoot "Api\CleanBookingV2.Api\Logs"
$frontendLogRoot = Join-Path $frontendRoot "logs"
$apiLog = Join-Path $apiLogRoot "api-dev.log"
$frontendLog = Join-Path $frontendLogRoot "frontend-dev.log"

if (-not (Test-Path -LiteralPath $apiProject)) {
    throw "API project was not found: $apiProject"
}

if (-not (Test-Path -LiteralPath (Join-Path $frontendRoot "package.json"))) {
    throw "Frontend package.json was not found: $frontendRoot"
}

if (-not (Get-Command wt -ErrorAction SilentlyContinue)) {
    throw "Windows Terminal was not found."
}

New-Item -ItemType Directory -Force -Path $apiLogRoot | Out-Null
New-Item -ItemType Directory -Force -Path $frontendLogRoot | Out-Null

$tempRoot = Join-Path $env:TEMP "CleanBookingV2"
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

$apiScript = Join-Path $tempRoot "Start-Api.ps1"
$frontendScript = Join-Path $tempRoot "Start-Frontend.ps1"

@"
Set-Location '$projectRoot'
dotnet run --project '$apiProject' --launch-profile http *>&1 | Tee-Object -FilePath '$apiLog' -Append
"@ | Set-Content -LiteralPath $apiScript -Encoding UTF8

@"
Set-Location '$frontendRoot'
npm run dev *>&1 | Tee-Object -FilePath '$frontendLog' -Append
"@ | Set-Content -LiteralPath $frontendScript -Encoding UTF8

wt `
    new-tab --title "CleanBookingV2 API" pwsh -NoExit -ExecutionPolicy Bypass -File "$apiScript" `
    `; new-tab --title "CleanBookingV2 Frontend" pwsh -NoExit -ExecutionPolicy Bypass -File "$frontendScript"

Write-Host ""
Write-Host "CleanBookingV2 development servers are starting."
Write-Host "API / Swagger: http://localhost:5217/swagger"
Write-Host "React app:     http://localhost:5173"
Write-Host "API log:       $apiLog"
Write-Host "Frontend log:  $frontendLog"
Write-Host ""
Write-Host "Keep both Windows Terminal tabs open while testing."