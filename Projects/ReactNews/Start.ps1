$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiPort = 5217
$frontendPort = 5173
$apiUrl = "http://localhost:$apiPort"
$frontendUrl = "http://localhost:$frontendPort"

$apiLogFolder = Join-Path $root "Backend\Api\ReactNews.Api\Logs"
$frontendLogFolder = Join-Path $root "Frontend\Public\Logs"

New-Item -ItemType Directory -Force -Path $apiLogFolder, $frontendLogFolder | Out-Null

$apiOut = Join-Path $apiLogFolder "api-dev.log"
$apiErr = Join-Path $apiLogFolder "api-dev.err.log"
$frontendOut = Join-Path $frontendLogFolder "frontend-dev.log"
$frontendErr = Join-Path $frontendLogFolder "frontend-dev.err.log"

foreach ($logFile in @($apiOut, $apiErr, $frontendOut, $frontendErr)) {
    if (-not (Test-Path -LiteralPath $logFile)) {
        New-Item -ItemType File -Path $logFile | Out-Null
    }
}

function Test-PortOpen {
    param([int]$Port)

    $client = [System.Net.Sockets.TcpClient]::new()

    try {
        $connection = $client.BeginConnect("127.0.0.1", $Port, $null, $null)
        $connected = $connection.AsyncWaitHandle.WaitOne(300)

        if (-not $connected) {
            return $false
        }

        $client.EndConnect($connection)
        return $true
    }
    catch {
        return $false
    }
    finally {
        $client.Close()
    }
}

function Show-RecentLog {
    param(
        [string]$Label,
        [string]$Path
    )

    if ((Test-Path -LiteralPath $Path) -and ((Get-Item -LiteralPath $Path).Length -gt 0)) {
        Write-Host ""
        Write-Host "$Label log:"
        Get-Content -LiteralPath $Path -Tail 30
    }
}

function Start-AndCheck {
    param(
        [string]$Name,
        [string]$FilePath,
        [string[]]$ArgumentList,
        [string]$WorkingDirectory,
        [string]$OutputLog,
        [string]$ErrorLog
    )

    $process = Start-Process -FilePath $FilePath `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $WorkingDirectory `
        -WindowStyle Hidden `
        -RedirectStandardOutput $OutputLog `
        -RedirectStandardError $ErrorLog `
        -PassThru

    Start-Sleep -Seconds 2

    if ($process.HasExited) {
        Show-RecentLog "$Name error" $ErrorLog
        Show-RecentLog "$Name output" $OutputLog
        throw "$Name stopped immediately. Check the logs above."
    }

    return $process
}

function Stop-ProcessTree {
    param(
        [System.Diagnostics.Process]$Process,
        [string]$Name
    )

    if (-not $Process -or $Process.HasExited) {
        return
    }

    Write-Host "Stopping $Name..."

    if ($IsWindows -or $env:OS -eq "Windows_NT") {
        taskkill.exe /PID $Process.Id /T /F | Out-Null
    }
    else {
        Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
    }
}

$apiProcess = $null
$frontendProcess = $null

try {
    if (Test-PortOpen $apiPort) {
        throw "ReactNews API port $apiPort is already in use. Stop the existing process first, then run this script again."
    }

    Write-Host "Starting ReactNews API on $apiUrl"
    $apiProcess = Start-AndCheck `
        -Name "ReactNews API" `
        -FilePath "dotnet" `
        -ArgumentList @("run", "--project", "Backend\Api\ReactNews.Api\ReactNews.Api.csproj", "--urls", $apiUrl) `
        -WorkingDirectory $root `
        -OutputLog $apiOut `
        -ErrorLog $apiErr

    if (Test-PortOpen $frontendPort) {
        throw "ReactNews frontend port $frontendPort is already in use. Stop the existing process first, then run this script again."
    }

    $npm = "npm"

    if ($IsWindows -or $env:OS -eq "Windows_NT") {
        $npm = "npm.cmd"
    }

    Write-Host "Starting ReactNews frontend on $frontendUrl"
    $frontendProcess = Start-AndCheck `
        -Name "ReactNews frontend" `
        -FilePath $npm `
        -ArgumentList @("run", "dev") `
        -WorkingDirectory (Join-Path $root "Frontend\Public") `
        -OutputLog $frontendOut `
        -ErrorLog $frontendErr

    Write-Host ""
    Write-Host "ReactNews start complete."
    Write-Host "API: $apiUrl"
    Write-Host "Frontend: $frontendUrl"
    Write-Host "API logs: $apiLogFolder"
    Write-Host "Frontend logs: $frontendLogFolder"
    Write-Host ""
    Write-Host "Leave this window open while using ReactNews."
    Write-Host "Open the frontend in your browser: $frontendUrl"
    Write-Host ""
    Write-Host "First-run account flow:"
    Write-Host "1. Open $frontendUrl/login"
    Write-Host "2. Create a Reader account to use Personal, Saved, and Profile."
    Write-Host "3. To use Editorial, configure AdminSeed user-secrets before starting the API."
    Write-Host "4. Passwords must be at least 8 characters."
    Write-Host ""
    Write-Host "Press Enter in this window to stop ReactNews."

    [pscustomobject]@{
        ApiProcessId = $apiProcess.Id
        FrontendProcessId = $frontendProcess.Id
        ApiUrl = $apiUrl
        FrontendUrl = $frontendUrl
        ApiLog = $apiOut
        FrontendLog = $frontendOut
    }

    Read-Host | Out-Null
}
finally {
    Stop-ProcessTree -Process $frontendProcess -Name "ReactNews frontend"
    Stop-ProcessTree -Process $apiProcess -Name "ReactNews API"
}
