using UnityEngine;
using UnityEditor;
using System.Collections;
using CesiumForUnity;
using Unity.EditorCoroutines.Editor;

public static class TilesetSceneOperations
{
    /// <summary>
    /// Selects and pings a GameObject in the hierarchy
    /// </summary>
    public static void SelectAndPingObject(GameObject gameObject)
    {
        Selection.activeGameObject = gameObject;
        EditorGUIUtility.PingObject(gameObject);
    }
    
    /// <summary>
    /// Focuses on a GameObject in the scene view and places the origin
    /// </summary>
    public static void FocusOnObject(GameObject gameObject, EditorWindow editorWindow)
    {
        // FIRST STEP: Select the object in hierarchy and ensure the selection is processed
        Selection.activeGameObject = gameObject;
        EditorGUIUtility.PingObject(gameObject);
        
        // Give Unity a moment to process the selection, then handle the framing and origin placement
        EditorApplication.delayCall += () => 
        {
            // SECOND STEP: Frame the object in the scene view
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
            
            // THIRD STEP: Wait for framing to complete, then place origin
            EditorCoroutineUtility.StartCoroutine(DelayedPlaceOrigin(gameObject), editorWindow);
        };
    }
    
    /// <summary>
    /// Opens the iTwin Platform web URL for the given metadata
    /// </summary>
    public static void OpenWebUrl(BentleyTilesetMetadata metadata)
    {
        string url = $"https://developer.bentley.com/my-itwins/{metadata.iTwinId}/{metadata.iModelId}/view";
        Application.OpenURL(url);
    }
    
    /// <summary>
    /// Delayed coroutine to place the origin after scene view framing
    /// </summary>
    private static IEnumerator DelayedPlaceOrigin(GameObject selectedObject)
    {
        // Wait for one frame to ensure SceneView has updated
        yield return null;
        
        // Find CesiumGeoreference in parent hierarchy of the selected object
        Transform current = selectedObject.transform;
        CesiumGeoreference georeference = null;
        while (current != null && georeference == null)
        {
            georeference = current.GetComponent<CesiumGeoreference>();
            if (georeference == null)
                current = current.parent;
        }

        if (georeference != null)
        {
            CesiumEditorUtility.PlaceGeoreferenceAtCameraPosition(georeference);
        }
    }
}
