@echo off
setlocal

REM Get correct LAN IP
for /f "tokens=2 delims=:" %%f in ('ipconfig ^| findstr /R "IPv4.*192\.168\."') do set ip=%%f
set ip=%ip:~1%

REM Set URL
set url=http://%ip%:5000

echo Starting app at http://%ip%:5000
start "" "ResponseApp.exe" --urls "http://0.0.0.0:5000"

start "" http://%ip%:5000
pause