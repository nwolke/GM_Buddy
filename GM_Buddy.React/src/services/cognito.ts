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
// Using a function to allow for dynamic retrieval (useful for testing)
function getConfig() {
  return {
    domain: import.meta.env.VITE_COGNITO_DOMAIN || '',
    clientId: import.meta.env.VITE_COGNITO_CLIENT_ID || '',
    redirectUri: import.meta.env.VITE_COGNITO_REDIRECT_URI || `${window.location.origin}/callback`,
    logoutUri: import.meta.env.VITE_COGNITO_LOGOUT_URI || window.location.origin,
    useCognito: import.meta.env.VITE_USE_COGNITO === 'true',
  };
}

const config = getConfig();

// Debug logging
if (config.useCognito) {
  console.log('? [Cognito Config] Cognito is ENABLED', {
    domain: config.domain,
    clientId: config.clientId ? `${config.clientId.substring(0, 10)}...` : 'NOT SET',
  });
} else {
  console.error('? [Cognito Config] Cognito is DISABLED');
  console.error('   VITE_USE_COGNITO =', import.meta.env.VITE_USE_COGNITO);
  console.error('   Expected: "true" (as a string, no quotes in .env file)');
  console.error('   ??  You need to RESTART the Vite dev server!');
  console.error('   1. Press Ctrl+C to stop npm run dev');
  console.error('   2. Run: npm run dev');
  console.error('   3. Refresh the browser');
}

// Token storage keys
const TOKEN_STORAGE_KEY = 'gm_buddy_tokens';
const PKCE_STORAGE_KEY = 'gm_buddy_pkce_verifier';

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
 * Generate a cryptographically random string for PKCE
 */
function generateRandomString(length: number): string {
  const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
  const randomValues = new Uint8Array(length);
  crypto.getRandomValues(randomValues);
  return Array.from(randomValues)
    .map((v) => charset[v % charset.length])
    .join('');
}

/**
 * Generate SHA256 hash and base64url encode for PKCE
 */
async function sha256(plain: string): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(plain);
  const hash = await crypto.subtle.digest('SHA-256', data);
  return base64UrlEncode(hash);
}

/**
 * Base64URL encode (without padding)
 */
function base64UrlEncode(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (let i = 0; i < bytes.length; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary)
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
}

/**
 * Redirect to Cognito Hosted UI for login with PKCE
 */
export async function redirectToLogin(): Promise<void> {
  if (!isCognitoEnabled()) {
    console.warn('Cognito is not configured. Using demo mode.');
    return;
  }

  // Generate PKCE code verifier and challenge
  const codeVerifier = generateRandomString(128);
  const codeChallenge = await sha256(codeVerifier);

  // Store code verifier for token exchange
  localStorage.setItem(PKCE_STORAGE_KEY, codeVerifier);

  const loginUrl = new URL(`https://${config.domain}/login`);
  loginUrl.searchParams.set('client_id', config.clientId);
  loginUrl.searchParams.set('response_type', 'code');
  loginUrl.searchParams.set('scope', 'openid email profile');
  loginUrl.searchParams.set('redirect_uri', config.redirectUri);
  loginUrl.searchParams.set('code_challenge', codeChallenge);
  loginUrl.searchParams.set('code_challenge_method', 'S256');

  console.log('[Cognito] Redirecting to login with PKCE');
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
 * Exchange authorization code for tokens using PKCE
 */
async function exchangeCodeForTokens(code: string): Promise<CognitoTokens | null> {
  const tokenUrl = `https://${config.domain}/oauth2/token`;

  // Retrieve the code verifier from storage
  const codeVerifier = localStorage.getItem(PKCE_STORAGE_KEY);
  if (!codeVerifier) {
    console.error('PKCE code verifier not found. Cannot exchange token.');
    return null;
  }

  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    client_id: config.clientId,
    code: code,
    redirect_uri: config.redirectUri,
    code_verifier: codeVerifier,  // PKCE code verifier
  });

  // Clean up the stored verifier
  localStorage.removeItem(PKCE_STORAGE_KEY);

  try {
    console.log('[Cognito] Exchanging code for tokens with PKCE...');
    const response = await fetch(tokenUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: body.toString(),
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('? Token exchange failed:', response.status, errorText);
      return null;
    }

    const data = await response.json();
    console.log('? Token exchange successful');

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
 * Refresh tokens using the refresh token
 * Returns new tokens or null if refresh fails
 */
export async function refreshTokens(): Promise<CognitoTokens | null> {
  // Get config dynamically to support testing
  const currentConfig = getConfig();
  const domain = currentConfig.domain;
  const clientId = currentConfig.clientId;
  
  if (!domain || !clientId) {
    console.warn('Cognito domain or client ID not configured. Cannot refresh tokens.');
    return null;
  }

  const stored = localStorage.getItem(TOKEN_STORAGE_KEY);
  if (!stored) {
    console.log('No tokens to refresh');
    return null;
  }

  let currentTokens: CognitoTokens;
  try {
    currentTokens = JSON.parse(stored) as CognitoTokens;
  } catch {
    console.error('Failed to parse stored tokens');
    clearTokens();
    return null;
  }

  if (!currentTokens.refreshToken) {
    console.error('No refresh token available');
    clearTokens();
    return null;
  }

  const tokenUrl = `https://${domain}/oauth2/token`;
  const body = new URLSearchParams({
    grant_type: 'refresh_token',
    client_id: clientId,
    refresh_token: currentTokens.refreshToken,
  });

  try {
    console.log('[Cognito] Refreshing tokens...');
    const response = await fetch(tokenUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: body.toString(),
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('❌ Token refresh failed:', response.status, errorText);
      clearTokens();
      return null;
    }

    const data = await response.json();
    console.log('✅ Token refresh successful');

    const newTokens: CognitoTokens = {
      accessToken: data.access_token,
      idToken: data.id_token,
      refreshToken: data.refresh_token || currentTokens.refreshToken, // Use new refresh token if provided, else keep existing
      expiresAt: Date.now() + (data.expires_in * 1000),
    };

    saveTokens(newTokens);
    return newTokens;
  } catch (err) {
    console.error('Token refresh error:', err);
    clearTokens();
    return null;
  }
}

/**
 * Load tokens from localStorage
 * Automatically refreshes tokens if they expire in less than 5 minutes
 */
export async function loadTokens(): Promise<CognitoTokens | null> {
  const stored = localStorage.getItem(TOKEN_STORAGE_KEY);
  if (!stored) return null;

  try {
    const tokens = JSON.parse(stored) as CognitoTokens;
    
    // Check if tokens are expired
    if (tokens.expiresAt && tokens.expiresAt < Date.now()) {
      console.log('Tokens expired, attempting refresh...');
      return await refreshTokens();
    }

    // Check if tokens expire in less than 5 minutes (300000 ms)
    const fiveMinutes = 5 * 60 * 1000;
    if (tokens.expiresAt && tokens.expiresAt < Date.now() + fiveMinutes) {
      console.log('Tokens expiring soon, proactively refreshing...');
      const refreshed = await refreshTokens();
      // If refresh fails, return current tokens (they're still valid for now)
      return refreshed || tokens;
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
export async function getAccessToken(): Promise<string | null> {
  const tokens = await loadTokens();
  return tokens?.accessToken || null;
}

/**
 * Get the current ID token for API calls
 * ID tokens contain user identity claims (sub, email, etc.)
 * Use this instead of access token for ASP.NET Core JWT authentication
 */
export async function getIdToken(): Promise<string | null> {
  const tokens = await loadTokens();
  return tokens?.idToken || null;
}

/**
 * Get the current user from stored tokens
 */
export async function getCurrentUser(): Promise<CognitoUser | null> {
  const tokens = await loadTokens();
  if (!tokens?.idToken) return null;
  return parseIdToken(tokens.idToken);
}

/**
 * Check if user is authenticated
 */
export async function isAuthenticated(): Promise<boolean> {
  const tokens = await loadTokens();
  return !!tokens;
}
