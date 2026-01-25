import { createContext, useContext, useState, useEffect, useCallback, useMemo } from 'react';
import { AUTH_API_BASE } from '../api';

// Create the Auth Context
const AuthContext = createContext(null);

// Helper to decode JWT payload (without verification - server still validates)
function parseJwt(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        return JSON.parse(jsonPayload);
    } catch (e) {
        console.error('Failed to parse JWT:', e);
        return null;
    }
}

// Check if token is expired
function isTokenExpired(token) {
    const payload = parseJwt(token);
    if (!payload || !payload.exp) return true;
    // exp is in seconds, Date.now() is in milliseconds
    return payload.exp * 1000 < Date.now();
}

// Storage keys
const TOKEN_KEY = 'gm_buddy_token';
const USER_KEY = 'gm_buddy_user';

/**
 * AuthProvider component - wraps your app to provide auth state
 */
export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [token, setToken] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Initialize auth state from localStorage on mount
    useEffect(() => {
        const storedToken = localStorage.getItem(TOKEN_KEY);
        const storedUser = localStorage.getItem(USER_KEY);

        if (storedToken && !isTokenExpired(storedToken)) {
            setToken(storedToken);
            if (storedUser) {
                try {
                    setUser(JSON.parse(storedUser));
                } catch (e) {
                    console.error('Failed to parse stored user:', e);
                }
            }
        } else {
            // Clear expired token
            localStorage.removeItem(TOKEN_KEY);
            localStorage.removeItem(USER_KEY);
        }
        setLoading(false);
    }, []);

    // Login function
    const login = useCallback(async (email, password) => {
        setError(null);
        setLoading(true);

        try {
            const response = await fetch(`${AUTH_API_BASE}/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ 
                    email, 
                    password,
                    clientId: 'gm-buddy-web' // Default web client identifier
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || `Login failed: ${response.status}`);
            }

            const data = await response.json();
            const accessToken = data.accessToken;

            if (!accessToken) {
                throw new Error('No access token received');
            }

            // Parse user info from token
            const payload = parseJwt(accessToken);
            const userInfo = {
                id: payload?.sub || payload?.id || payload?.account_id,
                email: payload?.email || email,
                name: payload?.name || payload?.first_name || email.split('@')[0],
                roles: payload?.roles || []
            };

            // Store in state and localStorage
            setToken(accessToken);
            setUser(userInfo);
            localStorage.setItem(TOKEN_KEY, accessToken);
            localStorage.setItem(USER_KEY, JSON.stringify(userInfo));

            return { success: true, user: userInfo };
        } catch (err) {
            setError(err.message);
            return { success: false, error: err.message };
        } finally {
            setLoading(false);
        }
    }, []);

    // Logout function
    const logout = useCallback(() => {
        setToken(null);
        setUser(null);
        setError(null);
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
    }, []);

    // Check if user is authenticated
    const isAuthenticated = useMemo(() => {
        return !!token && !isTokenExpired(token);
    }, [token]);

    // Get the current account ID (for API calls)
    const accountId = useMemo(() => {
        return user?.id || null;
    }, [user]);

    // Context value - memoized to prevent unnecessary re-renders
    const value = useMemo(() => ({
        user,
        token,
        loading,
        error,
        isAuthenticated,
        accountId,
        login,
        logout
    }), [user, token, loading, error, isAuthenticated, accountId, login, logout]);

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
}

/**
 * useAuth hook - use this in components to access auth state
 * 
 * @example
 * const { user, isAuthenticated, login, logout, accountId } = useAuth();
 */
export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
}

export default AuthContext;
