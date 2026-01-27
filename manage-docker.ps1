# ========================================
# GM Buddy - Docker Management Script
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("up", "down", "restart", "logs", "status")]
    [string]$Action = "status"
)

# Color functions
function Write-Success {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Cyan
}

function Write-Warning {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Red
}

# Get the script directory (where docker-compose.yml is)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Info "========================================="
Write-Info "   GM Buddy - Docker Manager"
Write-Info "========================================="
Write-Host ""

switch ($Action) {
    "up" {
        Write-Info "Starting GM Buddy containers..."
        docker-compose up -d
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "? Containers started successfully!"
            Write-Info "`nWaiting 15 seconds for services to initialize..."
            Start-Sleep -Seconds 15
            
            Write-Success "`nGM Buddy is now running!"
            Write-Host "  • Frontend: http://localhost:49505" -ForegroundColor White
            Write-Host "  • Server API: http://localhost:5000" -ForegroundColor White
            Write-Host "  • Auth API: http://localhost:6000" -ForegroundColor White
            Write-Host "  • pgAdmin: http://localhost:15435" -ForegroundColor White
            
            $openBrowser = Read-Host "`nOpen browser? (y/n)"
            if ($openBrowser -eq 'y') {
                Start-Process "http://localhost:49505"
            }
        } else {
            Write-Error "? Failed to start containers"
        }
    }
    
    "down" {
        Write-Info "Stopping GM Buddy containers..."
        docker-compose down
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "? Containers stopped and removed successfully!"
        } else {
            Write-Warning "? Docker Compose failed, trying manual stop..."
            docker stop gm_buddy_client gm_buddy_pgadmin gm_buddy_server gm_buddy_authorization gm_buddy_postgres 2>$null
            docker rm gm_buddy_client gm_buddy_pgadmin gm_buddy_server gm_buddy_authorization gm_buddy_postgres 2>$null
            Write-Success "? Containers stopped manually"
        }
    }
    
    "restart" {
        Write-Info "Restarting GM Buddy..."
        & $MyInvocation.MyCommand.Path -Action down
        Write-Host ""
        Start-Sleep -Seconds 2
        & $MyInvocation.MyCommand.Path -Action up
    }
    
    "logs" {
        Write-Info "Showing logs (Ctrl+C to exit)..."
        docker-compose logs -f
    }
    
    "status" {
        Write-Info "Container Status:"
        Write-Host ""
        docker ps -a --filter "name=gm_buddy" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        
        Write-Host "`n"
        Write-Info "Available commands:"
        Write-Host "  .\manage-docker.ps1 -Action up       # Start containers" -ForegroundColor White
        Write-Host "  .\manage-docker.ps1 -Action down     # Stop containers" -ForegroundColor White
        Write-Host "  .\manage-docker.ps1 -Action restart  # Restart containers" -ForegroundColor White
        Write-Host "  .\manage-docker.ps1 -Action logs     # View logs" -ForegroundColor White
        Write-Host "  .\manage-docker.ps1 -Action status   # Show this status" -ForegroundColor White
    }
}

Write-Host ""
