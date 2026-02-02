# GitHub Secrets Configuration Guide

This document lists all GitHub secrets required for the GM_Buddy CI/CD pipelines.

## How to Add Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret listed below

---

## AWS Credentials (Required for all deployments)

### `AWS_ACCESS_KEY_ID`
- **Description**: AWS access key ID for authentication
- **Example**: `AKIAIOSFODNN7EXAMPLE`
- **Used in**: Backend deployment, Frontend deployment

### `AWS_SECRET_ACCESS_KEY`
- **Description**: AWS secret access key for authentication
- **Example**: `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY`
- **Used in**: Backend deployment, Frontend deployment

### `AWS_REGION`
- **Description**: AWS region where resources are deployed
- **Example**: `us-west-2`
- **Used in**: Backend deployment, Frontend deployment

---

## Backend Deployment Secrets (ECR & Elastic Beanstalk)

### `ECR_REPOSITORY_NAME`
- **Description**: Name of the ECR repository for Docker images
- **Example**: `gm-buddy-server`
- **Used in**: Backend deployment

### `EB_APPLICATION_NAME`
- **Description**: Elastic Beanstalk application name
- **Example**: `gm-buddy-server`
- **Used in**: Backend deployment

### `EB_ENVIRONMENT_NAME`
- **Description**: Elastic Beanstalk environment name
- **Example**: `gm-buddy-server-prod`
- **Used in**: Backend deployment

### `EB_ENVIRONMENT_URL`
- **Description**: Elastic Beanstalk environment URL (for verification)
- **Example**: `http://gm-buddy-server-prod.us-west-2.elasticbeanstalk.com`
- **Used in**: Backend deployment (optional, for display)

---

## Frontend Deployment Secrets (S3 & CloudFront)

### `S3_BUCKET_NAME`
- **Description**: S3 bucket name for hosting the React build
- **Example**: `gm-buddy-frontend`
- **Used in**: Frontend deployment

### `CLOUDFRONT_DISTRIBUTION_ID`
- **Description**: CloudFront distribution ID
- **Example**: `E1234567890ABC`
- **Used in**: Frontend deployment

### `CLOUDFRONT_URL`
- **Description**: CloudFront distribution URL (for verification)
- **Example**: `https://d1234567890.cloudfront.net`
- **Used in**: Frontend deployment (optional, for display)

---

## Frontend Environment Variables (React/Vite)

### `VITE_API_URL`
- **Description**: Backend API URL (CloudFront or Elastic Beanstalk URL)
- **Example**: `https://api.gm-buddy.com` or `https://d2zsk9max2no60.cloudfront.net`
- **Used in**: Frontend deployment

### `VITE_COGNITO_DOMAIN`
- **Description**: AWS Cognito domain
- **Example**: `gm-buddy.auth.us-west-2.amazoncognito.com`
- **Used in**: Frontend deployment

### `VITE_COGNITO_CLIENT_ID`
- **Description**: AWS Cognito client ID
- **Example**: `4sa64ep99bsehme2gq2jfinlui`
- **Used in**: Frontend deployment

### `VITE_COGNITO_REDIRECT_URI`
- **Description**: Cognito redirect URI after login
- **Example**: `https://d1234567890.cloudfront.net/callback`
- **Used in**: Frontend deployment

### `VITE_COGNITO_LOGOUT_URI`
- **Description**: Cognito redirect URI after logout
- **Example**: `https://d1234567890.cloudfront.net`
- **Used in**: Frontend deployment

---

## Elastic Beanstalk Environment Variables

These should be configured in the **Elastic Beanstalk Console** → **Configuration** → **Software** → **Environment properties**:

### Database Configuration
- `DbSettings__Host` - Database host (e.g., RDS endpoint)
- `DbSettings__Port` - Database port (default: `5432`)
- `DbSettings__Database` - Database name (e.g., `gm_buddy`)
- `DbSettings__Username` - Database username
- `DbSettings__Password` - Database password

### AWS Cognito Configuration
- `Cognito__Region` - AWS Cognito region (e.g., `us-west-2`)
- `Cognito__UserPoolId` - Cognito user pool ID (e.g., `us-west-2_3H6SIoARI`)
- `Cognito__ClientId` - Cognito client ID (e.g., `4sa64ep99bsehme2gq2jfinlui`)
- `Cognito__Domain` - Cognito domain prefix (e.g., `gm-buddy`)

### ASP.NET Core Configuration
- `ASPNETCORE_ENVIRONMENT` - Set to `Production`
- `ASPNETCORE_HTTP_PORTS` - Set to `8080`

---

## Summary: Required Secrets Checklist

Use this checklist to verify all secrets are configured:

### AWS Credentials (3)
- [ ] `AWS_ACCESS_KEY_ID`
- [ ] `AWS_SECRET_ACCESS_KEY`
- [ ] `AWS_REGION`

### Backend (4)
- [ ] `ECR_REPOSITORY_NAME`
- [ ] `EB_APPLICATION_NAME`
- [ ] `EB_ENVIRONMENT_NAME`
- [ ] `EB_ENVIRONMENT_URL` (optional)

### Frontend S3/CloudFront (3)
- [ ] `S3_BUCKET_NAME`
- [ ] `CLOUDFRONT_DISTRIBUTION_ID`
- [ ] `CLOUDFRONT_URL` (optional)

### Frontend React Environment (5)
- [ ] `VITE_API_URL`
- [ ] `VITE_COGNITO_DOMAIN`
- [ ] `VITE_COGNITO_CLIENT_ID`
- [ ] `VITE_COGNITO_REDIRECT_URI`
- [ ] `VITE_COGNITO_LOGOUT_URI`

---

## Notes

- All secrets are **case-sensitive**
- Never commit secrets to the repository
- Use GitHub Environments for different deployment stages (production, staging)
- Rotate credentials regularly for security
- Keep this document updated when adding new secrets
