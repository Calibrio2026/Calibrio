param(
  [switch]$OpenOnly,
  [switch]$StopExistingOnly
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$app = Join-Path $root "app"
$runtime = Join-Path $root "runtime\node.exe"
$serverScript = Join-Path $app "start-server.cmd"
$serverEntry = Join-Path $app "desktop-server.mjs"
$logDir = Join-Path $root "logs"
$port = "3001"
$url = "http://localhost:$port/"

function Test-CalibrioReady {
  try {
    Invoke-WebRequest -UseBasicParsing -Uri $url -TimeoutSec 1 | Out-Null
    return $true
  } catch {
    return $false
  }
}

function Find-PreferredBrowser {
  $candidates = @(
    $env:CALIBRIO_BROWSER,
    (Join-Path $env:LOCALAPPDATA "BraveSoftware\Brave-Browser\Application\brave.exe"),
    (Join-Path $env:ProgramFiles "BraveSoftware\Brave-Browser\Application\brave.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "BraveSoftware\Brave-Browser\Application\brave.exe")
  ) | Where-Object { $_ }

  foreach ($candidate in $candidates) {
    if ($candidate -and (Test-Path $candidate)) {
      return $candidate
    }
  }

  return $null
}

function Stop-ExistingCalibrioServer {
  $runtimePath = try {
    [System.IO.Path]::GetFullPath((Resolve-Path $runtime).Path)
  } catch {
    [System.IO.Path]::GetFullPath($runtime)
  }

  $listeners = Get-NetTCPConnection -LocalPort ([int]$port) -State Listen -ErrorAction SilentlyContinue
  foreach ($listener in $listeners) {
    $processId = [int]$listener.OwningProcess
    if ($processId -eq $PID) {
      continue
    }

    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
    if (-not $process) {
      continue
    }

    $processPath = $null
    try {
      $processPath = [System.IO.Path]::GetFullPath($process.Path)
    } catch {
      $processPath = $null
    }

    if ($processPath -and ($processPath -ieq $runtimePath)) {
      Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
      Start-Sleep -Milliseconds 300
    }
  }
}

if (-not (Test-Path $logDir)) {
  New-Item -ItemType Directory -Path $logDir | Out-Null
}

if (-not (Test-Path $runtime)) {
  Write-Host "Calibrio could not find its bundled runtime."
  Write-Host "Expected: $runtime"
  Read-Host "Press Enter to close"
  exit 1
}

if (-not (Test-Path $serverScript)) {
  Write-Host "Calibrio could not find its application files."
  Write-Host "Expected: $serverScript"
  Read-Host "Press Enter to close"
  exit 1
}

if (-not (Test-Path $serverEntry)) {
  Write-Host "Calibrio could not find its application server."
  Write-Host "Expected: $serverEntry"
  Read-Host "Press Enter to close"
  exit 1
}

if ($StopExistingOnly) {
  Stop-ExistingCalibrioServer
  exit 0
}

if (-not $OpenOnly) {
  if (-not (Test-CalibrioReady)) {
    Stop-ExistingCalibrioServer
    Set-Content -Path (Join-Path $logDir "server.log") -Value "Starting Calibrio on port $port"

    Start-Process -FilePath $serverScript -ArgumentList $port -WorkingDirectory $app -WindowStyle Hidden | Out-Null
  }
}

$ready = $false
for ($i = 0; $i -lt 40; $i++) {
  if (Test-CalibrioReady) {
    $ready = $true
    break
  }
  Start-Sleep -Milliseconds 250
}

if (-not $ready) {
  Write-Host "Calibrio is starting slowly. Opening the app window anyway."
}

$browser = Find-PreferredBrowser
if ($browser) {
  Start-Process -FilePath $browser -ArgumentList @("--new-window", $url) | Out-Null
} else {
  $windowProcess = New-Object System.Diagnostics.ProcessStartInfo
  $windowProcess.FileName = $url
  $windowProcess.UseShellExecute = $true
  [System.Diagnostics.Process]::Start($windowProcess) | Out-Null
}
