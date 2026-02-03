# Quick Start Guide

This guide will help you get GM_Buddy running locally in under 5 minutes.

## Prerequisites

- Docker and Docker Compose installed
- Git

## Steps

### 1. Clone the Repository

```bash
git clone https://github.com/nwolke/GM_Buddy.git
cd GM_Buddy
```

### 2. Create Local Environment File

```bash
cp .env.example .env
```

### 3. Edit the `.env` File

Open `.env` in your editor and set a password:

```bash
# Required: Set a secure password
POSTGRES_PASSWORD=YourSecurePassword123!

# Optional: For local dev, you can disable Cognito
VITE_USE_COGNITO=false
```

All other values have sensible defaults for local development.

### 4. Start the Application

```bash
docker compose up --build
```

Wait for all services to start (about 2-3 minutes on first run).

### 5. Access the Application

Open your browser:

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8080
- **pgAdmin**: http://localhost:15435 (optional database management)
  - Email: `dev@example.com` (from .env)
  - Password: `admin123` (from .env)

### 6. Stop the Application

Press `Ctrl+C` in the terminal, then:

```bash
docker compose down
```

To remove all data and start fresh:

```bash
docker compose down -v
```

---

## Development Workflow

### Backend Changes

The backend is rebuilt when you run `docker compose up --build`. For faster iteration:

```bash
cd GM_Buddy.Server
dotnet watch run
```

### Frontend Changes

For hot reload during development:

```bash
cd GM_Buddy.React
npm install --legacy-peer-deps
npm run dev
```

The frontend will be available at http://localhost:3000 with instant updates.

---

## Common Issues

### Port Already in Use

If you see errors about ports being in use:

```bash
# Check what's using the ports
lsof -i :3000  # Frontend
lsof -i :8080  # Backend
lsof -i :15432 # PostgreSQL

# Kill the process or change ports in .env
```

### PostgreSQL Won't Start

```bash
# Remove old data and restart
docker compose down -v
docker compose up --build
```

### Can't Connect to Backend

1. Check backend is running: `curl http://localhost:8080`
2. Check `VITE_API_URL` in `GM_Buddy.React/.env.local` or `.env.development`
3. Restart the services

---

## Next Steps

- Review [CONFIGURATION.md](./CONFIGURATION.md) for detailed configuration options
- Review [GITHUB_SECRETS.md](./GITHUB_SECRETS.md) for production deployment setup
- Explore the codebase!

---

## Need Help?

Open an issue on GitHub with:
- Your operating system
- Docker version (`docker --version`)
- Complete error message
- Steps you've tried
