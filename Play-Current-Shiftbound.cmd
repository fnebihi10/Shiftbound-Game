@echo off
setlocal
set "SHIFTBOUND_PLAYER=%~dp0UnityProject\Builds\WindowsPolished\Shiftbound.exe"
if not exist "%SHIFTBOUND_PLAYER%" (
  echo Current player has not been built. Open GoldenRooftops in Unity.
  pause
  exit /b 1
)
start "Shiftbound - current GoldenRooftops" /D "%~dp0UnityProject\Builds\WindowsPolished" "%SHIFTBOUND_PLAYER%"
