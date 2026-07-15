@echo off
setlocal

set "CALIBRIO_HOME=%~dp0"
set "CALIBRIO_EXE=%CALIBRIO_HOME%Calibrio.exe"

if not exist "%CALIBRIO_EXE%" (
  echo Calibrio could not find Calibrio.exe.
  echo Expected: "%CALIBRIO_EXE%"
  pause
  exit /b 1
)

start "" "%CALIBRIO_EXE%"
endlocal
