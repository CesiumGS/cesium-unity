# Authentication System

This directory contains all components related to OAuth 2.0 authentication with Bentley's iTwin platform using PKCE (Proof Key for Code Exchange) for enhanced security.

## Overview

The authentication system provides secure access to Bentley's iTwin platform APIs through:
- OAuth 2.0 authorization with PKCE flow
- Secure token storage and management
- Automatic token refresh capabilities
- Session persistence across Unity Editor restarts

## Architecture

The authentication system follows a layered architecture:
- **Core Components**: Handle token lifecycle and state management
- **OAuth Components**: Implement the OAuth 2.0 PKCE flow
- **Main Manager**: Orchestrates the authentication process

## Security Features

### PKCE (Proof Key for Code Exchange)
- Generates cryptographically secure code verifier and challenge
- Prevents authorization code interception attacks
- No client secret required, suitable for public clients

### Secure Storage
- Tokens are encrypted before storage in Unity Editor Preferences
- Sensitive data is cleared from memory when no longer needed
- Session data persists securely between Unity sessions

### Token Management
- Automatic token expiration checking
- Proactive token refresh before expiration
- Graceful handling of expired or invalid tokens

## Directory Structure

```
Authentication/
├── README.md                   # This file - Authentication overview
├── Core/                       # Core authentication managers
│   ├── AuthStateManager.cs    # Token lifecycle and validation
│   ├── AuthConfigManager.cs   # Configuration and storage
│   └── BentleyAuthCore.cs     # Core authentication logic
└── OAuth/                      # OAuth 2.0 PKCE implementation
    ├── PKCEManager.cs          # PKCE code generation and validation
    ├── HttpListenerManager.cs # Local HTTP server for callbacks
    └── TokenExchangeClient.cs  # Token exchange and refresh
```

## Authentication Flow

1. **Initialization**: Load configuration and check for existing valid tokens
2. **PKCE Generation**: Create code verifier and challenge for the OAuth flow
3. **Authorization Request**: Open browser to Bentley's authorization server
4. **Local Callback**: Start local HTTP server to receive authorization code
5. **Token Exchange**: Exchange authorization code for access and refresh tokens
6. **Token Storage**: Securely store tokens with encryption
7. **Token Refresh**: Automatically refresh tokens before expiration

## Component Responsibilities

### Core Components (`/Core`)

#### AuthStateManager.cs
- **Purpose**: Manages token lifecycle including storage, validation, and expiry checking
- **Key Methods**:
  - `GetCurrentAccessToken()`: Returns valid access token or null if expired
  - `IsLoggedIn()`: Determines authentication status
  - `SaveTokens()`: Securely stores tokens with encryption
  - `ClearTokens()`: Removes all stored authentication data

#### AuthConfigManager.cs
- **Purpose**: Handles configuration management and secure storage operations
- **Key Methods**:
  - `SaveClientId()`: Stores client ID in Editor Preferences
  - `LoadClientId()`: Retrieves stored client ID
  - `EncryptToken()`: Encrypts sensitive data before storage
  - `DecryptToken()`: Decrypts stored sensitive data

#### BentleyAuthCore.cs
- **Purpose**: Core authentication logic and workflow orchestration
- **Key Methods**:
  - `StartAuthenticationFlow()`: Initiates the complete OAuth flow
  - `ProcessAuthorizationCode()`: Handles callback with authorization code
  - `RefreshAccessToken()`: Refreshes expired access tokens

### OAuth Components (`/OAuth`)

#### PKCEManager.cs
- **Purpose**: Implements PKCE (Proof Key for Code Exchange) security enhancement
- **Key Methods**:
  - `GenerateCodeVerifier()`: Creates cryptographically secure code verifier
  - `GenerateCodeChallenge()`: Creates SHA256 challenge from verifier
  - `ValidateCodeChallenge()`: Validates PKCE parameters

#### HttpListenerManager.cs
- **Purpose**: Manages local HTTP server for OAuth callback handling
- **Key Methods**:
  - `StartListener()`: Starts local server on available port
  - `WaitForCallback()`: Waits for OAuth callback with authorization code
  - `StopListener()`: Cleanly stops the HTTP server

#### TokenExchangeClient.cs
- **Purpose**: Handles HTTP requests for token exchange and refresh operations
- **Key Methods**:
  - `ExchangeCodeForTokens()`: Exchanges authorization code for tokens
  - `RefreshTokens()`: Refreshes access token using refresh token
  - `ValidateTokenResponse()`: Validates and parses token responses

## Usage Patterns

### Basic Authentication Check
```csharp
if (authManager.IsLoggedIn())
{
    string token = authManager.GetCurrentAccessToken();
    // Use token for API requests
}
else
{
    // Initiate login flow
    yield return authManager.GetAccessTokenCoroutine(OnTokenReceived);
}
```

### Automatic Token Refresh
The system automatically handles token refresh:
- Checks token expiration before each API call
- Refreshes tokens proactively with 5-minute buffer
- Falls back to full re-authentication if refresh fails

## Error Handling

### Network Errors
- HTTP connection failures are logged and user-friendly messages displayed
- Retry logic for transient network issues
- Graceful degradation when authentication services are unavailable

### Token Errors
- Invalid or expired tokens trigger automatic refresh attempts
- Malformed tokens are cleared and require re-authentication
- Token storage errors fall back to session-only authentication

### User Cancellation
- Browser-based authentication can be cancelled by the user
- Local HTTP server is properly cleaned up on cancellation
- UI state is restored to pre-authentication state

## Security Considerations

### Data Protection
- All sensitive data (tokens, secrets) is encrypted before storage
- Memory containing sensitive data is cleared after use
- No sensitive data is logged or exposed in error messages

### Network Security
- All communications use HTTPS for production endpoints
- Local callback server uses HTTP only for localhost (standard OAuth practice)
- PKCE prevents authorization code interception attacks

### Session Management
- Tokens have appropriate expiration times
- Sessions can be invalidated/logged out cleanly
- No long-lived client secrets or sensitive configuration

## Development Guidelines

### Adding Authentication Features
1. Determine if the feature belongs in Core or OAuth layer
2. Follow the existing dependency injection pattern
3. Implement comprehensive error handling
4. Ensure sensitive data is handled securely
5. Add appropriate unit tests for authentication logic

### Testing Authentication
- Mock HTTP responses for unit testing
- Test error scenarios (network failures, invalid tokens, etc.)
- Verify secure storage and cleanup operations
- Test the complete OAuth flow in integration tests

### Security Review
When modifying authentication code:
1. Review all data storage for encryption requirements
2. Ensure no sensitive data appears in logs
3. Verify proper cleanup of sensitive data from memory
4. Test error handling doesn't expose sensitive information
5. Validate OAuth flow compliance with security best practices
