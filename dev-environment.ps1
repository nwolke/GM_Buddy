# ========================================
# GM Buddy - Complete Development Environment Manager
# ========================================
# This script demonstrates advanced PowerShell features:
# - Parameter validation
# - Functions
# - Error handling
# - Colored output
# - Process management
# - File operations

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("start", "stop", "restart", "status", "clean", "reset")]
    [string]$Action = "status",
    
    [switch]$OpenBrowser,
    [switch]$Verbose
)

# ========================================
# Configuration
# ========================================
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ClientDir = Join-Path $ScriptDir "gm_buddy.client"
$URLs = @{
    Frontend = "http://localhost:49505"
    ServerAPI = "http://localhost:5000"
    AuthAPI = "http://localhost:6000"
    PgAdmin = "http://localhost:15435"
}

# ========================================
# Helper Functions
# ========================================

function Write-Banner {
    param([string]$Text)
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Magenta
    Write-Host "   $Text" -ForegroundColor Magenta
    Write-Host "=========================================" -ForegroundColor Magenta
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Cyan
}

function Write-Warning {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Red
}

function Test-DockerRunning {
    try {
        $null = docker ps 2>&1
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

function Get-GMBuddyContainerStatus {
    $containers = docker ps -a --filter "name=gm_buddy" --format "{{.Names}}|{{.Status}}|{{.State}}" 2>$null
    
    $status = @{
        Running = @()
        Stopped = @()
        Total = 0
    }
    
    foreach ($container in $containers) {
        if ($container) {
            $parts = $container -split '\|'
            $name = $parts[0]
            $state = $parts[2]
            
            $status.Total++
            if ($state -eq "running") {
                $status.Running += $name
            } else {
                $status.Stopped += $name
            }
        }
    }
    
    return $status
}

function Start-GMBuddyEnvironment {
    Write-Banner "Starting GM Buddy Development Environment"
    
    # Check Docker
    if (-not (Test-DockerRunning)) {
        Write-Error "Docker is not running! Please start Docker Desktop."
        return $false
    }
    
    # Start Docker containers
    Write-Info "Starting Docker containers..."
    Set-Location $ScriptDir
    
    docker-compose up -d
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to start Docker containers"
        return $false
    }
    
    Write-Success "Docker containers started"
    
    # Wait for services
    Write-Info "Waiting for services to initialize (15 seconds)..."
    Start-Sleep -Seconds 15
    
    # Show status
    Write-Success "Environment is ready!"
    Write-Host ""
    Write-Host "Available Services:" -ForegroundColor White
    foreach ($service in $URLs.GetEnumerator()) {
        Write-Host "  • $($service.Key): $($service.Value)" -ForegroundColor Cyan
    }
    
    # Open browser
    if ($OpenBrowser) {
        Write-Info "Opening browser..."
        Start-Process $URLs.Frontend
    }
    
    return $true
}

function Stop-GMBuddyEnvironment {
    Write-Banner "Stopping GM Buddy Development Environment"
    
    Set-Location $ScriptDir
    
    Write-Info "Stopping Docker containers..."
    docker-compose down
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "All containers stopped and removed"
    } else {
        Write-Warning "docker-compose down failed, trying manual cleanup..."
        
        $containerNames = @(
            "gm_buddy_client",
            "gm_buddy_pgadmin",
            "gm_buddy_server",
            "gm_buddy_authorization",
            "gm_buddy_postgres"
        )
        
        docker stop $containerNames 2>$null
        docker rm $containerNames 2>$null
        
        Write-Success "Manual cleanup completed"
    }
}

function Show-GMBuddyStatus {
    Write-Banner "GM Buddy Environment Status"
    
    # Docker status
    if (Test-DockerRunning) {
        Write-Success "Docker is running"
    } else {
        Write-Error "Docker is not running"
        return
    }
    
    # Container status
    $status = Get-GMBuddyContainerStatus
    
    if ($status.Total -eq 0) {
        Write-Warning "No GM Buddy containers found"
        Write-Info "Run with -Action start to create containers"
        return
    }
    
    Write-Host ""
    Write-Host "Container Summary:" -ForegroundColor White
    Write-Host "  Total: $($status.Total)" -ForegroundColor Cyan
    Write-Host "  Running: $($status.Running.Count)" -ForegroundColor Green
    Write-Host "  Stopped: $($status.Stopped.Count)" -ForegroundColor Yellow
    
    Write-Host ""
    Write-Host "Detailed Status:" -ForegroundColor White
    docker ps -a --filter "name=gm_buddy" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    # Check if services are accessible
    if ($status.Running.Count -gt 0) {
        Write-Host ""
        Write-Host "Service URLs:" -ForegroundColor White
        foreach ($service in $URLs.GetEnumerator()) {
            Write-Host "  • $($service.Key): $($service.Value)" -ForegroundColor Cyan
        }
    }
}

function Clear-GMBuddyData {
    Write-Banner "Cleaning GM Buddy Development Environment"
    
    $confirm = Read-Host "This will remove all containers, volumes, and build cache. Continue? (yes/no)"
    
    if ($confirm -ne "yes") {
        Write-Warning "Cleanup cancelled"
        return
    }
    
    Write-Info "Stopping containers..."
    Stop-GMBuddyEnvironment
    
    Write-Info "Removing volumes..."
    docker volume rm gm_buddy_application_pgadmin-data 2>$null
    
    Write-Info "Removing build cache..."
    docker builder prune -f
    
    Write-Info "Removing node_modules..."
    if (Test-Path (Join-Path $ClientDir "node_modules")) {
        Remove-Item (Join-Path $ClientDir "node_modules") -Recurse -Force
        Write-Success "node_modules removed"
    }
    
    Write-Success "Cleanup complete!"
    Write-Info "Run -Action start to rebuild from scratch"
}

function Reset-GMBuddyEnvironment {
    Write-Banner "Resetting GM Buddy Development Environment"
    
    Stop-GMBuddyEnvironment
    Start-Sleep -Seconds 2
    
    Write-Info "Rebuilding containers..."
    Set-Location $ScriptDir
    docker-compose up -d --build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Environment reset and rebuilt!"
    } else {
        Write-Error "Failed to rebuild environment"
    }
}

# ========================================
# Main Script Logic
# ========================================

Set-Location $ScriptDir

switch ($Action) {
    "start" {
        $success = Start-GMBuddyEnvironment
        if ($success -and $OpenBrowser) {
            Start-Process $URLs.Frontend
        }
    }
    
    "stop" {
        Stop-GMBuddyEnvironment
    }
    
    "restart" {
        Stop-GMBuddyEnvironment
        Start-Sleep -Seconds 2
        Start-GMBuddyEnvironment
    }
    
    "status" {
        Show-GMBuddyStatus
    }
    
    "clean" {
        Clear-GMBuddyData
    }
    
    "reset" {
        Reset-GMBuddyEnvironment
    }
}

Write-Host ""
