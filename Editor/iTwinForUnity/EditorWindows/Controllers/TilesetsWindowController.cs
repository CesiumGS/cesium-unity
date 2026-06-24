using UnityEngine;
using UnityEditor;

public class TilesetsWindowController
{
    private BentleyTilesetsWindow parentWindow;
    
    public TilesetsWindowController(BentleyTilesetsWindow parentWindow)
    {
        this.parentWindow = parentWindow;
    }
    
    /// <summary>
    /// Handles the editor update for animation timing
    /// </summary>
    public void OnEditorUpdate()
    {
        // Update spinner animation
        if (Time.realtimeSinceStartup - parentWindow.lastRepaintTime > TilesetConstants.SPINNER_FRAME_RATE)
        {
            parentWindow.lastRepaintTime = Time.realtimeSinceStartup;
            parentWindow.spinnerFrame = (parentWindow.spinnerFrame + 1) % TilesetConstants.SpinnerFrames.Length;
            parentWindow.Repaint();
        }
    }
    
    /// <summary>
    /// Handles keyboard input for the window
    /// </summary>
    public void HandleKeyboardInput()
    {
        Event e = Event.current;
        
        if (e.type == EventType.KeyDown)
        {
            // F5 to refresh
            if (e.keyCode == KeyCode.F5)
            {
                // Refresh through data manager if available
                var dataManager = new TilesetDataManager(parentWindow);
                dataManager.RefreshSavedViews();
                e.Use();
            }
            
            // CTRL+F to focus search box
            if (e.keyCode == KeyCode.F && e.control)
            {
                GUI.FocusControl(TilesetConstants.SEARCH_CONTROL_NAME);
                e.Use();
            }
        }
    }
    
    /// <summary>
    /// Gets the current spinner frame character
    /// </summary>
    public string GetCurrentSpinnerFrame()
    {
        return TilesetConstants.SpinnerFrames[parentWindow.spinnerFrame];
    }
}
