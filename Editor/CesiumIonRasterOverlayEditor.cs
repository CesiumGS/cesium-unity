using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumIonRasterOverlay))]
    public class CesiumIonRasterOverlayEditor : Editor
    {
        private CesiumIonRasterOverlay _ionOverlay;
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _ionAssetID;
        private SerializedProperty _ionAccessToken;

        private void OnEnable()
        {
            this._ionOverlay = (CesiumIonRasterOverlay)this.target;
            this._rasterOverlayEditor = 
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                    this.target,
                                                    typeof(CesiumRasterOverlayEditor));

            this._ionAssetID = this.serializedObject.FindProperty("_ionAssetID");
            this._ionAccessToken = this.serializedObject.FindProperty("_ionAccessToken");
        }

        private void OnDisable()
        {
            if(this._rasterOverlayEditor != null)
            {
                DestroyImmediate(this._rasterOverlayEditor);
            }
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            DrawTroubleshootButton();
            EditorGUILayout.Space(5);
            DrawIonProperties();
            EditorGUILayout.Space(5);
            DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawTroubleshootButton()
        {
            GUIContent troubleshootTokenContent = new GUIContent(
               "Troubleshoot Token",
               "Check if the Cesium ion token used to access this raster overlay is working " +
               "correctly, and fix it if necessary.");
            if (GUILayout.Button(troubleshootTokenContent))
            {
                IonTokenTroubleshootingWindow.ShowWindow(this._ionOverlay, false);
            }
        }

        private void DrawIonProperties()
        {
            GUIContent ionAssetIDContent = new GUIContent(
                "ion Asset ID",
                "The ID of the Cesium ion asset to use.");
            EditorGUILayout.DelayedIntField(this._ionAssetID, ionAssetIDContent);

            GUIContent ionAccessTokenContent = new GUIContent(
                "ion Access Token",
                "The access token to use to access the Cesium ion resource.");
            EditorGUILayout.DelayedTextField(this._ionAccessToken, ionAccessTokenContent);
        }

        private void DrawRasterOverlayProperties()
        {
            if (this._rasterOverlayEditor != null)
            {
                this._rasterOverlayEditor.OnInspectorGUI();
            }
        }
    }
}