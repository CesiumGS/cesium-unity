using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BentleyTilesetMetadata))]
public class BentleyTilesetMetadataEditor : Editor
{
    private GUIStyle headerStyle;
    private GUIStyle subheaderStyle;
    private GUIStyle thumbnailBoxStyle;
    private GUIStyle infoBoxStyle;
    private GUIStyle copyButtonStyle;
    private Texture2D headerBackground;
    private Texture2D sectionBackground;
    
    private bool showDetails = false;
    
    private void InitStyles()
    {
        if (headerStyle != null) return;
        
        // Create header style with white text
        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = Color.white;
        headerStyle.margin = new RectOffset(0, 0, 10, 4);
        
        // Create subheader style with white text
        subheaderStyle = new GUIStyle(EditorStyles.boldLabel);
        subheaderStyle.fontSize = 12;
        subheaderStyle.normal.textColor = Color.white;
        subheaderStyle.margin = new RectOffset(4, 0, 8, 2);
        
        // Create thumbnail box style
        thumbnailBoxStyle = new GUIStyle(EditorStyles.helpBox);
        thumbnailBoxStyle.padding = new RectOffset(10, 10, 10, 10);
        thumbnailBoxStyle.margin = new RectOffset(0, 0, 5, 10);
        
        // Create info box style
        infoBoxStyle = new GUIStyle(EditorStyles.helpBox);
        infoBoxStyle.padding = new RectOffset(12, 12, 12, 12);
        infoBoxStyle.margin = new RectOffset(0, 0, 8, 8);
        
        // Create copy button style
        copyButtonStyle = new GUIStyle(GUI.skin.button);
        copyButtonStyle.fontSize = 10;
        copyButtonStyle.padding = new RectOffset(4, 4, 2, 2);
        copyButtonStyle.normal.textColor = Color.white;
        
        // Create background textures
        headerBackground = new Texture2D(1, 1);
        headerBackground.SetPixel(0, 0, new Color(0.92f, 0.92f, 0.92f));
        headerBackground.Apply();
        
        sectionBackground = new Texture2D(1, 1);
        sectionBackground.SetPixel(0, 0, new Color(0.97f, 0.97f, 0.97f));
        sectionBackground.Apply();
    }
    
    public override void OnInspectorGUI()
    {
        InitStyles();
        
        BentleyTilesetMetadata metadata = (BentleyTilesetMetadata)target;
        
        // MOVED: iModel Thumbnail section to be first - at the top
        Texture2D iModelThumb = metadata.GetIModelThumbnail();
        
        if (iModelThumb != null)
        {
            EditorGUILayout.BeginVertical(thumbnailBoxStyle);
            EditorGUILayout.LabelField("iModel Thumbnail", headerStyle);
            
            // Create full-width container for thumbnail
            Rect thumbRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(200));
            GUI.Box(thumbRect, GUIContent.none);
            
            // Draw the texture filling the space (StretchToFill)
            GUI.DrawTexture(thumbRect, iModelThumb, ScaleMode.StretchToFill);
            
            EditorGUILayout.EndVertical();
        }
        
        // Primary Info Section with white text
        EditorGUILayout.BeginVertical(thumbnailBoxStyle);
        
        // Display model name and description
        EditorGUILayout.LabelField("Primary Information", headerStyle);
        EditorGUILayout.Space(2);
        
        // Create a white text label style
        GUIStyle whiteTextStyle = new GUIStyle(EditorStyles.textField);
        whiteTextStyle.normal.textColor = Color.white;
        
        // Property fields with copy buttons
        DrawPropertyWithCopyButton("iModel Name", metadata.iModelName, whiteTextStyle);
        DrawPropertyWithCopyButton("Description", metadata.iModelDescription, whiteTextStyle);
        DrawPropertyWithCopyButton("Export Date", metadata.exportDate, whiteTextStyle);
        
        EditorGUILayout.EndVertical();
        
        // Details Foldout with white text
        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldoutHeader);
        foldoutStyle.normal.textColor = Color.white;
        foldoutStyle.onNormal.textColor = Color.white;
        showDetails = EditorGUILayout.Foldout(showDetails, "Additional Details", true, foldoutStyle);
        
        if (showDetails)
        {
            EditorGUILayout.BeginVertical(infoBoxStyle);
            
            DrawPropertyWithCopyButton("iTwin ID", metadata.iTwinId, whiteTextStyle);
            DrawPropertyWithCopyButton("iModel ID", metadata.iModelId, whiteTextStyle);
            
            string changesetDisplay = !string.IsNullOrEmpty(metadata.changesetId) ? metadata.changesetId : "Latest";
            DrawPropertyWithCopyButton("Changeset", changesetDisplay, whiteTextStyle);
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(10);
        
        // Button section removed - moved to BentleyTilesetsWindow
    }
    
    private void DrawPropertyWithCopyButton(string label, string value, GUIStyle textStyle)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Label
        EditorGUILayout.LabelField(label, textStyle, GUILayout.Width(120));
        
        // Text field (read-only)
        GUI.enabled = false;
        EditorGUILayout.TextField(value, textStyle);
        GUI.enabled = true;
        
        // Copy button with icon instead of text
        GUIContent copyIcon = EditorGUIUtility.IconContent("Clipboard");
        copyIcon.tooltip = "Copy value to clipboard";
        
        if (GUILayout.Button(copyIcon, copyButtonStyle, GUILayout.Width(30), GUILayout.Height(18)))
        {
            GUIUtility.systemCopyBuffer = value;
            EditorUtility.DisplayDialog("Copied", $"Copied to clipboard", "OK");
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void OnDisable()
    {
        // Clean up textures
        if (headerBackground != null) DestroyImmediate(headerBackground);
        if (sectionBackground != null) DestroyImmediate(sectionBackground);
    }
}