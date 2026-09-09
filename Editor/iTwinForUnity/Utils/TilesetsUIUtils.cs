using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class TilesetsUIStyles
{
    // UI Styles
    public static GUIStyle headerStyle;
    public static GUIStyle titleStyle;
    public static GUIStyle descriptionStyle;
    public static GUIStyle searchBoxStyle;
    public static GUIStyle cardStyle;
    public static GUIStyle buttonStyle;
    public static GUIStyle iconButtonStyle;
    public static GUIStyle emptyStateStyle;
    public static GUIStyle spinnerStyle;
    public static GUIStyle breadcrumbStyle;
    public static GUIStyle infoLabelStyle;
    public static GUIStyle backButtonStyle;
    public static GUIStyle footerStyle;
    public static GUIStyle dividerStyle;
    public static GUIStyle selectionCardStyle;
    public static GUIStyle itemTitleStyle;
    public static GUIStyle itemDescriptionStyle;

    // Textures
    private static Texture2D cardBackgroundTexture;
    private static Texture2D cardBackgroundHoverTexture;
    private static Texture2D noThumbnailTexture;
    private static Texture2D dividerTexture;

    public static void InitializeStyles()
    {
        if (headerStyle != null) return;
        
        // Initialize textures first
        InitializeTextures();
        
        // Initialize icons as well
        TilesetsIconHelper.InitializeIcons();
        
        // Styles matching the BentleyWorkflowEditorWindow
        selectionCardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(12, 12, 12, 12),
            margin = new RectOffset(0, 0, 8, 8)
        };
        
        itemTitleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            margin = new RectOffset(0, 0, 0, 2)
        };
        
        itemDescriptionStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            fontSize = 11,
            fontStyle = FontStyle.Normal,
            padding = new RectOffset(2, 2, 0, 5)
        };
        
        // Header styles
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            margin = new RectOffset(0, 0, 6, 6),
            padding = new RectOffset(10, 10, 6, 6),
            normal = { textColor = TilesetConstants.PrimaryTextColor }
        };
        
        breadcrumbStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            margin = new RectOffset(0, 0, 4, 12),
            normal = { textColor = TilesetConstants.PrimaryTextColor }
        };
        
        // Content styles
        titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            margin = new RectOffset(0, 0, 4, 2),
            normal = { textColor = TilesetConstants.PrimaryTextColor }
        };
        
        descriptionStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            fontSize = 12,
            wordWrap = true,
            margin = new RectOffset(0, 0, 2, 8),
            normal = { textColor = TilesetConstants.SecondaryTextColor }
        };
        
        // Container styles
        cardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(12, 12, 12, 12),
            margin = new RectOffset(0, 0, 0, 12),
            normal = { background = cardBackgroundTexture }
        };
        
        // Control styles
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            padding = new RectOffset(8, 8, 6, 6),
            margin = new RectOffset(2, 2, 0, 0),
            normal = { textColor = TilesetConstants.PrimaryTextColor }
        };
        
        iconButtonStyle = new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(6, 6, 4, 4),
            margin = new RectOffset(4, 0, 0, 0),
            fixedWidth = 28,
            fixedHeight = 24
        };
        
        backButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            padding = new RectOffset(8, 12, 6, 6),
            margin = new RectOffset(0, 0, 0, 12),
            fixedHeight = 26,
            normal = { textColor = TilesetConstants.PrimaryTextColor }
        };
        
        // Increase search box height and padding for larger icon
        searchBoxStyle = new GUIStyle(EditorStyles.toolbarSearchField)
        {
            padding = new RectOffset(30, 6, 4, 4), // Increased left padding for larger icon
            margin = new RectOffset(0, 4, 0, 0),
            fixedHeight = TilesetConstants.SEARCH_BOX_HEIGHT
        };
        
        // Status styles
        emptyStateStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 14,
            wordWrap = true,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = TilesetConstants.SecondaryTextColor }
        };
        
        spinnerStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = TilesetConstants.AccentColor }
        };
        
        infoLabelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 11,
            normal = { textColor = TilesetConstants.SecondaryTextColor }
        };
        
        footerStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 10,
            normal = { textColor = TilesetConstants.SecondaryTextColor }
        };
        
        dividerStyle = new GUIStyle()
        {
            normal = { background = dividerTexture },
            margin = new RectOffset(0, 0, 4, 4),
            fixedHeight = 1
        };
    }
    
    private static void InitializeTextures()
    {
        if (cardBackgroundTexture == null)
        {
            cardBackgroundTexture = CreateColorTexture(TilesetConstants.CardBgColor);
            cardBackgroundHoverTexture = CreateColorTexture(TilesetConstants.CardBgHoverColor);
            noThumbnailTexture = CreateColorTexture(new Color(0.18f, 0.18f, 0.18f));
            dividerTexture = CreateColorTexture(new Color(0.3f, 0.3f, 0.3f));
        }
    }
    
    private static Texture2D CreateColorTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
    
    public static Texture2D GetCardBackgroundTexture() => cardBackgroundTexture;
    public static Texture2D GetCardBackgroundHoverTexture() => cardBackgroundHoverTexture;
    public static Texture2D GetNoThumbnailTexture() => noThumbnailTexture;
    
    public static void CleanupTextures()
    {
        if (cardBackgroundTexture != null) Object.DestroyImmediate(cardBackgroundTexture);
        if (cardBackgroundHoverTexture != null) Object.DestroyImmediate(cardBackgroundHoverTexture);
        if (noThumbnailTexture != null) Object.DestroyImmediate(noThumbnailTexture);
        if (dividerTexture != null) Object.DestroyImmediate(dividerTexture);
        
        cardBackgroundTexture = null;
        cardBackgroundHoverTexture = null;
        noThumbnailTexture = null;
        dividerTexture = null;
    }
}

public static class TilesetsIconHelper
{
    // Icon cache
    private static GUIContent searchIcon;
    private static GUIContent clearIcon;
    private static GUIContent backIcon;
    private static GUIContent hierarchyIcon;
    private static GUIContent focusIcon;
    private static GUIContent webIcon;
    
    public static void InitializeIcons()
    {
        if (searchIcon == null)
        {
            clearIcon = EditorGUIUtility.IconContent("clear");
            searchIcon = EditorGUIUtility.IconContent("Search Icon");
            backIcon = EditorGUIUtility.IconContent("back");
            hierarchyIcon = EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow");
            focusIcon = EditorGUIUtility.IconContent("SceneViewCamera");
            webIcon = EditorGUIUtility.IconContent("BuildSettings.Web.Small");
        }
    }
    
    public static GUIContent GetSearchIcon() 
    {
        InitializeIcons();
        return searchIcon;
    }
    
    public static GUIContent GetClearIcon() 
    {
        InitializeIcons();
        return clearIcon;
    }
    
    public static GUIContent GetBackIcon() 
    {
        InitializeIcons();
        return backIcon;
    }
    
    public static GUIContent GetHierarchyIcon() 
    {
        InitializeIcons();
        return hierarchyIcon;
    }
    
    public static GUIContent GetFocusIcon() 
    {
        InitializeIcons();
        return focusIcon;
    }
    
    public static GUIContent GetWebIcon() 
    {
        InitializeIcons();
        return webIcon;
    }
}

public static class TilesetsPaginationHelper
{
    // Returns list of page numbers to display (with -1 representing ellipsis)
    public static List<int> GetPaginationNumbers(int currentPage, int totalPages)
    {
        List<int> pages = new List<int>();
        
        // For small number of pages, show all pages
        if (totalPages <= 5)
        {
            for (int i = 0; i < totalPages; i++)
                pages.Add(i);
            return pages;
        }
        
        // Always show first page
        pages.Add(0);
        
        // For first page
        if (currentPage == 0)
        {
            pages.Add(1); // Show page 2
            pages.Add(-1); // Ellipsis
            pages.Add(totalPages - 1); // Last page
        }
        // For second page
        else if (currentPage == 1)
        {
            pages.Add(1); // Page 2 (current)
            pages.Add(2); // Page 3
            pages.Add(-1); // Ellipsis
            pages.Add(totalPages - 1); // Last page
        }
        // For third page
        else if (currentPage == 2)
        {
            pages.Add(1); // Page 2
            pages.Add(2); // Page 3 (current)
            pages.Add(3); // Page 4
            pages.Add(-1); // Ellipsis
            pages.Add(totalPages - 1); // Last page
        }
        // For middle pages
        else if (currentPage > 2 && currentPage < totalPages - 3)
        {
            pages.Add(-1); // Ellipsis
            pages.Add(currentPage - 1); // Previous page
            pages.Add(currentPage); // Current page
            pages.Add(currentPage + 1); // Next page
            pages.Add(-1); // Ellipsis
            pages.Add(totalPages - 1); // Last page
        }
        // For third-to-last page
        else if (currentPage == totalPages - 3)
        {
            pages.Add(-1); // Ellipsis
            pages.Add(totalPages - 4); // Page before current
            pages.Add(totalPages - 3); // Current page
            pages.Add(totalPages - 2); // Page after current
            pages.Add(totalPages - 1); // Last page
        }
        // For second-to-last page
        else if (currentPage == totalPages - 2)
        {
            pages.Add(-1); // Ellipsis
            pages.Add(totalPages - 3); // Page before current
            pages.Add(totalPages - 2); // Current page
            pages.Add(totalPages - 1); // Last page
        }
        // For last page
        else if (currentPage == totalPages - 1)
        {
            pages.Add(-1); // Ellipsis
            pages.Add(totalPages - 2); // Page before current
            pages.Add(totalPages - 1); // Current page (last page)
        }
        
        return pages;
    }
}
