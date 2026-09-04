using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using CesiumForUnity;

public class BentleyHelperMethods
{
    private BentleyWorkflowEditorWindow parentWindow;
    
    public BentleyHelperMethods(BentleyWorkflowEditorWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    // Helper method for creating consistent fallback descriptions
    public string GetDisplayDescription(IModel model)
    {
        // If we have a description, use it
        if (!string.IsNullOrEmpty(model.description))
            return model.description;
            
        // Create a fallback description based on what we know
        if (!string.IsNullOrEmpty(model.displayName))
            return $"iModel created for {model.displayName}";
            
        // Last resort fallback
        return $"iModel {model.id.Substring(0, Math.Min(8, model.id.Length))}...";
    }
    
    // Add this public method to the BentleyWorkflowEditorWindow class
    public string GetAuthToken()
    {
        if (parentWindow.authManager != null)
        {
            string token = parentWindow.authManager.GetCurrentAccessToken();
            // Always save token to EditorPrefs when requested
            if (!string.IsNullOrEmpty(token))
            {
                UnityEditor.EditorPrefs.SetString("Bentley_Auth_Token", token);
                return token;
            }
        }
        return null;
    }
    
    // Add this method to your BentleyWorkflowEditorWindow class
    public void UpdateMetadataWithThumbnail(BentleyTilesetMetadata metadata, ITwin selectedITwin, IModel selectedIModel)
    {
        // Check if thumbnail is loaded now
        if (selectedIModel != null && selectedIModel.thumbnailLoaded)
        {
            // Get changeset info
            string changesetVersion = "";
            string changesetDescription = "";
            string changesetCreatedDate = "";
            
            if (!string.IsNullOrEmpty(parentWindow.changesetId) && selectedIModel?.changesets != null)
            {
                var changeset = selectedIModel.changesets.FirstOrDefault(cs => cs.id == parentWindow.changesetId);
                if (changeset != null)
                {
                    changesetVersion = changeset.version;
                    changesetDescription = changeset.description;
                    changesetCreatedDate = changeset.createdDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            
            // Update the metadata now that we have thumbnails
            UnityEditor.Undo.RecordObject(metadata, "Update Bentley Tileset Metadata");
            metadata.SetExtendedMetadata(
                parentWindow.selectedITwinId,
                selectedITwin?.displayName ?? "",
                parentWindow.selectedIModelId,
                selectedIModel?.displayName ?? "",
                selectedIModel?.description ?? "",
                parentWindow.changesetId,
                changesetVersion,
                changesetDescription,
                changesetCreatedDate,
                parentWindow.finalTilesetUrl,
                selectedITwin?.thumbnail,
                selectedIModel?.thumbnail
            );
            
            UnityEditor.EditorUtility.SetDirty(metadata);
        }
        else
        {
            // If still not loaded, try again later
            UnityEditor.EditorApplication.delayCall += () => UpdateMetadataWithThumbnail(metadata, selectedITwin, selectedIModel);
        }
    }
    
    public void StopCurrentCoroutine()
    {
        if (parentWindow.currentCoroutineHandle != null)
        {
            EditorCoroutineUtility.StopCoroutine(parentWindow.currentCoroutineHandle);
            parentWindow.currentCoroutineHandle = null;
            if (parentWindow.currentState != WorkflowState.LoggedIn && parentWindow.currentState != WorkflowState.ExportComplete && parentWindow.currentState != WorkflowState.Error)
            {
                parentWindow.UpdateLoginState();
                parentWindow.statusMessage = "Operation cancelled by user.";
            }
            parentWindow.Repaint();
        }
    }
    
    // Add this helper method to delay placing the origin
    public IEnumerator DelayedPlaceOrigin(GameObject selectedObject)
    {
        // Wait for two frames to ensure SceneView has fully updated
        yield return null;
        yield return null;
        
        // Find CesiumGeoreference in parent hierarchy of the selected object
        Transform current = selectedObject.transform;
        CesiumForUnity.CesiumGeoreference georeference = null;
        while (current != null && georeference == null)
        {
            georeference = current.GetComponent<CesiumForUnity.CesiumGeoreference>();
            if (georeference == null)
                current = current.parent;
        }

        if (georeference != null)
        {
            CesiumForUnity.CesiumEditorUtility.PlaceGeoreferenceAtCameraPosition(georeference);
        }
    }
}
