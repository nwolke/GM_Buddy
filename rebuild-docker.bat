@echo off
echo.
echo ========================================
echo Rebuilding Docker Containers for Cognito
echo ========================================
echo.

echo Step 1: Stopping all containers...
docker-compose down

echo.
echo Step 2: Removing old containers and images...
docker-compose rm -f
docker rmi gmbuddyreact gmbuddyserver 2>NUL

echo.
echo Step 3: Rebuilding with new environment variables...
docker-compose build --no-cache

echo.
echo Step 4: Starting containers...
docker-compose up -d

echo.
echo ========================================
echo Rebuild Complete!
echo ========================================
echo.
echo The app should now be running at:
echo   Frontend: http://localhost:3000
echo   Backend:  http://localhost:5000
echo.
echo Check logs with: docker-compose logs -f
echo.
echo IMPORTANT: 
echo - Open http://localhost:3000 in your browser
echo - Press F12 to open DevTools
echo - Look for: [Cognito Config] Cognito is ENABLED
echo.
pause
