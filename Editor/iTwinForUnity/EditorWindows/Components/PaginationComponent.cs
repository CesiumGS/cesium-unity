using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PaginationComponent
{
    private BentleyTilesetsWindow parentWindow;
    
    public PaginationComponent(BentleyTilesetsWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Draws pagination controls if needed
    /// </summary>
    public void DrawPaginationControls(List<BentleyTilesetMetadata> filteredViews)
    {
        // Only draw pagination if we have more items than fit on one page
        if (filteredViews.Count <= TilesetConstants.ITEMS_PER_PAGE)
            return;
            
        // Calculate total pages
        int totalPages = Mathf.CeilToInt((float)filteredViews.Count / TilesetConstants.ITEMS_PER_PAGE);
        
        // Make sure current page is valid
        if (parentWindow.currentPage >= totalPages)
            parentWindow.currentPage = totalPages - 1;
        
        // Create a centered horizontal group for all pagination controls
        EditorGUILayout.BeginHorizontal();
        
        // Add flexible space to push everything to the center
        GUILayout.FlexibleSpace();
        
        // Previous page button
        DrawPreviousButton();
        
        // Page number buttons
        DrawPageNumbers(totalPages);
        
        // Next page button
        DrawNextButton(totalPages);
        
        // Add flexible space to push everything to the center
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// Gets the visible items for the current page
    /// </summary>
    public void GetPagedItems(List<BentleyTilesetMetadata> filteredViews, out int startIndex, out int endIndex)
    {
        startIndex = parentWindow.currentPage * TilesetConstants.ITEMS_PER_PAGE;
        endIndex = Mathf.Min(startIndex + TilesetConstants.ITEMS_PER_PAGE, filteredViews.Count);
    }
    
    private void DrawPreviousButton()
    {
        EditorGUI.BeginDisabledGroup(parentWindow.currentPage <= 0);
        if (GUILayout.Button("◄", GUILayout.Height(TilesetConstants.BUTTON_HEIGHT), GUILayout.Width(30)))
        {
            parentWindow.currentPage--;
            parentWindow.shouldClearSearchFocus = true;
            parentWindow.Repaint();
        }
        EditorGUI.EndDisabledGroup();
    }
    
    private void DrawPageNumbers(int totalPages)
    {
        // Get page numbers to display using the helper
        List<int> pageNumbers = TilesetsPaginationHelper.GetPaginationNumbers(parentWindow.currentPage, totalPages);
        
        // Display page numbers
        foreach (int pageIndex in pageNumbers)
        {
            if (pageIndex == -1)
            {
                // This is an ellipsis
                GUILayout.Label("...", GUILayout.Width(20));
            }
            else
            {
                DrawPageButton(pageIndex);
            }
        }
    }
    
    private void DrawPageButton(int pageIndex)
    {
        // Create button style
        GUIStyle pageButtonStyle = new GUIStyle(GUI.skin.button);
        
        // Highlight current page
        if (pageIndex == parentWindow.currentPage)
        {
            pageButtonStyle.fontStyle = FontStyle.Bold;
            pageButtonStyle.normal.textColor = TilesetConstants.AccentColor;
        }
        
        if (GUILayout.Button((pageIndex + 1).ToString(), pageButtonStyle, GUILayout.Width(30), GUILayout.Height(TilesetConstants.BUTTON_HEIGHT)))
        {
            parentWindow.currentPage = pageIndex;
            parentWindow.shouldClearSearchFocus = true;
            parentWindow.Repaint();
        }
    }
    
    private void DrawNextButton(int totalPages)
    {
        EditorGUI.BeginDisabledGroup(parentWindow.currentPage >= totalPages - 1);
        if (GUILayout.Button("►", GUILayout.Height(TilesetConstants.BUTTON_HEIGHT), GUILayout.Width(30)))
        {
            parentWindow.currentPage++;
            parentWindow.shouldClearSearchFocus = true;
            parentWindow.Repaint();
        }
        EditorGUI.EndDisabledGroup();
    }
}
