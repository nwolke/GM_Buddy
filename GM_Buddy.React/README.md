
# GM Buddy React Frontend

A React-based frontend for the GM Buddy NPC Management application. This replaces the previous Blazor frontend with a modern React/Vite/TailwindCSS stack.

## Features

- ?? **NPC Management** - Create, edit, and delete NPCs for your TTRPG campaigns
- ?? **Relationship Tracking** - Define and visualize relationships between NPCs
- ??? **Network Graph** - Interactive force-directed graph showing NPC connections
- ?? **Authentication Ready** - Prepared for AWS Cognito integration
- ?? **Docker Support** - Easy deployment with Docker

## Development Setup

### Prerequisites

- Node.js 18+ 
- npm or pnpm
- GM_Buddy.Server running on http://localhost:5000 (for API calls)

### Installation

```bash
cd GM_Buddy.React
npm install
npm run dev
```

The development server will start at http://localhost:3000

### Environment Variables

Copy `.env.development` and configure as needed:

```
VITE_API_URL=http://localhost:5000
```

## Docker Deployment

The application is configured to run in Docker alongside the backend:

```bash
# From the root directory
docker-compose up -d
```

This will:
- Build the React app
- Serve it via nginx on port 3000
- Proxy API requests to the backend server

## Project Structure

```
GM_Buddy.React/
??? src/
?   ??? app/
?   ?   ??? components/     # React components
?   ?   ?   ??? ui/         # shadcn/ui components
?   ?   ?   ??? NPCCard.tsx
?   ?   ?   ??? NPCForm.tsx
?   ?   ?   ??? NPCNetwork.tsx
?   ?   ?   ??? RelationshipManager.tsx
?   ?   ??? App.tsx         # Main app component
?   ??? contexts/           # React contexts (Auth)
?   ??? hooks/              # Custom React hooks
?   ??? services/           # API service layer
?   ??? styles/             # CSS and Tailwind styles
?   ??? types/              # TypeScript type definitions
?   ??? main.tsx            # Entry point
??? Dockerfile              # Production Docker build
??? nginx.conf              # Nginx configuration
??? package.json
??? tsconfig.json
??? vite.config.ts
```

## API Integration

The frontend connects to the GM_Buddy.Server API:

- `GET /api/Npcs` - Fetch NPCs for a user
- `GET /api/Npcs/{id}` - Get single NPC
- `GET /api/Relationships/types` - Get relationship types
- `POST /api/Relationships` - Create relationship

In development, Vite proxies `/api` requests to the backend.
In production, nginx handles the proxying.

## Tech Stack

- **React 18** - UI library
- **Vite** - Build tool
- **TypeScript** - Type safety
- **TailwindCSS 4** - Styling
- **shadcn/ui** - Component library
- **react-force-graph-2d** - Network visualization
- **axios** - HTTP client

  