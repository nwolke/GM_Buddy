#!/usr/bin/env pwsh
# ===========================================
# GM_Buddy Docker & Visual Studio Cache Cleanup Script
# ===========================================
# This script cleans up Docker images, build caches, and Visual Studio
# temporary files that can cause "cannot find gm_buddy_server" errors.

param(
    [switch]$SkipDockerPrune,
    [switch]$SkipVSCache,
    [switch]$SkipBinObj,
    [switch]$Rebuild
)

Write-Host "?? GM_Buddy Cache Cleanup Script" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# 1. Stop and remove Docker containers
Write-Host "?? Stopping Docker containers..." -ForegroundColor Yellow
docker-compose down
if ($LASTEXITCODE -ne 0) {
    Write-Host "??  Warning: docker-compose down failed (might not be running)" -ForegroundColor Yellow
}
Write-Host "? Docker containers stopped" -ForegroundColor Green
Write-Host ""

# 2. Remove specific Docker images
Write-Host "???  Removing gm-buddy-server/gmbuddyserver images..." -ForegroundColor Yellow
$images = @(
    docker images -q "gm-buddy-server"
    docker images -q "gmbuddyserver"
) | Where-Object { $_ } | Select-Object -Unique
if ($images) {
    docker rmi $images -f
    Write-Host "? Removed gm-buddy-server/gmbuddyserver images" -ForegroundColor Green
} else {
    Write-Host "??  No gm-buddy-server or gmbuddyserver images found" -ForegroundColor Gray
}

$images = docker images -q "*gmbuddyreact*"
if ($images) {
    docker rmi $images -f
    Write-Host "? Removed gmbuddyreact images" -ForegroundColor Green
} else {
    Write-Host "??  No gmbuddyreact images found" -ForegroundColor Gray
}
Write-Host ""

# 3. Clean Docker build cache
if (-not $SkipDockerPrune) {
    Write-Host "?? Cleaning Docker build cache..." -ForegroundColor Yellow
    docker builder prune -a -f
    Write-Host "? Docker build cache cleaned" -ForegroundColor Green
    Write-Host ""
}

# 4. Remove Visual Studio .vs folder
if (-not $SkipVSCache) {
    Write-Host "?? Removing Visual Studio cache (.vs folder)..." -ForegroundColor Yellow
    if (Test-Path ".\.vs") {
        Remove-Item -Recurse -Force ".\.vs"
        Write-Host "? Visual Studio cache removed" -ForegroundColor Green
    } else {
        Write-Host "??  No .vs folder found" -ForegroundColor Gray
    }
    Write-Host ""
}

# 5. Clean bin/obj folders
if (-not $SkipBinObj) {
    Write-Host "???  Removing bin and obj folders..." -ForegroundColor Yellow
    $binObjFolders = Get-ChildItem -Recurse -Include bin,obj -Directory -ErrorAction SilentlyContinue
    if ($binObjFolders) {
        $binObjFolders | Remove-Item -Recurse -Force
        Write-Host "? Removed $($binObjFolders.Count) bin/obj folders" -ForegroundColor Green
    } else {
        Write-Host "??  No bin/obj folders found" -ForegroundColor Gray
    }
    Write-Host ""
}

# 6. Optional rebuild
if ($Rebuild) {
    Write-Host "?? Rebuilding Docker images from scratch..." -ForegroundColor Yellow
    docker-compose build --no-cache
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Docker images rebuilt successfully" -ForegroundColor Green
    } else {
        Write-Host "? Docker rebuild failed" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

Write-Host "?? Cleanup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Open Visual Studio 2022" -ForegroundColor White
Write-Host "  2. Select 'Container (Dockerfile)' launch profile" -ForegroundColor White
Write-Host "  3. Press F5 to debug" -ForegroundColor White
Write-Host ""
Write-Host "Or run:" -ForegroundColor Cyan
Write-Host "  docker-compose up --build" -ForegroundColor White
Write-Host ""
