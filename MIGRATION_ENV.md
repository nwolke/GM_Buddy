# ?? Environment File Migration

## What Changed?

The `.env` file is no longer tracked in Git to protect sensitive credentials.

## Action Required (One-time setup)

If you just pulled the latest changes, follow these steps:

### 1. Create your local `.env` file

```bash
# From the repository root
cp .env.example .env
```

### 2. Edit `.env` with your values

Open `.env` and replace the placeholders:

```env
# PostgreSQL Configuration
POSTGRES_PASSWORD=your_secure_password_here

# pgAdmin Configuration
PGADMIN_DEFAULT_EMAIL=your_email@example.com
PGADMIN_DEFAULT_PASSWORD=your_secure_password_here

# Working directory for PostgreSQL
PWD=/path/to/your/postgres
```

**For local development, you can use:**
```env
POSTGRES_PASSWORD=12345
PGADMIN_DEFAULT_EMAIL=your_email@example.com
PGADMIN_DEFAULT_PASSWORD=12345
PWD=d:/Dev/postgres
```

### 3. Verify Docker services start

```bash
docker-compose up gm_buddy_postgres gm_buddy_pgadmin
```

---

## Why This Change?

### Security Benefits
- ? Passwords/secrets are no longer exposed in Git history
- ? Each developer can use their own credentials
- ? Production secrets stay separate from dev environment
- ? Reduces risk of accidental credential leaks

### Best Practices
- `.env.example` - Committed to Git (template with placeholders)
- `.env` - Local only (ignored by Git, contains real secrets)
- `.env.local` - Optional local overrides (also ignored)

---

## Files Affected

| File | Status | Purpose |
|------|--------|---------|
| `.env.example` | ? New (tracked) | Template for team members |
| `.env` | ?? Now ignored | Your actual secrets (create locally) |
| `.gitignore` | ?? Updated | Excludes `.env`, `db-data/`, `pgadmin-data/` |

---

## Troubleshooting

### "docker-compose fails with authentication error"
- Verify your `.env` file exists in the repository root
- Check that `POSTGRES_PASSWORD` matches in `.env` and any appsettings

### "I accidentally committed .env"
```bash
# Remove from Git but keep local file
git rm --cached .env
git commit -m "Remove .env from tracking"
```

### "Need to reset database"
```bash
docker-compose down -v
docker-compose up gm_buddy_postgres
```

---

## Questions?

See [README.md](./README.md) or [DEV_SETUP.md](./DEV_SETUP.md) for full setup instructions.
