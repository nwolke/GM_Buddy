# AWS Cognito Authentication Guide

This document describes the AWS Cognito integration using the Hosted UI for authentication.

## Overview

The application uses AWS Cognito Hosted UI for:
- User registration and authentication
- Password management (reset, change)
- OAuth2/OIDC token flow
- Role-based authorization via Cognito Groups

## Architecture

```
????????????????     ???????????????????     ????????????????????
?    User      ??????? Cognito Hosted  ???????  GM_Buddy.Web    ?
?   Browser    ??????? UI (Login Page) ???????  (Blazor App)    ?
????????????????     ???????????????????     ????????????????????
                              ?                       ?
                              ? ID/Access Tokens      ? Bearer Token
                              ?                       ?
                     ???????????????????     ????????????????????
                     ?  Cookie Auth    ?     ?  GM_Buddy.Server ?
                     ?  (Session)      ?     ?  (API)           ?
                     ???????????????????     ????????????????????
```

## Authentication Flow

1. User clicks "Sign In" in the app
2. App redirects to Cognito Hosted UI
3. User logs in (or signs up) on Cognito's page
4. Cognito redirects back with authorization code
5. App exchanges code for tokens (ID, Access, Refresh)
6. Tokens are stored in a secure cookie
7. Access token is forwarded to API for authenticated requests

## Cognito Configuration

### Required Settings

| Setting | Value |
|---------|-------|
| Region | `us-west-2` |
| User Pool ID | `us-west-2_3H6SIoARI` |
| Client ID | `3tu41ptf62ntlqso884tl3aaem` |
| Domain | `gm-buddy` (your Cognito domain prefix) |

### App Client Configuration in Cognito Console

1. **Callback URL**: `https://localhost:5001/signin-oidc` (dev) or your production URL
2. **Sign-out URL**: `https://localhost:5001/` (dev) or your production URL
3. **OAuth 2.0 Grant Types**: Authorization code grant
4. **OpenID Connect scopes**: openid, email, profile

### Setting Up Cognito Domain

1. Go to **App integration** ? **Domain**
2. Choose "Use a Cognito domain"
3. Enter a domain prefix (e.g., `gm-buddy`)
4. This creates: `https://gm-buddy.auth.us-west-2.amazoncognito.com`

## Role-Based Authorization

### Creating Groups in Cognito

1. Go to your User Pool ? **Groups**
2. Create groups that map to roles:
   - `Admin` - Full access
   - `GameMaster` - Campaign management
   - `Player` - Basic access

### Using Authorization in Blazor

```razor
@* Page-level authorization *@
@page "/admin"
@attribute [Authorize(Roles = "Admin")]

@* Component-level authorization *@
<AuthorizeView Roles="Admin">
    <Authorized>
        <p>Admin-only content</p>
    </Authorized>
</AuthorizeView>

@* Multiple roles *@
<AuthorizeView Roles="Admin,GameMaster">
    <Authorized>
        <p>Visible to Admins and GameMasters</p>
    </Authorized>
</AuthorizeView>
```

### Using Authorization in API

```csharp
[Authorize]
[ApiController]
public class NpcsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetNpcs() { }  // Any authenticated user
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteNpc(int id) { }  // Admin only
}
```

## Configuration Files

### appsettings.json (Web)

```json
{
  "Cognito": {
    "Region": "us-west-2",
    "UserPoolId": "us-west-2_3H6SIoARI",
    "ClientId": "3tu41ptf62ntlqso884tl3aaem",
    "ClientSecret": "your-client-secret",
    "Domain": "gm-buddy"
  }
}
```

### Docker Environment Variables

```yaml
environment:
  - Cognito__Region=us-west-2
  - Cognito__UserPoolId=us-west-2_3H6SIoARI
  - Cognito__ClientId=3tu41ptf62ntlqso884tl3aaem
  - Cognito__ClientSecret=your-client-secret
  - Cognito__Domain=gm-buddy
```

## Troubleshooting

### "redirect_mismatch" error
- Verify the callback URL in Cognito matches exactly
- Must include `/signin-oidc` path
- Check http vs https

### User not redirected after login
- Check the callback URL configuration
- Verify cookies are enabled

### Groups not appearing as roles
- Ensure user is assigned to a group in Cognito
- Groups are mapped to Role claims in `OnTokenValidated` event

### Token expired errors
- Access tokens expire in 60 minutes (configurable)
- Refresh tokens expire in 5 days
- Cookie expiration is set to 5 days to match

## Security Best Practices

1. **Use HTTPS** - Required for production
2. **Secure cookies** - HttpOnly and Secure flags are enabled
3. **Environment variables** - Never commit secrets to source control
4. **MFA** - Enable for admin accounts in Cognito
5. **Token validation** - API validates Cognito JWTs automatically
