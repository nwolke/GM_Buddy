// Centralized API configuration
// In development, use relative URLs so Vite's proxy can handle requests (avoids CORS)
// In production, these would be set to actual backend URLs via environment variables

// Use relative paths for proxy unless explicitly set to use absolute URLs
// This ensures the Vite dev server proxy is used in development
const useProxy = import.meta.env.VITE_USE_PROXY !== 'false';

const explicitAuthBase = import.meta.env.VITE_AUTH_API_BASE_URL;
const explicitApiBase = import.meta.env.VITE_API_BASE_URL;

// For local development: Use relative paths (Vite proxy handles routing)
// For production: Set VITE_AUTH_API_BASE_URL and VITE_API_BASE_URL in .env.production
export const AUTH_API_BASE = explicitAuthBase
    ? explicitAuthBase
    : (useProxy
        ? '' // Empty string for relative URLs (proxy intercepts)
        : 'https://localhost:7279');

export const API_BASE = explicitApiBase
    ? explicitApiBase
    : (useProxy
        ? '' // Empty string for relative URLs (proxy intercepts)
        : 'https://localhost:5001');

// Storage key for token (must match AuthContext)
const TOKEN_KEY = 'gm_buddy_token';

/**
 * Get the current auth token from localStorage
 */
export function getAuthToken() {
    return localStorage.getItem(TOKEN_KEY);
}

/**
 * Check if user is currently authenticated (has non-expired token)
 */
export function isAuthenticated() {
    const token = getAuthToken();
    if (!token) return false;
    
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const payload = JSON.parse(atob(base64));
        return payload.exp * 1000 > Date.now();
    } catch {
        return false;
    }
}

/**
 * Helper function for authenticated API calls
 * Automatically includes the Authorization header with Bearer token
 * 
 * @param {string} url - The API endpoint URL
 * @param {object} options - Fetch options (method, body, etc.)
 * @returns {Promise<any>} - The JSON response
 * @throws {Error} - If request fails or user is not authenticated
 * 
 * @example
 * // GET request
 * const npcs = await authFetch(`${API_BASE}/Npcs?account_id=${accountId}`);
 * 
 * @example
 * // POST request
 * const result = await authFetch(`${API_BASE}/Npcs`, {
 *     method: 'POST',
 *     body: JSON.stringify(npcData)
 * });
 */
export async function authFetch(url, options = {}) {
    const token = getAuthToken();
    
    if (!token) {
        throw new Error('Not authenticated. Please log in.');
    }

    const response = await fetch(url, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            ...options.headers,
        },
    });

    // Handle 401 Unauthorized - token expired or invalid
    if (response.status === 401) {
        // Clear the invalid token
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem('gm_buddy_user');
        throw new Error('Session expired. Please log in again.');
    }

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `HTTP ${response.status}: ${response.statusText}`);
    }

    // Handle empty responses
    const text = await response.text();
    return text ? JSON.parse(text) : null;
}

/**
 * Helper function for standard fetch with JSON headers (no auth required)
 * Use this for public endpoints
 */
export async function apiFetch(url, options = {}) {
    const { authToken, headers = {}, ...rest } = options;
    const response = await fetch(url, {
        ...rest,
        headers: {
            'Content-Type': 'application/json',
            ...headers,
            ...(authToken ? { Authorization: `Bearer ${authToken}` } : {})
        },
    });
    if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    return response.json();
}

// Debug logging (only in development)
if (import.meta.env.DEV) {
    console.log('[API Config] Use Proxy:', useProxy);
    console.log('[API Config] AUTH_API_BASE:', AUTH_API_BASE);
    console.log('[API Config] API_BASE:', API_BASE);
}
