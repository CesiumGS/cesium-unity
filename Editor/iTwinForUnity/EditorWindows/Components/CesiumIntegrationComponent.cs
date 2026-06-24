using UnityEngine;
using UnityEditor;
using CesiumForUnity;
using Unity.EditorCoroutines.Editor;
using System.Linq;
using System.Collections;

public class CesiumIntegrationComponent
{
    private BentleyWorkflowEditorWindow parentWindow;
    
    public CesiumIntegrationComponent(BentleyWorkflowEditorWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    public void DrawCesiumSection()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Cesium Integration", BentleyUIStyles.sectionHeaderStyle);
        
        // Draw divider under header
        var dividerRect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(dividerRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(8); 
        
        EditorGUILayout.BeginVertical(BentleyUIStyles.cardStyle);
        
        // Main recommended option
        EditorGUILayout.HelpBox("Create a Cesium 3D Tileset with the exported data.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // Create button centered
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUIContent createButtonContent = new GUIContent("Create Cesium Tileset", 
            "Create a new Cesium3DTileset with the exported URL");

        if (GUILayout.Button(createButtonContent, GUILayout.Height(36), GUILayout.Width(180)))
        {
            CreateAndApplyTileset();
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);

        // Advanced options foldout
        EditorGUILayout.Space(10);
        
        // Store the advanced options state in a class field
        parentWindow.advancedCesiumOptionsVisible = EditorGUILayout.Foldout(parentWindow.advancedCesiumOptionsVisible, "Advanced Options", true);
        
        if (parentWindow.advancedCesiumOptionsVisible)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Manual Tileset Assignment", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this option to apply the URL to an existing Cesium tileset in your scene.", MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            // Target tileset field
            parentWindow.targetCesiumTileset = (Cesium3DTileset)EditorGUILayout.ObjectField(
                new GUIContent("Target Cesium Tileset", "Drag a Cesium3DTileset component from your scene here."),
                parentWindow.targetCesiumTileset,
                typeof(Cesium3DTileset),
                true);
                
            EditorGUILayout.Space(5);
            
            // Apply button
            EditorGUI.BeginDisabledGroup(parentWindow.targetCesiumTileset == null);
            if (GUILayout.Button(new GUIContent("Apply URL to Existing Tileset", 
                "Apply the exported URL to the selected Cesium Tileset"), GUILayout.Height(30)))
            {
                ApplyUrlToCesium();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }
    
    // New method to create and apply a tileset
    private void CreateAndApplyTileset()
    {
        if (string.IsNullOrEmpty(parentWindow.finalTilesetUrl))
        {
            EditorUtility.DisplayDialog("Missing URL", "No exported URL is available. Complete the export process first.", "OK");
            return;
        }
        
        // Step 1: Find or create a CesiumGeoreference
        CesiumForUnity.CesiumGeoreference georeference = null;
        
        // Check if there's an existing CesiumGeoreference in the scene
        var references = Object.FindObjectsByType<CesiumForUnity.CesiumGeoreference>(FindObjectsSortMode.None);
        if (references != null && references.Length > 0)
        {
            georeference = references[0];
        }
        else
        {
            // Create a new CesiumGeoreference
            GameObject georeferenceObj = new GameObject("CesiumGeoreference");
            georeference = Undo.AddComponent<CesiumForUnity.CesiumGeoreference>(georeferenceObj);
        }
        
        // Step 2: Create a new GameObject for the tileset
        string tilesetName = "BentleyTileset";
        
        // Get iTwin/iModel names for better naming if available
        var selectedITwin = parentWindow.myITwins.FirstOrDefault(tw => tw.id == parentWindow.selectedITwinId);
        var selectedIModel = parentWindow.myIModels.FirstOrDefault(im => im.id == parentWindow.selectedIModelId);
        
        if (selectedIModel != null && !string.IsNullOrEmpty(selectedIModel.displayName))
        {
            tilesetName = selectedIModel.displayName;
        }
        else if (selectedITwin != null && !string.IsNullOrEmpty(selectedITwin.displayName))
        {
            tilesetName = selectedITwin.displayName + "_Model";
        }
        
        GameObject tilesetObj = new GameObject(tilesetName);
        Undo.RegisterCreatedObjectUndo(tilesetObj, "Create Bentley Tileset");
        
        // Make it a child of the georeference
        Undo.SetTransformParent(tilesetObj.transform, georeference.transform, "Set Tileset Parent");
        
        // Step 3: Add Cesium3DTileset component
        var tileset = Undo.AddComponent<CesiumForUnity.Cesium3DTileset>(tilesetObj);
        
        // Step 4: Set the URL and disable physics meshes
        Undo.RecordObject(tileset, "Set Tileset URL");
        tileset.url = parentWindow.finalTilesetUrl;
        tileset.tilesetSource = CesiumForUnity.CesiumDataSource.FromUrl;
        tileset.createPhysicsMeshes = false; // Disable physics meshes by default

        // Step 5: Add metadata component
        var metadata = Undo.AddComponent<BentleyTilesetMetadata>(tilesetObj);
        
        // Step 6: Set the metadata
        var selectedITwinObj = parentWindow.myITwins.FirstOrDefault(tw => tw.id == parentWindow.selectedITwinId);
        var selectedIModelObj = parentWindow.myIModels.FirstOrDefault(im => im.id == parentWindow.selectedIModelId);
        
        // Get changeset info
        string changesetVersion = "";
        string changesetDescription = "";
        string changesetCreatedDate = "";
        
        if (!string.IsNullOrEmpty(parentWindow.changesetId) && selectedIModelObj?.changesets != null)
        {
            var changeset = selectedIModelObj.changesets.FirstOrDefault(cs => cs.id == parentWindow.changesetId);
            if (changeset != null)
            {
                changesetVersion = changeset.version;
                changesetDescription = changeset.description;
                changesetCreatedDate = changeset.createdDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        
        // Set the extended metadata with thumbnails
        Undo.RecordObject(metadata, "Set Bentley Tileset Metadata");
        metadata.SetExtendedMetadata(
            parentWindow.selectedITwinId,
            selectedITwinObj?.displayName ?? "",
            parentWindow.selectedIModelId,
            selectedIModelObj?.displayName ?? "",
            selectedIModelObj?.description ?? "",
            parentWindow.changesetId,
            changesetVersion,
            changesetDescription,
            changesetCreatedDate,
            parentWindow.finalTilesetUrl,
            selectedITwinObj?.thumbnail,
            selectedIModelObj?.thumbnail
        );
        
        EditorUtility.SetDirty(tileset);
        EditorUtility.SetDirty(metadata);
        
        // Step 7: Select and focus the new tileset using the same logic as the Focus button
        Selection.activeGameObject = tilesetObj;
        EditorGUIUtility.PingObject(tilesetObj);

        // Use the same focus logic as TilesetSceneOperations.FocusOnObject
        EditorApplication.delayCall += () => 
        {
            // Frame the object in the scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.FrameSelected();
                sceneView.Repaint();
                EditorWindow.FocusWindowIfItsOpen<SceneView>();
            }
            else
            {
                SceneView.FrameLastActiveSceneView();
            }
            
            // Place origin using the same logic as the Focus button
            EditorApplication.delayCall += () => 
            {
                EditorCoroutineUtility.StartCoroutine(DelayedPlaceOriginAtCamera(tilesetObj), parentWindow);
            };
        };        
        
    }
    
    private void ApplyUrlToCesium()
    {
        if (parentWindow.targetCesiumTileset != null && !string.IsNullOrEmpty(parentWindow.finalTilesetUrl))
        {
            // Get metadata component or add one
            var metadata = parentWindow.targetCesiumTileset.GetComponent<BentleyTilesetMetadata>();
            if (metadata == null)
            {
                metadata = Undo.AddComponent<BentleyTilesetMetadata>(parentWindow.targetCesiumTileset.gameObject);
            }
            
            // Set the URL
            Undo.RecordObject(parentWindow.targetCesiumTileset, "Set Bentley Tileset URL");
            parentWindow.targetCesiumTileset.url = parentWindow.finalTilesetUrl;
            parentWindow.targetCesiumTileset.tilesetSource = CesiumForUnity.CesiumDataSource.FromUrl;
            
            // Get selected data for metadata
            var selectedITwin = parentWindow.myITwins.FirstOrDefault(tw => tw.id == parentWindow.selectedITwinId);
            var selectedIModel = parentWindow.myIModels.FirstOrDefault(im => im.id == parentWindow.selectedIModelId);
            
            // If thumbnails aren't loaded yet, try to trigger loading
            if (selectedIModel != null && !selectedIModel.thumbnailLoaded)
            {
                if (!selectedIModel.loadingThumbnail)
                {
                    EditorCoroutineUtility.StartCoroutine(parentWindow.FetchIModelThumbnail(selectedIModel), parentWindow);
                }
            }
            
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
            
            // Set basic metadata immediately (will be updated with thumbnails later if needed)
            Undo.RecordObject(metadata, "Set Bentley Tileset Metadata");
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
            
            EditorUtility.SetDirty(parentWindow.targetCesiumTileset);
            EditorUtility.SetDirty(metadata);
            
            // If thumbnails are still loading, set up a callback to update metadata when they're ready
            if ((selectedITwin != null && selectedITwin.loadingThumbnail) || 
                (selectedIModel != null && selectedIModel.loadingThumbnail))
            {
                parentWindow.UpdateMetadataWithThumbnail(metadata, selectedITwin, selectedIModel);
            }
            
            EditorUtility.DisplayDialog("URL Applied", "The exported URL has been applied to the selected Cesium tileset.", "OK");
        }
        else if (string.IsNullOrEmpty(parentWindow.finalTilesetUrl))
        {
            EditorUtility.DisplayDialog("Missing URL", "No exported URL is available. Complete the export process first.", "OK");
        }
    }
    
    // Add this helper method to delay placing the origin at camera position (like the Focus button)
    private IEnumerator DelayedPlaceOriginAtCamera(GameObject selectedObject)
    {
        // Wait for one frame to ensure SceneView has updated
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
            // Use the same logic as the Focus button - place georeference at camera position
            CesiumForUnity.CesiumEditorUtility.PlaceGeoreferenceAtCameraPosition(georeference);
        }
    }
}
