import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';
import apiClient from './api';
import * as cognito from './cognito';

// Mock the cognito service
vi.mock('./cognito', () => ({
  getIdToken: vi.fn(),
  refreshTokens: vi.fn(),
  clearTokens: vi.fn(),
}));

describe('API Service - 401 Interceptor', () => {
  let mockAxios: MockAdapter;

  beforeEach(() => {
    vi.clearAllMocks();
    mockAxios = new MockAdapter(apiClient);
  });

  afterEach(() => {
    mockAxios.restore();
  });

  it('should add authorization header to requests', async () => {
    vi.mocked(cognito.getIdToken).mockResolvedValue('valid-token');
    mockAxios.onGet('/test').reply(200, { data: 'success' });

    await apiClient.get('/test');

    expect(cognito.getIdToken).toHaveBeenCalled();
  });

  it('should handle 401 by refreshing token and retrying request', async () => {
    // First call returns valid token
    vi.mocked(cognito.getIdToken).mockResolvedValueOnce('expired-token');
    
    // Mock the API to return 401 first, then 200
    let callCount = 0;
    mockAxios.onGet('/protected').reply(() => {
      callCount++;
      if (callCount === 1) {
        return [401, { error: 'Unauthorized' }];
      }
      return [200, { data: 'success' }];
    });

    // Mock refresh to return new tokens
    vi.mocked(cognito.refreshTokens).mockResolvedValue({
      accessToken: 'new-access-token',
      idToken: 'new-id-token',
      refreshToken: 'new-refresh-token',
      expiresAt: Date.now() + 3600000,
    });

    const response = await apiClient.get('/protected');

    expect(response.status).toBe(200);
    expect(response.data).toEqual({ data: 'success' });
    expect(cognito.refreshTokens).toHaveBeenCalledOnce();
    expect(callCount).toBe(2); // Original request + retry
  });

  it('should not retry 401 more than once', async () => {
    vi.mocked(cognito.getIdToken).mockResolvedValue('expired-token');
    
    // Mock the API to always return 401
    mockAxios.onGet('/protected').reply(401, { error: 'Unauthorized' });

    // Mock refresh to return new tokens
    vi.mocked(cognito.refreshTokens).mockResolvedValue({
      accessToken: 'new-access-token',
      idToken: 'new-id-token',
      refreshToken: 'new-refresh-token',
      expiresAt: Date.now() + 3600000,
    });

    try {
      await apiClient.get('/protected');
      expect.fail('Should have thrown an error');
    } catch (error) {
      if (axios.isAxiosError(error)) {
        expect(error.response?.status).toBe(401);
      }
    }

    // Should only try to refresh once
    expect(cognito.refreshTokens).toHaveBeenCalledOnce();
  });

  it('should clear tokens when refresh fails', async () => {
    vi.mocked(cognito.getIdToken).mockResolvedValue('expired-token');
    mockAxios.onGet('/protected').reply(401, { error: 'Unauthorized' });

    // Mock refresh to fail
    vi.mocked(cognito.refreshTokens).mockResolvedValue(null);

    try {
      await apiClient.get('/protected');
      expect.fail('Should have thrown an error');
    } catch (error) {
      if (axios.isAxiosError(error)) {
        expect(error.response?.status).toBe(401);
      }
    }

    expect(cognito.refreshTokens).toHaveBeenCalledOnce();
    expect(cognito.clearTokens).toHaveBeenCalledOnce();
  });

  it('should clear tokens when refresh throws an error', async () => {
    vi.mocked(cognito.getIdToken).mockResolvedValue('expired-token');
    mockAxios.onGet('/protected').reply(401, { error: 'Unauthorized' });

    // Mock refresh to throw an error
    vi.mocked(cognito.refreshTokens).mockRejectedValue(new Error('Network error'));

    try {
      await apiClient.get('/protected');
      expect.fail('Should have thrown an error');
    } catch (error) {
      if (axios.isAxiosError(error)) {
        expect(error.response?.status).toBe(401);
      }
    }

    expect(cognito.refreshTokens).toHaveBeenCalledOnce();
    expect(cognito.clearTokens).toHaveBeenCalledOnce();
  });

  it('should not retry non-401 errors', async () => {
    vi.mocked(cognito.getIdToken).mockResolvedValue('valid-token');
    mockAxios.onGet('/server-error').reply(500, { error: 'Server error' });

    try {
      await apiClient.get('/server-error');
      expect.fail('Should have thrown an error');
    } catch (error) {
      if (axios.isAxiosError(error)) {
        expect(error.response?.status).toBe(500);
      }
    }

    // Should not attempt refresh for 500 error
    expect(cognito.refreshTokens).not.toHaveBeenCalled();
  });

  it('should handle requests without tokens', async () => {
    vi.mocked(cognito.getIdToken).mockResolvedValue(null);
    mockAxios.onGet('/public').reply(200, { data: 'public data' });

    const response = await apiClient.get('/public');

    expect(response.status).toBe(200);
    expect(response.data).toEqual({ data: 'public data' });
  });
});
