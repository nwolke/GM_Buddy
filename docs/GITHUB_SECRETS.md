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
| `COGNITO_USER_POOL_ID` | Cognito User Pool ID | `us-west-2_3H6SIoARI` |
| `COGNITO_CLIENT_ID` | Cognito App Client ID | `3tu41ptf62ntlqso884tl3aaem` |
| `COGNITO_CLIENT_SECRET` | Cognito App Client Secret | `1ffua8rcl3ok...` |
| `COGNITO_DOMAIN` | Cognito domain prefix | `us-west-23h6sioari` |

### Optional Secrets

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `DOCKER_REGISTRY` | Docker registry URL | `ghcr.io/username` |

## Usage in GitHub Actions

Reference secrets in workflow files:

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy
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

For local development, create a `.env` file (git-ignored) based on `.env.example`:

```bash
cp .env.example .env
# Edit .env with your actual values
```

The `appsettings.Development.json` files (also git-ignored) contain secrets for local debugging:
- `GM_Buddy.Web/appsettings.Development.json`
- `GM_Buddy.Server/appsettings.Development.json`

## Security Notes

1. **Never commit secrets** to the repository
2. **Rotate secrets** if they are accidentally exposed
3. **Use environment-specific secrets** (dev, staging, prod)
4. **Limit secret access** to only required workflows
5. **Audit secret usage** via GitHub's audit log
