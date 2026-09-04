using UnityEngine; 
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Primary client for orchestrating mesh export workflows from Bentley's iTwin platform.
/// Handles export initiation, status monitoring, progress tracking, and file download coordination.
/// All methods return Unity coroutines for asynchronous execution in the Editor environment.
/// </summary>
public class MeshExportClient
{    
    /// <summary>
    /// Base URL for Bentley's mesh export API endpoints
    /// </summary>
    private const string BaseUrl = "https://api.bentley.com/mesh-export/";    
    
    /// <summary>
    /// Request payload for initiating a new mesh export operation
    /// </summary>
    [Serializable]
    public class StartExportRequest
    {
        /// <summary>
        /// Unique identifier of the iModel to export
        /// </summary>
        public string iModelId;
        
        /// <summary>
        /// Unique identifier of the specific changeset to export
        /// </summary>
        public string changesetId;
        
        /// <summary>
        /// Type of export format requested (e.g., "3DTILES", "GLTF")
        /// </summary>
        public string exportType;
    }
    
    /// <summary>
    /// Response wrapper for export initiation API calls
    /// </summary>
    public class StartExportResponse 
    { 
        /// <summary>
        /// The created export job details
        /// </summary>
        [JsonProperty("export")] public ExportWrapper Export { get; set; } 
    }
    
    /// <summary>
    /// Response wrapper for export status retrieval API calls
    /// </summary>
    public class GetExportResponse 
    { 
        /// <summary>
        /// The current export job details and status
        /// </summary>
        [JsonProperty("export")] public ExportWrapper Export { get; set; } 
    }
    
    /// <summary>
    /// Represents a mesh export job with its current status and download information
    /// </summary>
    public class ExportWrapper
    {
        /// <summary>
        /// Unique identifier for this export job
        /// </summary>
        [JsonProperty("id")] public string Id { get; set; }
        
        /// <summary>
        /// Current status of the export (e.g., "Queued", "Processing", "Complete", "Failed")
        /// </summary>
        [JsonProperty("status")] public string Status { get; set; }
        
        /// <summary>
        /// Human-readable name for this export job
        /// </summary>
        [JsonProperty("displayName")] public string DisplayName { get; set; }
        
        /// <summary>
        /// Navigation links for accessing export resources
        /// </summary>
        [JsonProperty("_links")] public ExportLinks Links { get; set; }
        
        /// <summary>
        /// Convenience property to access the mesh download URL
        /// </summary>
        public string Href => Links?.Mesh?.Href;
    }
    
    /// <summary>
    /// Contains navigation links for export-related resources
    /// </summary>
    public class ExportLinks 
    { 
        /// <summary>
        /// Link to the exported mesh data
        /// </summary>
        [JsonProperty("mesh")] public MeshLink Mesh { get; set; } 
    }
    
    /// <summary>
    /// Contains the URL for downloading mesh data
    /// </summary>
    public class MeshLink 
    { 
        /// <summary>
        /// Direct URL to the mesh file download
        /// </summary>
        [JsonProperty("href")] public string Href { get; set; } 
    }
    
    /// <summary>
    /// Response wrapper for listing existing exports for an iModel
    /// </summary>
    public class ListExportsResponse
    {
        /// <summary>
        /// Array of existing export jobs for the requested iModel
        /// </summary>
        [JsonProperty("exports")]
        public ExportWrapper[] Exports { get; set; }
    }

    /// <summary>
    /// Initiates a new mesh export job for the specified iModel and changeset.
    /// This coroutine sends the export request to Bentley's API and returns the export job details.
    /// </summary>
    /// <param name="accessToken">Valid OAuth access token for API authentication</param>
    /// <param name="iModelId">Unique identifier of the iModel to export</param>
    /// <param name="changesetId">Unique identifier of the changeset to export</param>
    /// <param name="exportType">Desired export format type</param>
    /// <param name="callback">Callback invoked with export result or error message</param>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator StartExportCoroutine(
        string accessToken,
        string iModelId,
        string changesetId,
        string exportType,
        Action<ExportWrapper, string> callback)
    {
        var reqObj = new StartExportRequest
        {
            iModelId    = iModelId,
            changesetId = changesetId,
            exportType  = exportType
        };
        string json = JsonConvert.SerializeObject(reqObj);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest(BaseUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(body),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        request.SetRequestHeader("Accept",       "application/vnd.bentley.itwin-platform.v1+json");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest(); // Works in Editor Coroutine

        if (request.result != UnityWebRequest.Result.Success)
        {
             string error = $"StartExport failed ({request.responseCode}): {request.error}\n{request.downloadHandler?.text}";
            Debug.LogError("MeshExportClient_Editor: " + error);
            callback?.Invoke(null, error);
        }
        else
        {
            try
            {
                 string responseText = request.downloadHandler.text;
                var resp = JsonConvert.DeserializeObject<StartExportResponse>(responseText);
                 if (resp?.Export == null) {
                     throw new JsonException("Parsed response or 'export' field is null.");
                 }
                callback?.Invoke(resp.Export, null);
            }
            catch (Exception ex)
            {
                 string error = $"Failed to parse StartExport response: {ex.Message}\nResponse JSON: {request.downloadHandler?.text}";
                Debug.LogError("MeshExportClient_Editor: " + error);
                callback?.Invoke(null, error);
            }
        }
    }

    /// <summary>
    /// Coroutine to poll the Mesh‑Export API until the export is complete.
    /// Called by the EditorWindow using EditorCoroutineUtility.
    /// </summary>
    public IEnumerator GetExportCoroutine(
    string accessToken,
    string exportId,
    Action<ExportWrapper, string> callback, // Callback parameters: ExportWrapper result, string error
    int pollIntervalSeconds = 5)            // Increased default interval
{
    string url = BaseUrl + exportId;
    int attempts = 0;

    while (true)
    {
         attempts++;
        using var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        request.SetRequestHeader("Accept",       "application/vnd.bentley.itwin-platform.v1+json");

        yield return request.SendWebRequest(); // Works in Editor Coroutine

        if (request.result != UnityWebRequest.Result.Success)
        {
             string error = $"GetExport failed ({request.responseCode}): {request.error}\n{request.downloadHandler?.text}";
            Debug.LogError("MeshExportClient_Editor: " + error);
            callback?.Invoke(null, error);
            yield break; // Exit coroutine on error
        }

        try
        {
             string responseText = request.downloadHandler.text;

            var resp = JsonConvert.DeserializeObject<GetExportResponse>(responseText);
             if (resp?.Export == null) {
                 throw new JsonException("Parsed response or 'export' field is null.");
             }
            var export = resp.Export;

            if (export.Status.Equals("complete", StringComparison.OrdinalIgnoreCase) ||
                export.Status.Equals("succeeded", StringComparison.OrdinalIgnoreCase)) // Handle 'succeeded' as well
            {
                if (string.IsNullOrEmpty(export.Href)) {
                    string error = "Export completed/succeeded but download href is missing.";
                     Debug.LogError("MeshExportClient_Editor: " + error + "\nResponse JSON: " + responseText);
                    callback?.Invoke(null, error);
                } else {
                    callback?.Invoke(export, null);
                }
                yield break; // Exit coroutine on completion
            }
             else if (export.Status.Equals("failed", StringComparison.OrdinalIgnoreCase) ||
                      export.Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
             {
                 string error = $"Export job ended with status: {export.Status}";
                 Debug.LogError("MeshExportClient_Editor: " + error + "\nResponse JSON: " + responseText);
                 callback?.Invoke(null, error);
                 yield break; // Exit coroutine on failure/cancellation
             }
             // Continue polling for other statuses like 'running', 'created', 'queued'
        }
        catch (Exception ex)
        {
             string error = $"Failed to parse GetExport response: {ex.Message}\nResponse JSON: {request.downloadHandler?.text}";
            Debug.LogError("MeshExportClient_Editor: " + error);
            callback?.Invoke(null, error);
            yield break; // Exit coroutine on parsing error
        }
         
        // Use WaitForSecondsRealtime in Editor Coroutines if precise timing is needed,
        // but WaitForSeconds is generally fine here.
        yield return new WaitForSeconds(pollIntervalSeconds);
    }
}

    /// <summary>
    /// Coroutine to get or start a mesh export job.
    /// Called by the EditorWindow using EditorCoroutineUtility.
    /// </summary>
    public IEnumerator GetOrStartExportCoroutine(
        string accessToken,
        string iModelId,
        string changesetId,
        string exportType,
        Action<ExportWrapper, string> callback)
    {
        // 1. Try to get existing export
        string url = $"{BaseUrl}?iModelId={iModelId}&exportType={exportType}";
        if (!string.IsNullOrEmpty(changesetId))
            url += $"&changesetId={changesetId}";

        bool shouldStartExport = false;
        ExportWrapper existingExport = null;

        using (var getRequest = UnityWebRequest.Get(url))
        {
            getRequest.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            getRequest.SetRequestHeader("Accept", "application/vnd.bentley.itwin-platform.v1+json");

            yield return getRequest.SendWebRequest();

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var responseText = getRequest.downloadHandler.text;
                    var listResp = JsonConvert.DeserializeObject<ListExportsResponse>(responseText);
                    if (listResp?.Exports != null && listResp.Exports.Length > 0)
                    {
                        existingExport = listResp.Exports[0];
                        // If already complete and has href, return it
                        if ((existingExport.Status.Equals("complete", StringComparison.OrdinalIgnoreCase) ||
                             existingExport.Status.Equals("succeeded", StringComparison.OrdinalIgnoreCase)) &&
                            !string.IsNullOrEmpty(existingExport.Href))
                        {
                            callback?.Invoke(existingExport, null);
                            yield break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Failed to parse existing exports: " + ex.Message);
                    shouldStartExport = true; // Continue to start export
                }
            }
            else
            {
                shouldStartExport = true; // Continue to start export
            }
        }

        if (existingExport != null && !shouldStartExport)
        {
            // Poll until complete
            yield return GetExportCoroutine(accessToken, existingExport.Id, callback);
            yield break;
        }

        // 2. Start new export
#pragma warning disable CS0219 // Variable is assigned but its value is never used
        bool done = false;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
        ExportWrapper startedExport = null;
        string startError = null;
        yield return StartExportCoroutine(accessToken, iModelId, changesetId, exportType, (result, error) =>
        {
            startedExport = result;
            startError = error;
            done = true;
        });
        if (!string.IsNullOrEmpty(startError) || startedExport == null)
        {
            callback?.Invoke(null, startError ?? "Failed to start export.");
            yield break;
        }
        // 3. Poll until complete
        yield return GetExportCoroutine(accessToken, startedExport.Id, callback);
    }
}