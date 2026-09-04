using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class BentleyUIStyles
{
    public static GUIStyle sectionHeaderStyle;
    public static GUIStyle subheaderStyle;
    public static GUIStyle foldoutStyle;
    public static GUIStyle richTextLabelStyle;
    public static GUIStyle helpBoxStyle;
    public static GUIStyle cardStyle;
    public static GUIStyle itemTitleStyle;
    public static GUIStyle itemDescriptionStyle;
    public static GUIStyle buttonRightAlignStyle;
    public static GUIStyle selectionCardStyle;
    public static GUIStyle headerDividerStyle;
    public static GUIStyle stepIndicatorStyle;
    public static GUIStyle searchBoxStyle;
    
    static BentleyUIStyles()
    {
        sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            margin = new RectOffset(0, 0, 10, 6),
            fontStyle = FontStyle.Bold
        };

        subheaderStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            margin = new RectOffset(0, 0, 8, 4),
            alignment = TextAnchor.MiddleCenter // Add this line
        };

        foldoutStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13,
            margin = new RectOffset(0, 0, 5, 5)
        };

        richTextLabelStyle = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            wordWrap = true,
            padding = new RectOffset(5, 5, 5, 5)
        };

        helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(15, 15, 15, 15),
            margin = new RectOffset(0, 0, 10, 10)
        };
        
        cardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(12, 12, 12, 12),
            margin = new RectOffset(0, 0, 5, 5)
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
        
        buttonRightAlignStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleRight
        };
        
        selectionCardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(12, 12, 12, 12),
            margin = new RectOffset(0, 0, 8, 8)
        };
        
        headerDividerStyle = new GUIStyle()
        {
            normal = { background = EditorGUIUtility.whiteTexture },
            margin = new RectOffset(0, 0, 4, 12),
            fixedHeight = 1
        };
        
        stepIndicatorStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
        };

        searchBoxStyle = new GUIStyle(EditorStyles.textField)
        {
            margin = new RectOffset(0, 0, 5, 5),
            padding = new RectOffset(5, 20, 2, 2),  // Extra padding for icon
        };
    }
}

public static class BentleyIconHelper
{
    // --- Icon Helper ---
    public static GUIContent GetIconContent(string text, string iconName, string fallbackTooltip = null)
    {
        Texture icon = EditorGUIUtility.IconContent(iconName)?.image;
        if (icon != null)
        {
            return new GUIContent(text, icon, fallbackTooltip ?? text);
        }
        // Fallback to text-only if icon not found
        return new GUIContent(text, fallbackTooltip ?? text);
    }
    
    public static GUIContent GetIconOnlyContent(string iconName, string tooltip)
    {
        Texture icon = EditorGUIUtility.IconContent(iconName)?.image;
        if (icon != null)
        {
            return new GUIContent(icon, tooltip);
        }
        // Fallback to a simple text representation or empty if icon is crucial
        return new GUIContent("?", tooltip); // Placeholder if icon fails
    }
}

public static class BentleyPaginationHelper
{
    // Returns list of page numbers to display (with -1 representing ellipsis)
    public static List<int> GetPaginationNumbers(int currentPage, int totalPages)
    {
        List<int> pages = new List<int>();
        
        if (totalPages <= 5)
        {
            for (int i = 0; i < totalPages; i++)
                pages.Add(i);
            return pages;
        }
        
        if (currentPage == 0)
        {
            pages.AddRange(new int[] { 0, 1, 2, -1, totalPages - 1 });
        }
        else if (currentPage == 1)
        {
            pages.AddRange(new int[] { 0, 1, 2, 3, -1, totalPages - 1 });
        }
        else if (currentPage == 2)
        {
            pages.AddRange(new int[] { 0, 1, 2, 3, 4, -1, totalPages - 1 });
        }
        else if (currentPage > 2 && currentPage < totalPages - 3)
        {
            pages.AddRange(new int[] { 0, -1, currentPage - 1, currentPage, currentPage + 1, -1, totalPages - 1 });
        }
        else if (currentPage == totalPages - 3)
        {
            pages.AddRange(new int[] { 0, -1, totalPages - 5, totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1 });
        }
        else if (currentPage == totalPages - 2)
        {
            pages.AddRange(new int[] { 0, -1, totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1 });
        }
        else if (currentPage == totalPages - 1)
        {
            pages.AddRange(new int[] { 0, -1, totalPages - 3, totalPages - 2, totalPages - 1 });
        }
        
        return pages;
    }
}
