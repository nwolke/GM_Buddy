@echo off
REM ========================================
REM GM Buddy - Docker Down Script
REM ========================================

echo Stopping and removing GM Buddy containers...
echo.

docker stop gm_buddy_client gm_buddy_pgadmin gm_buddy_server gm_buddy_authorization gm_buddy_postgres
docker rm gm_buddy_client gm_buddy_pgadmin gm_buddy_server gm_buddy_authorization gm_buddy_postgres

echo.
echo All containers stopped and removed!
echo.
pause
