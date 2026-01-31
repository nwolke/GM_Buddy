# AWS Deployment Plan with GitHub Actions

## Overview

This document outlines the plan for deploying GM Buddy to AWS infrastructure using GitHub Actions. The deployment consists of three main components:

1. **Database (PostgreSQL on RDS)** - Database migration files
2. **Backend Server (App Runner)** - ASP.NET Core application
3. **Frontend (S3 + CloudFront)** - React/Vite application

## Prerequisites

### AWS Resources Required

1. **RDS PostgreSQL Instance**
   - PostgreSQL 15+ database
   - Security group allowing access from App Runner and migration runner
   - Database name, username, password stored in AWS Secrets Manager or GitHub Secrets

2. **ECR (Elastic Container Registry)**
   - Repository for server Docker images: `gm-buddy-server`
   - Existing repository referenced: `107912522122.dkr.ecr.us-west-2.amazonaws.com/gm-buddy-server`

3. **App Runner Service**
   - Service configured to pull from ECR
   - Environment variables configured (Cognito, Database connection string)
   - Auto-deploy enabled or manual deployment

4. **S3 Bucket**
   - Bucket for static website hosting (e.g., `gm-buddy-web`)
   - Configured for static website hosting
   - Public read access or CloudFront origin

5. **CloudFront Distribution**
   - Distribution pointing to S3 bucket
   - SSL certificate configured
   - Custom domain (optional)

6. **AWS Cognito**
   - User Pool configured
   - App Client configured
   - Domain configured

### GitHub Secrets Required

The following secrets need to be configured in GitHub repository settings:

```
AWS_REGION                    # e.g., us-west-2
AWS_ACCOUNT_ID                # e.g., 107912522122
AWS_ACCESS_KEY_ID             # IAM user with deployment permissions
AWS_SECRET_ACCESS_KEY         # IAM user secret key

# RDS Database
DB_HOST                       # RDS endpoint
DB_PORT                       # Usually 5432
DB_NAME                       # Database name
DB_USER                       # Master username
DB_PASSWORD                   # Master password

# App Runner
APP_RUNNER_SERVICE_ARN        # Full ARN of App Runner service

# S3 & CloudFront
S3_BUCKET_NAME                # S3 bucket for React app
CLOUDFRONT_DISTRIBUTION_ID    # CloudFront distribution ID

# Cognito Configuration (for React build)
VITE_COGNITO_DOMAIN
VITE_COGNITO_CLIENT_ID
VITE_COGNITO_REDIRECT_URI
VITE_COGNITO_LOGOUT_URI
VITE_USE_COGNITO              # "true" or "false"
```

### IAM Permissions Required

The IAM user/role used by GitHub Actions needs:

- **ECR**: `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:GetDownloadUrlForLayer`, `ecr:BatchGetImage`, `ecr:PutImage`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`
- **App Runner**: `apprunner:StartDeployment`, `apprunner:DescribeService`
- **S3**: `s3:PutObject`, `s3:PutObjectAcl`, `s3:DeleteObject`, `s3:ListBucket`
- **CloudFront**: `cloudfront:CreateInvalidation`
- **Secrets Manager** (optional): `secretsmanager:GetSecretValue`

---

## Component 1: Database Migration to RDS

### Overview

Deploy `init.sql` and future migration files to the RDS PostgreSQL instance.

### Strategy

- **Initial Setup**: Run `init.sql` on new RDS instances
- **Future Migrations**: Version-controlled SQL files in a `migrations/` directory
- **Idempotent Scripts**: All scripts use `CREATE TABLE IF NOT EXISTS`, `ON CONFLICT DO NOTHING`, etc.
- **Migration Tool**: Use psql or a migration runner container

### Workflow: `database-migration.yml`

**Trigger**: 
- Manual workflow dispatch (for safety)
- Optionally on push to main (if migrations directory changes)

**Steps**:

1. **Checkout code**
   ```yaml
   - uses: actions/checkout@v4
   ```

2. **Setup PostgreSQL client** (psql)
   ```yaml
   - name: Install PostgreSQL client
     run: |
       sudo apt-get update
       sudo apt-get install -y postgresql-client
   ```

3. **Run init.sql**
   ```yaml
   - name: Run database initialization
     env:
       PGHOST: ${{ secrets.DB_HOST }}
       PGPORT: ${{ secrets.DB_PORT }}
       PGDATABASE: ${{ secrets.DB_NAME }}
       PGUSER: ${{ secrets.DB_USER }}
       PGPASSWORD: ${{ secrets.DB_PASSWORD }}
     run: |
       psql -f init.sql
   ```

4. **Run additional migrations** (if migrations directory exists)
   ```yaml
   - name: Run additional migrations
     env:
       PGHOST: ${{ secrets.DB_HOST }}
       PGPORT: ${{ secrets.DB_PORT }}
       PGDATABASE: ${{ secrets.DB_NAME }}
       PGUSER: ${{ secrets.DB_USER }}
       PGPASSWORD: ${{ secrets.DB_PASSWORD }}
     run: |
       if [ -d "migrations" ]; then
         for migration in migrations/*.sql; do
           echo "Running migration: $migration"
           psql -f "$migration"
         done
       fi
   ```

### Future Migration Management

**Option A**: Simple numbered migrations
```
migrations/
  001_add_session_table.sql
  002_add_inventory_table.sql
```

**Option B**: Use a migration tool like Flyway or Liquibase
- More robust version tracking
- Automatic rollback support
- Migration history table

---

## Component 2: Server Deployment to App Runner

### Overview

Build the ASP.NET Core application as a Docker image, push to ECR, and deploy to App Runner.

### Current Configuration

- Dockerfile: `GM_Buddy.Server/Dockerfile.production`
- Base image: `mcr.microsoft.com/dotnet/sdk:9.0` (build), `mcr.microsoft.com/dotnet/aspnet:9.0` (runtime)
- Port: 8080
- Multi-stage build: Yes

### Workflow: `deploy-server.yml`

**Trigger**:
- Push to `main` branch (paths: `GM_Buddy.Server/**`, `GM_Buddy.Business/**`, `GM_Buddy.Data/**`, `GM_Buddy.Contracts/**`)
- Manual workflow dispatch

**Steps**:

1. **Checkout code**
   ```yaml
   - uses: actions/checkout@v4
   ```

2. **Configure AWS credentials**
   ```yaml
   - name: Configure AWS credentials
     uses: aws-actions/configure-aws-credentials@v4
     with:
       aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
       aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
       aws-region: ${{ secrets.AWS_REGION }}
   ```

3. **Login to Amazon ECR**
   ```yaml
   - name: Login to Amazon ECR
     id: login-ecr
     uses: aws-actions/amazon-ecr-login@v2
   ```

4. **Build, tag, and push Docker image**
   ```yaml
   - name: Build, tag, and push image to Amazon ECR
     env:
       ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
       ECR_REPOSITORY: gm-buddy-server
       IMAGE_TAG: ${{ github.sha }}
     run: |
       docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG \
         -f GM_Buddy.Server/Dockerfile.production .
       docker tag $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG \
         $ECR_REGISTRY/$ECR_REPOSITORY:latest
       docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
       docker push $ECR_REGISTRY/$ECR_REPOSITORY:latest
   ```

5. **Deploy to App Runner**
   ```yaml
   - name: Deploy to App Runner
     run: |
       aws apprunner start-deployment \
         --service-arn ${{ secrets.APP_RUNNER_SERVICE_ARN }}
   ```

6. **Wait for deployment** (optional)
   ```yaml
   - name: Wait for deployment
     run: |
       aws apprunner wait service-running \
         --service-arn ${{ secrets.APP_RUNNER_SERVICE_ARN }}
   ```

### App Runner Configuration Requirements

The App Runner service should be configured with:

**Environment Variables**:
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `Cognito__*`: AWS Cognito settings
- `ASPNETCORE_ENVIRONMENT`: Production
- Any other application-specific settings from `appsettings.json`

**Health Check**:
- Path: `/health` or `/api/health` (if implemented)
- Interval: 30 seconds
- Timeout: 5 seconds

**Auto Scaling**:
- Min instances: 1
- Max instances: 3-5 (adjust based on load)

---

## Component 3: Frontend Deployment to S3 + CloudFront

### Overview

Build the React/Vite application with production Cognito configuration and deploy to S3 bucket, then invalidate CloudFront cache.

### Current Configuration

- Dockerfile: `GM_Buddy.React/Dockerfile`
- Build tool: Vite
- Build arguments: Cognito configuration (VITE_COGNITO_*)
- Output directory: `dist/`
- Production server: Nginx (in container), but for S3 we use static files only

### Workflow: `deploy-frontend.yml`

**Trigger**:
- Push to `main` branch (paths: `GM_Buddy.React/**`)
- Manual workflow dispatch

**Steps**:

1. **Checkout code**
   ```yaml
   - uses: actions/checkout@v4
   ```

2. **Setup Node.js**
   ```yaml
   - name: Setup Node.js
     uses: actions/setup-node@v4
     with:
       node-version: '20'
       cache: 'npm'
       cache-dependency-path: GM_Buddy.React/package-lock.json
   ```

3. **Install dependencies**
   ```yaml
   - name: Install dependencies
     working-directory: GM_Buddy.React
     run: npm ci --legacy-peer-deps
   ```

4. **Build React app with production Cognito config**
   ```yaml
   - name: Build React app
     working-directory: GM_Buddy.React
     env:
       VITE_COGNITO_DOMAIN: ${{ secrets.VITE_COGNITO_DOMAIN }}
       VITE_COGNITO_CLIENT_ID: ${{ secrets.VITE_COGNITO_CLIENT_ID }}
       VITE_COGNITO_REDIRECT_URI: ${{ secrets.VITE_COGNITO_REDIRECT_URI }}
       VITE_COGNITO_LOGOUT_URI: ${{ secrets.VITE_COGNITO_LOGOUT_URI }}
       VITE_USE_COGNITO: ${{ secrets.VITE_USE_COGNITO }}
     run: npm run build
   ```

5. **Configure AWS credentials**
   ```yaml
   - name: Configure AWS credentials
     uses: aws-actions/configure-aws-credentials@v4
     with:
       aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
       aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
       aws-region: ${{ secrets.AWS_REGION }}
   ```

6. **Deploy to S3**
   ```yaml
   - name: Deploy to S3
     run: |
       aws s3 sync GM_Buddy.React/dist/ s3://${{ secrets.S3_BUCKET_NAME }}/ \
         --delete \
         --cache-control "public, max-age=31536000, immutable" \
         --exclude "*.html" \
         --exclude "*.json"
       
       # Upload HTML and JSON files with shorter cache
       aws s3 sync GM_Buddy.React/dist/ s3://${{ secrets.S3_BUCKET_NAME }}/ \
         --exclude "*" \
         --include "*.html" \
         --include "*.json" \
         --cache-control "public, max-age=0, must-revalidate"
   ```

7. **Invalidate CloudFront cache**
   ```yaml
   - name: Invalidate CloudFront cache
     run: |
       aws cloudfront create-invalidation \
         --distribution-id ${{ secrets.CLOUDFRONT_DISTRIBUTION_ID }} \
         --paths "/*"
   ```

### S3 Bucket Configuration Requirements

**Bucket Policy** (if not using CloudFront):
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::gm-buddy-web/*"
    }
  ]
}
```

**Static Website Hosting**:
- Index document: `index.html`
- Error document: `index.html` (for SPA routing)

### CloudFront Configuration Requirements

**Origin**:
- Origin domain: S3 bucket website endpoint or S3 bucket with OAI
- Origin protocol: HTTPS only

**Default Cache Behavior**:
- Viewer protocol policy: Redirect HTTP to HTTPS
- Allowed HTTP methods: GET, HEAD, OPTIONS
- Cache policy: Optimize for SPA (short cache for HTML, long cache for assets)
- Response headers policy: CORS headers if needed

**Custom Error Responses**:
- 403 → 200 /index.html (for SPA routing)
- 404 → 200 /index.html (for SPA routing)

**SSL Certificate**:
- Use ACM certificate for custom domain
- Or use CloudFront default certificate

---

## Component 4: CloudFront Configuration Changes

### Potential Changes Needed

1. **Cache Invalidation Strategy**
   - Automatic invalidation on deployment (included in workflow above)
   - Targeted invalidation for specific paths if needed
   - Consider using versioned URLs for assets to avoid invalidation costs

2. **CORS Configuration**
   - Ensure CloudFront forwards appropriate CORS headers
   - Configure response headers policy if API calls from frontend

3. **Security Headers**
   - Add security headers via CloudFront Functions or Lambda@Edge:
     - `Strict-Transport-Security`
     - `X-Content-Type-Options`
     - `X-Frame-Options`
     - `X-XSS-Protection`
     - `Content-Security-Policy`

4. **API Gateway Integration** (if applicable)
   - If using API Gateway for backend, configure behavior for `/api/*`
   - Route API requests to App Runner or API Gateway
   - Configure appropriate cache settings (likely no cache for API)

5. **Custom Domain and SSL**
   - Configure custom domain (e.g., gmbuddy.com)
   - Route 53 DNS configuration
   - ACM certificate validation

6. **Geo-Restriction** (optional)
   - Configure geo-restriction if needed for compliance

### CloudFront Function Example (Security Headers)

```javascript
function handler(event) {
    var response = event.response;
    var headers = response.headers;

    headers['strict-transport-security'] = { value: 'max-age=63072000; includeSubdomains; preload'};
    headers['x-content-type-options'] = { value: 'nosniff'};
    headers['x-frame-options'] = { value: 'DENY'};
    headers['x-xss-protection'] = { value: '1; mode=block'};
    headers['referrer-policy'] = { value: 'same-origin'};

    return response;
}
```

---

## Deployment Workflow Summary

### Complete Deployment Order

For a fresh deployment or major release:

1. **Database Migration** (`database-migration.yml`)
   - Run manually or on schema changes
   - Validates migration success before proceeding

2. **Server Deployment** (`deploy-server.yml`)
   - Builds and pushes Docker image to ECR
   - Triggers App Runner deployment
   - Waits for health checks to pass

3. **Frontend Deployment** (`deploy-frontend.yml`)
   - Builds React app with production config
   - Deploys to S3
   - Invalidates CloudFront cache

### Environment Promotion Strategy

**Option A**: Branch-based deployment
- `dev` branch → Dev environment
- `staging` branch → Staging environment
- `main` branch → Production environment

**Option B**: Tag-based deployment
- Tag format: `v1.0.0`, `v1.0.0-rc1`
- Production deploys only on version tags
- Pre-release tags deploy to staging

**Option C**: Manual approval
- Workflows require manual approval for production
- Use GitHub Environments with protection rules

---

## Monitoring and Rollback

### Monitoring

1. **App Runner**
   - CloudWatch Logs for application logs
   - CloudWatch Metrics for performance
   - Health check endpoint monitoring

2. **CloudFront**
   - CloudWatch Metrics for request rate, error rate
   - Access logs to S3 (if enabled)

3. **RDS**
   - CloudWatch Metrics for database performance
   - Enhanced Monitoring (if enabled)
   - Slow query logs

### Rollback Strategy

1. **Server Rollback**
   - Re-deploy previous Docker image tag from ECR
   - App Runner keeps previous version available
   - Manual rollback via AWS Console or CLI

2. **Frontend Rollback**
   - Re-run workflow with previous commit SHA
   - Or manually sync previous dist/ to S3

3. **Database Rollback**
   - **No automatic rollback** for migrations
   - Use RDS snapshots for disaster recovery
   - Write down migrations for manual rollback

---

## Testing Strategy

### Pre-Deployment Testing

1. **Local Testing**
   - Use docker-compose for full stack testing
   - Test with production-like environment variables

2. **CI Testing**
   - Existing `.NET` workflow validates server build and tests
   - Add React build validation to CI

3. **Staging Environment**
   - Deploy to staging before production
   - Run smoke tests on staging

### Post-Deployment Validation

1. **Server Health Check**
   - Verify App Runner service is running
   - Test API endpoints

2. **Frontend Validation**
   - Verify CloudFront serves updated content
   - Test authentication flow
   - Test API calls from frontend

3. **Database Validation**
   - Verify migrations applied successfully
   - Check table schemas and seed data

---

## Security Considerations

1. **Secrets Management**
   - Use GitHub Secrets for sensitive values
   - Consider AWS Secrets Manager for application secrets
   - Rotate credentials regularly

2. **Least Privilege**
   - IAM roles with minimum required permissions
   - Separate IAM users for different environments

3. **Network Security**
   - RDS in private subnet (VPC)
   - App Runner with VPC connector for RDS access
   - Security groups with minimal open ports

4. **HTTPS Only**
   - CloudFront enforces HTTPS
   - App Runner supports HTTPS by default
   - RDS connection encrypted in transit

5. **Dependency Scanning**
   - Enable Dependabot for dependency updates
   - Scan Docker images for vulnerabilities
   - Regular security audits

---

## Cost Optimization

1. **App Runner**
   - Right-size instance configuration
   - Use auto-scaling to reduce idle instances
   - Consider provisioned concurrency vs auto-scaling

2. **RDS**
   - Use appropriate instance size
   - Enable auto-scaling for storage
   - Use RDS snapshots instead of continuous backups for dev

3. **S3 and CloudFront**
   - Use S3 lifecycle policies for old versions
   - Optimize CloudFront cache hit ratio
   - Consider CloudFront price class based on geographic distribution

4. **ECR**
   - Implement lifecycle policy to remove old images
   - Keep last N images or images from last X days

---

## Next Steps

1. **Create GitHub Secrets**
   - Add all required secrets to repository settings
   - Validate secret values with AWS

2. **Create Workflow Files**
   - Implement `database-migration.yml`
   - Implement `deploy-server.yml`
   - Implement `deploy-frontend.yml`

3. **Test Workflows**
   - Test each workflow individually
   - Test complete deployment flow
   - Verify rollback procedures

4. **Documentation**
   - Update README with deployment instructions
   - Document environment setup
   - Create runbook for common issues

5. **Setup Monitoring**
   - Configure CloudWatch alarms
   - Setup log aggregation
   - Create dashboard for key metrics

---

## Appendix: Example Workflow Files

### A.1 Database Migration Workflow

```yaml
name: Database Migration

on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment to deploy to'
        required: true
        type: choice
        options:
          - development
          - staging
          - production

jobs:
  migrate:
    runs-on: ubuntu-latest
    environment: ${{ github.event.inputs.environment }}
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Install PostgreSQL client
        run: |
          sudo apt-get update
          sudo apt-get install -y postgresql-client
      
      - name: Run init.sql
        env:
          PGHOST: ${{ secrets.DB_HOST }}
          PGPORT: ${{ secrets.DB_PORT }}
          PGDATABASE: ${{ secrets.DB_NAME }}
          PGUSER: ${{ secrets.DB_USER }}
          PGPASSWORD: ${{ secrets.DB_PASSWORD }}
        run: |
          echo "Running init.sql..."
          psql -f init.sql -v ON_ERROR_STOP=1
          echo "Migration completed successfully"
      
      - name: Run additional migrations
        env:
          PGHOST: ${{ secrets.DB_HOST }}
          PGPORT: ${{ secrets.DB_PORT }}
          PGDATABASE: ${{ secrets.DB_NAME }}
          PGUSER: ${{ secrets.DB_USER }}
          PGPASSWORD: ${{ secrets.DB_PASSWORD }}
        run: |
          if [ -d "migrations" ]; then
            for migration in migrations/*.sql; do
              echo "Running migration: $migration"
              psql -f "$migration" -v ON_ERROR_STOP=1
            done
            echo "All migrations completed successfully"
          else
            echo "No additional migrations found"
          fi
```

### A.2 Server Deployment Workflow

```yaml
name: Deploy Server to App Runner

on:
  push:
    branches:
      - main
    paths:
      - 'GM_Buddy.Server/**'
      - 'GM_Buddy.Business/**'
      - 'GM_Buddy.Data/**'
      - 'GM_Buddy.Contracts/**'
      - '.github/workflows/deploy-server.yml'
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ secrets.AWS_REGION }}
      
      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v2
      
      - name: Build, tag, and push image to Amazon ECR
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: gm-buddy-server
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG \
            -f GM_Buddy.Server/Dockerfile.production .
          docker tag $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG \
            $ECR_REGISTRY/$ECR_REPOSITORY:latest
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:latest
          echo "Image pushed: $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG"
      
      - name: Deploy to App Runner
        run: |
          echo "Starting App Runner deployment..."
          aws apprunner start-deployment \
            --service-arn ${{ secrets.APP_RUNNER_SERVICE_ARN }}
          echo "Deployment triggered successfully"
```

### A.3 Frontend Deployment Workflow

```yaml
name: Deploy Frontend to S3 + CloudFront

on:
  push:
    branches:
      - main
    paths:
      - 'GM_Buddy.React/**'
      - '.github/workflows/deploy-frontend.yml'
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: GM_Buddy.React/package-lock.json
      
      - name: Install dependencies
        working-directory: GM_Buddy.React
        run: npm ci --legacy-peer-deps
      
      - name: Build React app
        working-directory: GM_Buddy.React
        env:
          VITE_COGNITO_DOMAIN: ${{ secrets.VITE_COGNITO_DOMAIN }}
          VITE_COGNITO_CLIENT_ID: ${{ secrets.VITE_COGNITO_CLIENT_ID }}
          VITE_COGNITO_REDIRECT_URI: ${{ secrets.VITE_COGNITO_REDIRECT_URI }}
          VITE_COGNITO_LOGOUT_URI: ${{ secrets.VITE_COGNITO_LOGOUT_URI }}
          VITE_USE_COGNITO: ${{ secrets.VITE_USE_COGNITO }}
        run: npm run build
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ secrets.AWS_REGION }}
      
      - name: Deploy to S3
        run: |
          # Upload assets with long cache
          aws s3 sync GM_Buddy.React/dist/ s3://${{ secrets.S3_BUCKET_NAME }}/ \
            --delete \
            --cache-control "public, max-age=31536000, immutable" \
            --exclude "*.html" \
            --exclude "*.json"
          
          # Upload HTML and JSON with short cache
          aws s3 sync GM_Buddy.React/dist/ s3://${{ secrets.S3_BUCKET_NAME }}/ \
            --exclude "*" \
            --include "*.html" \
            --include "*.json" \
            --cache-control "public, max-age=0, must-revalidate"
          
          echo "Files uploaded to S3 successfully"
      
      - name: Invalidate CloudFront cache
        run: |
          INVALIDATION_ID=$(aws cloudfront create-invalidation \
            --distribution-id ${{ secrets.CLOUDFRONT_DISTRIBUTION_ID }} \
            --paths "/*" \
            --query 'Invalidation.Id' \
            --output text)
          echo "CloudFront invalidation created: $INVALIDATION_ID"
```

---

## Conclusion

This plan provides a comprehensive approach to deploying GM Buddy to AWS using GitHub Actions. The modular workflow design allows for independent deployment of each component while maintaining dependencies where necessary.

Key considerations:
- **Security**: All secrets managed via GitHub Secrets
- **Automation**: Full CI/CD pipeline with minimal manual intervention
- **Reliability**: Health checks and monitoring at each layer
- **Flexibility**: Manual workflow dispatch for controlled deployments
- **Cost-effective**: Right-sized resources with auto-scaling

Once AWS resources are provisioned and configured, these workflows can be implemented and tested incrementally, starting with the database migration, then server deployment, and finally frontend deployment.
