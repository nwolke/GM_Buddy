# Manual Deployment Guide

This guide explains how to manually deploy GM_Buddy to AWS using GitHub Actions.

---

## Overview

Deployments are **manual-only** to prevent accidental deployments to production. You trigger deployments through the **GitHub Actions UI** using workflow_dispatch events.

### Why Manual Deployments?

✅ **Full control** - You decide exactly when to deploy  
✅ **No accidents** - Pushing to `main` won't trigger deployment  
✅ **Environment selection** - Choose production or staging per deployment  
✅ **Safety** - Review code before deploying  

---

## How to Deploy

### Prerequisites

1. ✅ All required GitHub secrets configured (see [GITHUB_SECRETS.md](./GITHUB_SECRETS.md))
2. ✅ Code merged to `main` branch
3. ✅ CI tests passing (build-and-test.yml)

---

## Step-by-Step: Deploy Backend

### 1. Navigate to GitHub Actions

1. Go to your repository: `https://github.com/nwolke/GM_Buddy`
2. Click the **Actions** tab at the top
3. In the left sidebar, click **"Deploy Backend to AWS"**

### 2. Trigger the Deployment

1. Click the **"Run workflow"** button (top right)
2. You'll see a dropdown with options:
   - **Branch**: Select `main` (or another branch to deploy)
   - **Deployment environment**: Choose `production` or `staging`
3. Click the green **"Run workflow"** button

### 3. Monitor the Deployment

1. The workflow will appear in the list
2. Click on it to see real-time logs
3. Watch each step:
   - ✓ Checkout code
   - ✓ Configure AWS credentials
   - ✓ Login to Amazon ECR
   - ✓ Build and push Docker image
   - ✓ Deploy to Elastic Beanstalk
   - ✓ Verify deployment

### 4. Verify Success

- ✅ Green checkmark = successful deployment
- ❌ Red X = deployment failed (click to see error logs)
- Check your Elastic Beanstalk environment in AWS Console

**Expected time**: 5-10 minutes

---

## Step-by-Step: Deploy Frontend

### 1. Navigate to GitHub Actions

1. Go to your repository: `https://github.com/nwolke/GM_Buddy`
2. Click the **Actions** tab
3. In the left sidebar, click **"Deploy Frontend to AWS"**

### 2. Trigger the Deployment

1. Click the **"Run workflow"** button
2. Select options:
   - **Branch**: Usually `main`
   - **Deployment environment**: `production` or `staging`
3. Click **"Run workflow"**

### 3. Monitor the Deployment

Watch the workflow progress:
- ✓ Checkout code
- ✓ Setup Node.js
- ✓ Install dependencies
- ✓ Build React app with production env vars
- ✓ Upload to S3
- ✓ Invalidate CloudFront cache

### 4. Verify Success

- Check CloudFront URL in browser
- Verify new changes are visible
- May take 1-2 minutes for CloudFront cache invalidation

**Expected time**: 3-5 minutes

---

## Understanding GitHub Actions Runners

### What are GitHub Actions Runners?

GitHub Actions uses **runners** - virtual machines that execute your workflows. Think of them as temporary cloud computers that:

1. **Spin up** when you trigger a workflow
2. **Execute** all the steps in your workflow
3. **Shut down** when complete

### Runner Specifications

For GM_Buddy, workflows use:
- **OS**: Ubuntu (latest)
- **Location**: GitHub's cloud infrastructure
- **Cost**: Free for public repos, usage limits for private repos

### What Happens During Deployment?

```
┌─────────────────────────────────────────────────────┐
│ You Click "Run workflow" in GitHub UI               │
└─────────────────┬───────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ GitHub Actions spins up a runner (Ubuntu VM)        │
└─────────────────┬───────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ Runner executes workflow steps:                     │
│  1. Checkout code from GitHub                       │
│  2. Setup tools (Node.js, .NET, AWS CLI, Docker)    │
│  3. Build your application                          │
│  4. Use AWS credentials from GitHub secrets         │
│  5. Push to ECR / Upload to S3                      │
│  6. Deploy to Elastic Beanstalk / Invalidate CF     │
└─────────────────┬───────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ Deployment complete, runner shuts down              │
└─────────────────────────────────────────────────────┘
```

---

## Deployment Workflows Explained

### Backend Deployment (`deploy-backend.yml`)

**Trigger**: Manual only (workflow_dispatch)

**What it does**:
1. Builds .NET application in Docker container
2. Tags image with commit SHA
3. Pushes to AWS ECR (Elastic Container Registry)
4. Updates Elastic Beanstalk to use new image
5. Waits for deployment to complete

**Requires**:
- `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION`
- `ECR_REPOSITORY_NAME`
- `EB_APPLICATION_NAME`, `EB_ENVIRONMENT_NAME`

### Frontend Deployment (`deploy-frontend.yml`)

**Trigger**: Manual only (workflow_dispatch)

**What it does**:
1. Installs npm dependencies
2. Creates `.env.production` from GitHub secrets
3. Builds React app with Vite
4. Uploads `dist/` folder to S3
5. Invalidates CloudFront cache

**Requires**:
- `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION`
- `S3_BUCKET_NAME`
- `CLOUDFRONT_DISTRIBUTION_ID`
- All `VITE_*` environment variables

---

## Deployment Checklist

Before deploying, verify:

### ✅ Pre-Deployment Checklist

- [ ] Code changes merged to `main`
- [ ] CI tests passing (green checkmark on latest commit)
- [ ] All GitHub secrets configured correctly
- [ ] AWS resources exist (ECR, EB, S3, CloudFront)
- [ ] Reviewed changes to be deployed
- [ ] Notified team (if applicable)

### ✅ Post-Deployment Checklist

- [ ] Workflow completed successfully (green checkmark)
- [ ] Backend: Elastic Beanstalk shows "Health: Ok"
- [ ] Frontend: CloudFront URL shows new version
- [ ] Functionality testing completed
- [ ] No errors in browser console / backend logs

---

## Troubleshooting

### Deployment Failed - Now What?

1. **Click on the failed workflow** to see detailed logs
2. **Find the red X** step that failed
3. **Read the error message** - usually indicates the problem
4. **Common issues**:

#### AWS Credentials Invalid
```
Error: The security token included in the request is invalid
```
**Fix**: Check `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` in GitHub secrets

#### ECR Repository Not Found
```
Error: repository not found
```
**Fix**: Verify `ECR_REPOSITORY_NAME` matches your actual ECR repository

#### Elastic Beanstalk Environment Not Found
```
Error: environment not found
```
**Fix**: Check `EB_APPLICATION_NAME` and `EB_ENVIRONMENT_NAME`

#### S3 Bucket Access Denied
```
Error: Access Denied
```
**Fix**: Verify AWS IAM permissions for S3 and CloudFront

#### CloudFront Distribution Not Found
```
Error: distribution not found
```
**Fix**: Verify `CLOUDFRONT_DISTRIBUTION_ID` is correct

### Re-running a Failed Deployment

1. Fix the issue (update secrets, fix AWS resources, etc.)
2. Go back to GitHub Actions
3. Click **"Re-run failed jobs"** or trigger a new deployment

---

## Advanced: Deploying Specific Branches

You can deploy from branches other than `main`:

1. When clicking "Run workflow"
2. Change the **"Use workflow from"** dropdown
3. Select your branch (e.g., `develop`, `hotfix/critical-bug`)
4. Click "Run workflow"

**Use case**: Testing staging deployments before production

---

## Deployment Environments

The workflows support two environments:

### Production
- **When**: Stable, tested code
- **URL**: Your production CloudFront/EB URLs
- **Select**: Choose "production" when running workflow

### Staging
- **When**: Testing before production
- **URL**: Your staging environment URLs (if configured)
- **Select**: Choose "staging" when running workflow

**Note**: You need separate AWS resources for staging (separate EB environment, S3 bucket, CloudFront distribution)

---

## Security Best Practices

### ✅ DO:
- Keep GitHub secrets up to date
- Rotate AWS access keys regularly
- Use least-privilege IAM policies
- Review workflow logs after deployment
- Test in staging before production

### ❌ DON'T:
- Share AWS credentials in chat/email
- Commit secrets to the repository
- Use root AWS credentials
- Deploy untested code to production
- Skip the CI build before deploying

---

## Alternative: Deploy via GitHub CLI

You can also trigger deployments from the command line:

```bash
# Install GitHub CLI
# https://cli.github.com/

# Deploy backend
gh workflow run "Deploy Backend to AWS" \
  --ref main \
  --field environment=production

# Deploy frontend  
gh workflow run "Deploy Frontend to AWS" \
  --ref main \
  --field environment=production

# View workflow runs
gh run list --workflow "Deploy Backend to AWS"
```

---

## Comparison: GitHub Actions vs Other CI/CD

| Feature | GitHub Actions | GitLab CI/CD | Jenkins |
|---------|---------------|--------------|---------|
| **Hosted** | ✅ Yes (free for public) | ✅ Yes | ❌ Self-hosted |
| **Integration** | ✅ Native GitHub | ✅ Native GitLab | 🔧 Plugin needed |
| **Manual Triggers** | ✅ workflow_dispatch | ✅ when: manual | ✅ Build parameters |
| **Secrets Management** | ✅ Built-in | ✅ Built-in | 🔧 Plugins |
| **Setup Complexity** | ⭐⭐ Easy | ⭐⭐ Easy | ⭐⭐⭐⭐ Complex |

**Note**: This repository uses **GitHub Actions**, not GitLab runners.

---

## Cost Considerations

### GitHub Actions Minutes

- **Public repositories**: Unlimited minutes (free)
- **Private repositories**: 
  - Free tier: 2,000 minutes/month
  - Paid: $0.008 per minute (Linux runners)

### Estimated Usage

- **Backend deployment**: ~5-8 minutes
- **Frontend deployment**: ~3-5 minutes
- **CI build/test**: ~3-5 minutes

**Monthly estimate** (private repo):
- 10 deployments = ~110 minutes
- Well within free tier

---

## Quick Reference

### Backend Deployment
```
GitHub → Actions → "Deploy Backend to AWS" → Run workflow → main → production → Run
```

### Frontend Deployment
```
GitHub → Actions → "Deploy Frontend to AWS" → Run workflow → main → production → Run
```

### Check Status
```
GitHub → Actions → Click workflow run → View logs
```

---

## Getting Help

### Deployment Issues
1. Check workflow logs in GitHub Actions
2. Verify GitHub secrets are set correctly
3. Check AWS Console for resource status
4. See [CONFIGURATION.md](./CONFIGURATION.md) for detailed config
5. Open a GitHub issue with error logs

### Documentation
- [GITHUB_SECRETS.md](./GITHUB_SECRETS.md) - Required secrets
- [CONFIGURATION.md](./CONFIGURATION.md) - Complete configuration
- [QUICKSTART.md](./QUICKSTART.md) - Local development setup

---

## Summary

✅ **Manual deployment** = Full control over when you deploy  
✅ **GitHub Actions UI** = Click "Run workflow" button  
✅ **GitHub Actions runners** = Cloud VMs that execute your workflows  
✅ **No automatic deployments** = Push to `main` won't deploy  
✅ **Environment selection** = Choose production or staging per deployment  

**Next Steps**:
1. Configure all GitHub secrets
2. Navigate to Actions tab in GitHub
3. Click "Run workflow" to deploy
4. Monitor deployment progress
5. Verify deployment success
