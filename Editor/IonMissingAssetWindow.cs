using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public class IonMissingAssetWindow : EditorWindow
    {
        private string _missingAssetName = "";
        private string _missingAssetURL = "";

        public static void ShowWindow(string assetName, long assetID)
        {
            IonMissingAssetWindow currentWindow =
                GetWindow<IonMissingAssetWindow>("Asset is not available in MyAssets");

            currentWindow._missingAssetName = assetName;
            currentWindow._missingAssetURL = "https://cesium.com/ion/assetdepot/" + assetID;

            currentWindow.ShowModalUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Before " + this._missingAssetName + " can be added " +
                "to your level, it must be added to \"My Assets\" in your Cesium ion account.",
                EditorStyles.wordWrappedLabel);

            if (EditorGUILayout.LinkButton("Open this asset in the Cesium ion Asset Depot"))
            {
                Application.OpenURL(this._missingAssetURL);
            }

            EditorGUILayout.LabelField(
                "Click \"Add to my assets\" in the Cesium ion web page, " +
                "then return to Cesium For Unity and try adding this asset again.",
                EditorStyles.wordWrappedLabel);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Close", GUILayout.Width(100)))
            {
                this.Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}