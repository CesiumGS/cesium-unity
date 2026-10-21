using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

public class ExportComponent
{
    private BentleyWorkflowEditorWindow parentWindow;
    
    public ExportComponent(BentleyWorkflowEditorWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    public void DrawExportSection()
    {
        EditorGUILayout.LabelField("Export Settings", BentleyUIStyles.sectionHeaderStyle);
        
        // Draw divider under header
        var dividerRect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(dividerRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(8);
        
        EditorGUILayout.BeginVertical(BentleyUIStyles.cardStyle);

        // Skip displaying these when export is complete
        if (parentWindow.currentState != WorkflowState.ExportComplete)
        {
            if (!string.IsNullOrEmpty(parentWindow.selectedIModelId))
            {
                EditorGUILayout.LabelField("iModel ID", parentWindow.selectedIModelId);
            }
            else
            {
                parentWindow.iModelId = EditorGUILayout.TextField(new GUIContent("iModel ID", "The ID of the iModel to export"), parentWindow.iModelId);
            }
            
            EditorGUILayout.Space(10);

            bool canStartExport = (parentWindow.currentState == WorkflowState.StartingExport || parentWindow.currentState == WorkflowState.LoggedIn || 
                                  parentWindow.currentState == WorkflowState.ExportComplete)
                                  && !string.IsNullOrEmpty(parentWindow.selectedIModelId) && parentWindow.currentCoroutineHandle == null;

            EditorGUI.BeginDisabledGroup(!canStartExport);

            GUIContent exportButtonContent = new GUIContent("Start Export", "Begin the mesh export workflow");

            if (GUILayout.Button(exportButtonContent, GUILayout.Height(30)))
            {
                parentWindow.StopCurrentCoroutine();
                parentWindow.statusMessage = "Starting export workflow...";
                parentWindow.currentState = WorkflowState.StartingExport;
                parentWindow.Repaint();
                parentWindow.currentCoroutineHandle = EditorCoroutineUtility.StartCoroutine(parentWindow.RunFullExportWorkflowCoroutine(), parentWindow);
            }
            EditorGUI.EndDisabledGroup();

            // Back button that goes to data selection
            if (canStartExport)
            {
                if (GUILayout.Button("← Back to Data Selection", GUILayout.Height(22)))
                {
                    parentWindow.currentState = WorkflowState.SelectIModel;
                    parentWindow.statusMessage = "Select an iModel to export.";
                    parentWindow.Repaint();
                }
            }

            if (!canStartExport && parentWindow.currentCoroutineHandle == null)
            {
                EditorGUILayout.HelpBox("Select an iModel to start an export.", MessageType.Warning);
            }
        }

        // Export status indicators
        if (parentWindow.currentCoroutineHandle != null && 
            (parentWindow.currentState == WorkflowState.StartingExport || 
             parentWindow.currentState == WorkflowState.PollingExport || 
             parentWindow.currentState == WorkflowState.LoggingIn))
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox($"{parentWindow.statusMessage}", MessageType.Info);
            
            // Progress bar
            EditorGUILayout.Space(5);
            float progress = (float)(EditorApplication.timeSinceStartup % 2.0f) / 2.0f;
            Rect progressRect = EditorGUILayout.GetControlRect(false, 8);
            EditorGUI.DrawRect(progressRect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
            Rect fillRect = new Rect(progressRect);
            fillRect.width = fillRect.width * progress;
            EditorGUI.DrawRect(fillRect, new Color(0.0f, 0.7f, 0.0f, 0.7f));
        }
        else if (parentWindow.currentState == WorkflowState.Error)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox($"Error: {parentWindow.statusMessage}", MessageType.Error);
        }
        // Show success message only when export is complete
        else if (parentWindow.currentState == WorkflowState.ExportComplete)
        {
            EditorGUILayout.Space(15);
            
            // Success message and icon
            EditorGUILayout.BeginHorizontal();
            GUIStyle successIconStyle = new GUIStyle(EditorStyles.label);
            successIconStyle.normal.textColor = Color.green;
            GUIContent checkIcon = new GUIContent("✓");
            EditorGUILayout.LabelField(checkIcon, successIconStyle, GUILayout.Width(20));
            EditorGUILayout.LabelField("Export Complete!", BentleyUIStyles.subheaderStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Your export has been successfully completed. You can now use the Cesium Integration section below to apply the exported tileset to your scene.", MessageType.Info);
            
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Start New Export", GUILayout.Height(30)))
            {
                parentWindow.currentState = WorkflowState.LoggedIn;
                parentWindow.statusMessage = "Ready to begin a new export.";
                parentWindow.Repaint();
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(15);
    }
}
