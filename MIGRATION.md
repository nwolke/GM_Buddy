# Migration Guide: Old Configuration to New Configuration

This guide helps you migrate from the old configuration setup to the new streamlined configuration.

## What Changed?

### File Changes

#### Deleted Files
- ❌ Old root `.env.example` and `.env.local.example`
- ❌ Old `docker-compose.yml`, `docker-compose.override.yml`, `docker-compose.dcproj`
- ❌ Old `.dockerignore`
- ❌ `GM_Buddy.React/.env.development`, `.env.production`, `.env.local.example`
- ❌ `GM_Buddy.React/Dockerfile`, `nginx.conf`
- ❌ `GM_Buddy.Server/Dockerfile`, `Dockerfile.production`
- ❌ `GM_Buddy.Server/appsettings.Development.json` (had hardcoded values)
- ❌ `.github/workflows/dotnet.yml`
- ❌ Old `Dockerrun.aws.json`

#### New Files
- ✅ **Root**:
  - `.env.example` (comprehensive template)
  - `docker-compose.yml` (cleaner, production-ready)
  - `docker-compose.override.yml` (for developer customization)
  - `.dockerignore` (optimized)
  
- ✅ **Backend** (`GM_Buddy.Server/`):
  - `Dockerfile` (local development)
  - `Dockerfile.production` (AWS ECR optimized)
  - `appsettings.Development.json` (clean, no secrets)
  - `appsettings.Production.json` (new)
  - Updated `appsettings.json` (simplified)
  
- ✅ **Frontend** (`GM_Buddy.React/`):
  - `Dockerfile` (multi-stage with Nginx)
  - `nginx.conf` (production-ready)
  - `.env.development` (local defaults)
  - `.env.local.example` (developer overrides)
  - `.env.production` (template with placeholders)
  
- ✅ **GitHub Actions**:
  - `build-and-test.yml` (CI)
  - `deploy-backend.yml` (ECR + Elastic Beanstalk)
  - `deploy-frontend.yml` (S3 + CloudFront)
  
- ✅ **Documentation**:
  - `CONFIGURATION.md` (complete config guide)
  - `GITHUB_SECRETS.md` (required secrets)
  - `QUICKSTART.md` (fast setup)
  - `MIGRATION.md` (this file)

---

## Migration Steps

### For Local Development

#### 1. Update Your Environment File

If you had a working `.env` file:

```bash
# Backup your old .env
cp .env .env.backup

# Create new .env from template
cp .env.example .env
```

Then copy your values from `.env.backup` to the new `.env`:

**Database settings** (same structure):
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`

**Cognito settings** (same, but new keys added):
- `COGNITO_REGION`
- `COGNITO_USER_POOL_ID`
- `COGNITO_CLIENT_ID`
- `COGNITO_DOMAIN` (new)

**React settings** (now in root .env):
- `VITE_API_URL`
- `VITE_USE_COGNITO`
- `VITE_COGNITO_DOMAIN`
- `VITE_COGNITO_CLIENT_ID`
- `VITE_COGNITO_REDIRECT_URI`
- `VITE_COGNITO_LOGOUT_URI`

#### 2. Restart Docker Containers

```bash
# Stop old containers
docker compose down -v

# Start with new configuration
docker compose up --build
```

#### 3. Verify Everything Works

- Frontend: http://localhost:3000
- Backend: http://localhost:8080
- Database: localhost:15432

---

### For Production Deployment

#### Old Setup
You may have been deploying manually or using custom scripts.

#### New Setup
GitHub Actions handles everything automatically!

##### Required Actions:

1. **Add GitHub Secrets**
   
   Follow the [GITHUB_SECRETS.md](./GITHUB_SECRETS.md) guide to add all required secrets to your repository.

2. **Update AWS Resources**

   Make sure you have:
   - ECR repository (e.g., `gm-buddy-server`)
   - Elastic Beanstalk application and environment
   - S3 bucket for frontend
   - CloudFront distribution
   - Cognito user pool

3. **Configure Elastic Beanstalk Environment Variables**

   In AWS Console → Elastic Beanstalk → Your Environment → Configuration → Software:
   
   Add these environment properties:
   - `DbSettings__Host`
   - `DbSettings__Port`
   - `DbSettings__Database`
   - `DbSettings__Username`
   - `DbSettings__Password`
   - `Cognito__Region`
   - `Cognito__UserPoolId`
   - `Cognito__ClientId`
   - `Cognito__Domain`
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ASPNETCORE_HTTP_PORTS=8080`

4. **Deploy**

   **Automatic deployment** (on push to main):
   ```bash
   git push origin main
   ```

   **Manual deployment**:
   - Go to GitHub → Actions
   - Select "Deploy Backend to AWS" or "Deploy Frontend to AWS"
   - Click "Run workflow"

---

## Key Differences

### Environment Variables

#### Old Way
- Scattered across multiple `.env` files
- Some values hardcoded in `appsettings.Development.json`
- Inconsistent naming

#### New Way
- Single `.env.example` template for local dev
- All sensitive data in environment variables
- Consistent `Section__Property` format for backend
- `VITE_` prefix for frontend

### Docker Compose

#### Old Way
- Mixed development and production concerns
- Unclear which variables were required
- Hardcoded database host in `appsettings.json`

#### New Way
- Clear separation: `docker-compose.yml` for local dev
- Production uses AWS services (RDS, etc.)
- All configuration via environment variables
- Healthchecks for proper startup ordering

### Dockerfiles

#### Old Way
- Single Dockerfile for both dev and production
- Suboptimal layer caching
- Missing multi-stage builds

#### New Way
- Separate `Dockerfile` (dev) and `Dockerfile.production`
- Optimized multi-stage builds
- Better layer caching
- Smaller production images

### GitHub Actions

#### Old Way
- Single `dotnet.yml` for build/test only
- No automated deployment

#### New Way
- `build-and-test.yml` - CI for both backend and frontend
- `deploy-backend.yml` - Automated deployment to AWS EB
- `deploy-frontend.yml` - Automated deployment to S3/CloudFront
- Environment-based deployments (production/staging)

---

## Troubleshooting Migration

### Issue: Old containers won't stop

```bash
docker ps -a
docker stop $(docker ps -aq)
docker rm $(docker ps -aq)
```

### Issue: Old volumes interfering

```bash
docker volume ls
docker volume prune
```

### Issue: Port conflicts

The new setup uses the same ports. If you have old services running:

```bash
# Linux/Mac
lsof -i :3000
lsof -i :8080
lsof -i :15432

# Windows
netstat -ano | findstr :3000
netstat -ano | findstr :8080
netstat -ano | findstr :15432
```

### Issue: Missing environment variables

Check the `.env.example` file for all required variables. Every variable there should be in your `.env`.

---

## Rollback Plan

If you need to rollback to the old configuration:

```bash
# Checkout the commit before the migration
git checkout <commit-hash-before-migration>

# Or create a branch from it
git checkout -b old-config <commit-hash-before-migration>
```

---

## Questions?

- Check [CONFIGURATION.md](./CONFIGURATION.md) for detailed explanations
- Check [QUICKSTART.md](./QUICKSTART.md) for setup help
- Open a GitHub issue if you encounter problems

---

## Benefits of the New Setup

✅ **Clearer separation** between local dev and production  
✅ **Better security** - no hardcoded credentials  
✅ **Automated deployments** via GitHub Actions  
✅ **Easier onboarding** - single `.env` file to configure  
✅ **Production-optimized** Docker builds  
✅ **Comprehensive documentation**  
✅ **Consistent patterns** across frontend and backend  
