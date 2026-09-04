using System;
using UnityEditor;

/// <summary>
/// Manages token lifecycle including storage, validation, and expiry checking
/// </summary>
public class AuthStateManager
{
    private BentleyAuthManager parentAuth;
    
    public AuthStateManager(BentleyAuthManager parentAuth)
    {
        this.parentAuth = parentAuth;
    }
    
    /// <summary>
    /// Gets the current valid access token, returning null if expired/invalid.
    /// Does NOT automatically trigger refresh or login.
    /// </summary>
    public string GetCurrentAccessToken()
    {
        if (!string.IsNullOrEmpty(parentAuth.accessToken) && 
            parentAuth.expiryTimeUtc > DateTime.UtcNow.AddMinutes(AuthConstants.TOKEN_EXPIRY_BUFFER_MINUTES))
            return parentAuth.accessToken;
        return null;
    }
    
    /// <summary>
    /// Determines if user is logged in based on valid tokens
    /// </summary>
    public bool IsLoggedIn()
    {
        return GetCurrentAccessToken() != null || !string.IsNullOrEmpty(parentAuth.refreshToken);
    }
    
    /// <summary>
    /// Gets the token expiry time in UTC
    /// </summary>
    public DateTime GetExpiryTimeUtc()
    {
        return parentAuth.expiryTimeUtc;
    }
    
    /// <summary>
    /// Processes token response and updates internal state
    /// </summary>
    public void ProcessTokenResponse(string json)
    {
        try
        {
            var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(json);
            parentAuth.accessToken = tokenResponse.access_token;
            parentAuth.refreshToken = tokenResponse.refresh_token;
            parentAuth.expiryTimeUtc = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);
            
            SaveTokensToPrefs();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"BentleyAuthManager_Editor: Failed to process token response: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Saves tokens to EditorPrefs for persistence
    /// </summary>
    public void SaveTokensToPrefs()
    {
        if (!string.IsNullOrEmpty(parentAuth.accessToken))
            EditorPrefs.SetString(AuthConstants.PREF_ACCESS_TOKEN, parentAuth.accessToken);
        
        if (!string.IsNullOrEmpty(parentAuth.refreshToken))
            EditorPrefs.SetString(AuthConstants.PREF_REFRESH_TOKEN, parentAuth.refreshToken);
        
        EditorPrefs.SetString(AuthConstants.PREF_EXPIRY, parentAuth.expiryTimeUtc.ToString("O"));
    }
    
    /// <summary>
    /// Loads stored tokens from EditorPrefs
    /// </summary>
    public void LoadStoredTokens()
    {
        if (EditorPrefs.HasKey(AuthConstants.PREF_ACCESS_TOKEN) && EditorPrefs.HasKey(AuthConstants.PREF_EXPIRY))
        {
            parentAuth.accessToken = EditorPrefs.GetString(AuthConstants.PREF_ACCESS_TOKEN);
            parentAuth.refreshToken = EditorPrefs.GetString(AuthConstants.PREF_REFRESH_TOKEN, "");
            string storedExpiry = EditorPrefs.GetString(AuthConstants.PREF_EXPIRY);
            
            if (DateTime.TryParse(storedExpiry, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            {
                parentAuth.expiryTimeUtc = dt;
                // Clear expired tokens immediately
                if (parentAuth.expiryTimeUtc <= DateTime.UtcNow)
                {
                    ClearTokens();
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("BentleyAuthManager_Editor: Could not parse stored expiry time, clearing tokens.");
                ClearTokens();
            }
        }
    }
    
    /// <summary>
    /// Clears all stored tokens and state
    /// </summary>
    public void ClearTokens()
    {
        parentAuth.accessToken = null;
        parentAuth.refreshToken = null;
        parentAuth.expiryTimeUtc = DateTime.MinValue;
        
        // Clear from EditorPrefs
        EditorPrefs.DeleteKey(AuthConstants.PREF_ACCESS_TOKEN);
        EditorPrefs.DeleteKey(AuthConstants.PREF_REFRESH_TOKEN);
        EditorPrefs.DeleteKey(AuthConstants.PREF_EXPIRY);
    }
    
    [Serializable]
    private class TokenResponse 
    { 
        public string access_token; 
        public string refresh_token; 
        public int expires_in; 
    }
}
