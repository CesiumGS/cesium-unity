#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumGeoJsonDocumentRasterOverlay))]
    public class CesiumGeoJsonDocumentRasterOverlayEditor : Editor
    {
        private CesiumGeoJsonDocumentRasterOverlay _overlay;
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _source;
        private SerializedProperty _url;
        private SerializedProperty _ionAssetID;
        private SerializedProperty _ionAccessToken;
        private SerializedProperty _ionServer;
        private SerializedProperty _mipLevels;
        private SerializedProperty _defaultStyle;

        private void OnEnable()
        {
            this._overlay = (CesiumGeoJsonDocumentRasterOverlay)this.target;
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                    this.target,
                    typeof(CesiumRasterOverlayEditor));

            this._source = this.serializedObject.FindProperty("_source");
            this._url = this.serializedObject.FindProperty("_url");
            this._ionAssetID = this.serializedObject.FindProperty("_ionAssetID");
            this._ionAccessToken = this.serializedObject.FindProperty("_ionAccessToken");
            this._ionServer = this.serializedObject.FindProperty("_ionServer");
            this._mipLevels = this.serializedObject.FindProperty("_mipLevels");
            this._defaultStyle = this.serializedObject.FindProperty("_defaultStyle");
        }

        private void OnDisable()
        {
            if (this._rasterOverlayEditor != null)
            {
                DestroyImmediate(this._rasterOverlayEditor);
            }
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            DrawInspectorButtons();
            EditorGUILayout.Space(5);
            DrawSourceProperties();
            EditorGUILayout.Space(5);
            DrawStyleProperties();
            EditorGUILayout.Space(5);
            DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawInspectorButtons()
        {
            GUILayout.BeginHorizontal();
            var refreshOverlayContent = new GUIContent(
                "Refresh Overlay",
                "Refreshes this overlay.");
            if (GUILayout.Button(refreshOverlayContent))
            {
                this._overlay.Refresh();
            }

            CesiumGeoJsonDocumentRasterOverlaySource source =
                (CesiumGeoJsonDocumentRasterOverlaySource)this._source.enumValueIndex;
            if (source == CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon)
            {
                var troubleshootTokenContent = new GUIContent(
                    "Troubleshoot Token",
                    "Check if the Cesium ion token used to access this raster overlay is working " +
                    "correctly, and fix it if necessary.");
                if (GUILayout.Button(troubleshootTokenContent))
                {
                    IonTokenTroubleshootingWindow.ShowWindow(this._overlay, false);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSourceProperties()
        {
            GUIContent sourceContent = new GUIContent(
                "Source",
                "The source from which to load the GeoJSON document.");
            EditorGUILayout.PropertyField(this._source, sourceContent);

            CesiumGeoJsonDocumentRasterOverlaySource source =
                (CesiumGeoJsonDocumentRasterOverlaySource)this._source.enumValueIndex;

            if (source == CesiumGeoJsonDocumentRasterOverlaySource.FromUrl)
            {
                GUIContent urlContent = new GUIContent(
                    "URL",
                    "The URL from which to load the GeoJSON document.");
                EditorGUILayout.DelayedTextField(this._url, urlContent);
            }
            else if (source == CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon)
            {
                GUIContent ionAssetIDContent = new GUIContent(
                    "ion Asset ID",
                    "The ID of the Cesium ion asset to use.");
                EditorGUILayout.DelayedIntField(this._ionAssetID, ionAssetIDContent);

                GUIContent ionAccessTokenContent = new GUIContent(
                    "ion Access Token",
                    "The access token to use to access the Cesium ion resource. " +
                    "If empty, the default token from the ion server will be used.");
                EditorGUILayout.DelayedTextField(this._ionAccessToken, ionAccessTokenContent);

                GUIContent ionServerContent = new GUIContent(
                    "ion Server",
                    "The Cesium ion server to use.");
                EditorGUILayout.PropertyField(this._ionServer, ionServerContent);
            }
        }

        private void DrawStyleProperties()
        {
            EditorGUILayout.LabelField("Style", EditorStyles.boldLabel);

            GUIContent mipLevelsContent = new GUIContent(
                "Mip Levels",
                "The number of mip levels to generate for each tile of this raster overlay. " +
                "Additional mip levels can improve the visual quality of tiles farther from " +
                "the camera at the cost of additional rasterization time.");
            EditorGUILayout.DelayedIntField(this._mipLevels, mipLevelsContent);

            GUIContent defaultStyleContent = new GUIContent(
                "Default Style",
                "The default style to use for this raster overlay. If no style is set on a " +
                "GeoJSON object or any of its parents, this style will be used instead.");
            EditorGUILayout.PropertyField(this._defaultStyle, defaultStyleContent, true);
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
#endif
