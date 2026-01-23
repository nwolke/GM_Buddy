/**
 * Cognito Hosted UI Service
 * 
 * This service handles authentication using AWS Cognito Hosted UI.
 * It uses the OAuth2/OIDC Authorization Code flow.
 * 
 * Flow:
 * 1. User clicks "Login" -> redirected to Cognito Hosted UI
 * 2. User logs in on Cognito
 * 3. Cognito redirects back with authorization code
 * 4. We exchange the code for tokens
 * 5. We use the tokens to authenticate API calls
 */

// Cognito configuration from environment variables
const config = {
  domain: import.meta.env.VITE_COGNITO_DOMAIN || '',
  clientId: import.meta.env.VITE_COGNITO_CLIENT_ID || '',
  redirectUri: import.meta.env.VITE_COGNITO_REDIRECT_URI || `${window.location.origin}/callback`,
  logoutUri: import.meta.env.VITE_COGNITO_LOGOUT_URI || window.location.origin,
  useCognito: import.meta.env.VITE_USE_COGNITO === 'true',
};

// Token storage keys
const TOKEN_STORAGE_KEY = 'gm_buddy_tokens';

interface CognitoTokens {
  accessToken: string;
  idToken: string;
  refreshToken: string;
  expiresAt: number;
}

interface CognitoUser {
  sub: string;
  email: string;
  name?: string;
}

/**
 * Check if Cognito is configured and enabled
 */
export function isCognitoEnabled(): boolean {
  return config.useCognito && !!config.domain && !!config.clientId;
}

/**
 * Redirect to Cognito Hosted UI for login
 */
export function redirectToLogin(): void {
  if (!isCognitoEnabled()) {
    console.warn('Cognito is not configured. Using demo mode.');
    return;
  }

  const loginUrl = new URL(`https://${config.domain}/login`);
  loginUrl.searchParams.set('client_id', config.clientId);
  loginUrl.searchParams.set('response_type', 'code');
  loginUrl.searchParams.set('scope', 'openid email profile');
  loginUrl.searchParams.set('redirect_uri', config.redirectUri);

  window.location.href = loginUrl.toString();
}

/**
 * Redirect to Cognito Hosted UI for logout
 */
export function redirectToLogout(): void {
  if (!isCognitoEnabled()) {
    clearTokens();
    return;
  }

  clearTokens();

  const logoutUrl = new URL(`https://${config.domain}/logout`);
  logoutUrl.searchParams.set('client_id', config.clientId);
  logoutUrl.searchParams.set('logout_uri', config.logoutUri);

  window.location.href = logoutUrl.toString();
}

/**
 * Handle the callback from Cognito after login
 * Extracts the authorization code and exchanges it for tokens
 */
export async function handleCallback(): Promise<CognitoUser | null> {
  const urlParams = new URLSearchParams(window.location.search);
  const code = urlParams.get('code');
  const error = urlParams.get('error');

  if (error) {
    console.error('Cognito login error:', error, urlParams.get('error_description'));
    return null;
  }

  if (!code) {
    console.log('No authorization code in callback');
    return null;
  }

  try {
    const tokens = await exchangeCodeForTokens(code);
    if (tokens) {
      saveTokens(tokens);
      return parseIdToken(tokens.idToken);
    }
  } catch (err) {
    console.error('Failed to exchange code for tokens:', err);
  }

  return null;
}

/**
 * Exchange authorization code for tokens
 */
async function exchangeCodeForTokens(code: string): Promise<CognitoTokens | null> {
  const tokenUrl = `https://${config.domain}/oauth2/token`;

  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    client_id: config.clientId,
    code: code,
    redirect_uri: config.redirectUri,
  });

  try {
    const response = await fetch(tokenUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: body.toString(),
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Token exchange failed:', response.status, errorText);
      return null;
    }

    const data = await response.json();

    return {
      accessToken: data.access_token,
      idToken: data.id_token,
      refreshToken: data.refresh_token,
      expiresAt: Date.now() + (data.expires_in * 1000),
    };
  } catch (err) {
    console.error('Token exchange error:', err);
    return null;
  }
}

/**
 * Parse the ID token to extract user info
 */
function parseIdToken(idToken: string): CognitoUser | null {
  try {
    const payload = idToken.split('.')[1];
    const decoded = JSON.parse(atob(payload));

    return {
      sub: decoded.sub,
      email: decoded.email,
      name: decoded.name || decoded.email,
    };
  } catch (err) {
    console.error('Failed to parse ID token:', err);
    return null;
  }
}

/**
 * Save tokens to localStorage
 */
function saveTokens(tokens: CognitoTokens): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, JSON.stringify(tokens));
}

/**
 * Load tokens from localStorage
 */
export function loadTokens(): CognitoTokens | null {
  const stored = localStorage.getItem(TOKEN_STORAGE_KEY);
  if (!stored) return null;

  try {
    const tokens = JSON.parse(stored) as CognitoTokens;
    
    // Check if tokens are expired
    if (tokens.expiresAt && tokens.expiresAt < Date.now()) {
      console.log('Tokens expired, clearing...');
      clearTokens();
      return null;
    }

    return tokens;
  } catch {
    return null;
  }
}

/**
 * Clear tokens from localStorage
 */
export function clearTokens(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
}

/**
 * Get the current access token for API calls
 */
export function getAccessToken(): string | null {
  const tokens = loadTokens();
  return tokens?.accessToken || null;
}

/**
 * Get the current user from stored tokens
 */
export function getCurrentUser(): CognitoUser | null {
  const tokens = loadTokens();
  if (!tokens?.idToken) return null;
  return parseIdToken(tokens.idToken);
}

/**
 * Check if user is authenticated
 */
export function isAuthenticated(): boolean {
  return !!loadTokens();
}
