# GitHub Secrets Configuration

This document describes the secrets required for GitHub Actions CI/CD pipelines.

## Required Repository Secrets

Configure these in: **Settings ? Secrets and variables ? Actions ? Repository secrets**

### Database Secrets

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `POSTGRES_USER` | PostgreSQL username | `postgres` |
| `POSTGRES_PASSWORD` | PostgreSQL password | `your_secure_password` |
| `POSTGRES_DB` | Database name | `gm_buddy` |

### AWS Cognito Secrets

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `COGNITO_REGION` | AWS region | `us-west-2` |
| `COGNITO_USER_POOL_ID` | Cognito User Pool ID | `us-west-2_XXXXXXXXX` |
| `COGNITO_CLIENT_ID` | Cognito App Client ID | `your_client_id` |
| `COGNITO_CLIENT_SECRET` | Cognito App Client Secret | `your_client_secret` |
| `COGNITO_DOMAIN` | Cognito domain prefix | `your-domain-prefix` |

### React App Secrets (for Cognito-enabled builds)

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `USE_COGNITO` | Enable Cognito in React app | `true` or `false` |
| `REACT_REDIRECT_URI` | OAuth callback URL | `https://yourdomain.com/callback` |
| `REACT_LOGOUT_URI` | Post-logout redirect URL | `https://yourdomain.com` |

### Optional Secrets

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `DOCKER_REGISTRY` | Docker registry URL | `ghcr.io/username` |

## Usage in GitHub Actions

Reference secrets in workflow files:

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Build React app with Cognito
        run: |
          docker-compose build --build-arg VITE_COGNITO_DOMAIN=${{ secrets.COGNITO_DOMAIN }}.auth.${{ secrets.COGNITO_REGION }}.amazoncognito.com \
            --build-arg VITE_COGNITO_CLIENT_ID=${{ secrets.COGNITO_CLIENT_ID }} \
            --build-arg VITE_COGNITO_REDIRECT_URI=${{ secrets.REACT_REDIRECT_URI }} \
            --build-arg VITE_COGNITO_LOGOUT_URI=${{ secrets.REACT_LOGOUT_URI }} \
            --build-arg VITE_USE_COGNITO=${{ secrets.USE_COGNITO }} \
            gm_buddy_web

  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy with secrets
        env:
          POSTGRES_PASSWORD: ${{ secrets.POSTGRES_PASSWORD }}
          COGNITO_REGION: ${{ secrets.COGNITO_REGION }}
          COGNITO_USER_POOL_ID: ${{ secrets.COGNITO_USER_POOL_ID }}
          COGNITO_CLIENT_ID: ${{ secrets.COGNITO_CLIENT_ID }}
          COGNITO_CLIENT_SECRET: ${{ secrets.COGNITO_CLIENT_SECRET }}
          COGNITO_DOMAIN: ${{ secrets.COGNITO_DOMAIN }}
        run: |
          docker-compose up -d
```

## Local Development

For local development, you have two options:

### Option 1: Demo Mode (no Cognito)

Just use the existing `.env` file. The React app will show demo login buttons with test accounts.

### Option 2: Real Cognito Authentication

1. Copy `.env.local.example` to `.env.local`:
   ```bash
   cp .env.local.example .env.local
   ```

2. Edit `.env.local` with your Cognito values (get from AWS Console or team lead)

3. For the React app, copy `GM_Buddy.React/.env.local.example` to `.env.local`:
   ```bash
   cp GM_Buddy.React/.env.local.example GM_Buddy.React/.env.local
   ```

4. The `.env.local` files are gitignored and will override the base `.env` files.

### Server Configuration

The `appsettings.Development.json` files are gitignored and can contain secrets for local debugging:
- `GM_Buddy.Web/appsettings.Development.json`
- `GM_Buddy.Server/appsettings.Development.json`

Copy from `appsettings.json` and add your Cognito values.

## Security Notes

1. **Never commit secrets** to the repository
2. **Rotate secrets** if they are accidentally exposed
3. **Use environment-specific secrets** (dev, staging, prod)
4. **Limit secret access** to only required workflows
5. **Audit secret usage** via GitHub's audit log
6. **Use `.env.local` files** for local development secrets (gitignored)
