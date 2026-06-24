using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Refactored BentleyAuthManager using component-based architecture
/// Maintains exact same public API for backward compatibility
/// </summary>
public class BentleyAuthManager
{
    [Header("Bentley IMS Configuration")]
    public string clientId;
    public string redirectUri;

    // Core components
    private readonly AuthStateManager stateManager;
    private readonly BentleyAuthCore authCore;
    private readonly MainThreadSynchronizer synchronizer;

    // Internal state (exposed for components)
    internal string accessToken;
    internal string refreshToken;
    internal DateTime expiryTimeUtc;

    // Legacy callback support
    public Action<bool, string> OnLoginComplete;
    public Action<string> RequestExchangeCode;

    // Current authentication configuration
    private AuthConfigManager.AuthConfig currentConfig;

    public BentleyAuthManager()
    {
        // Initialize components
        stateManager = new AuthStateManager(this);
        authCore = new BentleyAuthCore(this, stateManager);
        synchronizer = new MainThreadSynchronizer();

        // Load stored client ID and tokens
        clientId = EditorPrefs.GetString(AuthConstants.PREF_CLIENT_ID, "");
        redirectUri = EditorPrefs.GetString(AuthConstants.PREF_REDIRECT_URI, AuthConstants.DEFAULT_REDIRECT_URI);
        stateManager.LoadStoredTokens();

        // Set default configuration
        UpdateAuthConfiguration();
    }

    /// <summary>
    /// Updates authentication configuration based on current client ID and redirect URI
    /// </summary>
    private void UpdateAuthConfiguration()
    {
        if (!string.IsNullOrEmpty(clientId))
        {
            currentConfig = AuthConfigManager.CreateConfig(
                clientId,
                !string.IsNullOrEmpty(redirectUri) ? redirectUri : AuthConstants.DEFAULT_REDIRECT_URI,
                AuthConstants.DEFAULT_SCOPES
            );
        }
        else
        {
            currentConfig = AuthConfigManager.CreateConfig(
                "native-2686", // Default mesh export client ID
                !string.IsNullOrEmpty(redirectUri) ? redirectUri : AuthConstants.DEFAULT_REDIRECT_URI,
                AuthConstants.MESH_EXPORT_SCOPE
            );
            clientId = currentConfig.ClientId;
        }
    }

    /// <summary>
    /// Call this when Client ID changes in the EditorWindow
    /// </summary>
    public void SaveClientId()
    {
        EditorPrefs.SetString(AuthConstants.PREF_CLIENT_ID, clientId);
        UpdateAuthConfiguration();
    }

    /// <summary>
    /// Call this when Redirect URI changes in the EditorWindow
    /// </summary>
    public void SaveRedirectUri()
    {
        EditorPrefs.SetString(AuthConstants.PREF_REDIRECT_URI, redirectUri);
        UpdateAuthConfiguration();
    }

    /// <summary>
    /// Gets the current valid access token, returning null if expired/invalid.
    /// Does NOT automatically trigger refresh or login.
    /// </summary>
    public string GetCurrentAccessToken()
    {
        return stateManager.GetCurrentAccessToken();
    }

    /// <summary>
    /// Determines if user is logged in based on valid tokens
    /// </summary>
    public bool IsLoggedIn()
    {
        return stateManager.IsLoggedIn();
    }

    /// <summary>
    /// Gets the token expiry time in UTC
    /// </summary>
    public DateTime GetExpiryTimeUtc()
    {
        return stateManager.GetExpiryTimeUtc();
    }

    /// <summary>
    /// Legacy coroutine interface for getting access token
    /// Tries current token, then refresh, then initiates login if needed
    /// </summary>
    public IEnumerator GetAccessTokenCoroutine(Action<string, string> callback)
    {
        // Convert async flow to coroutine for Unity compatibility
        var task = GetAccessTokenAsync();
        
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception != null)
        {
            callback?.Invoke(null, task.Exception.GetBaseException().Message);
        }
        else
        {
            callback?.Invoke(task.Result, null);
        }
    }

    /// <summary>
    /// Async method to get a valid access token
    /// </summary>
    public async Task<string> GetAccessTokenAsync()
    {
        try
        {
            // 1. Check current token
            string currentToken = GetCurrentAccessToken();
            if (currentToken != null)
            {
                return currentToken;
            }

            // 2. Try refreshing if refresh token available
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshResult = await TryRefreshTokenAsync();
                if (refreshResult.Success)
                {
                    return refreshResult.Token;
                }
                else
                {
                    Debug.LogWarning("BentleyAuthManager_Editor: Refresh failed. Proceeding to login.");
                    if (!string.IsNullOrEmpty(refreshResult.Error))
                    {
                        Logout(); // Clear potentially invalid tokens
                    }
                }
            }

            // 3. Initiate full login flow
            var loginResult = await PerformLoginAsync();
            if (loginResult.Success)
            {
                return loginResult.Token;
            }
            else
            {
                throw new InvalidOperationException($"Login failed: {loginResult.Error}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Error getting access token: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Legacy coroutine interface for token refresh
    /// </summary>
    public IEnumerator RefreshTokenCoroutine(Action<bool, string> callback)
    {
        var task = TryRefreshTokenAsync();
        
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception != null)
        {
            callback?.Invoke(false, task.Exception.GetBaseException().Message);
        }
        else
        {
            var result = task.Result;
            callback?.Invoke(result.Success, result.Error);
        }
    }

    /// <summary>
    /// Attempts to refresh the access token
    /// </summary>
    private async Task<AuthResult> TryRefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return new AuthResult { Success = false, Error = "No refresh token available" };
        }

        try
        {
            UpdateAuthConfiguration();
            
            var taskSource = new TaskCompletionSource<AuthResult>();

            await authCore.RefreshTokenAsync(
                currentConfig,
                onComplete: token => taskSource.SetResult(new AuthResult { Success = true, Token = token }),
                onError: error => taskSource.SetResult(new AuthResult { Success = false, Error = error })
            );

            return await taskSource.Task;
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// Legacy method for starting login (browser-based)
    /// </summary>
    public void StartLogin()
    {
        if (string.IsNullOrEmpty(clientId))
        {
            Debug.LogError("BentleyAuthManager_Editor: Client ID is not set!");
            OnLoginComplete?.Invoke(false, "Client ID is not set.");
            return;
        }

        // Start async login and handle callbacks
        _ = PerformLoginAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                synchronizer.ScheduleOnMainThread(() => 
                    OnLoginComplete?.Invoke(false, task.Exception?.GetBaseException()?.Message ?? "Login failed"));
            }
            else
            {
                var result = task.Result;
                synchronizer.ScheduleOnMainThread(() => 
                    OnLoginComplete?.Invoke(result.Success, result.Error));
            }
        });
    }

    /// <summary>
    /// Performs the complete login flow
    /// </summary>
    private async Task<AuthResult> PerformLoginAsync()
    {
        try
        {
            UpdateAuthConfiguration();
            
            if (!AuthConfigManager.ValidateConfig(currentConfig))
            {
                return new AuthResult { Success = false, Error = "Invalid authentication configuration" };
            }

            var taskSource = new TaskCompletionSource<AuthResult>();

            bool success = await authCore.StartAuthenticationFlow(
                currentConfig,
                onComplete: token => taskSource.SetResult(new AuthResult { Success = true, Token = token }),
                onError: error => taskSource.SetResult(new AuthResult { Success = false, Error = error })
            );

            if (!success)
            {
                return new AuthResult { Success = false, Error = "Failed to start authentication flow" };
            }

            return await taskSource.Task;
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// Legacy coroutine interface for code exchange
    /// </summary>
    public IEnumerator ExchangeCodeCoroutine(string authorizationCode, Action<bool, string> callback)
    {
        // This method is now handled internally by the authentication flow
        // Maintaining for backward compatibility but implementation is simplified
        callback?.Invoke(true, "Code exchange handled internally");
        yield break;
    }

    /// <summary>
    /// Clears all tokens and performs logout
    /// </summary>
    public void Logout()
    {
        authCore.Logout();
    }

    /// <summary>
    /// Legacy cleanup method
    /// </summary>
    public void Cleanup()
    {
        authCore.Logout();
    }

    /// <summary>
    /// Cleanup method that only stops HTTP listener without clearing tokens
    /// Use this when closing editor windows to preserve authentication across sessions
    /// </summary>
    public void CleanupWithoutLogout()
    {
        try
        {
            authCore.StopListener();
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Error during cleanup: {ex.Message}");
        }
    }

    /// <summary>
    /// Legacy method for stopping HTTP listener
    /// </summary>
    private void StopHttpListener()
    {
        // Now handled internally by HttpListenerManager
    }

    /// <summary>
    /// Legacy method for clearing tokens
    /// </summary>
    private void ClearTokens()
    {
        stateManager.ClearTokens();
    }

    /// <summary>
    /// Result structure for authentication operations
    /// </summary>
    private class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Error { get; set; }
    }
}
