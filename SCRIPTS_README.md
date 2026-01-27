# GM Buddy - Automation Scripts

This directory contains automation scripts to simplify common development tasks.

## ?? Available Scripts

### Batch Scripts (.bat) - Simple & Quick

| Script | Description | Usage |
|--------|-------------|-------|
| `docker-up.bat` | Start all Docker containers | Double-click or `.\docker-up.bat` |
| `docker-down.bat` | Stop and remove all containers | Double-click or `.\docker-down.bat` |

### PowerShell Scripts (.ps1) - Advanced & Flexible

| Script | Description | Usage |
|--------|-------------|-------|
| `manage-docker.ps1` | Full Docker management | See below |
| `manage-frontend.ps1` | Frontend development tasks | See below |

---

## ?? Quick Start

### Using Batch Scripts (Easiest)

**Start GM Buddy:**
```cmd
docker-up.bat
```

**Stop GM Buddy:**
```cmd
docker-down.bat
```

### Using PowerShell Scripts (More Control)

**First time setup** (run PowerShell as Administrator):
```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## ?? PowerShell Script Usage

### manage-docker.ps1

**Check container status:**
```powershell
.\manage-docker.ps1
# or
.\manage-docker.ps1 -Action status
```

**Start containers:**
```powershell
.\manage-docker.ps1 -Action up
```

**Stop containers:**
```powershell
.\manage-docker.ps1 -Action down
```

**Restart containers:**
```powershell
.\manage-docker.ps1 -Action restart
```

**View logs:**
```powershell
.\manage-docker.ps1 -Action logs
```

---

### manage-frontend.ps1

**Install dependencies:**
```powershell
.\manage-frontend.ps1 -Action install
```

**Start development server:**
```powershell
.\manage-frontend.ps1 -Action dev
# or just
.\manage-frontend.ps1
```

**Build for production:**
```powershell
.\manage-frontend.ps1 -Action build
```

**Preview production build:**
```powershell
.\manage-frontend.ps1 -Action preview
```

---

## ?? Common Workflows

### Full Development Setup
```powershell
# 1. Start backend services
.\manage-docker.ps1 -Action up

# 2. Start frontend (in a new terminal)
.\manage-frontend.ps1 -Action dev
```

### Quick Restart
```powershell
.\manage-docker.ps1 -Action restart
```

### End of Day Shutdown
```powershell
.\manage-docker.ps1 -Action down
```

---

## ?? Troubleshooting

### PowerShell: "cannot be loaded because running scripts is disabled"

**Solution:**
```powershell
# Run PowerShell as Administrator
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### docker-compose command not found

**Solution:**
Make sure Docker Desktop is running and installed.

### Containers won't stop

**Solution:**
```powershell
# Use the force!
docker stop $(docker ps -q) --force
docker rm $(docker ps -aq) --force
```

---

## ?? Learn More

See [SCRIPTING_GUIDE.md](docs/SCRIPTING_GUIDE.md) for a comprehensive guide to Batch and PowerShell scripting.

---

## ?? Contributing

Feel free to modify these scripts or create new ones! Common additions:
- Database backup/restore scripts
- Test runners
- Deployment scripts
- Code generation scripts

Just commit them to the repository so everyone can benefit!
