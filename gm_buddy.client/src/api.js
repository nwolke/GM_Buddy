// Centralized API configuration
// In development, use relative URLs so Vite's proxy can handle requests (avoids CORS)
// In production, these would be set to actual backend URLs via environment variables

// Use relative paths for proxy unless explicitly set to use absolute URLs
// This ensures the Vite dev server proxy is used in development
const useProxy = import.meta.env.VITE_USE_PROXY !== 'false';

// For local development: Use relative paths (Vite proxy handles routing)
// For production: Set VITE_AUTH_API_BASE_URL and VITE_API_BASE_URL in .env.production
export const AUTH_API_BASE = useProxy
    ? '' // Empty string for relative URLs (proxy intercepts)
    : (import.meta.env.VITE_AUTH_API_BASE_URL ?? 'https://localhost:7279');

export const API_BASE = useProxy
    ? '' // Empty string for relative URLs (proxy intercepts)
    : (import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001');

// Debug logging
console.log('[API Config] Use Proxy:', useProxy);
console.log('[API Config] AUTH_API_BASE:', AUTH_API_BASE);
console.log('[API Config] API_BASE:', API_BASE);
console.log('[API Config] import.meta.env.PROD:', import.meta.env.PROD);
console.log('[API Config] import.meta.env.DEV:', import.meta.env.DEV);
console.log('[API Config] import.meta.env.MODE:', import.meta.env.MODE);

// Helper function for standard fetch with JSON headers
export async function apiFetch(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
         ...options.headers,
    },
    });
    if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    return response.json();
}
