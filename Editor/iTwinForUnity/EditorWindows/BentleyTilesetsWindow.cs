using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class BentleyTilesetsWindow : EditorWindow
{
    #region Fields and Properties
    // Data - public for component access
    public Vector2 scrollPosition;
    public List<BentleyTilesetMetadata> savedViews = new List<BentleyTilesetMetadata>();
    public string searchText = "";
    public bool shouldClearSearchFocus = false;
    
    // Animation and UI states - public for component access
    public float lastRepaintTime = 0f;
    public int spinnerFrame = 0;
    public readonly string[] spinnerFrames = new string[] { "◐", "◓", "◑", "◒" };
    public int hoveredCardIndex = -1;
    
    // Colors
    private readonly Color headerBgColor = new Color(0.15f, 0.15f, 0.15f);
    private readonly Color cardBgColor = new Color(0.22f, 0.22f, 0.22f);
    private readonly Color cardBgHoverColor = new Color(0.26f, 0.26f, 0.26f);
    private readonly Color accentColor = new Color(0.2f, 0.6f, 0.9f);
    private readonly Color primaryTextColor = new Color(0.9f, 0.9f, 0.9f);
    private readonly Color secondaryTextColor = new Color(0.7f, 0.7f, 0.7f);
    
    // Pagination - public for component access
    public int currentPage = 0;
    public const int ITEMS_PER_PAGE = 3;
    
    // Textures and visual elements
    private Texture2D cardBackgroundTexture;
    private Texture2D cardBackgroundHoverTexture;
    private Texture2D noThumbnailTexture;
    private GUIContent searchIcon;
    private GUIContent clearIcon;
    private GUIContent backIcon;
    private GUIContent hierarchyIcon;
    private GUIContent focusIcon;
    private GUIContent webIcon;
    
    // Styles
    private GUIStyle headerStyle; 
    private GUIStyle titleStyle;
    private GUIStyle descriptionStyle;
    private GUIStyle searchBoxStyle;
    private GUIStyle cardStyle;
    private GUIStyle buttonStyle;
    private GUIStyle iconButtonStyle;
    private GUIStyle emptyStateStyle;
    private GUIStyle spinnerStyle;
    private GUIStyle breadcrumbStyle;
    private GUIStyle infoLabelStyle;
    private GUIStyle backButtonStyle;
    private GUIStyle footerStyle;
    private GUIStyle dividerStyle;
    private GUIStyle selectionCardStyle;
    private GUIStyle itemTitleStyle;
    private GUIStyle itemDescriptionStyle;
    
    // Component instances - following the established architecture pattern
    private TilesetsWindowController windowController;
    private TilesetDataManager dataManager;
    private SearchComponent searchComponent;
    private TilesetCardRenderer cardRenderer;
    private PaginationComponent paginationComponent;
    private EmptyStateComponent emptyStateComponent;
    #endregion

    [MenuItem("Bentley/Tilesets")]
    public static void ShowWindow()
    {
        var window = GetWindow<BentleyTilesetsWindow>("Tilesets");
        window.minSize = new Vector2(500, 400);
    }

    #region Initialization & Cleanup
    private void OnEnable()
    {
        // Initialize components following the established pattern
        InitializeComponents();
        
        // Refresh data through data manager
        dataManager?.RefreshSavedViews();
        
        EditorApplication.update += OnEditorUpdate;
    }

    private void InitializeComponents()
    {
        // Initialize all components following the dependency injection pattern
        windowController = new TilesetsWindowController(this);
        dataManager = new TilesetDataManager(this);
        searchComponent = new SearchComponent(this);
        cardRenderer = new TilesetCardRenderer(this);
        paginationComponent = new PaginationComponent(this);
        emptyStateComponent = new EmptyStateComponent(this);
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        TilesetsUIStyles.CleanupTextures();
    }
    
    private void OnEditorUpdate()
    {
        // Delegate to window controller
        windowController?.OnEditorUpdate();
    }
    #endregion
    
    #region GUI Methods
    private void OnGUI()
    {
        // Initialize styles if not already done
        TilesetsUIStyles.InitializeStyles();
        
        // Check if components are initialized
        if (searchComponent == null) return;
        
        EditorGUILayout.BeginVertical();
        
        DrawTilesetsSection();
        DrawFooter();
        
        EditorGUILayout.EndVertical();
        
        // Handle keyboard shortcuts through controller
        windowController?.HandleKeyboardInput();
    }
    
    private void DrawFooter()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(6);
    }
    
    private void DrawTilesetsSection()
    {
        GUILayout.Space(16);
        
        // Delegate search UI to search component
        searchComponent.DrawSearchSection();
        
        // Get filtered views from search component
        var filteredViews = searchComponent.GetFilteredViews();
        
        // Content area
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        
        if (savedViews.Count == 0 || filteredViews.Count == 0)
        {
            // Handle empty states
            searchComponent.GetEmptyStateMessage(out string title, out string description);
            emptyStateComponent.DrawEmptyState(title, description);
        }
        else
        {
            DrawMainContent(filteredViews);
        }
        
        GUILayout.Space(16);
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawMainContent(System.Collections.Generic.List<BentleyTilesetMetadata> filteredViews)
    {
        // Main content with scrollable list
        EditorGUILayout.BeginVertical();
        
        // Scrollable list
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Get paged items from pagination component
        paginationComponent.GetPagedItems(filteredViews, out int startIndex, out int endIndex);
        
        // Display only the current page using card renderer
        for (int i = startIndex; i < endIndex; i++)
        {
            cardRenderer.DrawTilesetCard(filteredViews[i], i);
        }
        
        EditorGUILayout.EndScrollView();
        
        GUILayout.Space(10);
        
        // Pagination controls
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        paginationComponent.DrawPaginationControls(filteredViews);
            
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    #endregion
}