using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System;

/// <summary>
/// Handles all authentication-related UI elements and user interactions within the workflow editor.
/// This component provides a complete authentication interface including client ID configuration,
/// login/logout controls, token management, and visual feedback for authentication states.
/// </summary>
public class AuthenticationComponent
{
    /// <summary>
    /// Reference to the parent workflow editor window for state updates and coordination
    /// </summary>
    private BentleyWorkflowEditorWindow parentWindow;
    
    /// <summary>
    /// Initializes the authentication component with a reference to the parent window.
    /// </summary>
    /// <param name="parentWindow">The workflow editor window that contains this component</param>
    public AuthenticationComponent(BentleyWorkflowEditorWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Renders the complete authentication section UI including client ID configuration,
    /// login/logout controls, token status display, and authentication progress feedback.
    /// The UI adapts its layout based on the current authentication state.
    /// </summary>
    public void DrawAuthSection()
    {
        // Section header with icon
        EditorGUILayout.BeginHorizontal();
        
        if (!parentWindow.authManager.IsLoggedIn())
        {
            // More prominent header when logged out
            EditorGUILayout.LabelField("Authentication", BentleyUIStyles.sectionHeaderStyle);
        }
        else
        {
            // Collapsible section when logged in
            parentWindow.showAuthSection = EditorGUILayout.Foldout(parentWindow.showAuthSection, "Authentication", true, BentleyUIStyles.foldoutStyle);
            
            // Show login status indicator
            GUIStyle statusStyle = new GUIStyle(EditorStyles.miniLabel);
            statusStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField("● Logged in", statusStyle, GUILayout.Width(70));
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Only check if section should be shown when logged in
        if (!parentWindow.authManager.IsLoggedIn() || parentWindow.showAuthSection)
        {
            // Rest of authentication section remains the same...
            EditorGUILayout.BeginVertical(BentleyUIStyles.cardStyle);
            
            // Client ID field with better spacing and alignment
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.LabelField("Client ID", GUILayout.Width(80));
            parentWindow.clientId = EditorGUILayout.TextField(parentWindow.clientId, GUILayout.Height(20));
            
            if (EditorGUI.EndChangeCheck())
            {
                parentWindow.authManager.clientId = parentWindow.clientId;
            }
            
            if (GUILayout.Button(BentleyIconHelper.GetIconOnlyContent("d_SaveAs", "Save Client ID to Editor Preferences"), 
                GUILayout.Width(28), GUILayout.Height(20)))
            {
                parentWindow.authManager.SaveClientId();
                EditorUtility.DisplayDialog("Client ID Saved", "Client ID saved to Editor Preferences.", "OK");
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Advanced Settings Section
            EditorGUILayout.Space(5);
            parentWindow.showAdvancedAuthSettings = EditorGUILayout.Foldout(
                parentWindow.showAdvancedAuthSettings, 
                "Advanced Settings", 
                true, 
                BentleyUIStyles.foldoutStyle
            );
            
            if (parentWindow.showAdvancedAuthSettings)
            {
                EditorGUILayout.BeginVertical(BentleyUIStyles.helpBoxStyle);
                
                // Redirect URI field
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.LabelField("Redirect URI", GUILayout.Width(80));
                parentWindow.redirectUri = EditorGUILayout.TextField(
                    parentWindow.redirectUri, 
                    GUILayout.Height(20)
                );
                
                if (EditorGUI.EndChangeCheck())
                {
                    parentWindow.authManager.redirectUri = parentWindow.redirectUri;
                }
                
                if (GUILayout.Button(BentleyIconHelper.GetIconOnlyContent("d_SaveAs", "Save Redirect URI to Editor Preferences"), 
                    GUILayout.Width(28), GUILayout.Height(20)))
                {
                    parentWindow.authManager.SaveRedirectUri();
                    EditorUtility.DisplayDialog("Redirect URI Saved", "Redirect URI saved to Editor Preferences.", "OK");
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Reset to default button
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset to Default", GUILayout.Width(120), GUILayout.Height(20)))
                {
                    parentWindow.redirectUri = AuthConstants.DEFAULT_REDIRECT_URI;
                    parentWindow.authManager.redirectUri = parentWindow.redirectUri;
                    parentWindow.authManager.SaveRedirectUri();
                    parentWindow.Repaint();
                }
                EditorGUILayout.EndHorizontal();
                
                // Help text
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField(
                    "⚠️ Important: Make sure this URI matches exactly with the Redirect URI configured in your iTwin app registration.", 
                    BentleyUIStyles.richTextLabelStyle
                );
                
                EditorGUILayout.EndVertical();
            }
            
            // Make login button more prominent when logged out
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(parentWindow.currentState == WorkflowState.LoggingIn);
            
            string loginButtonText = parentWindow.authManager.IsLoggedIn() ? "Refresh Token" : "Login to Bentley";
            GUIContent loginButtonContent = new GUIContent(loginButtonText, "Login with Bentley account");

            if (!parentWindow.authManager.IsLoggedIn())
            {
                // Larger, centered login button when logged out
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(loginButtonContent, GUILayout.Height(36), GUILayout.Width(200)))
                {
                    parentWindow.StopCurrentCoroutine();
                    parentWindow.currentState = WorkflowState.LoggingIn;
                    parentWindow.statusMessage = "Initiating login...";
                    parentWindow.Repaint();
                    parentWindow.currentCoroutineHandle = EditorCoroutineUtility.StartCoroutine(
                        parentWindow.authManager.GetAccessTokenCoroutine(parentWindow.OnGetTokenComplete), parentWindow);
                }
                GUILayout.FlexibleSpace();
            }
            else
            {
                // Normal sized button when logged in
                if (GUILayout.Button(loginButtonContent, GUILayout.Height(30)))
                {
                    parentWindow.StopCurrentCoroutine();
                    parentWindow.currentState = WorkflowState.LoggingIn;
                    parentWindow.statusMessage = "Initiating login...";
                    parentWindow.Repaint();
                    parentWindow.currentCoroutineHandle = EditorCoroutineUtility.StartCoroutine(
                        parentWindow.authManager.GetAccessTokenCoroutine(parentWindow.OnGetTokenComplete), parentWindow);
                }
                
                GUILayout.Space(10);
                GUIContent logoutButtonContent = new GUIContent("Logout", "Logout from Bentley account");

                if (GUILayout.Button(logoutButtonContent, GUILayout.Height(30)))
                {
                    parentWindow.StopCurrentCoroutine();
                    parentWindow.authManager.Logout();
                    parentWindow.UpdateLoginState();
                }
            }
            
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            // Add token expiration info when logged in
            if (parentWindow.authManager.IsLoggedIn() && parentWindow.authManager.GetCurrentAccessToken() != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Token expires:", GUILayout.Width(100));
                
                // Format expiration time with color based on how close it is to expiration
                DateTime expiry = parentWindow.authManager.GetExpiryTimeUtc().ToLocalTime();
                TimeSpan timeLeft = expiry - DateTime.Now;
                
                GUIStyle expiryStyle = new GUIStyle(EditorStyles.label);
                if (timeLeft.TotalMinutes < 5)
                    expiryStyle.normal.textColor = Color.red;
                else if (timeLeft.TotalMinutes < 15)
                    expiryStyle.normal.textColor = new Color(1.0f, 0.5f, 0.0f); // Orange
                
                EditorGUILayout.LabelField(expiry.ToString("HH:mm:ss - MM/dd/yyyy"), expiryStyle);
                EditorGUILayout.EndHorizontal();
            }
            
            // Show login status indicator when logging in
            if (parentWindow.currentState == WorkflowState.LoggingIn)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                // Create a horizontal group with fixed width to match the login button
                EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
                GUILayout.Label(BentleyWorkflowEditorWindow.SpinnerContent, GUILayout.Width(24), GUILayout.Height(24));
                // Use centered text style
                GUIStyle centeredLabelStyle = new GUIStyle(EditorStyles.label) { 
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12
                };
                EditorGUILayout.LabelField("Logging in...", centeredLabelStyle);
                EditorGUILayout.EndHorizontal();
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(15);
    }
}
