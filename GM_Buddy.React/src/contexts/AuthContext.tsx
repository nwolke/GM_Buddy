import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, AuthState } from '@/types/npc';
import { accountApi } from '@/services/api';
import * as cognito from '@/services/cognito';

interface AuthContextType extends AuthState {
  login: (cognitoSub: string, email: string) => Promise<void>;
  loginWithCognito: () => void;
  logout: () => void;
  isCognitoMode: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// For demo purposes, we'll use localStorage to simulate authentication
// In production, this would integrate with AWS Cognito Hosted UI
const STORAGE_KEY = 'gm_buddy_auth';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [authState, setAuthState] = useState<AuthState>({
    isAuthenticated: false,
    user: null,
    loading: true,
  });

  const isCognitoMode = cognito.isCognitoEnabled();
  
  if (isCognitoMode) {
    console.log('? [AuthContext] Cognito mode: ENABLED');
  } else {
    console.warn('??  [AuthContext] Cognito is not configured. Use demo login instead.');
    console.warn('    If you set up .env.local, you need to RESTART Vite dev server!');
  }

  useEffect(() => {
    const initAuth = async () => {
      // Check if this is a Cognito callback
      if (window.location.pathname === '/callback' && isCognitoMode) {
        console.log('[AuthContext] Handling Cognito callback...');
        const cognitoUser = await cognito.handleCallback();
        if (cognitoUser) {
          try {
            // Sync with backend (cognitoSub is extracted from JWT token on backend)
            const syncResponse = await accountApi.syncAccount(cognitoUser.email);
            const user: User = {
              cognitoSub: cognitoUser.sub,
              email: cognitoUser.email,
              accountId: syncResponse.accountId,
            };
            localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
            setAuthState({ isAuthenticated: true, user, loading: false });
            // Clear the URL params
            window.history.replaceState({}, '', '/');
            return;
          } catch (err) {
            console.error('[AuthContext] Failed to sync Cognito user:', err);
          }
        }
      }

      // Check for existing session
      const storedAuth = localStorage.getItem(STORAGE_KEY);
      if (storedAuth) {
        try {
          const user = JSON.parse(storedAuth) as User;
          
          // Validate tokens and refresh if necessary
          const tokens = await cognito.loadTokens();
          if (!tokens) {
            // Tokens are invalid or expired and couldn't be refreshed
            console.log('[AuthContext] Tokens expired and refresh failed, clearing session...');
            localStorage.removeItem(STORAGE_KEY);
            cognito.clearTokens();
            setAuthState({ isAuthenticated: false, user: null, loading: false });
            return;
          }
          
          console.log('[AuthContext] Restored session for user:', user.email, 'accountId:', user.accountId);
          setAuthState({ isAuthenticated: true, user, loading: false });
        } catch {
          console.log('[AuthContext] Invalid stored auth data, clearing...');
          localStorage.removeItem(STORAGE_KEY);
          setAuthState({ isAuthenticated: false, user: null, loading: false });
        }
      } else {
        console.log('[AuthContext] No stored session found');
        setAuthState({ isAuthenticated: false, user: null, loading: false });
      }
    };

    initAuth();
  }, [isCognitoMode]);

  // Demo/manual login (for development without Cognito)
  // NOTE: This will NOT work with the secured endpoints that require JWT authentication
  // To use the app in development, either:
  // 1. Configure real Cognito authentication (recommended)
  // 2. Use a development bypass (requires backend changes)
  const login = async (cognitoSub: string, email: string) => {
    console.log('[AuthContext] Demo login with cognitoSub:', cognitoSub, 'email:', email);
    console.warn('[AuthContext] Demo login will fail - backend requires JWT authentication');
    
    // This will fail because the backend now requires a valid JWT token with 'sub' claim
    // The cognitoSub parameter is no longer accepted from request body for security reasons
    throw new Error('Demo login is not supported. Please configure Cognito authentication.');
  };

  // Cognito Hosted UI login
  const loginWithCognito = () => {
    if (isCognitoMode) {
      cognito.redirectToLogin();
    } else {
      console.warn('[AuthContext] Cognito is not configured. Use demo login instead.');
    }
  };

  const logout = () => {
    localStorage.removeItem(STORAGE_KEY);
    cognito.clearTokens();
    
    if (isCognitoMode) {
      cognito.redirectToLogout();
    } else {
      setAuthState({
        isAuthenticated: false,
        user: null,
        loading: false,
      });
    }
  };

  return (
    <AuthContext.Provider value={{ ...authState, login, loginWithCognito, logout, isCognitoMode }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
