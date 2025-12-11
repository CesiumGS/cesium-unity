using UnityEngine;

/// <summary>
/// Centralized constants and configuration for Bentley authentication system
/// </summary>
public static class AuthConstants
{
    // OAuth Configuration
    public const string DEFAULT_REDIRECT_URI = "http://localhost:58789/";
    public const string DEFAULT_SCOPES = "itwin-platform";
    public const string MESH_EXPORT_SCOPE = "mesh-export";
    public const string GENERAL_SCOPE = "itwin-platform";
    public const string AUTHORIZATION_URL = "https://ims.bentley.com/as/authorization.oauth2";
    public const string TOKEN_URL = "https://ims.bentley.com/as/token.oauth2";
    
    // EditorPrefs Keys
    public const string PREF_ACCESS_TOKEN = "Bentley_Editor_AccessToken";
    public const string PREF_REFRESH_TOKEN = "Bentley_Editor_RefreshToken";
    public const string PREF_EXPIRY = "Bentley_Editor_TokenExpiry";
    public const string PREF_CLIENT_ID = "Bentley_Editor_ClientId";
    public const string PREF_REDIRECT_URI = "Bentley_Editor_RedirectUri";
    
    // Timing Constants
    public const int TOKEN_EXPIRY_BUFFER_MINUTES = 1;
    
    // HTTP Response Messages
    public const string SUCCESS_RESPONSE_HTML = @"<html><body style='display: flex; flex-direction: column; justify-content: center; align-items: center; height: 100vh; margin: 0; font-family: Arial, sans-serif; background-color: #f5f5f5;'><h1 style='color: #28a745; text-align: center;'>Authentication Successful</h1><p style='color: #666; text-align: center;'>You can close this window now.</p></body></html>";
    
    public const string ERROR_RESPONSE_HTML = @"<html><body style='display: flex; flex-direction: column; justify-content: center; align-items: center; height: 100vh; margin: 0; font-family: Arial, sans-serif; background-color: #f5f5f5;'><h1 style='color: #dc3545; text-align: center;'>Authentication Failed</h1><p style='color: #666; text-align: center;'>Please close this window and try again.</p></body></html>";
}
