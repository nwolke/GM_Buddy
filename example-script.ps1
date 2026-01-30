# ========================================
# Example: Simple PowerShell Script Tutorial
# ========================================
# This script demonstrates key PowerShell concepts
# Run it to see how different features work!

# 1. VARIABLES
Write-Host "`n=== VARIABLES ===" -ForegroundColor Cyan
$projectName = "GM Buddy"
$version = "1.0.0"
$isProduction = $false

Write-Host "Project: $projectName"
Write-Host "Version: $version"
Write-Host "Production: $isProduction"

# 2. CONDITIONALS
Write-Host "`n=== CONDITIONALS ===" -ForegroundColor Cyan
if ($isProduction) {
    Write-Host "Running in PRODUCTION mode" -ForegroundColor Red
} else {
    Write-Host "Running in DEVELOPMENT mode" -ForegroundColor Green
}

# 3. LOOPS
Write-Host "`n=== LOOPS ===" -ForegroundColor Cyan
$services = @("Frontend", "Backend", "Database", "Auth")

Write-Host "Available services:"
foreach ($service in $services) {
    Write-Host "  • $service" -ForegroundColor Yellow
}

# 4. FUNCTIONS
Write-Host "`n=== FUNCTIONS ===" -ForegroundColor Cyan

function Get-ServiceStatus {
    param([string]$ServiceName)
    
    Write-Host "Checking status of $ServiceName..." -ForegroundColor Gray
    # Simulate checking
    Start-Sleep -Milliseconds 500
    
    $status = Get-Random -Minimum 0 -Maximum 2
    if ($status -eq 1) {
        Write-Host "  ? $ServiceName is running" -ForegroundColor Green
        return $true
    } else {
        Write-Host "  ? $ServiceName is stopped" -ForegroundColor Red
        return $false
    }
}

foreach ($service in $services) {
    Get-ServiceStatus -ServiceName $service
}

# 5. FILE OPERATIONS
Write-Host "`n=== FILE OPERATIONS ===" -ForegroundColor Cyan

$testFile = "test-file.txt"

# Create a file
"This is a test file created by PowerShell" | Set-Content $testFile
Write-Host "Created: $testFile"

# Check if file exists
if (Test-Path $testFile) {
    Write-Host "File exists: $testFile" -ForegroundColor Green
    
    # Read the file
    $content = Get-Content $testFile
    Write-Host "Content: $content"
    
    # Delete the file
    Remove-Item $testFile
    Write-Host "Deleted: $testFile"
}

# 6. ARRAYS & OBJECTS
Write-Host "`n=== ARRAYS & OBJECTS ===" -ForegroundColor Cyan

$containerInfo = @{
    Name = "gm_buddy_client"
    Port = 49505
    Status = "Running"
    Uptime = "2 hours"
}

Write-Host "Container Information:"
foreach ($key in $containerInfo.Keys) {
    Write-Host "  $key: $($containerInfo[$key])" -ForegroundColor Cyan
}

# 7. ERROR HANDLING
Write-Host "`n=== ERROR HANDLING ===" -ForegroundColor Cyan

try {
    Write-Host "Attempting risky operation..."
    
    # Simulate an operation that might fail
    $random = Get-Random -Minimum 0 -Maximum 2
    if ($random -eq 0) {
        throw "Something went wrong!"
    }
    
    Write-Host "? Operation successful!" -ForegroundColor Green
} catch {
    Write-Host "? Error caught: $_" -ForegroundColor Red
    Write-Host "  Don't worry, we handled it gracefully!" -ForegroundColor Yellow
}

# 8. PARAMETERS (example)
Write-Host "`n=== SCRIPT PARAMETERS ===" -ForegroundColor Cyan
Write-Host "This script could accept parameters like:"
Write-Host '  .\example-script.ps1 -Environment "Production" -EnableDebug' -ForegroundColor Gray
Write-Host ""
Write-Host "To add parameters, use this at the top of your script:"
Write-Host 'param([string]$Environment, [switch]$EnableDebug)' -ForegroundColor Gray

# 9. USEFUL COMMANDS
Write-Host "`n=== USEFUL COMMANDS ===" -ForegroundColor Cyan
Write-Host "Current directory: $(Get-Location)"
Write-Host "Script directory: $PSScriptRoot"
Write-Host "Computer name: $env:COMPUTERNAME"
Write-Host "Username: $env:USERNAME"
Write-Host "PowerShell version: $($PSVersionTable.PSVersion)"

# 10. INTERACTIVE INPUT
Write-Host "`n=== INTERACTIVE INPUT ===" -ForegroundColor Cyan
$response = Read-Host "Would you like to learn more about PowerShell? (yes/no)"

if ($response -eq "yes") {
    Write-Host "`nGreat! Here are some resources:" -ForegroundColor Green
    Write-Host "  • Microsoft Docs: https://docs.microsoft.com/powershell/"
    Write-Host "  • Get-Help: Type 'Get-Help about_*' to see all help topics"
    Write-Host "  • Practice: Try modifying this script!"
} else {
    Write-Host "`nNo problem! You can always come back later." -ForegroundColor Yellow
}

# END
Write-Host "`n=== TUTORIAL COMPLETE ===" -ForegroundColor Magenta
Write-Host "You've seen the basics of PowerShell scripting!"
Write-Host "Now try creating your own scripts for GM Buddy!" -ForegroundColor Green
Write-Host ""

# Keep window open
Read-Host "Press Enter to exit"
