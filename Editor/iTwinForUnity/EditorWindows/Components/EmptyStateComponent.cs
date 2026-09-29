using UnityEngine;
using UnityEditor;

public class EmptyStateComponent
{
    private BentleyTilesetsWindow parentWindow;
    
    public EmptyStateComponent(BentleyTilesetsWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Draws the empty state display
    /// </summary>
    public void DrawEmptyState(string title, string description)
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        GUILayout.FlexibleSpace();
        
        // Title with larger font
        GUILayout.Label(title, new GUIStyle(TilesetsUIStyles.emptyStateStyle) { fontSize = 16 });
        GUILayout.Space(8);
        
        // Description
        GUILayout.Label(description, TilesetsUIStyles.emptyStateStyle);
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// Draws a loading spinner state
    /// </summary>
    public void DrawLoadingState(string message = "Loading...")
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        GUILayout.FlexibleSpace();
        
        // Get current spinner frame from controller
        var controller = new TilesetsWindowController(parentWindow);
        string spinnerFrame = controller.GetCurrentSpinnerFrame();
        
        // Spinner
        GUILayout.Label(spinnerFrame, TilesetsUIStyles.spinnerStyle);
        GUILayout.Space(8);
        
        // Loading message
        GUILayout.Label(message, TilesetsUIStyles.emptyStateStyle);
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }
}
