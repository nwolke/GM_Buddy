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

  useEffect(() => {
    const initAuth = async () => {
      // Check if this is a Cognito callback
      if (window.location.pathname === '/callback' && isCognitoMode) {
        console.log('[AuthContext] Handling Cognito callback...');
        const cognitoUser = await cognito.handleCallback();
        if (cognitoUser) {
          try {
            // Sync with backend
            const syncResponse = await accountApi.syncAccount(cognitoUser.sub, cognitoUser.email);
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
  const login = async (cognitoSub: string, email: string) => {
    console.log('[AuthContext] Demo login with cognitoSub:', cognitoSub, 'email:', email);
    try {
      // Sync account with backend (creates if doesn't exist, returns account info)
      const syncResponse = await accountApi.syncAccount(cognitoSub, email);
      console.log('[AuthContext] Sync response:', syncResponse);
      
      const user: User = {
        cognitoSub,
        email,
        accountId: syncResponse.accountId,
      };

      console.log('[AuthContext] Login successful, accountId:', user.accountId);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
      setAuthState({
        isAuthenticated: true,
        user,
        loading: false,
      });
    } catch (error) {
      console.error('[AuthContext] Login failed:', error);
      throw error;
    }
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
