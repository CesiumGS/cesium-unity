using UnityEngine;
using UnityEditor;

public class WorkflowStepIndicatorComponent
{
    private BentleyWorkflowEditorWindow parentWindow;
    private WorkflowStateManager stateManager;
    
    public WorkflowStepIndicatorComponent(BentleyWorkflowEditorWindow parentWindow, WorkflowStateManager stateManager)
    {
        this.parentWindow = parentWindow;
        this.stateManager = stateManager;
    }
    
    public void DrawWorkflowStepIndicator()
    {
        // Create a breadcrumb-like step indicator
        EditorGUILayout.BeginHorizontal();
        
        // Current state for navigation control
        int currentStepLevel = stateManager.GetStepLevel(parentWindow.currentState);
        
        // Step 1: Authentication
        GUIStyle step1Style = new GUIStyle(BentleyUIStyles.stepIndicatorStyle);
        step1Style.richText = true;
        if (currentStepLevel >= 1) {
            string step1Text = currentStepLevel == 1 ? "<b>1. Authentication</b>" : "1. Authentication";
            if (GUILayout.Button(step1Text, step1Style, GUILayout.Height(20)))
            {
                parentWindow.currentState = WorkflowState.LoggedIn;
                parentWindow.statusMessage = "Returned to Authentication.";
                // Clear all search text when navigating via step indicator
                parentWindow.iTwinsSearchText = "";
                parentWindow.iModelsSearchText = "";
                parentWindow.Repaint();
            }
        } else {
            EditorGUILayout.LabelField("<b>1. Authentication</b>", step1Style);
        }
        
        EditorGUILayout.LabelField("→", step1Style, GUILayout.Width(15));
        
        // Step 2: Select Data - Clickable if we're past this step
        GUIStyle step2Style = new GUIStyle(BentleyUIStyles.stepIndicatorStyle);
        step2Style.richText = true;
        if (currentStepLevel > 2) {
            string step2Text = "2. Select Data";
            if (GUILayout.Button(step2Text, step2Style, GUILayout.Height(20)))
            {
                parentWindow.currentState = WorkflowState.SelectITwin;
                parentWindow.statusMessage = "Returned to Data Selection.";
                // Clear all search text when navigating via step indicator
                parentWindow.iTwinsSearchText = "";
                parentWindow.iModelsSearchText = "";
                parentWindow.Repaint();
            }
        } else {
            string step2Text = currentStepLevel == 2 ? "<b>2. Select Data</b>" : "2. Select Data";
            EditorGUILayout.LabelField(step2Text, step2Style);
        }
        
        EditorGUILayout.LabelField("→", step1Style, GUILayout.Width(15));
        
        // Step 3: Export - Never clickable as a navigation option
        GUIStyle step3Style = new GUIStyle(BentleyUIStyles.stepIndicatorStyle);
        step3Style.richText = true;
        string step3Text = currentStepLevel == 3 ? "<b>3. Export</b>" : "3. Export";
        EditorGUILayout.LabelField(step3Text, step3Style);
        
        EditorGUILayout.EndHorizontal();
        
        // Draw a subtle divider
        var dividerRect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(dividerRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        EditorGUILayout.Space(8);
    }
}
