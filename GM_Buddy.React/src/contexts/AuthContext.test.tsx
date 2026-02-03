import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, renderHook } from '@testing-library/react'
import { AuthProvider, useAuth } from '@/contexts/AuthContext'
import { accountApi } from '@/services/api'
import * as cognito from '@/services/cognito'

// Mock the API and cognito service
vi.mock('@/services/api', () => ({
  accountApi: {
    syncAccount: vi.fn(),
  },
}))

vi.mock('@/services/cognito', () => ({
  isCognitoEnabled: vi.fn(),
  handleCallback: vi.fn(),
  redirectToLogin: vi.fn(),
  redirectToLogout: vi.fn(),
  clearTokens: vi.fn(),
}))

// Test component that uses the auth context
function TestComponent() {
  const auth = useAuth()
  return (
    <div>
      <div data-testid="is-authenticated">{auth.isAuthenticated.toString()}</div>
      <div data-testid="loading">{auth.loading.toString()}</div>
      <div data-testid="user-email">{auth.user?.email || 'none'}</div>
      <div data-testid="cognito-mode">{auth.isCognitoMode.toString()}</div>
      <button onClick={auth.loginWithCognito}>Login with Cognito</button>
      <button onClick={auth.logout}>Logout</button>
    </div>
  )
}

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    window.history.replaceState({}, '', '/')
    
    // Default to non-Cognito mode
    vi.mocked(cognito.isCognitoEnabled).mockReturnValue(false)
  })

  it('should provide auth context', async () => {
    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false')
    })

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('false')
    expect(screen.getByTestId('user-email')).toHaveTextContent('none')
  })

  it('should throw error when useAuth is used outside AuthProvider', () => {
    // Suppress console.error for this test
    const originalError = console.error
    console.error = vi.fn()

    // renderHook will throw if the hook throws during render
    expect(() => {
      renderHook(() => useAuth())
    }).toThrow('useAuth must be used within an AuthProvider')

    console.error = originalError
  })

  it('should restore session from localStorage', async () => {
    const mockUser = {
      cognitoSub: 'test-sub',
      email: 'test@test.com',
      accountId: 1,
    }
    localStorage.setItem('gm_buddy_auth', JSON.stringify(mockUser))

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false')
    })

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('true')
    expect(screen.getByTestId('user-email')).toHaveTextContent('test@test.com')
  })

  it('should handle invalid stored auth data', async () => {
    localStorage.setItem('gm_buddy_auth', 'invalid-json')

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false')
    })

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('false')
    expect(screen.getByTestId('user-email')).toHaveTextContent('none')
    expect(localStorage.getItem('gm_buddy_auth')).toBeNull()
  })

  it('should handle Cognito callback', async () => {
    vi.mocked(cognito.isCognitoEnabled).mockReturnValue(true)
    vi.mocked(cognito.handleCallback).mockResolvedValue({
      sub: 'cognito-sub',
      email: 'cognito@test.com',
    })
    vi.mocked(accountApi.syncAccount).mockResolvedValue({
      accountId: 123,
      email: 'cognito@test.com',
      subscriptionTier: 'free',
      createdAt: new Date().toISOString(),
    })

    // Simulate Cognito callback URL
    window.history.replaceState({}, '', '/callback?code=test-code')

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false')
    })

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('true')
    expect(screen.getByTestId('user-email')).toHaveTextContent('cognito@test.com')
    expect(accountApi.syncAccount).toHaveBeenCalledWith('cognito@test.com')
  })

  it('should redirect to Cognito login when loginWithCognito is called in Cognito mode', async () => {
    vi.mocked(cognito.isCognitoEnabled).mockReturnValue(true)

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false')
    })

    const loginButton = screen.getByText('Login with Cognito')
    loginButton.click()

    expect(cognito.redirectToLogin).toHaveBeenCalled()
  })

  it('should logout and clear storage', async () => {
    const mockUser = {
      cognitoSub: 'test-sub',
      email: 'test@test.com',
      accountId: 1,
    }
    localStorage.setItem('gm_buddy_auth', JSON.stringify(mockUser))

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false')
    })

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('true')

    const logoutButton = screen.getByText('Logout')
    logoutButton.click()

    await waitFor(() => {
      expect(localStorage.getItem('gm_buddy_auth')).toBeNull()
    })

    expect(cognito.clearTokens).toHaveBeenCalled()
    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('false')
    expect(screen.getByTestId('user-email')).toHaveTextContent('none')
  })

  it('should expose isCognitoMode correctly', async () => {
    vi.mocked(cognito.isCognitoEnabled).mockReturnValue(true)

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    )

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('false')
    })

    expect(screen.getByTestId('cognito-mode')).toHaveTextContent('true')
  })
})
