using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Core authentication logic coordinator that orchestrates the OAuth flow
/// </summary>
public class BentleyAuthCore
{
    private readonly BentleyAuthManager parentAuth;
    private readonly AuthStateManager stateManager;
    private readonly PKCEManager pkceManager;
    private readonly HttpListenerManager httpManager;
    private readonly TokenExchangeClient tokenClient;
    private readonly MainThreadSynchronizer synchronizer;
    
    public BentleyAuthCore(BentleyAuthManager parentAuth, AuthStateManager stateManager)
    {
        this.parentAuth = parentAuth;
        this.stateManager = stateManager;
        this.pkceManager = new PKCEManager();
        this.httpManager = new HttpListenerManager();
        this.tokenClient = new TokenExchangeClient();
        this.synchronizer = new MainThreadSynchronizer();
    }
    
    /// <summary>
    /// Initiates the complete OAuth authentication flow
    /// </summary>
    public async Task<bool> StartAuthenticationFlow(AuthConfigManager.AuthConfig config, Action<string> onComplete, Action<string> onError)
    {
        try
        {
            if (!AuthConfigManager.ValidateConfig(config))
            {
                onError?.Invoke("Invalid authentication configuration");
                return false;
            }
            
            // Generate PKCE parameters
            string codeVerifier = pkceManager.GenerateCodeVerifier();
            string codeChallenge = pkceManager.GenerateCodeChallenge(codeVerifier);
            string state = pkceManager.GenerateSecureState();
            
            // Start HTTP listener
            if (!httpManager.StartListener(config.RedirectUri).Result)
            {
                onError?.Invoke("Failed to start HTTP listener");
                return false;
            }
            
            // Generate and open authorization URL
            string authUrl = config.GenerateAuthorizationUrl(state, codeChallenge);
            Application.OpenURL(authUrl);
            
            // Wait for callback
            var callbackResult = await httpManager.WaitForCallback();
            
            if (callbackResult.IsError)
            {
                await synchronizer.ExecuteOnMainThread(() => onError?.Invoke(callbackResult.Error));
                return false;
            }
            
            // Validate state parameter
            if (callbackResult.State != state)
            {
                await synchronizer.ExecuteOnMainThread(() => onError?.Invoke("State parameter mismatch - possible CSRF attack"));
                return false;
            }
            
            // Exchange authorization code for tokens
            string tokenResponse = await tokenClient.ExchangeCodeForTokens(config, callbackResult.Code, codeVerifier);
            
            if (string.IsNullOrEmpty(tokenResponse))
            {
                await synchronizer.ExecuteOnMainThread(() => onError?.Invoke("Failed to exchange authorization code for tokens"));
                return false;
            }
            
            // Process token response on main thread
            await synchronizer.ExecuteOnMainThread(() =>
            {
                try
                {
                    stateManager.ProcessTokenResponse(tokenResponse);
                    onComplete?.Invoke(stateManager.GetCurrentAccessToken());
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Failed to process token response: {ex.Message}");
                }
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Authentication flow failed: {ex.Message}");
            await synchronizer.ExecuteOnMainThread(() => onError?.Invoke($"Authentication failed: {ex.Message}"));
            return false;
        }
        finally
        {
            httpManager.StopListener();
        }
    }
    
    /// <summary>
    /// Attempts to refresh the access token using stored refresh token
    /// </summary>
    public async Task<bool> RefreshTokenAsync(AuthConfigManager.AuthConfig config, Action<string> onComplete, Action<string> onError)
    {
        try
        {
            if (string.IsNullOrEmpty(parentAuth.refreshToken))
            {
                await synchronizer.ExecuteOnMainThread(() => onError?.Invoke("No refresh token available"));
                return false;
            }
            
            string tokenResponse = await tokenClient.RefreshAccessToken(config, parentAuth.refreshToken);
            
            if (string.IsNullOrEmpty(tokenResponse))
            {
                await synchronizer.ExecuteOnMainThread(() => onError?.Invoke("Failed to refresh access token"));
                return false;
            }
            
            await synchronizer.ExecuteOnMainThread(() =>
            {
                try
                {
                    stateManager.ProcessTokenResponse(tokenResponse);
                    onComplete?.Invoke(stateManager.GetCurrentAccessToken());
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Failed to process refresh token response: {ex.Message}");
                }
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Token refresh failed: {ex.Message}");
            await synchronizer.ExecuteOnMainThread(() => onError?.Invoke($"Token refresh failed: {ex.Message}"));
            return false;
        }
    }
    
    /// <summary>
    /// Performs complete logout including token cleanup
    /// </summary>
    public void Logout()
    {
        try
        {
            stateManager.ClearTokens();
            httpManager.StopListener();
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Error during logout: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Stops only the HTTP listener without clearing tokens
    /// </summary>
    public void StopListener()
    {
        try
        {
            httpManager.StopListener();
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Error stopping HTTP listener: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Represents the result of an OAuth callback
    /// </summary>
    public class CallbackResult
    {
        public bool IsError => !string.IsNullOrEmpty(Error);
        public string Code { get; set; }
        public string State { get; set; }
        public string Error { get; set; }
    }
}
