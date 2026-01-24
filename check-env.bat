@echo off
echo.
echo ========================================
echo Checking Vite Environment Configuration
echo ========================================
echo.

cd GM_Buddy.React

echo Current directory:
cd

echo.
echo Checking if .env.local exists...
if exist .env.local (
    echo ? .env.local found
    echo.
    echo Contents:
    type .env.local
) else (
    echo ? .env.local NOT FOUND
    echo    Expected at: GM_Buddy.React\.env.local
)

echo.
echo ========================================
echo.
echo IMPORTANT: Vite must be RESTARTED to load .env.local
echo.
echo In the terminal running 'npm run dev':
echo   1. Press Ctrl+C
echo   2. Run: npm run dev
echo   3. Refresh browser (Ctrl+Shift+R)
echo.
echo ========================================
echo.

pause
