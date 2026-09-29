using UnityEngine;
using UnityEditor;

public enum WorkflowState
{
    Idle, LoggingIn, LoggedIn,
    FetchingITwins, SelectITwin,
    FetchingIModels, SelectIModel,
    StartingExport, PollingExport, ExportComplete, Error
}

public class WorkflowStateManager
{
    private BentleyWorkflowEditorWindow parentWindow;
    
    public WorkflowStateManager(BentleyWorkflowEditorWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    // Helper method to determine which step level we're at
    public int GetStepLevel(WorkflowState state)
    {
        if (state <= WorkflowState.LoggedIn)
            return 1; // Authentication
        else if (state <= WorkflowState.SelectIModel) 
            return 2; // Select Data
        else
            return 3; // Export
    }
    
    public MessageType GetMessageType(WorkflowState state)
    {
        switch (state)
        {
            case WorkflowState.Idle:
            case WorkflowState.LoggingIn:
                return MessageType.Info;
            case WorkflowState.LoggedIn:
            case WorkflowState.FetchingITwins:
            case WorkflowState.SelectITwin:
            case WorkflowState.FetchingIModels:
            case WorkflowState.SelectIModel:
            case WorkflowState.StartingExport:
            case WorkflowState.PollingExport:
                return MessageType.Info;
            case WorkflowState.ExportComplete:
                return MessageType.Info;
            case WorkflowState.Error:
                return MessageType.Error;
            default:
                return MessageType.Info;
        }
    }
}
