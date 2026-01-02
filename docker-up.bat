@echo off
REM ========================================
REM GM Buddy - Docker Up Script
REM ========================================

echo Starting GM Buddy with Docker Compose...
echo.

docker-compose up -d

echo.
echo GM Buddy containers are starting!
echo Opening browser in 15 seconds...
timeout /t 15 /nobreak >nul

start http://localhost:49505

echo.
echo GM Buddy is now running at http://localhost:49505
echo.
pause
