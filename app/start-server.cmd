@echo off
set "CALIBRIO_LOG_DIR=%~dp0..\logs"
set "CALIBRIO_PORT=%~1"
if "%CALIBRIO_PORT%"=="" set "CALIBRIO_PORT=3001"
set "HOST=0.0.0.0"
if not exist "%CALIBRIO_LOG_DIR%" mkdir "%CALIBRIO_LOG_DIR%"
echo Starting Calibrio on port %CALIBRIO_PORT% > "%CALIBRIO_LOG_DIR%\server.log"
"%~dp0..\runtime\node.exe" "%~dp0desktop-server.mjs" %CALIBRIO_PORT% >> "%CALIBRIO_LOG_DIR%\server.log" 2>&1
