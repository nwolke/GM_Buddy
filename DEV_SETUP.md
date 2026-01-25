# GM Buddy - Local Development Setup

## Server Ports (HTTPS mode)

| Service | HTTPS Port | HTTP Port | Launch Profile |
|---------|-----------|-----------|----------------|
| **Authorization Server** | 7279 | 5079 | `https` profile in launchSettings.json |
| **Main API Server** | 5001 | 5000 | `https` profile in launchSettings.json |
| **Vite Dev Server** | 49505 | 49505 | Configured in vite.config.js |

## Starting the Application

### Option 1: Run Individual Services (Recommended for dev)

1. **Start Authorization Server** (in Visual Studio):
   - Set `GM_Buddy.Authorization` as startup project
   - Ensure the launch profile is set to `https`
   - Press F5 or click Run
   - Verify it's running at `https://localhost:7279`

2. **Start Main API Server** (in Visual Studio):
   - Set `GM_Buddy.Server` as startup project
   - Ensure the launch profile is set to `https`
   - Press F5 or click Run
   - Verify it's running at `https://localhost:5001`

3. **Start Vite Dev Server** (in terminal):
   ```bash
   cd gm_buddy.client
   npm run dev
   ```
   - Vite will try to use HTTPS on port 49505
   - If HTTPS fails, it will fall back to HTTP

### Option 2: Docker Compose

```bash
docker-compose up
```

## CORS Configuration

Both servers are configured to accept requests from:
- `https://localhost:49505` (HTTPS Vite)
- `http://localhost:49505` (HTTP Vite fallback)

## Environment Variables

### Client (.env.development)

```env
VITE_AUTH_API_BASE_URL=https://localhost:7279
VITE_API_BASE_URL=https://localhost:5001
```

## Troubleshooting CORS

If you still see CORS errors:

1. **Check server console output** - verify both servers started on the correct ports
2. **Check browser console** - note the exact origin in the error message
3. **Verify middleware order** in Program.cs:
   ```
   UseHttpsRedirection() 
   ? UseCors("AllowSpecificOrigins") 
   ? UseAuthentication() 
   ? UseAuthorization() 
   ? MapControllers()
   ```
4. **Hard refresh browser** (Ctrl+Shift+R) to clear cache
5. **Check Vite console** - ensure proxy is configured and running

## Testing the Setup

1. Open browser to `http://localhost:49505` (or `https://localhost:49505`)
2. Click "NPCs" - should fetch from `https://localhost:5001/Npcs?account_id=1`
3. Click "Login" - should post to `https://localhost:7279/login`
4. Check browser Network tab to verify requests are succeeding

## SSL Certificate Issues

If you get SSL certificate errors:

```bash
# Trust the .NET dev certificate
dotnet dev-certs https --trust

# For Vite client certificate
cd gm_buddy.client
npm run dev
# Follow prompts to trust the generated certificate
```
