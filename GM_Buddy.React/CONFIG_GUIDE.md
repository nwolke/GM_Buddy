# GM Buddy - Configuration Guide

## Environment Files

### `.env.development.local` (gitignored)
**Used for:** Local development with Vite dev server
**How to use:**
```bash
npm run dev
# Runs on http://localhost:3000
# Connects to backend at http://localhost:5000
```

### `.env.production.local` (gitignored)
**Used for:** Testing production builds locally before deploying
**How to use:**
```bash
npm run build
npm run preview
# Runs on http://localhost:4173
# Connects to backend at http://localhost:5000
```

### `.env.production` (committed)
**Used for:** AWS production deployments
**Modified by:** GitHub Actions during deployment

---

## Workflow Guide

### ?? Local Development (Fast iteration)
```bash
# 1. Start backend
docker-compose up gm_buddy_server gm_buddy_postgres -d

# 2. Start frontend
cd GM_Buddy.React
npm run dev
```
Visit: `http://localhost:3000`

---

### ?? Test Production Build (Before AWS deploy)
```bash
# 1. Start backend
docker-compose up gm_buddy_server gm_buddy_postgres -d

# 2. Build and preview
cd GM_Buddy.React
npm run build
npm run preview
```
Visit: `http://localhost:4173`

**This matches your AWS setup!**
- ? Production build (optimized, minified)
- ? Direct API calls (no nginx proxy)
- ? Same behavior as S3 + Elastic Beanstalk

---

### ?? Deploy to AWS
```bash
# Push to GitHub - Actions handles the rest
git push origin main
```

---

## Quick Reference

| Scenario | Command | URL | Backend | Config File |
|----------|---------|-----|---------|-------------|
| **Dev** | `npm run dev` | :3000 | :5000 | `.env.development.local` |
| **Test Prod** | `npm run preview` | :4173 | :5000 | `.env.production.local` |
| **AWS** | GitHub Actions | CloudFront | EB | `.env.production` |

---

## Cognito Callback URLs

Make sure these are configured in AWS Cognito Console:
- `http://localhost:3000/callback` (dev)
- `http://localhost:4173/callback` (prod testing)
- `https://d2zsk9max2no60.cloudfront.net/callback` (AWS)

---

## Troubleshooting

**CORS errors?**
Check `GM_Buddy.Server/Program.cs` line 164 - ensure your port is allowed

**Backend not responding?**
```bash
docker logs gm_buddy_server -f
```

**Environment variables not working?**
Restart Vite after changing `.env.*` files!
