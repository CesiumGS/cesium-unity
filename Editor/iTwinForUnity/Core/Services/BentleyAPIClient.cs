using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

/// <summary>
/// Handles all communication with Bentley's iTwin platform APIs.
/// This service class manages HTTP requests for iTwins, iModels, changesets, and thumbnails.
/// All methods return Unity coroutines for asynchronous execution in the Editor.
/// </summary>
public class BentleyAPIClient
{
    /// <summary>
    /// Reference to the parent editor window for state updates and UI refreshing
    /// </summary>
    private BentleyWorkflowEditorWindow parentWindow;
    
    /// <summary>
    /// Initializes the API client with a reference to the parent editor window.
    /// </summary>
    /// <param name="parentWindow">The editor window that will receive API responses and state updates</param>
    public BentleyAPIClient(BentleyWorkflowEditorWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Retrieves all iTwins accessible to the authenticated user.
    /// Filters out inactive iTwins and resets pagination state.
    /// Updates the parent window's state to SelectITwin on success or Error on failure.
    /// </summary>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator GetMyITwinsCoroutine()
    {
        // Reset pagination when loading iTwins
        parentWindow.iTwinsCurrentPage = 0;
    
        string token = parentWindow.authManager.GetCurrentAccessToken();
        var req = UnityWebRequest.Get("https://api.bentley.com/itwins?includeInactive=false");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.SetRequestHeader("Accept", "application/vnd.bentley.itwin-platform.v1+json");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<ITwinsResponse>(req.downloadHandler.text);
            parentWindow.myITwins = response.iTwins;
            
            // Don't prefetch all thumbnails - we'll load them on demand
            // as each page is displayed
            
            parentWindow.currentState = WorkflowState.SelectITwin;
        }
        else
        {
            parentWindow.statusMessage = $"Failed to fetch iTwins: {req.error}";
            parentWindow.currentState = WorkflowState.Error;
        }
        parentWindow.currentCoroutineHandle = null;
        parentWindow.Repaint();
    }

    /// <summary>
    /// Fetches the thumbnail image for a specific iTwin asynchronously.
    /// Sets the loading state on the iTwin object and updates it with the downloaded texture.
    /// </summary>
    /// <param name="twin">The iTwin object to fetch the thumbnail for</param>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator FetchITwinThumbnail(ITwin twin)
    {
        twin.loadingThumbnail = true;
        var url = $"https://api.bentley.com/itwins/{twin.id}/image";
        var req = UnityWebRequestTexture.GetTexture(url);
        req.SetRequestHeader("Authorization", $"Bearer {parentWindow.authManager.GetCurrentAccessToken()}");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            twin.thumbnail = DownloadHandlerTexture.GetContent(req);
        twin.loadingThumbnail = false;

        // Mark thumbnail as loaded
        twin.thumbnailLoaded = true;
    }

    /// <summary>
    /// Retrieves all iModels within a specific iTwin.
    /// Uses the v2 API format and resets pagination state for the new dataset.
    /// Updates the parent window's state to SelectIModel on success or Error on failure.
    /// </summary>
    /// <param name="itwinId">The unique identifier of the iTwin to query</param>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator GetITwinIModelsCoroutine(string itwinId)
    {
        // Reset pagination when loading new iModels
        parentWindow.iModelsCurrentPage = 0;

        string token = parentWindow.authManager.GetCurrentAccessToken();

        // Update to use v2 API format exactly like the web version
        var req = UnityWebRequest.Get($"https://api.bentley.com/imodels?iTwinId={itwinId}");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.SetRequestHeader("Accept", "application/vnd.bentley.itwin-platform.v2+json"); // Match web version
        req.SetRequestHeader("Prefer", "return=representation"); // Add this header to match web

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<IModelsResponse>(req.downloadHandler.text);
                parentWindow.myIModels = response.iModels;
                
                
                foreach (var model in parentWindow.myIModels)
                {
                    // If any model has a missing description, we'll flag it
                    if (string.IsNullOrEmpty(model.description))
                    {
                        // Initialize with a default message
                        model.description = $"iModel created for {model.displayName}";
                    }
                }
                
                parentWindow.currentState = WorkflowState.SelectIModel;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing iModels response: {ex.Message}");
                parentWindow.statusMessage = "Failed to parse iModels data";
                parentWindow.currentState = WorkflowState.Error;
            }
        }
        else if (req.result == UnityWebRequest.Result.ProtocolError && req.responseCode == 429)
        {
            // Specific handling for rate limit error
            Debug.LogWarning("Rate limit hit when fetching iModels. Please try again in a few minutes.");
            parentWindow.statusMessage = "Rate limit exceeded. Please try again later.";
            parentWindow.currentState = WorkflowState.LoggedIn;  // Return to logged in state instead of error
            parentWindow.currentCoroutineHandle = null;
        }
        else
        {
            parentWindow.statusMessage = $"Failed to fetch iModels: {req.error}";
            parentWindow.currentState = WorkflowState.Error;
        }
        parentWindow.currentCoroutineHandle = null;
        parentWindow.Repaint();
    }

    /// <summary>
    /// Fetches the thumbnail image for a specific iModel asynchronously.
    /// Includes optimization to avoid duplicate requests and rate limiting handling.
    /// Implements throttling to prevent excessive UI repaints during bulk thumbnail loading.
    /// </summary>
    /// <param name="model">The iModel object to fetch the thumbnail for</param>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator FetchIModelThumbnail(IModel model)
    {
        if (model.loadingThumbnail || model.thumbnail != null)
            yield break;
            
        model.loadingThumbnail = true;
        yield return new WaitForSeconds(0.2f);

        var url = $"https://api.bentley.com/imodels/{model.id}/thumbnail";
        var req = UnityWebRequestTexture.GetTexture(url);
        req.SetRequestHeader("Authorization", $"Bearer {parentWindow.authManager.GetCurrentAccessToken()}");
        yield return req.SendWebRequest();
        
        if (req.result == UnityWebRequest.Result.Success)
        {
            model.thumbnail = DownloadHandlerTexture.GetContent(req);
        }
        else if (req.result == UnityWebRequest.Result.ProtocolError && req.responseCode == 429)
        {
            yield return new WaitForSeconds(2.0f);
        }
        
        model.loadingThumbnail = false;
        model.thumbnailLoaded = true; // Mark as loaded regardless of success/failure
        
        // Throttle repaints for performance
        if (UnityEditor.EditorApplication.timeSinceStartup - parentWindow.lastRepaintTime > 0.05f)
        {
            parentWindow.lastRepaintTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
            parentWindow.Repaint();
        }
    }

    /// <summary>
    /// Fetches detailed information for a specific iModel, including description and metadata.
    /// Includes optimization to avoid duplicate requests and handles API rate limiting.
    /// Updates the iModel object with the retrieved description data.
    /// </summary>
    /// <param name="model">The iModel object to fetch details for</param>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator FetchIModelDetailsCoroutine(IModel model)
    {
        if (model.loadingDetails || !string.IsNullOrEmpty(model.description))
            yield break;
            
        model.loadingDetails = true;
        yield return new WaitForSeconds(0.2f);
        
        var url = $"https://api.bentley.com/imodels/{model.id}";
        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {parentWindow.authManager.GetCurrentAccessToken()}");
        req.SetRequestHeader("Accept", "application/vnd.bentley.itwin-platform.v1+json");
        
        yield return req.SendWebRequest();
        
        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<IModelDetailsResponse>(req.downloadHandler.text);
                if (response?.iModel != null && !string.IsNullOrEmpty(response.iModel.description))
                {
                    model.description = response.iModel.description;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error parsing iModel details: {ex.Message}");
            }
        }
        else if (req.result == UnityWebRequest.Result.ProtocolError && req.responseCode == 429)
        {
            yield return new WaitForSeconds(2.0f);
        }
        else
        {
            Debug.LogWarning($"Failed to fetch iModel details: {req.error}");
        }
        
        model.loadingDetails = false;
        model.detailsLoaded = true; // Mark as loaded regardless
        
        // Throttle repaints for performance
        if (UnityEditor.EditorApplication.timeSinceStartup - parentWindow.lastRepaintTime > 0.05f)
        {
            parentWindow.lastRepaintTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
            parentWindow.Repaint();
        }
    }

    /// <summary>
    /// Fetches changesets for a specific iModel, retrieving up to 20 most recent changesets.
    /// Avoids duplicate requests and sorts changesets by creation date (newest first).
    /// Updates the iModel object with the retrieved changesets data.
    /// </summary>
    /// <param name="model">The iModel object to fetch changesets for</param>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator FetchIModelChangesets(IModel model)
    {
        if (model.loadingChangesets || (model.changesets != null && model.changesets.Count > 0))
            yield break;
        
        model.loadingChangesets = true;
        
        var url = $"https://api.bentley.com/imodels/{model.id}/changesets?$top=20";
        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {parentWindow.authManager.GetCurrentAccessToken()}");
        req.SetRequestHeader("Accept", "application/vnd.bentley.itwin-platform.v2+json");
        req.SetRequestHeader("Prefer", "return=representation");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<ChangeSetsResponse>(req.downloadHandler.text);
                model.changesets = response.changesets ?? new List<ChangeSet>();
                
                // Sort by creation date, newest first (to match web UI behavior)
                model.changesets = model.changesets.OrderByDescending(cs => cs.createdDate).ToList();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error parsing changesets: {ex.Message}");
                model.changesets = new List<ChangeSet>();
            }
        }
        else if (req.result == UnityWebRequest.Result.ProtocolError && req.responseCode == 429)
        {
            yield return new WaitForSeconds(2.0f);
        }
        else
        {
            Debug.LogWarning($"Failed to fetch changesets: {req.error}");
            model.changesets = new List<ChangeSet>();
        }
        
        model.loadingChangesets = false;
        
        // Throttle repaints for performance
        if (UnityEditor.EditorApplication.timeSinceStartup - parentWindow.lastRepaintTime > 0.05f)
        {
            parentWindow.lastRepaintTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
            parentWindow.Repaint();
        }
    }

    /// <summary>
    /// Legacy method for retrieving changesets by iModel ID.
    /// Retrieves up to 20 changesets and updates the parent window's state.
    /// Consider using FetchIModelChangesets for new implementations.
    /// </summary>
    /// <param name="imodelId">The unique identifier of the iModel</param>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator GetChangeSetsCoroutine(string imodelId)
    {
        string token = parentWindow.authManager.GetCurrentAccessToken();
        
        var req = UnityWebRequest.Get($"https://api.bentley.com/imodels/{imodelId}/changesets?$top=20");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.SetRequestHeader("Accept", "application/vnd.bentley.itwin-platform.v2+json");
        req.SetRequestHeader("Prefer", "return=representation");
        
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<ChangeSetsResponse>(req.downloadHandler.text);
            parentWindow.myChangeSets = response.changesets;
            parentWindow.currentState = WorkflowState.StartingExport;
        }
        else
        {
            parentWindow.statusMessage = $"Failed to fetch changesets: {req.error}";
            parentWindow.currentState = WorkflowState.Error;
        }
        parentWindow.currentCoroutineHandle = null;
        parentWindow.Repaint();
    }

    /// <summary>
    /// Orchestrates the complete mesh export workflow from start to finish.
    /// Handles export initiation, progress monitoring, and final tileset URL retrieval.
    /// Updates the parent window's state throughout the process to reflect current status.
    /// </summary>
    /// <returns>Unity coroutine for asynchronous execution</returns>
    public IEnumerator RunFullExportWorkflowCoroutine()
    {
        parentWindow.finalTilesetUrl = "";
        string currentAccessToken = parentWindow.authManager.GetCurrentAccessToken();
        string exportIModelId = !string.IsNullOrEmpty(parentWindow.selectedIModelId) ? parentWindow.selectedIModelId : parentWindow.iModelId;

        parentWindow.currentState = WorkflowState.StartingExport;
        parentWindow.statusMessage = $"Checking for existing export or starting new export for iModel '{exportIModelId}'...";
        parentWindow.Repaint();

        MeshExportClient.ExportWrapper exportResult = null;
        string exportError = null;

        // Use GetOrStartExportCoroutine to check for existing exports first
        var exportOp = parentWindow.meshClient.GetOrStartExportCoroutine(
            currentAccessToken, 
            exportIModelId, 
            parentWindow.changesetId, 
            parentWindow.exportType,
            (result, error) => {
                exportResult = result;
                exportError = error;
            }
        );
        yield return EditorCoroutineUtility.StartCoroutine(exportOp, parentWindow);

        if (!string.IsNullOrEmpty(exportError))
        {
            parentWindow.statusMessage = $"Export failed: {exportError}";
            parentWindow.currentState = WorkflowState.Error;
            parentWindow.currentCoroutineHandle = null;
            parentWindow.Repaint();
            yield break;
        }
        
        if (exportResult == null || string.IsNullOrEmpty(exportResult.Href))
        {
            parentWindow.statusMessage = "Export job started but no URL returned. Check the Bentley iTwin portal for job status.";
            parentWindow.currentState = WorkflowState.Error;
            parentWindow.currentCoroutineHandle = null;
            parentWindow.Repaint();
            yield break;
        }

        string downloadHref = exportResult.Href;
        parentWindow.statusMessage = $"Export started! Download Href: {downloadHref}";

        try
        {
            int qIndex = downloadHref.IndexOf('?');
            if (qIndex >= 0)
                parentWindow.finalTilesetUrl = downloadHref.Insert(qIndex, "/tileset.json");
            else
                parentWindow.finalTilesetUrl = downloadHref.TrimEnd('/') + "/tileset.json";

            parentWindow.statusMessage = "Export job initiated successfully! Tileset URL ready.";
            parentWindow.currentState = WorkflowState.ExportComplete;
        }
        catch (Exception ex)
        {
            parentWindow.statusMessage = $"Error building final URL: {ex.Message}";
            parentWindow.currentState = WorkflowState.Error;
            parentWindow.finalTilesetUrl = "";
        }

        parentWindow.currentCoroutineHandle = null;
        parentWindow.Repaint();
    }
}
