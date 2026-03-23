# GM Buddy

A web app for tabletop RPG game masters to manage campaigns, track NPCs and player characters, and visualize the relationships between them.

**Live:** [gmbuddy.nwolke.com](https://gmbuddy.nwolke.com)

---

## Features

- **Campaigns** — Create and manage multiple TTRPG campaigns
- **NPCs & PCs** — Track non-player and player characters with notes and details
- **Relationships** — Define and visualize connections between characters and organizations
- **Organizations** — Model factions, guilds, and other groups
- **Relationship graph** — Interactive force-directed graph showing character and organization connections
- **Authentication** — Secure sign-in via AWS Cognito (OAuth 2.0 / PKCE)

---

## Tech Stack

### Frontend
- **React 18** + **TypeScript** — UI framework
- **Vite** — Build tool and dev server
- **Tailwind CSS v4** — Utility-first styling
- **Radix UI** — Accessible headless component primitives
- **react-force-graph-2d** — Force-directed relationship graph visualization
- **React Router v6** — Client-side routing
- **Axios** — HTTP client
- **Vitest** + **Testing Library** — Unit and component testing

### Backend
- **ASP.NET Core 9.0** — REST API
- **Dapper** — Lightweight SQL micro-ORM
- **PostgreSQL** — Primary database
- **Npgsql** — PostgreSQL .NET driver
- **JWT Bearer auth** — Token validation against AWS Cognito

### Infrastructure
- **AWS Cognito** — User authentication (OAuth 2.0 / PKCE)
- **AWS S3 + CloudFront** — Frontend hosting and CDN
- **AWS Elastic Beanstalk + ECR** — Backend container hosting
- **Docker** — Multi-stage container builds for both frontend and backend
- **GitHub Actions** — CI/CD for build, test, and deployment

---

## Project Structure

```
GM_Buddy/
├── GM_Buddy.Server/          # ASP.NET Core Web API
├── GM_Buddy.Business/        # Business logic layer
├── GM_Buddy.Data/            # Data access (Dapper repositories)
├── GM_Buddy.Contracts/       # Shared DTOs and interfaces
├── GM_Buddy.Business.UnitTests/
├── GM_Buddy.Business.ComponentTests/
├── GM_Buddy.ServiceDefaults/ # Shared .NET Aspire service config
├── GM_Buddy.AppHost/         # .NET Aspire app host
├── GM_Buddy.React/           # React frontend (Vite)
├── migrations/               # SQL database migrations
└── docs/                     # Additional documentation
```

---

## Local Development

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (for the database, or bring your own PostgreSQL)

### 1. Clone and configure environment

```bash
git clone https://github.com/nwolke/GM_Buddy.git
cd GM_Buddy
cp .env.example .env
```

Edit `.env` with your local values (see [Environment Variables](#environment-variables) below).

### 2. Start the database

```bash
docker compose up -d
```

Or point `appsettings.Development.json` at an existing PostgreSQL instance and run migrations from the `migrations/` directory.

### 3. Run the backend

```bash
cd GM_Buddy.Server
dotnet run
```

API is available at `http://localhost:8080`. Swagger UI at `http://localhost:8080/swagger`.

### 4. Run the frontend

```bash
cd GM_Buddy.React
npm install --legacy-peer-deps
npm run dev
```

Frontend is available at `http://localhost:3000`.

---

## Environment Variables

Copy `.env.example` to `.env` and fill in the following:

| Variable | Description |
|---|---|
| `POSTGRES_USER` | PostgreSQL username |
| `POSTGRES_PASSWORD` | PostgreSQL password |
| `POSTGRES_DB` | Database name (default: `gm_buddy`) |
| `COGNITO_REGION` | AWS region for Cognito (e.g. `us-west-2`) |
| `COGNITO_USER_POOL_ID` | Cognito User Pool ID |
| `COGNITO_CLIENT_ID` | Cognito App Client ID |
| `COGNITO_DOMAIN` | Cognito hosted UI domain prefix |
| `VITE_API_URL` | Backend API URL for the frontend |
| `VITE_USE_COGNITO` | Set to `false` to bypass auth in local dev |
| `VITE_COGNITO_DOMAIN` | Full Cognito hosted UI domain |
| `VITE_COGNITO_CLIENT_ID` | Cognito client ID (frontend) |
| `VITE_COGNITO_REDIRECT_URI` | OAuth callback URL |
| `VITE_COGNITO_LOGOUT_URI` | Post-logout redirect URL |

Backend configuration is also read from `GM_Buddy.Server/appsettings.json` and environment-specific overrides (`appsettings.Development.json`, etc.).

---

## Running Tests

### Backend

```bash
dotnet test
```

### Frontend

```bash
cd GM_Buddy.React

npm run test        # watch mode
npm run test:run    # single run
npm run test:coverage
```

---

## Deployment

Deployments are triggered manually via GitHub Actions workflows:

- **Frontend** (`deploy-frontend.yml`) — Builds the React app and syncs to S3, then invalidates the CloudFront distribution.
- **Backend** (`deploy-backend.yml`) — Builds a production Docker image, pushes to Amazon ECR, and deploys to Elastic Beanstalk.

CI runs automatically on pushes and PRs to `main` and `develop` via `build-and-test.yml`.

---

## Contact

Built by Nathan Wolke.

- Email: [gmbuddy@outlook.com](mailto:gmbuddy@outlook.com)
- GitHub: [github.com/nwolke](https://github.com/nwolke)
