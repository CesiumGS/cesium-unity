using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TilesetSearchManager
{
    private BentleyTilesetsWindow parentWindow;
    
    public TilesetSearchManager(BentleyTilesetsWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Updates the search text and resets pagination if changed
    /// </summary>
    public void UpdateSearchText(string newSearchText)
    {
        if (newSearchText != parentWindow.searchText)
        {
            parentWindow.searchText = newSearchText;
            parentWindow.currentPage = 0; // Reset to first page when search changes
        }
    }
    
    /// <summary>
    /// Clears the search text and focus
    /// </summary>
    public void ClearSearch()
    {
        parentWindow.searchText = "";
        parentWindow.shouldClearSearchFocus = true;
    }
    
    /// <summary>
    /// Sets focus to the search field
    /// </summary>
    public void FocusSearchField()
    {
        GUI.FocusControl(TilesetConstants.SEARCH_CONTROL_NAME);
    }
    
    /// <summary>
    /// Handles the clear search focus logic
    /// </summary>
    public void HandleClearSearchFocus()
    {
        if (parentWindow.shouldClearSearchFocus)
        {
            GUI.FocusControl(null);
            parentWindow.shouldClearSearchFocus = false;
        }
    }
    
    /// <summary>
    /// Checks if search results are empty
    /// </summary>
    public bool HasSearchResults(List<BentleyTilesetMetadata> filteredViews)
    {
        return filteredViews.Count > 0;
    }
    
    /// <summary>
    /// Gets the appropriate empty state message
    /// </summary>
    public void GetEmptyStateMessage(out string title, out string description)
    {
        if (parentWindow.savedViews.Count == 0)
        {
            title = "No Bentley iTwin tilesets found in the scene";
            description = "Import iTwin data using the Bentley Mesh Export tool first.";
        }
        else
        {
            title = $"No results matching '{parentWindow.searchText}'";
            description = "Try a different search term or clear the search.";
        }
    }
}
