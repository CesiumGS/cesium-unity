using UnityEngine;
using UnityEditor;

public class WelcomeComponent
{
    private BentleyWorkflowEditorWindow parentWindow;
    
    public WelcomeComponent(BentleyWorkflowEditorWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    public void DrawWelcomeSection()
    {
        EditorGUILayout.BeginVertical(BentleyUIStyles.cardStyle);
        
        // Bentley logo/header - Fix the title getting cut off
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        // Use wordWrapped style to prevent cutting off
        GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel) { 
        fontSize = 18, 
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        wordWrap = true    // Add word wrap
        };
        EditorGUILayout.LabelField("Bentley iTwin Mesh Export", titleStyle, GUILayout.ExpandWidth(true));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    
        EditorGUILayout.Space(15);
    
        
        // Welcome message
        EditorGUILayout.LabelField(
            "Welcome to the Bentley iTwin Mesh Export tool for Unity. This tool allows you to export 3D models from iTwin and use them in your Unity projects with Cesium.",
            new GUIStyle(EditorStyles.wordWrappedLabel) { fontSize = 12 }
        );
        
        EditorGUILayout.Space(10);
        
        // How it works section
        EditorGUILayout.LabelField("How it works:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("1. Log in with your Bentley account", BentleyUIStyles.richTextLabelStyle);
        EditorGUILayout.LabelField("2. Select an iTwin project and iModel", BentleyUIStyles.richTextLabelStyle);
        EditorGUILayout.LabelField("3. Choose a changeset version to export", BentleyUIStyles.richTextLabelStyle);
        EditorGUILayout.LabelField("4. Apply the exported model to a Cesium tileset", BentleyUIStyles.richTextLabelStyle);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Prerequisites section
        EditorGUILayout.LabelField("Prerequisites:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("• A Bentley iTwin account with access to projects", BentleyUIStyles.richTextLabelStyle);
        EditorGUILayout.LabelField("• Client ID from the Bentley Developer Portal", BentleyUIStyles.richTextLabelStyle);
        EditorGUILayout.LabelField("• Cesium for Unity package installed", BentleyUIStyles.richTextLabelStyle);
        EditorGUILayout.LabelField("• Make sure that your Redirect URI matches with the one in the file Assets/Scripts/BentleyAuthManager", BentleyUIStyles.richTextLabelStyle);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);

        // Fix the instruction text getting cut off
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIStyle instructionStyle = new GUIStyle(EditorStyles.boldLabel) { 
            fontSize = 13, 
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,   // Ensure text wraps if needed
            fixedWidth = 0     // Let Unity calculate width based on content
        };
        
        // Get started prompt with arrow pointing to login section
        EditorGUILayout.LabelField("Please log in below to get started ↓", instructionStyle, 
        GUILayout.ExpandWidth(true), GUILayout.MinWidth(250));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    
        EditorGUILayout.Space(5);
    
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(15);
    }
}
