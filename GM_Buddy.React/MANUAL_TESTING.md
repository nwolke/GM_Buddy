# Token Refresh Implementation - Manual Testing Guide

This guide provides instructions for manually testing the token refresh functionality.

## Overview

The implementation adds:
1. **Automatic token refresh** - Tokens are refreshed automatically when they expire in less than 5 minutes
2. **API 401 retry** - Failed API calls due to expired tokens automatically retry once after refreshing
3. **Session restoration** - User sessions are validated and refreshed on page load

## Prerequisites

- AWS Cognito must be configured with proper environment variables:
  ```
  VITE_COGNITO_DOMAIN=your-domain.auth.region.amazoncognito.com
  VITE_COGNITO_CLIENT_ID=your_client_id
  VITE_USE_COGNITO=true
  ```

## Test Scenarios

### 1. Test Proactive Token Refresh

**Goal**: Verify tokens are refreshed before they expire

**Steps**:
1. Log in to the application
2. Open browser DevTools → Application → Local Storage
3. Find `gm_buddy_tokens` and note the `expiresAt` timestamp
4. Modify the `expiresAt` to be 2 minutes in the future: `Date.now() + 120000`
5. Make any API call (e.g., navigate to NPCs page)
6. Check the console logs - you should see:
   - `Tokens expiring soon, proactively refreshing...`
   - `[Cognito] Refreshing tokens...`
   - `✅ Token refresh successful`
7. Verify the `expiresAt` in localStorage has been updated to a new time

**Expected Result**: Tokens are automatically refreshed before making the API call

### 2. Test Expired Token Handling

**Goal**: Verify expired tokens are refreshed or cleared

**Steps**:
1. Log in to the application
2. Open browser DevTools → Application → Local Storage
3. Find `gm_buddy_tokens` and modify `expiresAt` to a past timestamp: `Date.now() - 1000`
4. Refresh the page
5. Check the console logs - you should see either:
   - Successful refresh with `✅ Token refresh successful`, OR
   - Token cleared with `Tokens expired and refresh failed, clearing session...`

**Expected Result**: 
- If refresh token is valid: New tokens are obtained and user stays logged in
- If refresh token is invalid: User is logged out

### 3. Test API 401 Retry

**Goal**: Verify API calls retry after token refresh on 401 errors

**Steps**:
1. Log in to the application
2. Open browser DevTools → Network tab
3. Manually set tokens to be expired (see step 3 in Test 2)
4. Make an API call (e.g., load NPCs page)
5. Check the Network tab - you should see:
   - Initial request to `/api/Npcs` → 401 Unauthorized
   - Request to `/oauth2/token` → 200 OK (token refresh)
   - Retry request to `/api/Npcs` → 200 OK
6. Check console logs for:
   - `[API Interceptor] 401 Unauthorized - attempting token refresh...`
   - `[API Interceptor] Token refreshed, retrying request...`

**Expected Result**: The API call succeeds after refreshing the token

### 4. Test Refresh Failure Handling

**Goal**: Verify proper cleanup when token refresh fails

**Steps**:
1. Log in to the application
2. Open browser DevTools → Application → Local Storage
3. Find `gm_buddy_tokens` and modify the `refreshToken` to an invalid value: `"invalid-token"`
4. Modify `expiresAt` to trigger a refresh: `Date.now() + 120000`
5. Make any API call or refresh the page
6. Check console logs for:
   - `[Cognito] Refreshing tokens...`
   - `❌ Token refresh failed: 400 ...`
7. Verify that:
   - `gm_buddy_tokens` is removed from localStorage
   - User is logged out
   - User is redirected to login page (if configured)

**Expected Result**: Invalid refresh tokens result in logout

### 5. Test No Infinite Retry Loop

**Goal**: Verify 401 errors don't cause infinite retry loops

**Steps**:
1. Set up a scenario where token refresh fails (see Test 4)
2. With expired tokens, make an API call
3. Monitor the Network tab
4. Verify that the token refresh endpoint is called only ONCE
5. Verify that the original API call fails and is not retried indefinitely

**Expected Result**: Each request attempts to refresh only once, preventing loops

## Debugging Tips

### Console Logs to Watch For

**Successful Refresh Flow**:
```
Tokens expiring soon, proactively refreshing...
[Cognito] Refreshing tokens...
✅ Token refresh successful
```

**Failed Refresh Flow**:
```
[Cognito] Refreshing tokens...
❌ Token refresh failed: 400 Invalid refresh token
```

**API 401 Retry Flow**:
```
[API Interceptor] 401 Unauthorized - attempting token refresh...
[Cognito] Refreshing tokens...
✅ Token refresh successful
[API Interceptor] Token refreshed, retrying request...
```

### Common Issues

1. **"Cognito is not configured"** - Verify environment variables are set correctly
2. **Refresh always fails** - Check that refresh tokens are being stored and returned by Cognito
3. **User logged out unexpectedly** - Verify refresh token hasn't expired (typically valid for 30 days)
4. **Tokens not refreshing proactively** - Check the 5-minute threshold logic in `loadTokens()`

## Automated Tests

Run the test suite to verify all functionality:

```bash
npm test
```

Key test files:
- `src/services/cognito.test.ts` - Token refresh logic (16 tests)
- `src/services/api.test.ts` - API interceptor logic (7 tests)
- `src/contexts/AuthContext.test.tsx` - Session restoration (8 tests)

All tests should pass successfully.
