# GM Buddy

A full-stack application for Game Masters to manage NPCs, campaigns, and game systems.

## Tech Stack

- **Backend**: .NET 9 (ASP.NET Core)
- **Frontend**: React + TypeScript + Vite
- **Database**: PostgreSQL
- **Authentication**: AWS Cognito
- **Infrastructure**: Docker, AWS (S3, CloudFront, ECR, Elastic Beanstalk)
- **CI/CD**: GitHub Actions

---

## 🚀 Quick Start

**Get up and running in 5 minutes!**

```bash
# Clone and setup
git clone https://github.com/nwolke/GM_Buddy.git
cd GM_Buddy
cp .env.example .env

# Edit .env and set POSTGRES_PASSWORD
# Then start everything with Docker
docker compose up --build
```

**Access the application:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:8080

👉 **See [QUICKSTART.md](./QUICKSTART.md) for detailed setup instructions**

---

## 📚 Documentation

- **[QUICKSTART.md](./QUICKSTART.md)** - Get started in 5 minutes
- **[CONFIGURATION.md](./CONFIGURATION.md)** - Complete configuration guide
- **[OBSERVABILITY_QUICKSTART.md](./OBSERVABILITY_QUICKSTART.md)** - Quick setup for logging and metrics
- **[OBSERVABILITY.md](./OBSERVABILITY.md)** - Comprehensive observability guide
- **[GITHUB_SECRETS.md](./GITHUB_SECRETS.md)** - Required secrets for CI/CD
- **[MIGRATION.md](./MIGRATION.md)** - Migrating from old configuration
- **[DEV_SETUP.md](./DEV_SETUP.md)** - Detailed development setup

---

## 🏗️ Architecture

### Local Development
```
Docker Compose
├── PostgreSQL (port 15432)
├── pgAdmin (port 15435)
├── Backend API (port 8080)
└── Frontend (port 3000)
```

### Production (AWS)
```
Users → CloudFront → S3 (Frontend)
                  ↓
              API Calls
                  ↓
        Elastic Beanstalk (Backend)
                  ↓
             RDS PostgreSQL
                  ↓
            AWS Cognito (Auth)
```

---

## 🛠️ Development

### Prerequisites

- Docker and Docker Compose (required)
- .NET 9 SDK (optional, for local development without Docker)
- Node.js 20+ (optional, for local development without Docker)

### Running Locally

**With Docker (Recommended):**
```bash
docker compose up --build
```

**Without Docker:**
```bash
# Terminal 1: Start PostgreSQL (needs to be running)
docker compose up gm_buddy_postgres

# Terminal 2: Start Backend
cd GM_Buddy.Server
dotnet run

# Terminal 3: Start Frontend
cd GM_Buddy.React
npm install --legacy-peer-deps
npm run dev
```

### Running Tests

**Backend:**
```bash
dotnet test GM_Buddy.sln
```

**Frontend:**
```bash
cd GM_Buddy.React
npm run test
npm run test:coverage
```

---

## 🚢 Deployment

The repository includes manual deployment pipelines via GitHub Actions:

- **`build-and-test.yml`** - Runs on every push/PR to validate code
- **`deploy-backend.yml`** - Manual deployment: Backend to AWS (ECR → Elastic Beanstalk)
- **`deploy-frontend.yml`** - Manual deployment: Frontend to AWS (S3 → CloudFront)

**Deployments are manual-only** for safety and control.

**How to deploy:**
1. Create AWS IAM deployment user (see [AWS_IAM_SETUP.md](./AWS_IAM_SETUP.md))
2. Configure AWS resources (ECR, EB, S3, CloudFront, Cognito)
3. Add required GitHub secrets (see [GITHUB_SECRETS.md](./GITHUB_SECRETS.md))
4. Trigger deployment via GitHub Actions UI (see [MANUAL_DEPLOYMENT.md](./MANUAL_DEPLOYMENT.md))

---

## 📁 Project Structure

```
GM_Buddy/
├── GM_Buddy.Server/          # ASP.NET Core backend
├── GM_Buddy.React/           # React + TypeScript frontend
├── GM_Buddy.Business/        # Business logic layer
├── GM_Buddy.Data/            # Data access layer
├── GM_Buddy.Contracts/       # Shared contracts/DTOs
├── .github/workflows/        # GitHub Actions CI/CD
├── CONFIGURATION.md          # Configuration guide
├── GITHUB_SECRETS.md         # Required secrets
├── QUICKSTART.md             # Quick setup guide
└── MIGRATION.md              # Migration guide
```

---

## 🗄️ Database Management

The database is automatically initialized with schema and seed data from `init.sql` when PostgreSQL starts.

**Reset database:**
```bash
docker compose down -v
docker compose up gm_buddy_postgres --build
```

**Access pgAdmin:**
1. Navigate to http://localhost:15435
2. Login with credentials from your `.env` file
3. Server connection is pre-configured via `servers.json`

---

## 🧪 Testing

**Backend tests:**
```bash
dotnet test GM_Buddy.sln
```

**Frontend tests:**
```bash
cd GM_Buddy.React
npm run test           # Interactive mode
npm run test:run       # Run once
npm run test:coverage  # With coverage
```

---

## 🔧 Troubleshooting

See [CONFIGURATION.md](./CONFIGURATION.md) for detailed troubleshooting.

**Common issues:**

### Port Conflicts
```bash
# Check what's using the ports
lsof -i :3000   # Frontend
lsof -i :8080   # Backend
lsof -i :15432  # PostgreSQL
```

### Database Connection
- Verify `POSTGRES_PASSWORD` is set in `.env`
- Ensure PostgreSQL healthcheck passes: `docker compose ps`

### Frontend Can't Connect to Backend
- Check `VITE_API_URL` in `.env` files
- Verify backend is running: `curl http://localhost:8080`

---

## 📝 License

[Add your license information here]

---

## 🤝 Contributing

Contributions are welcome! Please open an issue or submit a pull request.

---

## 📞 Support

For questions or issues, please open a GitHub issue.
