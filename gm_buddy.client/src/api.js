// Centralized API configuration
// Reads base URLs from Vite environment variables (set in .env.development, .env.production, etc.)

// Authorization server base URL (login, registration, JWT endpoints)
export const AUTH_API_BASE = import.meta.env.VITE_AUTH_API_BASE_URL ?? 'https://localhost:7256';

// Main API server base URL (NPC, game system, etc.)
export const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001';

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
