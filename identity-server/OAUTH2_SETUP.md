# OAuth2 Setup Guide

This identity server supports OAuth2 authentication via Google and Microsoft. Follow these steps to configure your OAuth2 providers.

## Google OAuth2 Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API:
   - Click "APIs & Services" > "Library"
   - Search for "Google+ API"
   - Click "Enable"

4. Create OAuth2 credentials:
   - Click "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth 2.0 Client ID"
   - Choose "Web application"
   - Add Authorized redirect URIs:
     - `http://localhost:5000/signin-google` (development)
     - `https://yourdomain.com/signin-google` (production)
   - Copy the Client ID and Client Secret

5. Update your `appsettings.json` or `appsettings.Development.json`:
```json
"OAuth2": {
  "Google": {
    "ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret"
  }
}
```

## Microsoft OAuth2 Setup

1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to "Azure Active Directory" > "App registrations"
3. Click "New registration"
4. Fill in the name and configure supported account types
5. Add Redirect URI:
   - Type: Web
   - URI: `http://localhost:5000/signin-microsoft` (development)
   - URI: `https://yourdomain.com/signin-microsoft` (production)
6. Click "Register"

7. Create a client secret:
   - Go to "Certificates & secrets"
   - Click "New client secret"
   - Copy the secret value (it won't be shown again)

8. Copy the Application (client) ID

9. Update your `appsettings.json` or `appsettings.Development.json`:
```json
"OAuth2": {
  "Microsoft": {
    "ClientId": "your-application-id",
    "ClientSecret": "your-client-secret"
  }
}
```

## API Endpoints

### Traditional Email/Password Sign In
```http
POST /api/auth/signin
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### Initiate OAuth2 Sign In
```http
GET /api/auth/signin/{provider}
```

Supported providers: `google`, `microsoft`

Example:
```
GET /api/auth/signin/google
```

This will redirect to the OAuth2 provider's login page.

### OAuth2 Callback
After the user authenticates with the OAuth2 provider, they are redirected to:
```
GET /api/auth/oauth2-callback/{provider}
```

This endpoint automatically:
1. Verifies the OAuth2 response
2. Extracts user information (email, name)
3. Links to existing user or creates new account
4. Returns a JWT token for authentication

## Using the JWT Token

After sign in (either traditional or OAuth2), you'll receive a response like:
```json
{
  "success": true,
  "message": "Sign in successful",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "surname": "Doe",
    "createdAt": "2026-06-26T..."
  }
}
```

Use the token in subsequent API requests:
```http
Authorization: Bearer {token}
```

## Important Notes

- Passwords created via OAuth2 are randomly generated since users don't provide one
- Users can use both traditional password login and OAuth2 login
- User accounts are linked by email address
- In production, update the JWT secret key to a strong, random value
