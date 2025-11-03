using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class ApiRateLimiter
{
    private static Dictionary<string, float> lastRequestTimes = new Dictionary<string, float>();
    private const float MIN_REQUEST_INTERVAL = 2.0f; // Increase to 2 seconds between requests to same endpoint
    private static float lastGlobalRequestTime = 0f;
    private const float GLOBAL_REQUEST_INTERVAL = 0.5f; // Global rate limiting

    public static bool CanMakeRequest(string endpoint)
    {
        float currentTime = (float)EditorApplication.timeSinceStartup;
        
        // Check global rate limiting
        if (currentTime - lastGlobalRequestTime < GLOBAL_REQUEST_INTERVAL)
            return false;
        
        if (lastRequestTimes.TryGetValue(endpoint, out float lastTime))
        {
            if (currentTime - lastTime < MIN_REQUEST_INTERVAL)
                return false;
        }
        
        lastRequestTimes[endpoint] = currentTime;
        lastGlobalRequestTime = currentTime;
        return true;
    }
}
