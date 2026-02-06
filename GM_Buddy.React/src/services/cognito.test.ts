import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import * as cognito from './cognito';

describe('Cognito Service - Token Refresh', () => {
  let originalFetch: typeof global.fetch;

  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    originalFetch = global.fetch;
    
    // Set up environment variables for testing
    vi.stubEnv('VITE_COGNITO_DOMAIN', 'test.auth.us-east-1.amazoncognito.com');
    vi.stubEnv('VITE_COGNITO_CLIENT_ID', 'test-client-id');
    vi.stubEnv('VITE_USE_COGNITO', 'true');
  });

  afterEach(() => {
    global.fetch = originalFetch;
    vi.unstubAllEnvs();
  });

  describe('refreshTokens', () => {
    it('should refresh tokens successfully', async () => {
      // Setup - store tokens with a refresh token
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 60000, // expires in 1 minute
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      // Mock successful refresh response
      global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          access_token: 'new-access-token',
          id_token: 'new-id-token',
          refresh_token: 'new-refresh-token',
          expires_in: 3600, // 1 hour
        }),
      });

      const result = await cognito.refreshTokens();

      expect(result).toBeTruthy();
      expect(result?.accessToken).toBe('new-access-token');
      expect(result?.idToken).toBe('new-id-token');
      expect(result?.refreshToken).toBe('new-refresh-token');
      expect(result?.expiresAt).toBeGreaterThan(Date.now());

      // Verify the new tokens are saved
      const stored = localStorage.getItem('gm_buddy_tokens');
      expect(stored).toBeTruthy();
      const parsed = JSON.parse(stored!);
      expect(parsed.accessToken).toBe('new-access-token');
    });

    it('should handle missing refresh token', async () => {
      // Setup - store tokens without a refresh token
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: '',
        expiresAt: Date.now() + 60000,
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      const result = await cognito.refreshTokens();

      expect(result).toBeNull();
      expect(localStorage.getItem('gm_buddy_tokens')).toBeNull(); // Should clear tokens
    });

    it('should handle refresh failure', async () => {
      // Setup - store tokens with a refresh token
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'invalid-refresh-token',
        expiresAt: Date.now() + 60000,
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      // Mock failed refresh response
      global.fetch = vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        text: async () => 'Invalid refresh token',
      });

      const result = await cognito.refreshTokens();

      expect(result).toBeNull();
      expect(localStorage.getItem('gm_buddy_tokens')).toBeNull(); // Should clear tokens
    });

    it('should handle network errors', async () => {
      // Setup - store tokens with a refresh token
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 60000,
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      // Mock network error
      global.fetch = vi.fn().mockRejectedValue(new Error('Network error'));

      const result = await cognito.refreshTokens();

      expect(result).toBeNull();
      expect(localStorage.getItem('gm_buddy_tokens')).toBeNull(); // Should clear tokens
    });

    it('should keep existing refresh token if new one not provided', async () => {
      // Setup - store tokens with a refresh token
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'existing-refresh-token',
        expiresAt: Date.now() + 60000,
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      // Mock refresh response without a new refresh token
      global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          access_token: 'new-access-token',
          id_token: 'new-id-token',
          // No refresh_token in response
          expires_in: 3600,
        }),
      });

      const result = await cognito.refreshTokens();

      expect(result).toBeTruthy();
      expect(result?.refreshToken).toBe('existing-refresh-token'); // Should keep existing
    });
  });

  describe('loadTokens', () => {
    it('should return valid tokens that are not expiring soon', async () => {
      const currentTokens = {
        accessToken: 'valid-access-token',
        idToken: 'valid-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 10 * 60 * 1000, // expires in 10 minutes
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      const result = await cognito.loadTokens();

      expect(result).toBeTruthy();
      expect(result?.accessToken).toBe('valid-access-token');
    });

    it('should refresh tokens when expiring in less than 5 minutes', async () => {
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 2 * 60 * 1000, // expires in 2 minutes
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      // Mock successful refresh
      global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          access_token: 'new-access-token',
          id_token: 'new-id-token',
          refresh_token: 'new-refresh-token',
          expires_in: 3600,
        }),
      });

      const result = await cognito.loadTokens();

      expect(result).toBeTruthy();
      expect(result?.accessToken).toBe('new-access-token');
      expect(global.fetch).toHaveBeenCalled();
    });

    it('should refresh tokens when already expired', async () => {
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() - 1000, // already expired
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      // Mock successful refresh
      global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          access_token: 'new-access-token',
          id_token: 'new-id-token',
          refresh_token: 'new-refresh-token',
          expires_in: 3600,
        }),
      });

      const result = await cognito.loadTokens();

      expect(result).toBeTruthy();
      expect(result?.accessToken).toBe('new-access-token');
      expect(global.fetch).toHaveBeenCalled();
    });

    it('should return current tokens if refresh fails but tokens still valid', async () => {
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 2 * 60 * 1000, // expires in 2 minutes (still valid)
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      // Mock failed refresh
      global.fetch = vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        text: async () => 'Invalid refresh token',
      });

      const result = await cognito.loadTokens();

      // Should return current tokens since they're still valid
      expect(result).toBeTruthy();
      expect(result?.accessToken).toBe('old-access-token');
    });

    it('should return null if tokens are expired and refresh fails', async () => {
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() - 1000, // already expired
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      // Mock failed refresh
      global.fetch = vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        text: async () => 'Invalid refresh token',
      });

      const result = await cognito.loadTokens();

      expect(result).toBeNull();
      expect(localStorage.getItem('gm_buddy_tokens')).toBeNull();
    });
  });

  describe('getIdToken', () => {
    it('should return valid ID token', async () => {
      const currentTokens = {
        accessToken: 'valid-access-token',
        idToken: 'valid-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 10 * 60 * 1000,
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      const token = await cognito.getIdToken();

      expect(token).toBe('valid-id-token');
    });

    it('should refresh and return new ID token when expiring soon', async () => {
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 2 * 60 * 1000,
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          access_token: 'new-access-token',
          id_token: 'new-id-token',
          refresh_token: 'new-refresh-token',
          expires_in: 3600,
        }),
      });

      const token = await cognito.getIdToken();

      expect(token).toBe('new-id-token');
    });

    it('should return null when no tokens exist', async () => {
      const token = await cognito.getIdToken();

      expect(token).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when valid tokens exist', async () => {
      const currentTokens = {
        accessToken: 'valid-access-token',
        idToken: 'valid-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 10 * 60 * 1000,
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      const authenticated = await cognito.isAuthenticated();

      expect(authenticated).toBe(true);
    });

    it('should return false when no tokens exist', async () => {
      const authenticated = await cognito.isAuthenticated();

      expect(authenticated).toBe(false);
    });

    it('should return true after successful refresh', async () => {
      const currentTokens = {
        accessToken: 'old-access-token',
        idToken: 'old-id-token',
        refreshToken: 'valid-refresh-token',
        expiresAt: Date.now() + 2 * 60 * 1000,
      };
      localStorage.setItem('gm_buddy_tokens', JSON.stringify(currentTokens));

      global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          access_token: 'new-access-token',
          id_token: 'new-id-token',
          refresh_token: 'new-refresh-token',
          expires_in: 3600,
        }),
      });

      const authenticated = await cognito.isAuthenticated();

      expect(authenticated).toBe(true);
    });
  });
});
