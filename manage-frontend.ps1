# ========================================
# GM Buddy - Frontend Development Script
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("dev", "build", "preview", "install")]
    [string]$Action = "dev"
)

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ClientDir = Join-Path $ScriptDir "GM_Buddy.React"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "   GM Buddy - React Frontend Manager" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if client directory exists
if (-not (Test-Path $ClientDir)) {
    Write-Host "Error: GM_Buddy.React directory not found!" -ForegroundColor Red
    exit 1
}

Set-Location $ClientDir

switch ($Action) {
    "install" {
        Write-Host "Installing dependencies..." -ForegroundColor Yellow
        npm install
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Dependencies installed successfully!" -ForegroundColor Green
        } else {
            Write-Host "? Installation failed" -ForegroundColor Red
        }
    }
    
    "dev" {
        Write-Host "Starting development server..." -ForegroundColor Yellow
        Write-Host "Server will run at: http://localhost:3000" -ForegroundColor Cyan
        Write-Host "API proxy: http://localhost:5000" -ForegroundColor Cyan
        Write-Host ""
        npm run dev
    }
    
    "build" {
        Write-Host "Building for production..." -ForegroundColor Yellow
        npm run build
        
        
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Build completed successfully!" -ForegroundColor Green
            Write-Host "Output directory: GM_Buddy.React/dist" -ForegroundColor Cyan
        } else {
            Write-Host "? Build failed" -ForegroundColor Red
        }
    }
    
    "preview" {
        Write-Host "Starting preview server..." -ForegroundColor Yellow
        Write-Host "First, building the project..." -ForegroundColor Cyan
        npm run build
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "Starting preview..." -ForegroundColor Yellow
            npm run preview
        } else {
            Write-Host "? Build failed, cannot preview" -ForegroundColor Red
        }
    }
}

Write-Host ""
