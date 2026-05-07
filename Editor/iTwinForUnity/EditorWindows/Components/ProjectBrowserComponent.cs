using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Provides a comprehensive browsing interface for iTwins and iModels with advanced features
/// including pagination, thumbnail loading, search integration, and responsive layout.
/// This component handles the display and selection of projects from Bentley's iTwin platform.
/// </summary>
public class ProjectBrowserComponent
{
    /// <summary>
    /// Reference to the parent workflow editor window for state management and coordination
    /// </summary>
    private BentleyWorkflowEditorWindow parentWindow;
    
    /// <summary>
    /// Initializes the project browser component with a reference to the parent window.
    /// </summary>
    /// <param name="parentWindow">The workflow editor window that contains this component</param>
    public ProjectBrowserComponent(BentleyWorkflowEditorWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Renders the main project browsing interface, adapting its display based on the current
    /// workflow state (iTwin selection vs iModel selection). Includes responsive layout,
    /// pagination controls, and integrated search functionality.
    /// </summary>
    public void DrawITwinIModelSelection()
    {
        // Clear section header with step indication
        EditorGUILayout.LabelField(
            parentWindow.currentState == WorkflowState.SelectITwin ? "Select an iTwin Project" : "Select an iModel", 
            BentleyUIStyles.sectionHeaderStyle);
        
        // Draw divider under header
        var dividerRect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(dividerRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(8);
        
        EditorGUILayout.BeginVertical(BentleyUIStyles.cardStyle);

        if (parentWindow.currentState == WorkflowState.LoggedIn)
        {
            EditorGUILayout.HelpBox("Click the button below to fetch your iTwin projects.", MessageType.Info);
            EditorGUILayout.Space(10); 
            
            // Center the button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Fetch my iTwins", "Retrieve your iTwin projects"), 
                GUILayout.Height(32), GUILayout.Width(200)))
            {
                parentWindow.currentState = WorkflowState.FetchingITwins;
                parentWindow.statusMessage = "Fetching iTwins...";
                parentWindow.Repaint();
                parentWindow.currentCoroutineHandle = EditorCoroutineUtility.StartCoroutine(parentWindow.GetMyITwinsCoroutine(), parentWindow);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        else if (parentWindow.currentState == WorkflowState.FetchingITwins || parentWindow.currentState == WorkflowState.FetchingIModels)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(BentleyWorkflowEditorWindow.SpinnerContent, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            // Create properly centered label style
            GUIStyle centeredHeaderStyle = new GUIStyle(BentleyUIStyles.subheaderStyle);
            centeredHeaderStyle.alignment = TextAnchor.MiddleCenter;
            
            // Use centered style and fixed width
            EditorGUILayout.LabelField(
                parentWindow.currentState == WorkflowState.FetchingITwins ? "Loading iTwins..." : "Loading iModels...", 
                centeredHeaderStyle, 
                GUILayout.MinWidth(150));
                
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal(); 
        }
        else if (parentWindow.currentState == WorkflowState.SelectITwin)
        {
            DrawITwinSelection();
        }
        else if (parentWindow.currentState == WorkflowState.SelectIModel)
        {
            DrawIModelSelection();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(15);
    }
    
    private void DrawITwinSelection()
    {
        // Add search box at top
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
        
        // Store the previous control's name to detect focus change
        string searchControlName = "iTwinSearchField";
        GUI.SetNextControlName(searchControlName);
        
        // Check if we should clear focus
        if (parentWindow.shouldClearSearchFocus)
        {
            GUI.FocusControl(null);
            parentWindow.shouldClearSearchFocus = false;
        }
        
        string newSearchText = EditorGUILayout.TextField(parentWindow.iTwinsSearchText, GUILayout.Height(20));
        if (newSearchText != parentWindow.iTwinsSearchText)
        {
            parentWindow.iTwinsSearchText = newSearchText;
            parentWindow.iTwinsCurrentPage = 0; // Reset to first page when search changes
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);
        
        // Filter iTwins based on search text
        List<ITwin> filteredITwins = string.IsNullOrEmpty(parentWindow.iTwinsSearchText) 
            ? parentWindow.myITwins 
            : parentWindow.myITwins.Where(tw => tw.displayName.IndexOf(parentWindow.iTwinsSearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        
                
        // Display message if no results
        if (filteredITwins.Count == 0 && !string.IsNullOrEmpty(parentWindow.iTwinsSearchText))
        {
            EditorGUILayout.HelpBox($"No iTwins found matching '{parentWindow.iTwinsSearchText}'", MessageType.Info);
        }
        
        // Calculate visible items based on current page and filtered results
        int startIndex = parentWindow.iTwinsCurrentPage * BentleyWorkflowEditorWindow.ITEMS_PER_PAGE;
        int endIndex = Mathf.Min(startIndex + BentleyWorkflowEditorWindow.ITEMS_PER_PAGE, filteredITwins.Count);
        
        // Display only the current page of filtered iTwins
        for (int i = startIndex; i < endIndex; i++)
        {
            var tw = filteredITwins[i];
            
            // Add this line to define the thumbnail endpoint
            string thumbnailEndpoint = $"thumbnail_itwin_{tw.id}";
            
            // Only fetch thumbnails for visible items if not already loading or loaded
            if (tw.thumbnail == null && !tw.loadingThumbnail && !tw.thumbnailLoaded && ApiRateLimiter.CanMakeRequest(thumbnailEndpoint))
            {
                EditorCoroutineUtility.StartCoroutine(parentWindow.FetchITwinThumbnail(tw), parentWindow);
            }
            
            EditorGUILayout.BeginVertical(BentleyUIStyles.selectionCardStyle);
            EditorGUILayout.BeginHorizontal();
            
            // Display thumbnail with proper sizing
            Rect thumbnailRect = EditorGUILayout.GetControlRect(false, 64, GUILayout.Width(64));
            Texture2D tex = tw.loadingThumbnail ? (Texture2D)BentleyWorkflowEditorWindow.SpinnerContent.image : tw.thumbnail ?? Texture2D.grayTexture;
            GUI.DrawTexture(thumbnailRect, tex, ScaleMode.ScaleAndCrop); // <-- changed here

            EditorGUILayout.BeginVertical(GUILayout.Height(64));
            
            // Display iTwin name with better styling
            EditorGUILayout.LabelField(tw.displayName, BentleyUIStyles.itemTitleStyle);
            // We could add id or other info here if helpful
            EditorGUILayout.LabelField($"Project ID: {tw.id.Substring(0, 8)}...", BentleyUIStyles.itemDescriptionStyle);
            
            EditorGUILayout.EndVertical();

            // Select button on the right with proper vertical centering
            EditorGUILayout.BeginVertical(GUILayout.Width(70), GUILayout.Height(64));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select", GUILayout.Height(24), GUILayout.Width(70)))
            {
                parentWindow.selectedITwinId = tw.id;
                parentWindow.iTwinsSearchText = ""; // Clear iTwin search text
                parentWindow.iModelsSearchText = ""; // Clear iModel search text for next screen
                parentWindow.shouldClearSearchFocus = true; // Add this line to clear focus
                parentWindow.currentState = WorkflowState.FetchingIModels;
                parentWindow.statusMessage = "Fetching iModels...";
                parentWindow.Repaint();
                parentWindow.currentCoroutineHandle = EditorCoroutineUtility.StartCoroutine(parentWindow.GetITwinIModelsCoroutine(tw.id), parentWindow);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        // Add page navigation buttons if we have more than one page in filtered results
        if (filteredITwins.Count > BentleyWorkflowEditorWindow.ITEMS_PER_PAGE)
        {
            DrawPaginationControls(filteredITwins.Count, ref parentWindow.iTwinsCurrentPage, "iTwins");
        }
        
        // Back button aligned to the left
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("← Back", "Return to previous step"), 
            GUILayout.Height(24), GUILayout.Width(100)))
        {
            parentWindow.currentState = WorkflowState.LoggedIn;
            parentWindow.iTwinsSearchText = ""; // Clear search text when going back
            parentWindow.shouldClearSearchFocus = true; // Add this line
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawIModelSelection()
    {
        // Add search box at top
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
        
        // Store the previous control's name to detect focus change
        string searchControlName = "iModelSearchField";
        GUI.SetNextControlName(searchControlName);
        
        // Check if we should clear focus
        if (parentWindow.shouldClearSearchFocus)
        {
            GUI.FocusControl(null);
            parentWindow.shouldClearSearchFocus = false;
        }
        
        string newSearchText = EditorGUILayout.TextField(parentWindow.iModelsSearchText, GUILayout.Height(20));
        if (newSearchText != parentWindow.iModelsSearchText)
        {
            parentWindow.iModelsSearchText = newSearchText;
            parentWindow.iModelsCurrentPage = 0; // Reset to first page when search changes
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);
        
        // Filter iModels based on search text
        List<IModel> filteredIModels = string.IsNullOrEmpty(parentWindow.iModelsSearchText) 
            ? parentWindow.myIModels 
            : parentWindow.myIModels.Where(im => im.displayName.IndexOf(parentWindow.iModelsSearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        
                
        // Display message if no results
        if (filteredIModels.Count == 0 && !string.IsNullOrEmpty(parentWindow.iModelsSearchText))
        {
            EditorGUILayout.HelpBox($"No iModels found matching '{parentWindow.iModelsSearchText}'", MessageType.Info);
        }
        
        // Calculate visible items based on current page and filtered results
        int startIndex = parentWindow.iModelsCurrentPage * BentleyWorkflowEditorWindow.ITEMS_PER_PAGE;
        int endIndex = Mathf.Min(startIndex + BentleyWorkflowEditorWindow.ITEMS_PER_PAGE, filteredIModels.Count);
        
        // Display only the current page of filtered iModels
        for (int i = startIndex; i < endIndex; i++)
        {
            var im = filteredIModels[i];
            
            // Add rate limiting to all API calls
            string thumbnailEndpoint = $"thumbnail_{im.id}";
            string detailsEndpoint = $"details_{im.id}";
            string changesetsEndpoint = $"changesets_{im.id}";
            
            // Only fetch thumbnail if needed and rate limiting allows
            if (im.thumbnail == null && !im.loadingThumbnail && !im.thumbnailLoaded && ApiRateLimiter.CanMakeRequest(thumbnailEndpoint))
            {
                EditorCoroutineUtility.StartCoroutine(parentWindow.FetchIModelThumbnail(im), parentWindow);
            }
            
            // Only fetch details if needed and rate limiting allows
            if (!im.loadingDetails && !im.detailsLoaded && ApiRateLimiter.CanMakeRequest(detailsEndpoint))
            {
                EditorCoroutineUtility.StartCoroutine(parentWindow.FetchIModelDetailsCoroutine(im), parentWindow);
            }
            
            // Only fetch changesets if needed and rate limiting allows
            if (!im.loadingChangesets && (im.changesets == null || im.changesets.Count == 0) && ApiRateLimiter.CanMakeRequest(changesetsEndpoint))
            {
                EditorCoroutineUtility.StartCoroutine(parentWindow.FetchIModelChangesets(im), parentWindow);
            }
            
            EditorGUILayout.BeginVertical(BentleyUIStyles.selectionCardStyle);

            // Header with name
            EditorGUILayout.LabelField(im.displayName, BentleyUIStyles.itemTitleStyle);
            
            // Subtle divider
            var itemDividerRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(itemDividerRect, new Color(0.5f, 0.5f, 0.5f, 0.2f));
            EditorGUILayout.Space(5);
            
            // Main content area with thumbnail and description
            EditorGUILayout.BeginHorizontal();
            
            // Left side - Thumbnail
            EditorGUILayout.BeginVertical(GUILayout.Width(90));
            // Display thumbnail with proper sizing and border
            Rect imageRect = EditorGUILayout.GetControlRect(false, 80, GUILayout.Width(80));
            Texture2D tex = im.loadingThumbnail ? (Texture2D)BentleyWorkflowEditorWindow.SpinnerContent.image : im.thumbnail ?? Texture2D.grayTexture;
            
            // Add a slight border around the image
            EditorGUI.DrawRect(new Rect(imageRect.x-1, imageRect.y-1, imageRect.width+2, imageRect.height+2), 
                new Color(0.3f, 0.3f, 0.3f, 0.5f));
            GUI.DrawTexture(imageRect, tex, ScaleMode.ScaleAndCrop); // <-- changed here
            EditorGUILayout.EndVertical();
            
            // Middle - Description
            EditorGUILayout.BeginVertical();
            
            // Description
            if (!string.IsNullOrEmpty(im.description))
            {
                // Limit description length to prevent UI issues
                string displayDescription = im.description;
                if (displayDescription.Length > 200)
                {
                    displayDescription = displayDescription.Substring(0, 197) + "...";
                }
                EditorGUILayout.LabelField(displayDescription, BentleyUIStyles.itemDescriptionStyle);
            }
            else if (im.loadingDetails)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(BentleyWorkflowEditorWindow.SpinnerContent, GUILayout.Width(16), GUILayout.Height(16));
                EditorGUILayout.LabelField("Loading description...", BentleyUIStyles.itemDescriptionStyle);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Use the helper method for consistent fallback text
                EditorGUILayout.LabelField(parentWindow.GetDisplayDescription(im), BentleyUIStyles.itemDescriptionStyle);
            }
            
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"ID: {im.id.Substring(0, Math.Min(im.id.Length, 12))}...", BentleyUIStyles.itemDescriptionStyle);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            // Changeset selection area
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Changeset:", GUILayout.Width(80));
            
            if (im.loadingChangesets)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(BentleyWorkflowEditorWindow.SpinnerContent, GUILayout.Width(16), GUILayout.Height(16));
                EditorGUILayout.LabelField("Loading changesets...");
                EditorGUILayout.EndHorizontal();
            }
            else if (im.changesets != null && im.changesets.Count > 0)
            {
                // Build options: "latest" + all changesets
                List<string> options = new List<string> { "Latest Changeset" };
        
                // Format each changeset similar to web version
                foreach (var cs in im.changesets)
                {
                    // Use a more readable format that matches the web experience
                    string displayText;
                    if (!string.IsNullOrEmpty(cs.description))
                    {
                        displayText = $"{cs.description}";
                    }
                    else 
                    {
                        displayText = $"Version {cs.version}";
                    }
                    options.Add(displayText);
                }

                // Show a more visually appealing dropdown
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Version:", GUILayout.Width(60));
                
                // Store previous index to detect changes
                int prevIndex = im.selectedChangesetIndex;
                im.selectedChangesetIndex = EditorGUILayout.Popup(im.selectedChangesetIndex, options.ToArray());
                
                // If index changed, update UI
                if (prevIndex != im.selectedChangesetIndex)
                {
                    parentWindow.Repaint();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("No changesets available");
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Bottom button bar with select button right-aligned
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            // Select button
            if (GUILayout.Button("Select for Export", GUILayout.Height(28), GUILayout.Width(120)))
            {
                // Save the selected iModel and changeset
                parentWindow.selectedIModelId = im.id;
                parentWindow.iModelId = parentWindow.selectedIModelId;
                parentWindow.iModelsSearchText = ""; // Clear iModel search text
                
                // Set the changeset ID based on selection
                if (im.selectedChangesetIndex == 0) {
                    // Use latest changeset (empty string)
                    parentWindow.changesetId = string.Empty;
                    parentWindow.statusMessage = "Using latest changeset for export.";
                } else if (im.changesets != null && im.changesets.Count > 0 && 
                           im.selectedChangesetIndex <= im.changesets.Count) {
                    // Use specific changeset
                    var selectedChangeset = im.changesets[im.selectedChangesetIndex - 1];
                    parentWindow.changesetId = selectedChangeset.id;
                    parentWindow.statusMessage = $"Using changeset: {selectedChangeset.description ?? selectedChangeset.version}";
                } else {
                    // Fallback if something went wrong
                    parentWindow.changesetId = string.Empty;
                    parentWindow.statusMessage = "Could not determine changeset, using latest.";
                }
                    
                // Skip the changeset selection state, go directly to export
                parentWindow.currentState = WorkflowState.StartingExport;
                parentWindow.Repaint();
                parentWindow.currentCoroutineHandle = EditorCoroutineUtility.StartCoroutine(parentWindow.RunFullExportWorkflowCoroutine(), parentWindow);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        // Add page navigation buttons for iModels
        if (filteredIModels.Count > BentleyWorkflowEditorWindow.ITEMS_PER_PAGE)
        {
            DrawPaginationControls(filteredIModels.Count, ref parentWindow.iModelsCurrentPage, "iModels");
        }
        
        // Back button aligned to the left
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("← Back", "Return to iTwin selection"), 
            GUILayout.Height(24), GUILayout.Width(100)))
        {
            parentWindow.currentState = WorkflowState.SelectITwin;
            parentWindow.iModelsSearchText = ""; // Clear search text when going back
            parentWindow.shouldClearSearchFocus = true; // Add this line
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawPaginationControls(int totalItems, ref int currentPage, string context)
    {
        // Calculate total pages based on FILTERED results
        int totalPages = Mathf.CeilToInt((float)totalItems / BentleyWorkflowEditorWindow.ITEMS_PER_PAGE);
        
        // Make sure current page is valid for filtered results
        if (currentPage >= totalPages)
            currentPage = totalPages - 1;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        // Previous page button
        EditorGUI.BeginDisabledGroup(currentPage <= 0);
        if (GUILayout.Button("◄", GUILayout.Height(24), GUILayout.Width(30)))
        {
            currentPage--;
            parentWindow.shouldClearSearchFocus = true;
            parentWindow.Repaint();
        }
        EditorGUI.EndDisabledGroup();
        
        // Get page numbers to display
        List<int> pageNumbers = BentleyPaginationHelper.GetPaginationNumbers(currentPage, totalPages);
        
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
                // Create button style
                GUIStyle pageButtonStyle = new GUIStyle(GUI.skin.button);
                
                // Highlight current page
                if (pageIndex == currentPage)
                {
                    pageButtonStyle.fontStyle = FontStyle.Bold;
                    pageButtonStyle.normal.textColor = new Color(0.2f, 0.5f, 0.9f);
                }
                
                if (GUILayout.Button((pageIndex + 1).ToString(), pageButtonStyle, GUILayout.Width(30), GUILayout.Height(24)))
                {
                    currentPage = pageIndex;
                    parentWindow.shouldClearSearchFocus = true;
                    parentWindow.Repaint();
                }
            }
        }
        
        // Next page button
        EditorGUI.BeginDisabledGroup(currentPage >= totalPages - 1);
        if (GUILayout.Button("►", GUILayout.Height(24), GUILayout.Width(30)))
        {
            currentPage++;
            parentWindow.shouldClearSearchFocus = true;
            parentWindow.Repaint();
        }
        EditorGUI.EndDisabledGroup();
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
}
