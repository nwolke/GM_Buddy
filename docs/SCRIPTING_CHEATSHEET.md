# Scripting Cheat Sheet

## ?? Quick Reference

### Batch (.bat) Basics

```batch
@echo off                     REM Hide command output
echo Hello World              REM Print text
set NAME=John                 REM Set variable
echo Hi %NAME%                REM Use variable (with %)
pause                         REM Wait for keypress
cd C:\folder                  REM Change directory
dir                           REM List files
mkdir newfolder               REM Create folder
del file.txt                  REM Delete file
copy src dest                 REM Copy file
if exist file.txt ( ... )     REM Check if file exists
goto :label                   REM Jump to label
:label                        REM Define label
exit /b 0                     REM Exit with code 0 (success)
REM This is a comment
```

### PowerShell (.ps1) Basics

```powershell
# This is a comment
Write-Host "Hello World"                  # Print (colored)
$name = "John"                            # Set variable
Write-Host "Hi $name"                     # Use variable (with $)
Read-Host "Press Enter"                   # Wait for input
Set-Location "C:\folder"                  # Change directory (cd works)
Get-ChildItem                             # List files (ls, dir work)
New-Item -Type Directory "folder"         # Create folder (mkdir works)
Remove-Item "file.txt"                    # Delete file (del works)
Copy-Item "src" "dest"                    # Copy file (copy works)
Test-Path "file.txt"                      # Check if file exists (returns $true/$false)
if ($true) { "yes" } else { "no" }        # If statement
foreach ($item in $list) { }              # For loop
function MyFunc { }                       # Function
param([string]$Name)                      # Script parameters
$LASTEXITCODE                             # Exit code of last command
```

---

## ?? Colors in PowerShell

```powershell
Write-Host "Red text" -ForegroundColor Red
Write-Host "Green text" -ForegroundColor Green
Write-Host "Yellow text" -ForegroundColor Yellow
Write-Host "Cyan text" -ForegroundColor Cyan
Write-Host "Magenta text" -ForegroundColor Magenta
Write-Host "White text" -ForegroundColor White
```

---

## ?? File Operations

### Batch
```batch
if exist "file.txt" (
    echo File exists
    del "file.txt"
) else (
    echo File does not exist
)
```

### PowerShell
```powershell
if (Test-Path "file.txt") {
    Write-Host "File exists"
    Remove-Item "file.txt"
} else {
    Write-Host "File does not exist"
}
```

---

## ?? Loops

### Batch
```batch
REM Loop through files
for %%f in (*.txt) do (
    echo Found: %%f
)

REM Loop through list
for %%i in (1 2 3 4 5) do (
    echo Number: %%i
)
```

### PowerShell
```powershell
# Loop through files
Get-ChildItem "*.txt" | ForEach-Object {
    Write-Host "Found: $_"
}

# Loop through list
foreach ($i in 1..5) {
    Write-Host "Number: $i"
}

# Alternative
1..5 | ForEach-Object {
    Write-Host "Number: $_"
}
```

---

## ? Conditionals

### Batch
```batch
if "%NAME%"=="John" (
    echo Hello John
) else (
    echo Who are you?
)

if exist "file.txt" (
    echo File exists
)

if %NUMBER% GTR 10 (
    echo Greater than 10
)
```

### PowerShell
```powershell
if ($name -eq "John") {
    Write-Host "Hello John"
} else {
    Write-Host "Who are you?"
}

if (Test-Path "file.txt") {
    Write-Host "File exists"
}

if ($number -gt 10) {
    Write-Host "Greater than 10"
}
```

---

## ?? Functions

### Batch
```batch
call :MyFunction "arg1" "arg2"
goto :end

:MyFunction
echo First arg: %~1
echo Second arg: %~2
exit /b 0

:end
```

### PowerShell
```powershell
function MyFunction {
    param(
        [string]$FirstArg,
        [string]$SecondArg
    )
    
    Write-Host "First arg: $FirstArg"
    Write-Host "Second arg: $SecondArg"
}

# Call it
MyFunction -FirstArg "hello" -SecondArg "world"
```

---

## ?? Error Handling

### Batch
```batch
docker-compose up -d
if errorlevel 1 (
    echo Error occurred!
    exit /b 1
)
echo Success!
```

### PowerShell
```powershell
try {
    docker-compose up -d
    
    if ($LASTEXITCODE -ne 0) {
        throw "Docker compose failed"
    }
    
    Write-Host "Success!"
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
```

---

## ?? User Input

### Batch
```batch
set /p NAME="Enter your name: "
echo Hello %NAME%
```

### PowerShell
```powershell
$name = Read-Host "Enter your name"
Write-Host "Hello $name"

# With validation
do {
    $choice = Read-Host "Continue? (y/n)"
} while ($choice -ne 'y' -and $choice -ne 'n')
```

---

## ?? Common Patterns

### Check if running as Admin (PowerShell)
```powershell
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Please run as Administrator" -ForegroundColor Red
    exit 1
}
```

### Script parameters (PowerShell)
```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$RequiredParam,
    
    [Parameter(Mandatory=$false)]
    [string]$OptionalParam = "default value",
    
    [ValidateSet("option1", "option2", "option3")]
    [string]$LimitedOptions,
    
    [switch]$EnableFeature  # Boolean flag
)

# Usage:
# .\script.ps1 -RequiredParam "value" -EnableFeature
```

### Get script directory
```powershell
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir
```

---

## ?? Docker Commands

### Batch
```batch
REM Start containers
docker-compose up -d

REM Stop containers
docker-compose down

REM View logs
docker-compose logs -f

REM Rebuild
docker-compose up -d --build
```

### PowerShell
```powershell
# Start containers
docker-compose up -d

# Stop containers
docker-compose down

# View logs
docker-compose logs -f

# Rebuild
docker-compose up -d --build

# Check status
docker ps -a --filter "name=gm_buddy"

# Stop specific containers
$containers = @("container1", "container2")
docker stop $containers
docker rm $containers
```

---

## ?? Useful PowerShell Cmdlets

```powershell
Get-Command                    # List all commands
Get-Help <command>             # Get help for a command
Get-ChildItem                  # List files (alias: ls, dir)
Set-Location                   # Change directory (alias: cd)
Get-Content                    # Read file content (alias: cat)
Set-Content                    # Write to file
Add-Content                    # Append to file
Test-Path                      # Check if file/folder exists
Select-String                  # Search text (like grep)
Measure-Object                 # Count, sum, average, etc.
Where-Object                   # Filter (alias: where, ?)
ForEach-Object                 # Loop (alias: foreach, %)
Sort-Object                    # Sort (alias: sort)
Group-Object                   # Group (alias: group)
```

---

## ?? Tips

1. **In PowerShell**: Use `Get-Command *search*` to find commands
2. **Get help**: `Get-Help <command> -Examples`
3. **Tab completion**: Press Tab to auto-complete
4. **History**: Press Up arrow for previous commands
5. **Stop script**: Press Ctrl+C
6. **Pipeline**: Pipe output to next command: `Get-ChildItem | Where-Object { $_.Length -gt 1MB }`

---

## ?? Resources

- **Batch**: https://ss64.com/nt/
- **PowerShell**: https://docs.microsoft.com/powershell/
- **Built-in help**: `Get-Help about_*` (lists all help topics)
