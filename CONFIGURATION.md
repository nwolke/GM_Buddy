# GM_Buddy Configuration Guide

This guide explains the configuration and deployment setup for GM_Buddy.

## Table of Contents

1. [Overview](#overview)
2. [Local Development](#local-development)
3. [Production Deployment](#production-deployment)
4. [GitHub Actions CI/CD](#github-actions-cicd)
5. [Configuration Files Reference](#configuration-files-reference)

---

## Overview

GM_Buddy is a full-stack application consisting of:

- **Frontend**: React + TypeScript + Vite (deployed to AWS S3 + CloudFront)
- **Backend**: ASP.NET Core 9 (deployed to AWS Elastic Beanstalk via Docker/ECR)
- **Database**: PostgreSQL
- **Authentication**: AWS Cognito

---

## Local Development

### Prerequisites

- Docker and Docker Compose
- .NET 9 SDK (optional, for running outside Docker)
- Node.js 20+ (optional, for running outside Docker)

### Quick Start

1. **Copy the environment file**:
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env`** with your local settings:
   - Set `POSTGRES_PASSWORD` to a secure password
   - Set `USE_COGNITO=false` for local development without AWS Cognito
   - Update other values as needed

3. **Start the application**:
   ```bash
   docker-compose up --build
   ```

4. **Access the application**:
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:8080
   - pgAdmin: http://localhost:15435
   - PostgreSQL: localhost:15432

### Services

The `docker-compose.yml` defines the following services:

- **gm_buddy_postgres**: PostgreSQL database
- **gm_buddy_pgadmin**: pgAdmin database management tool
- **gm_buddy_server**: ASP.NET Core backend API
- **gm_buddy_web**: React frontend served by Nginx

### Development Without Docker

#### Backend

```bash
cd GM_Buddy.Server
dotnet run
```

Ensure you have a PostgreSQL instance running and update `appsettings.Development.json` or set environment variables.

#### Frontend

```bash
cd GM_Buddy.React
npm install --legacy-peer-deps
npm run dev
```

The frontend will be available at http://localhost:3000 with hot module replacement.

---

## Production Deployment

### Architecture

```
┌─────────────┐
│   Users     │
└──────┬──────┘
       │
       ▼
┌─────────────────────┐
│   CloudFront CDN    │  (Frontend Distribution)
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│     S3 Bucket       │  (Static React Build)
└─────────────────────┘

       │
       ▼ API Calls
┌─────────────────────┐
│ Elastic Beanstalk   │  (Backend Server)
│   + Docker (ECR)    │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│   RDS PostgreSQL    │  (Database)
└─────────────────────┘

       │
       ▼ Authentication
┌─────────────────────┐
│   AWS Cognito       │  (User Management)
└─────────────────────┘
```

### AWS Resources Required

1. **IAM Deployment User**: User with permissions for ECR, EB, S3, CloudFront (see [AWS_IAM_SETUP.md](./AWS_IAM_SETUP.md))
2. **ECR Repository**: For Docker images (`gm-buddy-server`)
3. **Elastic Beanstalk**: For running the backend
4. **S3 Bucket**: For hosting the frontend build
5. **CloudFront Distribution**: For serving the frontend globally
6. **Cognito User Pool**: For authentication
7. **RDS PostgreSQL**: For the database (or use external PostgreSQL)

### Manual Deployment

#### Backend

1. Build the Docker image:
   ```bash
   docker build -t gm-buddy-server:latest -f GM_Buddy.Server/Dockerfile.production .
   ```

2. Tag and push to ECR:
   ```bash
   aws ecr get-login-password --region us-west-2 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-west-2.amazonaws.com
   docker tag gm-buddy-server:latest <account-id>.dkr.ecr.us-west-2.amazonaws.com/gm-buddy-server:latest
   docker push <account-id>.dkr.ecr.us-west-2.amazonaws.com/gm-buddy-server:latest
   ```

3. Update `Dockerrun.aws.json` with your ECR image URI

4. Deploy to Elastic Beanstalk using EB CLI or AWS Console

#### Frontend

1. Build the React application:
   ```bash
   cd GM_Buddy.React
   npm ci --legacy-peer-deps
   npm run build
   ```

2. Upload to S3:
   ```bash
   aws s3 sync dist/ s3://your-bucket-name --delete
   ```

3. Invalidate CloudFront cache:
   ```bash
   aws cloudfront create-invalidation --distribution-id YOUR_DIST_ID --paths "/*"
   ```

---

## GitHub Actions CI/CD

The repository includes three GitHub Actions workflows:

### 1. Build and Test (`build-and-test.yml`)

**Triggers**: Push/PR to `main` or `develop`

**Purpose**: Runs tests for both backend and frontend

**Jobs**:
- Test .NET backend
- Test React frontend (typecheck, tests, build)

### 2. Deploy Backend (`deploy-backend.yml`)

**Triggers**: 
- **Manual only** - via workflow_dispatch in GitHub Actions UI

**Purpose**: Builds and deploys the backend to AWS

**Steps**:
1. Build Docker image
2. Push to ECR
3. Update Dockerrun.aws.json
4. Deploy to Elastic Beanstalk

**How to Deploy**: See [MANUAL_DEPLOYMENT.md](./MANUAL_DEPLOYMENT.md)

**Required Secrets**: See [GITHUB_SECRETS.md](./GITHUB_SECRETS.md)

### 3. Deploy Frontend (`deploy-frontend.yml`)

**Triggers**: 
- **Manual only** - via workflow_dispatch in GitHub Actions UI

**Purpose**: Builds and deploys the frontend to AWS

**Steps**:
1. Build React application with production env vars
2. Upload to S3
3. Invalidate CloudFront cache

**How to Deploy**: See [MANUAL_DEPLOYMENT.md](./MANUAL_DEPLOYMENT.md)

**Required Secrets**: See [GITHUB_SECRETS.md](./GITHUB_SECRETS.md)

### Setting Up GitHub Secrets

See the complete guide in [GITHUB_SECRETS.md](./GITHUB_SECRETS.md) for all required secrets.

---

## Configuration Files Reference

### Root Directory

- **`.env.example`**: Template for local environment variables
- **`.dockerignore`**: Files to exclude from Docker builds
- **`docker-compose.yml`**: Local development orchestration
- **`docker-compose.override.yml`**: Optional developer customizations
- **`Dockerrun.aws.json`**: Elastic Beanstalk Docker configuration

### Backend (GM_Buddy.Server)

- **`Dockerfile`**: Local development Docker build
- **`Dockerfile.production`**: Production optimized Docker build for AWS
- **`appsettings.json`**: Base application configuration
- **`appsettings.Development.json`**: Development-specific settings
- **`appsettings.Production.json`**: Production-specific settings

### Frontend (GM_Buddy.React)

- **`Dockerfile`**: Docker build for React application
- **`nginx.conf`**: Nginx configuration for serving the React app
- **`.env.development`**: Development environment variables (tracked)
- **`.env.local.example`**: Template for local overrides
- **`.env.production`**: Production environment variables (tracked, with placeholders)

### GitHub Actions

- **`.github/workflows/build-and-test.yml`**: CI pipeline
- **`.github/workflows/deploy-backend.yml`**: Backend deployment pipeline
- **`.github/workflows/deploy-frontend.yml`**: Frontend deployment pipeline

---

## Environment Variables

### Backend (ASP.NET Core)

Environment variables override `appsettings.json` using the format `Section__Property`.

**Database**:
- `DbSettings__Host`
- `DbSettings__Port`
- `DbSettings__Database`
- `DbSettings__Username`
- `DbSettings__Password`

**Cognito**:
- `Cognito__Region`
- `Cognito__UserPoolId`
- `Cognito__ClientId`
- `Cognito__Domain`

**ASP.NET Core**:
- `ASPNETCORE_ENVIRONMENT` (Development/Production)
- `ASPNETCORE_HTTP_PORTS` (e.g., 8080)

### Frontend (React/Vite)

Vite requires environment variables to be prefixed with `VITE_`.

- `VITE_API_URL`: Backend API URL
- `VITE_USE_COGNITO`: Enable/disable Cognito (`true`/`false`)
- `VITE_COGNITO_DOMAIN`: Cognito domain
- `VITE_COGNITO_CLIENT_ID`: Cognito client ID
- `VITE_COGNITO_REDIRECT_URI`: Redirect after login
- `VITE_COGNITO_LOGOUT_URI`: Redirect after logout

---

## Troubleshooting

### Local Development

**Issue**: PostgreSQL connection fails
- Verify `POSTGRES_PASSWORD` is set in `.env`
- Check if port 15432 is available
- Ensure Docker containers are running: `docker-compose ps`

**Issue**: Frontend can't connect to backend
- Verify `VITE_API_URL` in `.env` or `.env.local`
- Check backend is running: `curl http://localhost:8080`

### Production Deployment

**Issue**: Elastic Beanstalk deployment fails
- Check ECR image URI in `Dockerrun.aws.json`
- Verify all EB environment variables are set
- Review EB logs in AWS Console

**Issue**: CloudFront shows old version
- Ensure CloudFront invalidation ran successfully
- Check S3 bucket contents
- Verify cache settings in CloudFront distribution

---

## Additional Resources

- [AWS Elastic Beanstalk Documentation](https://docs.aws.amazon.com/elasticbeanstalk/)
- [AWS ECR Documentation](https://docs.aws.amazon.com/ecr/)
- [AWS CloudFront Documentation](https://docs.aws.amazon.com/cloudfront/)
- [AWS Cognito Documentation](https://docs.aws.amazon.com/cognito/)
- [Vite Environment Variables](https://vitejs.dev/guide/env-and-mode.html)
- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

---

## Support

For issues or questions, please open a GitHub issue in the repository.
