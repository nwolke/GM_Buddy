# AWS Deployment Quick Start Guide

## Overview

This guide provides a quick reference for implementing the AWS deployment plan. For detailed information, see [AWS_DEPLOYMENT_PLAN.md](./AWS_DEPLOYMENT_PLAN.md).

## Prerequisites Checklist

### AWS Resources

- [ ] RDS PostgreSQL instance created and accessible
- [ ] ECR repository created: `gm-buddy-server`
- [ ] App Runner service created and configured
- [ ] S3 bucket created for static website hosting
- [ ] CloudFront distribution created pointing to S3 bucket
- [ ] AWS Cognito User Pool and App Client configured
- [ ] IAM user/role with appropriate permissions created

### GitHub Secrets

Navigate to **Settings > Secrets and variables > Actions** in your repository and add:

#### AWS Configuration
- `AWS_REGION` - Your AWS region (e.g., `us-west-2`)
- `AWS_ACCOUNT_ID` - Your AWS account ID (e.g., `107912522122`)
- `AWS_ACCESS_KEY_ID` - IAM access key
- `AWS_SECRET_ACCESS_KEY` - IAM secret key

#### Database Configuration
- `DB_HOST` - RDS endpoint
- `DB_PORT` - Database port (usually `5432`)
- `DB_NAME` - Database name
- `DB_USER` - Database username
- `DB_PASSWORD` - Database password

#### Deployment Targets
- `APP_RUNNER_SERVICE_ARN` - Full ARN of App Runner service
- `S3_BUCKET_NAME` - S3 bucket name for React app
- `CLOUDFRONT_DISTRIBUTION_ID` - CloudFront distribution ID

#### Cognito Configuration
- `VITE_COGNITO_DOMAIN` - Cognito domain
- `VITE_COGNITO_CLIENT_ID` - Cognito app client ID
- `VITE_COGNITO_REDIRECT_URI` - Redirect URI after login
- `VITE_COGNITO_LOGOUT_URI` - Redirect URI after logout
- `VITE_USE_COGNITO` - Set to `true` for production

## Workflow Files to Create

Create these workflow files in `.github/workflows/`:

### 1. Database Migration (with Flyway - Recommended)
**File**: `.github/workflows/database-migration.yml`

**Purpose**: Deploy database schema and migrations to RDS using Flyway

**Trigger**: Manual workflow dispatch or push to `db/migration/**`

**Why Flyway**: 
- Free and open source (Apache License 2.0)
- Automatic version tracking and validation
- Prevents accidental schema changes with checksums
- Built-in rollback support

**Example**: See Appendix A.4 in the deployment plan

**Alternative**: Basic psql approach in Appendix A.1

### 2. Server Deployment
**File**: `.github/workflows/deploy-server.yml`

**Purpose**: Build and deploy ASP.NET server to App Runner

**Trigger**: Push to main (server code changes) or manual dispatch

**Example**: See Appendix A.2 in the deployment plan

### 3. Frontend Deployment
**File**: `.github/workflows/deploy-frontend.yml`

**Purpose**: Build and deploy React app to S3 + CloudFront

**Trigger**: Push to main (React code changes) or manual dispatch

**Example**: See Appendix A.3 in the deployment plan

## Initial Deployment Steps

1. **Setup GitHub Secrets**
   - Add all required secrets to repository
   - Verify values are correct

2. **Prepare Database Migrations (if using Flyway)**
   - Create `db/migration/` directory
   - Move `init.sql` content to `V1__Initial_schema.sql`
   - Future migrations follow pattern: `V2__description.sql`

3. **Run Database Migration**
   - Go to Actions tab in GitHub
   - Select "Database Migration" workflow
   - Click "Run workflow"
   - Select environment (production)
   - Verify migration completed successfully

3. **Deploy Server**
   - Either push to main branch with server changes
   - Or manually trigger "Deploy Server to App Runner" workflow
   - Verify App Runner service updated successfully

4. **Deploy Frontend**
   - Either push to main branch with React changes
   - Or manually trigger "Deploy Frontend to S3 + CloudFront" workflow
   - Verify files uploaded to S3 and CloudFront invalidation completed

5. **Test Deployment**
   - Access CloudFront URL or custom domain
   - Test authentication with Cognito
   - Test API calls to App Runner backend
   - Verify database connectivity

## Deployment Order

For complete deployments:

```
Database → Server → Frontend
   ↓         ↓         ↓
  RDS    App Runner   S3 + CloudFront
```

## Common Issues and Solutions

### Database Migration Fails
- **Issue**: Connection timeout
- **Solution**: Check RDS security group allows connections from GitHub Actions IPs or use VPC connector

### Server Deployment Fails
- **Issue**: ECR permission denied
- **Solution**: Verify IAM user has ECR permissions and correct region

### Frontend Build Fails
- **Issue**: Missing Cognito environment variables
- **Solution**: Verify all `VITE_COGNITO_*` secrets are set

### CloudFront Still Shows Old Content
- **Issue**: Cache not invalidated
- **Solution**: Wait for invalidation to complete (can take 5-15 minutes) or manually invalidate

## Monitoring

After deployment, monitor:

1. **App Runner**: Check CloudWatch Logs for application errors
2. **CloudFront**: Monitor request count and error rates
3. **RDS**: Check database connections and performance metrics

## Rollback Procedures

### Server Rollback
```bash
# Manually deploy previous image
aws apprunner start-deployment --service-arn <ARN>
```

### Frontend Rollback
- Re-run frontend workflow with previous commit SHA
- Or manually sync previous build to S3

### Database Rollback
- Use RDS snapshot to restore previous state
- **Warning**: This will lose any data created since snapshot

## Next Steps After Deployment

1. Set up CloudWatch alarms for critical metrics
2. Configure log aggregation
3. Set up automated backups for RDS
4. Configure lifecycle policies for ECR images
5. Review and optimize costs
6. Set up staging environment for testing

## Support

For detailed information on:
- AWS resource configuration
- Security considerations
- Cost optimization
- Monitoring setup

See the complete [AWS_DEPLOYMENT_PLAN.md](./AWS_DEPLOYMENT_PLAN.md).
