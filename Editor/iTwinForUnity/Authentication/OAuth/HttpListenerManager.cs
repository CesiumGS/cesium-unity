using System;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

/// <summary>
/// Manages HTTP listener for OAuth callback handling
/// </summary>
public class HttpListenerManager
{
    private HttpListener listener;
    private TaskCompletionSource<BentleyAuthCore.CallbackResult> callbackCompletion;
    private string redirectUri;
    
    /// <summary>
    /// Parse query string into a NameValueCollection (Unity-compatible replacement for HttpUtility.ParseQueryString)
    /// </summary>
    private static NameValueCollection ParseQueryString(string query)
    {
        var result = new NameValueCollection();
        
        if (string.IsNullOrEmpty(query))
            return result;
            
        // Remove leading '?' if present
        if (query.StartsWith("?"))
            query = query.Substring(1);
            
        string[] pairs = query.Split('&');
        foreach (string pair in pairs)
        {
            if (string.IsNullOrEmpty(pair)) continue;
            
            string[] parts = pair.Split('=');
            if (parts.Length >= 2)
            {
                string key = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(parts[0]);
                string value = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(parts[1]);
                result[key] = value;
            }
            else if (parts.Length == 1)
            {
                string key = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(parts[0]);
                result[key] = "";
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Starts the HTTP listener on the specified redirect URI
    /// </summary>
    public Task<bool> StartListener(string redirectUri)
    {
        try
        {
            this.redirectUri = redirectUri;
            
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out Uri uri))
            {
                Debug.LogError($"BentleyAuthManager_Editor: Invalid redirect URI: {redirectUri}");
                return Task.FromResult(false);
            }
            
            listener = new HttpListener();
            listener.Prefixes.Add($"{uri.Scheme}://{uri.Host}:{uri.Port}/");
            
            listener.Start();
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Failed to start HTTP listener: {ex.Message}");
            return Task.FromResult(false);
        }
    }
    
    /// <summary>
    /// Waits for the OAuth callback and returns the result
    /// </summary>
    public async Task<BentleyAuthCore.CallbackResult> WaitForCallback()
    {
        if (listener == null || !listener.IsListening)
        {
            return new BentleyAuthCore.CallbackResult { Error = "HTTP listener not started" };
        }
        
        callbackCompletion = new TaskCompletionSource<BentleyAuthCore.CallbackResult>();
        
        try
        {
            // Start listening for requests in background
            _ = Task.Run(async () =>
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    await ProcessCallback(context);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"BentleyAuthManager_Editor: Error processing callback: {ex.Message}");
                    callbackCompletion?.TrySetResult(new BentleyAuthCore.CallbackResult { Error = ex.Message });
                }
            });
            
            return await callbackCompletion.Task;
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Error waiting for callback: {ex.Message}");
            return new BentleyAuthCore.CallbackResult { Error = ex.Message };
        }
    }
    
    /// <summary>
    /// Processes the incoming OAuth callback request
    /// </summary>
    private async Task ProcessCallback(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        try
        {
            // Parse query parameters
            var queryParams = ParseQueryString(request.Url.Query);
            
            var result = new BentleyAuthCore.CallbackResult();
            
            // Check for error in callback
            if (!string.IsNullOrEmpty(queryParams["error"]))
            {
                result.Error = $"OAuth error: {queryParams["error"]} - {queryParams["error_description"]}";
                await SendHtmlResponse(response, AuthConstants.ERROR_RESPONSE_HTML);
            }
            else if (!string.IsNullOrEmpty(queryParams["code"]))
            {
                result.Code = queryParams["code"];
                result.State = queryParams["state"];
                await SendHtmlResponse(response, AuthConstants.SUCCESS_RESPONSE_HTML);
            }
            else
            {
                result.Error = "No authorization code or error received in callback";
                await SendHtmlResponse(response, AuthConstants.ERROR_RESPONSE_HTML);
            }
            
            callbackCompletion?.TrySetResult(result);
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Error processing OAuth callback: {ex.Message}");
            
            try
            {
                await SendHtmlResponse(response, AuthConstants.ERROR_RESPONSE_HTML);
            }
            catch { }
            
            callbackCompletion?.TrySetResult(new BentleyAuthCore.CallbackResult { Error = ex.Message });
        }
    }
    
    /// <summary>
    /// Sends HTML response to the browser
    /// </summary>
    private async Task SendHtmlResponse(HttpListenerResponse response, string html)
    {
        try
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";
            response.StatusCode = 200;
            
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Error sending HTML response: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Stops the HTTP listener
    /// </summary>
    public void StopListener()
    {
        try
        {
            if (listener?.IsListening == true)
            {
                listener.Stop();
            }
            
            listener?.Close();
            listener = null;
            
            // Cancel any pending callback
            callbackCompletion?.TrySetCanceled();
            callbackCompletion = null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"BentleyAuthManager_Editor: Error stopping HTTP listener: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Checks if the listener is currently running
    /// </summary>
    public bool IsListening => listener?.IsListening == true;
}
