# Rebuild Docker containers with Cognito configuration

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Rebuilding Docker Containers for Cognito" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Step 1: Stopping all containers..." -ForegroundColor Yellow
docker-compose down

Write-Host "`nStep 2: Removing old containers and images..." -ForegroundColor Yellow
docker-compose rm -f
docker rmi gmbuddyreact gmbuddyserver -ErrorAction SilentlyContinue

Write-Host "`nStep 3: Rebuilding with new environment variables..." -ForegroundColor Yellow
docker-compose build --no-cache

Write-Host "`nStep 4: Starting containers..." -ForegroundColor Yellow
docker-compose up -d

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Rebuild Complete!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Write-Host "The app should now be running at:" -ForegroundColor White
Write-Host "  Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "  Backend:  http://localhost:5000" -ForegroundColor Cyan

Write-Host "`nCheck logs with: docker-compose logs -f" -ForegroundColor Gray

Write-Host "`nIMPORTANT:" -ForegroundColor Yellow
Write-Host "- Open http://localhost:3000 in your browser" -ForegroundColor White
Write-Host "- Press F12 to open DevTools" -ForegroundColor White
Write-Host "- Look for: [Cognito Config] Cognito is ENABLED" -ForegroundColor White

Write-Host "`nPress any key to view logs..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

docker-compose logs -f
