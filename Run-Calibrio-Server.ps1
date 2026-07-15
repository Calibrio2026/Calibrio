$ErrorActionPreference = "Stop"

$calibrioHome = Split-Path -Parent $MyInvocation.MyCommand.Path
$appDir = Join-Path $calibrioHome "app"
$serverScript = Join-Path $appDir "start-server.cmd"
$logDir = Join-Path $calibrioHome "logs"
$log = Join-Path $logDir "run-server.log"

try {
  if (!(Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir | Out-Null
  }

  "Launching Calibrio server at $(Get-Date -Format o)" | Out-File -FilePath $log -Append
  "Server script: $serverScript" | Out-File -FilePath $log -Append

  Set-Location $appDir
  & $serverScript 3001 *>> $log
} catch {
  "Calibrio server launch failed at $(Get-Date -Format o)" | Out-File -FilePath $log -Append
  $_ | Out-String | Out-File -FilePath $log -Append
  Read-Host "Calibrio server failed. Press Enter to close"
}
