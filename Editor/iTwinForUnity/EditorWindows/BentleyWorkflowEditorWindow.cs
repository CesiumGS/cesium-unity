using UnityEngine;
using UnityEditor;
using System.Collections;
using CesiumForUnity;
using Unity.EditorCoroutines.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

public class BentleyWorkflowEditorWindow : EditorWindow
{
    // Managers
    public BentleyAuthManager authManager;
    public MeshExportClient meshClient;

    // Input Parameters
    public string clientId = "";
    public string redirectUri = "";
    public string iModelId = "YOUR_IMODEL_ID";
    public string changesetId = "";
    public string exportType = "3DTILES";

    // State - Using proper WorkflowState enum
    public WorkflowState currentState = WorkflowState.Idle;
    public string statusMessage = "Ready";
    public string finalTilesetUrl = "";
    public EditorCoroutine currentCoroutineHandle = null;

    // Target Cesium Tileset
    public Cesium3DTileset targetCesiumTileset;

    // EditorPrefs Keys
    private const string PrefIModelId = "Bentley_Editor_iModelId";
    private const string PrefChangesetId = "Bentley_Editor_ChangesetId";

    // iTwin/iModel/Changeset Selection State - Using proper data types
    public List<ITwin> myITwins = new List<ITwin>();
    public List<IModel> myIModels = new List<IModel>();
    public List<ChangeSet> myChangeSets = new List<ChangeSet>();
    public string selectedITwinId;
    public string selectedIModelId;

    public static readonly GUIContent SpinnerContent = EditorGUIUtility.IconContent("WaitSpin00");

    // UI State & Styling
    public bool showAuthSection = true;
    public bool showAdvancedAuthSettings = false;
    private Vector2 scrollPosition;

    // Pagination state variables
    public int iTwinsCurrentPage = 0;
    public int iModelsCurrentPage = 0;
    public const int ITEMS_PER_PAGE = 3;
    public string iTwinsSearchText = "";
    public string iModelsSearchText = "";

    // UI state flag
    public bool shouldClearSearchFocus = false;
    
    // Advanced options state
    public bool advancedCesiumOptionsVisible = false;
    
    // Animation timing
    public float lastRepaintTime = 0f;

    // Component instances - use object type to avoid circular dependencies during compilation
    private object stateManager;
    private object apiClient;
    private object helperMethods;
    private object welcomeComponent;
    private object stepIndicatorComponent;
    private object authComponent;
    private object projectBrowserComponent;
    private object exportComponent;
    private object cesiumComponent;

    [MenuItem("Bentley/Mesh Export")]
    public static void ShowWindow()
    {
        var window = GetWindow<BentleyWorkflowEditorWindow>("Bentley Mesh Export");
        window.minSize = new Vector2(500, 650);
    }

    private void OnEnable()
    {
        authManager = new BentleyAuthManager();
        meshClient = new MeshExportClient();
        authManager.RequestExchangeCode = StartExchangeCodeCoroutine;

        clientId = authManager.clientId;
        redirectUri = authManager.redirectUri;
        iModelId = EditorPrefs.GetString(PrefIModelId, iModelId);
        changesetId = EditorPrefs.GetString(PrefChangesetId, changesetId);

        // Initialize components - delay to avoid circular dependency
        EditorApplication.delayCall += InitializeComponents;

        EditorApplication.update += AnimateSpinner;
        
        UpdateLoginState();
    }

    private void InitializeComponents()
    {
        stateManager = new WorkflowStateManager(this);
        apiClient = new BentleyAPIClient(this);
        helperMethods = new BentleyHelperMethods(this);
        welcomeComponent = new WelcomeComponent(this);
        stepIndicatorComponent = new WorkflowStepIndicatorComponent(this, (WorkflowStateManager)stateManager);
        authComponent = new AuthenticationComponent(this);
        projectBrowserComponent = new ProjectBrowserComponent(this);
        exportComponent = new ExportComponent(this);
        cesiumComponent = new CesiumIntegrationComponent(this);
    }

    private void AnimateSpinner()
    {
        if (EditorApplication.timeSinceStartup - lastRepaintTime > 0.1f)
        {
            lastRepaintTime = (float)EditorApplication.timeSinceStartup;
            Repaint();
        }
    }

    private void OnDisable()
    {
        authManager?.SaveClientId();
        EditorPrefs.SetString(PrefIModelId, iModelId);
        EditorPrefs.SetString(PrefChangesetId, changesetId);
        StopCurrentCoroutine();
        // Only clean up HTTP listener, preserve tokens for next session
        authManager?.CleanupWithoutLogout();
        EditorApplication.update -= AnimateSpinner;
    }

    public void UpdateLoginState()
    {
        if (authManager.IsLoggedIn())
        {
            if (authManager.GetCurrentAccessToken() != null)
            {
                currentState = WorkflowState.LoggedIn;
                statusMessage = $"Logged In. Token expires: {authManager.GetExpiryTimeUtc().ToLocalTime()}";
            }
            else
            {
                currentState = WorkflowState.Idle;
                statusMessage = "Login expired. Refresh or Login needed.";
            }
        }
        else
        {
            currentState = WorkflowState.Idle;
            statusMessage = "Not logged in.";
        }
        Repaint();
    }

    private void OnGUI()
    {
        // Check if components are initialized
        if (welcomeComponent == null) return;
        
        EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(15, 15, 15, 15) });
        
        if (authManager.IsLoggedIn())
        {
            ((WorkflowStepIndicatorComponent)stepIndicatorComponent).DrawWorkflowStepIndicator();
            EditorGUILayout.Space(5);
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (!authManager.IsLoggedIn())
        {
            ((WelcomeComponent)welcomeComponent).DrawWelcomeSection();
            ((AuthenticationComponent)authComponent).DrawAuthSection();
        }
        else
        {
            ((AuthenticationComponent)authComponent).DrawAuthSection();
            
            if (currentState >= WorkflowState.LoggedIn && currentState <= WorkflowState.SelectIModel)
            {
                ((ProjectBrowserComponent)projectBrowserComponent).DrawITwinIModelSelection();
            }
            else
            {
                ((ExportComponent)exportComponent).DrawExportSection();
            }

            if (currentState == WorkflowState.ExportComplete)
            {
                ((CesiumIntegrationComponent)cesiumComponent).DrawCesiumSection();
            }
        }
        
        EditorGUILayout.Space(20);
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    // Auth callback methods
    private void StartExchangeCodeCoroutine(string code)
    {
        statusMessage = "Authorization code received, exchanging for token...";
        Repaint(); 
        EditorCoroutineUtility.StartCoroutine(
            authManager.ExchangeCodeCoroutine(code, OnExchangeCodeComplete), this);
    }

    private void OnExchangeCodeComplete(bool success, string error)
    {
        if (!success)
        {
            statusMessage = $"Error during token exchange: {error}";
            currentState = WorkflowState.Error;
            currentCoroutineHandle = null;
            Repaint();
        }
    }

    public void OnGetTokenComplete(string token, string error)
    {
        currentCoroutineHandle = null;
        if (!string.IsNullOrEmpty(error))
        {
            statusMessage = $"Failed to get token: {error}";
            currentState = WorkflowState.Error;
            Debug.LogError("BentleyWorkflowEditorWindow: " + statusMessage);
        }
        else
        {
            if (!string.IsNullOrEmpty(token))
            {
                EditorPrefs.SetString("Bentley_Auth_Token", token);
            }
            
            statusMessage = $"Token ready. Expires: {authManager.GetExpiryTimeUtc().ToLocalTime()}";
            currentState = WorkflowState.LoggedIn;
        }
        Repaint();
    }

    // Delegate coroutines to API client
    public IEnumerator GetMyITwinsCoroutine()
    {
        if (apiClient != null)
        {
            yield return ((BentleyAPIClient)apiClient).GetMyITwinsCoroutine();
        }
    }

    public IEnumerator FetchITwinThumbnail(ITwin twin)
    {
        if (apiClient != null)
        {
            yield return ((BentleyAPIClient)apiClient).FetchITwinThumbnail(twin);
        }
    }

    public IEnumerator GetITwinIModelsCoroutine(string itwinId)
    {
        if (apiClient != null)
        {
            yield return ((BentleyAPIClient)apiClient).GetITwinIModelsCoroutine(itwinId);
        }
    }

    public IEnumerator FetchIModelThumbnail(IModel model)
    {
        if (apiClient != null)
        {
            yield return ((BentleyAPIClient)apiClient).FetchIModelThumbnail(model);
        }
    }

    public IEnumerator FetchIModelDetailsCoroutine(IModel model)
    {
        if (apiClient != null)
        {
            yield return ((BentleyAPIClient)apiClient).FetchIModelDetailsCoroutine(model);
        }
    }

    public IEnumerator FetchIModelChangesets(IModel model)
    {
        if (apiClient != null)
        {
            yield return ((BentleyAPIClient)apiClient).FetchIModelChangesets(model);
        }
    }

    public IEnumerator GetChangeSetsCoroutine(string imodelId)
    {
        if (apiClient != null)
        {
            yield return ((BentleyAPIClient)apiClient).GetChangeSetsCoroutine(imodelId);
        }
    }

    public IEnumerator RunFullExportWorkflowCoroutine()
    {
        if (apiClient != null)
        {
            yield return ((BentleyAPIClient)apiClient).RunFullExportWorkflowCoroutine();
        }
    }

    // Delegate helper methods
    public string GetDisplayDescription(IModel model)
    {
        if (helperMethods != null)
            return ((BentleyHelperMethods)helperMethods).GetDisplayDescription(model);
        return "Loading...";
    }

    public string GetAuthToken()
    {
        if (helperMethods != null)
            return ((BentleyHelperMethods)helperMethods).GetAuthToken();
        return authManager?.GetCurrentAccessToken() ?? "";
    }

    public void UpdateMetadataWithThumbnail(BentleyTilesetMetadata metadata, ITwin selectedITwin, IModel selectedIModel)
    {
        if (helperMethods != null)
            ((BentleyHelperMethods)helperMethods).UpdateMetadataWithThumbnail(metadata, selectedITwin, selectedIModel);
    }

    public void StopCurrentCoroutine()
    {
        if (helperMethods != null)
            ((BentleyHelperMethods)helperMethods).StopCurrentCoroutine();
        else if (currentCoroutineHandle != null)
        {
            EditorCoroutineUtility.StopCoroutine(currentCoroutineHandle);
            currentCoroutineHandle = null;
        }
    }

    public IEnumerator DelayedPlaceOrigin(GameObject selectedObject)
    {
        if (helperMethods != null)
        {
            yield return ((BentleyHelperMethods)helperMethods).DelayedPlaceOrigin(selectedObject);
        }
    }
}
