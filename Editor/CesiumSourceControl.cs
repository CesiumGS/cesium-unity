using UnityEngine;
using UnityEditor;
using UnityEditor.VersionControl;

namespace CesiumForUnity
{
    public static class CesiumSourceControl
    {
        public static void PromptToCheckoutEditorSettings()
        {
            /*https://www.listechblog.com/2022/02/use-settingsprovider-to-save-and-load-settings-in-unity-project-settings-or-preferences-window
             https://docs.unity3d.com/ScriptReference/VersionControl.Asset.html
               https://docs.unity3d.com/ScriptReference/VersionControl.Provider.html
            if (Provider.enabled && Provider.isActive)
            {
                Asset settingsAsset = Provider.GetAssetByPath(CesiumEditorSettings.filePath);
                if (settingsAsset != null)
                {
                    if (EditorUtility.DisplayDialog("Cesium Source Control",
                        "The default access token is saved in " + CesiumEditorSettings.filePath
                        + "which is currently not checked out. " +
                        "Would you like to check it out from source control?",
                        "Yes", "No"))
                    {
                        // CHECK OUT
                    }
                }
            }*/
        }
    }
}
