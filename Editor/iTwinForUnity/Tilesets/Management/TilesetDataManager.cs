using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TilesetDataManager
{
    private BentleyTilesetsWindow parentWindow;
    
    public TilesetDataManager(BentleyTilesetsWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Refreshes the list of saved tileset views from the scene
    /// </summary>
    public void RefreshSavedViews()
    {
        parentWindow.savedViews = Object.FindObjectsByType<BentleyTilesetMetadata>(FindObjectsSortMode.None).ToList();
    }
    
    /// <summary>
    /// Gets filtered views based on search text
    /// </summary>
    public List<BentleyTilesetMetadata> GetFilteredViews()
    {
        if (string.IsNullOrEmpty(parentWindow.searchText))
        {
            return parentWindow.savedViews;
        }
        
        return parentWindow.savedViews.Where(m => 
            (m.iModelName?.IndexOf(parentWindow.searchText, System.StringComparison.OrdinalIgnoreCase) >= 0) || 
            (m.iModelDescription?.IndexOf(parentWindow.searchText, System.StringComparison.OrdinalIgnoreCase) >= 0)
        ).ToList();
    }
    
    /// <summary>
    /// Gets the display description for a tileset metadata, truncating if necessary
    /// </summary>
    public string GetDisplayDescription(BentleyTilesetMetadata metadata)
    {
        if (string.IsNullOrEmpty(metadata.iModelDescription))
        {
            return "No description available";
        }
        
        string displayDescription = metadata.iModelDescription;
        if (displayDescription.Length > TilesetConstants.MAX_DESCRIPTION_LENGTH)
        {
            displayDescription = displayDescription.Substring(0, TilesetConstants.MAX_DESCRIPTION_LENGTH - 3) + "...";
        }
        
        return displayDescription;
    }
    
    /// <summary>
    /// Gets the display ID for a tileset metadata, truncating if necessary
    /// </summary>
    public string GetDisplayId(BentleyTilesetMetadata metadata)
    {
        if (string.IsNullOrEmpty(metadata.iModelId))
        {
            return "No ID";
        }
        
        int maxLength = System.Math.Min(metadata.iModelId.Length, TilesetConstants.MAX_ID_DISPLAY_LENGTH);
        return $"ID: {metadata.iModelId.Substring(0, maxLength)}...";
    }
}
