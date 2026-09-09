using UnityEngine;
using UnityEditor;

public class SearchComponent
{
    private BentleyTilesetsWindow parentWindow;
    private TilesetSearchManager searchManager;
    
    public SearchComponent(BentleyTilesetsWindow parentWindow)
    {
        this.parentWindow = parentWindow;
        this.searchManager = new TilesetSearchManager(parentWindow);
    }
    
    /// <summary>
    /// Draws the search section UI
    /// </summary>
    public void DrawSearchSection()
    {
        // Search area
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        
        Rect searchRect = GUILayoutUtility.GetRect(GUIContent.none, TilesetsUIStyles.searchBoxStyle, 
            GUILayout.ExpandWidth(true), GUILayout.Height(TilesetConstants.SEARCH_BOX_HEIGHT));
            
        // Draw search icon - increased size
        var searchIcon = TilesetsIconHelper.GetSearchIcon();
        GUI.DrawTexture(new Rect(searchRect.x + 6, searchRect.y + 4, 20, 20), searchIcon.image);
        
        // Handle clear search focus
        searchManager.HandleClearSearchFocus();
        
        // Search field
        GUI.SetNextControlName(TilesetConstants.SEARCH_CONTROL_NAME);
        string newSearchText = EditorGUI.TextField(searchRect, parentWindow.searchText, TilesetsUIStyles.searchBoxStyle);
        
        // Update search text through manager
        searchManager.UpdateSearchText(newSearchText);
        
        // Clear button if search has text
        if (!string.IsNullOrEmpty(parentWindow.searchText))
        {
            var clearIcon = TilesetsIconHelper.GetClearIcon();
            Rect clearRect = new Rect(searchRect.xMax - 24, searchRect.y + 4, 20, 20);
            if (GUI.Button(clearRect, clearIcon, GUIStyle.none))
            {
                searchManager.ClearSearch();
            }
        }
        
        GUILayout.Space(16);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(12);
    }
    
    /// <summary>
    /// Gets filtered views based on current search text
    /// </summary>
    public System.Collections.Generic.List<BentleyTilesetMetadata> GetFilteredViews()
    {
        var dataManager = new TilesetDataManager(parentWindow);
        return dataManager.GetFilteredViews();
    }
    
    /// <summary>
    /// Gets the appropriate empty state message
    /// </summary>
    public void GetEmptyStateMessage(out string title, out string description)
    {
        searchManager.GetEmptyStateMessage(out title, out description);
    }
}
