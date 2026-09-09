using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Manages OAuth configuration and URL generation for authentication flow
/// </summary>
public class AuthConfigManager
{
    public static readonly Dictionary<string, string> FixedScopes = new Dictionary<string, string>
    {
        { "mesh-export", AuthConstants.MESH_EXPORT_SCOPE },
        { "general", AuthConstants.GENERAL_SCOPE }
    };
    
    /// <summary>
    /// Creates configuration for mesh export authentication
    /// </summary>
    public static AuthConfig CreateMeshExportConfig()
    {
        return new AuthConfig(
            clientId: "native-2686",
            redirectUri: AuthConstants.DEFAULT_REDIRECT_URI,
            scopes: FixedScopes["mesh-export"]
        );
    }
    
    /// <summary>
    /// Creates configuration for general authentication
    /// </summary>
    public static AuthConfig CreateGeneralConfig()
    {
        return new AuthConfig(
            clientId: "native-2686",
            redirectUri: AuthConstants.DEFAULT_REDIRECT_URI,
            scopes: FixedScopes["general"]
        );
    }
    
    /// <summary>
    /// Creates configuration from provided parameters
    /// </summary>
    public static AuthConfig CreateConfig(string clientId, string redirectUri, string scopes)
    {
        return new AuthConfig(clientId, redirectUri, scopes);
    }
    
    /// <summary>
    /// Validates authentication configuration
    /// </summary>
    public static bool ValidateConfig(AuthConfig config)
    {
        if (string.IsNullOrEmpty(config.ClientId))
        {
            Debug.LogError("BentleyAuthManager_Editor: Client ID cannot be null or empty");
            return false;
        }
        
        if (string.IsNullOrEmpty(config.RedirectUri))
        {
            Debug.LogError("BentleyAuthManager_Editor: Redirect URI cannot be null or empty");
            return false;
        }
        
        if (!Uri.TryCreate(config.RedirectUri, UriKind.Absolute, out _))
        {
            Debug.LogError("BentleyAuthManager_Editor: Invalid redirect URI format");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Encapsulates OAuth configuration parameters
    /// </summary>
    public class AuthConfig
    {
        public string ClientId { get; }
        public string RedirectUri { get; }
        public string Scopes { get; }
        
        public AuthConfig(string clientId, string redirectUri, string scopes)
        {
            ClientId = clientId;
            RedirectUri = redirectUri;
            Scopes = scopes;
        }
        
        /// <summary>
        /// Generates the authorization URL for OAuth flow
        /// </summary>
        public string GenerateAuthorizationUrl(string state, string codeChallenge)
        {
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = RedirectUri,
                ["scope"] = Scopes,
                ["state"] = state,
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256"
            };
            
            var queryString = new StringBuilder();
            foreach (var param in parameters)
            {
                if (queryString.Length > 0) queryString.Append("&");
                queryString.Append($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");
            }
            
            return $"{AuthConstants.AUTHORIZATION_URL}?{queryString}";
        }
    }
}
