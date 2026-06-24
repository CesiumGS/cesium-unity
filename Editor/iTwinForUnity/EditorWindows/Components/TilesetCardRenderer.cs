using UnityEngine;
using UnityEditor;

public class TilesetCardRenderer
{
    private BentleyTilesetsWindow parentWindow;
    
    public TilesetCardRenderer(BentleyTilesetsWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Draws a single tileset card
    /// </summary>
    public void DrawTilesetCard(BentleyTilesetMetadata metadata, int index)
    {
        bool isHovered = parentWindow.hoveredCardIndex == index;
        
        // Use selectionCardStyle like BentleyWorkflowEditorWindow
        EditorGUILayout.BeginVertical(TilesetsUIStyles.selectionCardStyle);
        
        // Store card rect - add a small element first to avoid the GetLastRect issue
        GUILayout.Space(1);
        Rect cardRect = GUILayoutUtility.GetLastRect();
        
        // Add hover effect
        if (isHovered)
            EditorGUI.DrawRect(cardRect, TilesetConstants.CardBgHoverColor);
        else
            EditorGUI.DrawRect(cardRect, TilesetConstants.CardBgColor);
        
        // Handle hover state
        HandleCardHover(cardRect, index);
        
        // Header with name - matching iTwin/iModel style
        EditorGUILayout.LabelField(metadata.iModelName, TilesetsUIStyles.itemTitleStyle);
        
        // Subtle divider
        DrawItemDivider();
        
        // Main content area with thumbnail and description
        DrawCardContent(metadata);
        
        // Action buttons
        DrawCardButtons(metadata);
        
        EditorGUILayout.EndVertical();
    }
    
    private void HandleCardHover(Rect cardRect, int index)
    {
        // Check hover state
        if (Event.current.type == EventType.Repaint)
        {
            // Calculate full card rect
            Rect fullCardRect = new Rect(cardRect.x, cardRect.y - 1, cardRect.width, 300); // approximate height
            
            if (fullCardRect.Contains(Event.current.mousePosition))
            {
                if (parentWindow.hoveredCardIndex != index)
                {
                    parentWindow.hoveredCardIndex = index;
                    parentWindow.Repaint();
                }
            }
            else if (parentWindow.hoveredCardIndex == index && !fullCardRect.Contains(Event.current.mousePosition))
            {
                parentWindow.hoveredCardIndex = -1;
                parentWindow.Repaint();
            }
        }
    }
    
    private void DrawItemDivider()
    {
        var itemDividerRect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(itemDividerRect, new Color(0.5f, 0.5f, 0.5f, 0.2f));
        EditorGUILayout.Space(5);
    }
    
    private void DrawCardContent(BentleyTilesetMetadata metadata)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Left side - Thumbnail
        DrawThumbnail(metadata);
        
        // Right side - Description and details
        DrawDescription(metadata);
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawThumbnail(BentleyTilesetMetadata metadata)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(90));
        
        // Display thumbnail with proper sizing and border
        Rect imageRect = EditorGUILayout.GetControlRect(false, TilesetConstants.THUMBNAIL_SIZE, GUILayout.Width(TilesetConstants.THUMBNAIL_SIZE));
        Texture2D thumbnail = metadata.GetIModelThumbnail();
        
        // Add a slight border around the image
        EditorGUI.DrawRect(new Rect(imageRect.x-1, imageRect.y-1, imageRect.width+2, imageRect.height+2), 
            new Color(0.3f, 0.3f, 0.3f, 0.5f));
            
        if (thumbnail != null)
        {
            GUI.DrawTexture(imageRect, thumbnail, ScaleMode.ScaleAndCrop);
        }
        else
        {
            GUI.DrawTexture(imageRect, TilesetsUIStyles.GetNoThumbnailTexture());
            GUI.Label(imageRect, "No Thumbnail", 
                new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 12 });
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDescription(BentleyTilesetMetadata metadata)
    {
        EditorGUILayout.BeginVertical();
        
        // Description
        var dataManager = new TilesetDataManager(parentWindow);
        string displayDescription = dataManager.GetDisplayDescription(metadata);
        EditorGUILayout.LabelField(displayDescription, TilesetsUIStyles.itemDescriptionStyle);
        
        EditorGUILayout.Space(4);
        
        // ID display
        string displayId = dataManager.GetDisplayId(metadata);
        EditorGUILayout.LabelField(displayId, TilesetsUIStyles.itemDescriptionStyle);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawCardButtons(BentleyTilesetMetadata metadata)
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        
        // Hierarchy button
        var hierarchyIcon = TilesetsIconHelper.GetHierarchyIcon();
        if (GUILayout.Button(new GUIContent(" Select", hierarchyIcon.image), TilesetsUIStyles.buttonStyle))
        {
            TilesetSceneOperations.SelectAndPingObject(metadata.gameObject);
        }
        
        // Focus button
        var focusIcon = TilesetsIconHelper.GetFocusIcon();
        if (GUILayout.Button(new GUIContent(" Focus", focusIcon.image), TilesetsUIStyles.buttonStyle))
        {
            TilesetSceneOperations.FocusOnObject(metadata.gameObject, parentWindow);
        }
        
        // Web button - Open in iTwin Platform
        var webIcon = TilesetsIconHelper.GetWebIcon();
        if (GUILayout.Button(new GUIContent(" Web", webIcon.image), TilesetsUIStyles.buttonStyle))
        {
            TilesetSceneOperations.OpenWebUrl(metadata);
        }
        
        EditorGUILayout.EndHorizontal();
    }
}
