# GM Buddy

A full-stack application for Game Masters to manage NPCs, campaigns, and game systems.

## Tech Stack

- **Backend**: .NET 9 (ASP.NET Core)
- **Frontend**: React + Vite
- **Database**: PostgreSQL
- **Authentication**: JWT with RSA signing keys
- **Container Orchestration**: Docker Compose
- **D&D 5e Data**: [dnd5eapi.co](https://www.dnd5eapi.co) (SRD API integration)

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Initial Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/nwolke/GM_Buddy.git
   cd GM_Buddy
   ```

2. **Create local environment file**
   ```bash
   cp .env.example .env
   ```
   Then edit `.env` with your local values:
   ```env
   POSTGRES_PASSWORD=your_secure_password
   PGADMIN_DEFAULT_EMAIL=your_email@example.com
   PGADMIN_DEFAULT_PASSWORD=your_secure_password
   PWD=/path/to/your/postgres
   ```

3. **Trust .NET dev certificates** (for HTTPS in local development)
   ```bash
   dotnet dev-certs https --trust
   ```

4. **Install client dependencies**
   ```bash
   cd gm_buddy.client
   npm install
   ```

---

## Running the Application

### Option 1: Docker Compose (Full Stack)

Start all services (PostgreSQL, pgAdmin, Authorization, API, Client):

```bash
docker-compose up --build
```

**Access points:**
- Client: http://localhost:49505
- API Server: https://localhost:5001
- Authorization Server: https://localhost:7279
- pgAdmin: http://localhost:15435
- PostgreSQL: localhost:15432

### Option 2: Local Development (Recommended)

Run services individually for easier debugging:

1. **Start PostgreSQL** (via Docker):
   ```bash
   docker-compose up gm_buddy_postgres gm_buddy_pgadmin
   ```

2. **Start Authorization Server** (Visual Studio or CLI):
   ```bash
   cd GM_Buddy.Authorization
   dotnet run --launch-profile https
   ```
   Runs on https://localhost:7279

3. **Start API Server** (Visual Studio or CLI):
   ```bash
   cd GM_Buddy.Server
   dotnet run --launch-profile https
   ```
   Runs on https://localhost:5001

4. **Start Client Dev Server**:
   ```bash
   cd gm_buddy.client
   npm run dev
   ```
   Runs on http://localhost:49505 (or https if certs available)

See [DEV_SETUP.md](./DEV_SETUP.md) for detailed port configuration.

---

## Database Management

### Initialize Database

The database schema and seed data are automatically applied when PostgreSQL starts via `init.sql`.

To manually reset the database:
```bash
docker-compose down -v
docker-compose up gm_buddy_postgres
```

### Access pgAdmin

1. Navigate to http://localhost:15435
2. Login with credentials from your `.env` file
3. Server connection is pre-configured via `servers.json`

---

## Project Structure

```
GM_Buddy/
??? GM_Buddy.Server/              # Main API (NPC endpoints, game logic)
??? GM_Buddy.Authorization/       # JWT authentication service
??? GM_Buddy.Business/            # Business logic & factories
??? GM_Buddy.Data/                # Data access layer (Dapper)
??? GM_Buddy.Contracts/           # Shared DTOs, interfaces, models
??? GM_Buddy.Business.UnitTests/  # Unit tests
??? gm_buddy.client/              # React frontend (Vite)
??? init.sql                      # Database schema & seed data
??? docker-compose.yml            # Container orchestration
??? .env.example                  # Environment template
```

---

## Configuration

### Environment Variables

| File | Purpose | Tracked in Git? |
|------|---------|-----------------|
| `.env.example` | Template with placeholders | ? Yes |
| `.env` | Your actual secrets | ? No (gitignored) |
| `gm_buddy.client/.env.development` | Vite API endpoints | ? Yes |
| `gm_buddy.client/.env.local` | Local overrides | ? No |

### CORS Configuration

Both servers allow requests from:
- `https://localhost:49505` (HTTPS Vite)
- `http://localhost:49505` (HTTP Vite fallback)

Adjust in `Program.cs` files if deploying to different origins.

---

## Testing

Run unit tests:
```bash
dotnet test
```

Run specific test project:
```bash
cd GM_Buddy.Business.UnitTests
dotnet test
```

---

## Common Issues

### CORS Errors
- Ensure both servers are running
- Verify Vite is on port 49505
- Check `UseCors()` comes before `UseAuthentication()` in `Program.cs`

### SSL Certificate Issues
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Port Conflicts
Check if ports are in use:
```bash
# Windows PowerShell
Get-NetTCPConnection -LocalPort 5001,7279,49505,15432,15435 | Select-Object LocalPort, State, OwningProcess
```

---

## D&D 5e API Integration

GM Buddy integrates with the [D&D 5th Edition SRD API](https://www.dnd5eapi.co) to provide access to official D&D 5e System Reference Document content.

**Features:**
- Access to spells, monsters, classes, races, equipment, and more
- TypeScript type definitions for all API responses
- Client-side service with error handling and logging
- Example code and documentation in `/GM_Buddy.React/src/services/`

**Documentation:**
- See [DND_API_INTEGRATION.md](/GM_Buddy.React/src/services/DND_API_INTEGRATION.md) for usage guide
- Example code: [dnd5eApiExamples.ts](/GM_Buddy.React/src/services/dnd5eApiExamples.ts)

**Legal:**
- Uses only SRD content licensed under OGL v1.0a
- Safe for commercial use with proper attribution
- No proprietary D&D content included

---

## Contributing

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Make your changes and commit: `git commit -m "Add feature"`
3. Push to your branch: `git push origin feature/your-feature`
4. Open a Pull Request

---

## License

[Specify your license here]

---

## Support

For issues or questions, please open an issue on [GitHub](https://github.com/nwolke/GM_Buddy/issues).
