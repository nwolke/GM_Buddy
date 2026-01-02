# Batch & PowerShell Scripting Guide for GM Buddy

## ?? Table of Contents
1. [What Are Scripts?](#what-are-scripts)
2. [Batch vs PowerShell](#batch-vs-powershell)
3. [Getting Started](#getting-started)
4. [Common Commands](#common-commands)
5. [Examples for GM Buddy](#examples-for-gm-buddy)
6. [Best Practices](#best-practices)

---

## ?? What Are Scripts?

**Scripts** are text files containing a series of commands that automate repetitive tasks. Instead of typing the same commands over and over, you write them once and run the script.

### Why Use Scripts?
- ?? **Save Time**: Automate repetitive tasks
- ?? **Consistency**: Same commands every time, no typos
- ?? **Documentation**: Scripts document your workflow
- ?? **Efficiency**: One click vs many manual steps

---

## ?? Batch vs PowerShell

| Feature | Batch (.bat) | PowerShell (.ps1) |
|---------|-------------|-------------------|
| **Age** | 1980s (DOS) | 2006 (Modern) |
| **Complexity** | Simple | Advanced |
| **Learning Curve** | Easy | Moderate |
| **Power** | Limited | Very Powerful |
| **Best For** | Simple tasks | Complex automation |
| **Data Handling** | Text only | Objects |
| **Syntax** | Simple | Rich & expressive |

### When to Use Batch
- ? Simple file operations
- ? Quick one-liners
- ? Legacy system compatibility
- ? When you need maximum compatibility

### When to Use PowerShell
- ? Complex logic (if/else, loops)
- ? Working with objects (files, databases, APIs)
- ? Error handling
- ? Modern Windows automation
- ? Anything requiring variables and functions

---

## ?? Getting Started

### Running Batch Scripts

1. **Create**: Save a file with `.bat` extension (e.g., `hello.bat`)
2. **Edit**: Use any text editor (Notepad, VS Code)
3. **Run**: Double-click the file, or run from Command Prompt

**Example:**
```batch
@echo off
echo Hello, World!
pause
```

### Running PowerShell Scripts

1. **Create**: Save a file with `.ps1` extension (e.g., `hello.ps1`)
2. **Edit**: Use VS Code or PowerShell ISE
3. **Run**: Right-click ? "Run with PowerShell", or from PowerShell terminal

?? **First Time Setup** (Security):
```powershell
# Open PowerShell as Administrator and run:
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

This allows you to run local scripts (necessary for security).

**Example:**
```powershell
Write-Host "Hello, World!" -ForegroundColor Green
Read-Host "Press Enter to exit"
```

---

## ?? Common Commands

### Batch Commands

```batch
@echo off              REM Don't show commands as they run
echo Hello             REM Print text to screen
pause                  REM Wait for user to press a key
set VAR=value          REM Create a variable
%VAR%                  REM Use a variable
cd C:\folder           REM Change directory
dir                    REM List files
mkdir newfolder        REM Create directory
del file.txt           REM Delete file
copy src dest          REM Copy file
move src dest          REM Move file
if exist file.txt (    REM Check if file exists
    echo File exists
)
goto :label            REM Jump to label
:label                 REM Define a label
REM This is a comment
```

### PowerShell Commands

```powershell
# Comments start with #
Write-Host "Text"                    # Print to screen (colored)
Write-Output "Text"                  # Output to pipeline
$var = "value"                       # Create variable
$var                                 # Use variable
Set-Location "C:\folder"             # Change directory (cd works too)
Get-ChildItem                        # List files (ls, dir work too)
New-Item -Type Directory "folder"    # Create directory (mkdir works)
Remove-Item "file.txt"               # Delete file (rm, del work)
Copy-Item "src" "dest"               # Copy file (copy works)
Move-Item "src" "dest"               # Move file (move works)
Test-Path "file.txt"                 # Check if file exists
if ($true) { "yes" } else { "no" }   # Conditional
foreach ($item in $array) { }        # Loop
function MyFunc { }                  # Define function
param([string]$Name)                 # Script parameters
```

---

## ?? Examples for GM Buddy

### 1. Docker Management (Batch)

**docker-down.bat** (Already created!)
```batch
@echo off
echo Stopping GM Buddy containers...
docker stop gm_buddy_client gm_buddy_pgadmin gm_buddy_server gm_buddy_authorization gm_buddy_postgres
docker rm gm_buddy_client gm_buddy_pgadmin gm_buddy_server gm_buddy_authorization gm_buddy_postgres
echo Done!
pause
```

**Usage**: Just double-click `docker-down.bat`

---

### 2. Docker Management (PowerShell)

**manage-docker.ps1** (Already created!)
```powershell
# Advanced script with parameters and colored output
.\manage-docker.ps1 -Action up       # Start containers
.\manage-docker.ps1 -Action down     # Stop containers
.\manage-docker.ps1 -Action restart  # Restart
.\manage-docker.ps1 -Action status   # Check status
```

---

### 3. Frontend Development (PowerShell)

**manage-frontend.ps1** (Already created!)
```powershell
.\manage-frontend.ps1 -Action install  # Install dependencies
.\manage-frontend.ps1 -Action dev      # Start dev server
.\manage-frontend.ps1 -Action build    # Build for production
.\manage-frontend.ps1 -Action preview  # Preview production build
```

---

### 4. Combined Workflow Script

Here's a more advanced example:

```powershell
# dev-workflow.ps1
param(
    [switch]$Backend,
    [switch]$Frontend,
    [switch]$FullStack
)

Write-Host "GM Buddy Development Workflow" -ForegroundColor Cyan

if ($FullStack -or $Backend) {
    Write-Host "`n?? Starting Docker containers..." -ForegroundColor Yellow
    docker-compose up -d
    Start-Sleep -Seconds 10
}

if ($FullStack -or $Frontend) {
    Write-Host "`n?? Starting Frontend dev server..." -ForegroundColor Yellow
    Set-Location "gm_buddy.client"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "npm run dev"
    Set-Location ..
}

Write-Host "`n? Development environment ready!" -ForegroundColor Green
```

**Usage:**
```powershell
.\dev-workflow.ps1 -FullStack   # Start everything
.\dev-workflow.ps1 -Backend     # Just Docker
.\dev-workflow.ps1 -Frontend    # Just frontend
```

---

## ?? Best Practices

### For Both

1. **Add comments** explaining what the script does
2. **Use meaningful names** (`start-dev.bat` not `s.bat`)
3. **Test incrementally** - don't write 100 lines without testing
4. **Keep it simple** - don't over-engineer
5. **Version control** - commit scripts to Git

### Batch Specific

```batch
@echo off                           REM Always start with this (hides commands)
setlocal                            REM Keep variables local to script
echo Starting process...            REM Tell user what's happening
if errorlevel 1 goto :error         REM Check for errors
goto :end                           REM Skip error handling if success

:error
echo An error occurred!
exit /b 1

:end
echo Success!
pause                               REM Keep window open
```

### PowerShell Specific

```powershell
# Always use explicit parameters
param(
    [Parameter(Mandatory=$true)]
    [string]$RequiredParam,
    
    [Parameter(Mandatory=$false)]
    [string]$OptionalParam = "default"
)

# Use try-catch for error handling
try {
    # Risky operation
    Get-Content "file.txt"
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

# Provide user feedback
Write-Host "Operation started..." -ForegroundColor Yellow
# ... do work ...
Write-Host "? Completed!" -ForegroundColor Green
```

---

## ?? Learning Resources

### Batch
- [SS64 Batch Reference](https://ss64.com/nt/)
- Simple and straightforward - learn by doing!

### PowerShell
- [Microsoft PowerShell Docs](https://docs.microsoft.com/en-us/powershell/)
- Built-in help: `Get-Help <command> -Examples`
- Practice: `Get-Command *service*` to explore commands

---

## ?? Quick Tips

### Make Scripts Run from Anywhere

**PowerShell Profile** (runs every time you open PowerShell):
```powershell
# Edit your profile
notepad $PROFILE

# Add aliases for your scripts
function GMBuddy-Up { & "C:\Path\To\GM_Buddy\manage-docker.ps1" -Action up }
function GMBuddy-Down { & "C:\Path\To\GM_Buddy\manage-docker.ps1" -Action down }

# Now you can just type:
GMBuddy-Up
```

### Keyboard Shortcuts

- **Ctrl+C**: Stop a running script
- **Up Arrow**: Recall previous command
- **Tab**: Auto-complete file/folder names

---

## ?? Your Scripts in GM Buddy

I've created these scripts for you:

1. **docker-down.bat** - Simple batch script to stop containers
2. **docker-up.bat** - Simple batch script to start containers
3. **manage-docker.ps1** - Advanced PowerShell Docker manager
4. **manage-frontend.ps1** - Frontend development manager

### Try Them Out!

```powershell
# Simple approach (Batch)
.\docker-up.bat
.\docker-down.bat

# Advanced approach (PowerShell)
.\manage-docker.ps1 -Action status
.\manage-docker.ps1 -Action up
.\manage-frontend.ps1 -Action dev
```

---

## ?? Next Steps

1. **Try the scripts**: Start with the simple batch files
2. **Modify them**: Change the echo messages, add your own commands
3. **Create new scripts**: Automate other tasks (database backups, testing, deployment)
4. **Learn more**: PowerShell is incredibly powerful - explore cmdlets!

**Remember**: Scripts are just text files with commands. Start simple, and gradually add complexity as you learn!

---

Happy scripting! ??
