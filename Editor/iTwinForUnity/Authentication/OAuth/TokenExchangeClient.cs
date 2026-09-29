using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Handles OAuth token exchange operations with the authorization server
/// </summary>
public class TokenExchangeClient
{
    private static readonly HttpClient httpClient = new HttpClient();
    
    static TokenExchangeClient()
    {
        httpClient.Timeout = TimeSpan.FromSeconds(30);
    }
    
    /// <summary>
    /// Exchanges authorization code for access and refresh tokens
    /// </summary>
    public async Task<string> ExchangeCodeForTokens(AuthConfigManager.AuthConfig config, string authorizationCode, string codeVerifier)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        
        if (string.IsNullOrEmpty(authorizationCode))
            throw new ArgumentException("Authorization code cannot be null or empty", nameof(authorizationCode));
        
        if (string.IsNullOrEmpty(codeVerifier))
            throw new ArgumentException("Code verifier cannot be null or empty", nameof(codeVerifier));
        
        try
        {
            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = config.ClientId,
                ["code"] = authorizationCode,
                ["redirect_uri"] = config.RedirectUri,
                ["code_verifier"] = codeVerifier
            };
            
            return await SendTokenRequest(parameters);
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Failed to exchange authorization code: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Refreshes the access token using the refresh token
    /// </summary>
    public async Task<string> RefreshAccessToken(AuthConfigManager.AuthConfig config, string refreshToken)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("Refresh token cannot be null or empty", nameof(refreshToken));
        
        try
        {
            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = config.ClientId,
                ["refresh_token"] = refreshToken
            };
            
            return await SendTokenRequest(parameters);
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Failed to refresh access token: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Sends a token request to the authorization server
    /// </summary>
    private async Task<string> SendTokenRequest(Dictionary<string, string> parameters)
    {
        try
        {
            var content = new FormUrlEncodedContent(parameters);
            
            var response = await httpClient.PostAsync(AuthConstants.TOKEN_URL, content);
            
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"BentleyAuthManager_Editor: Token request failed with status {response.StatusCode}: {errorContent}");
                return null;
            }
            
            string responseContent = await response.Content.ReadAsStringAsync();
            
            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: HTTP error during token request: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Token request timed out: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Unexpected error during token request: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Validates token response format before processing
    /// </summary>
    public bool ValidateTokenResponse(string tokenResponse)
    {
        if (string.IsNullOrEmpty(tokenResponse))
            return false;
        
        try
        {
            var tokenData = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenValidationModel>(tokenResponse);
            
            return !string.IsNullOrEmpty(tokenData.access_token) && 
                   tokenData.expires_in > 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Token response validation failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Model for token response validation
    /// </summary>
    private class TokenValidationModel
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }
}
