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

            Rect position = currentWindow.position;
            position.width = 400;
            position.height = 225;
            position.x = (Screen.width / 2) + (position.width / 2);
            position.y = (Screen.height / 2);
            currentWindow.position = position;

            currentWindow.ShowModalUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Before " + this._missingAssetName + " can be added " +
                "to your level, it must be added to \"My Assets\" in your Cesium ion account.",
                EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);

            if (EditorGUILayout.LinkButton("Open this asset in the Cesium ion Asset Depot"))
            {
                Application.OpenURL(this._missingAssetURL);
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Click \"Add to my assets\" in the Cesium ion web page, " +
                "then return to Cesium For Unity and try adding this asset again.",
                EditorStyles.wordWrappedLabel);

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Close", GUILayout.Width(180), GUILayout.Height(35)))
            {
                this.Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }
    }
}