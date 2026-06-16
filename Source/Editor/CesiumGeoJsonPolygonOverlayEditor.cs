#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumGeoJsonPolygonOverlay))]
    public class CesiumGeoJsonPolygonOverlayEditor : Editor
    {
        private CesiumGeoJsonPolygonOverlay _overlay;

        private SerializedProperty _source;
        private SerializedProperty _url;
        private SerializedProperty _ionAssetID;
        private SerializedProperty _ionAccessToken;
        private SerializedProperty _ionServer;

        private void OnEnable()
        {
            this._overlay = (CesiumGeoJsonPolygonOverlay)this.target;

            this._source = this.serializedObject.FindProperty("_source");
            this._url = this.serializedObject.FindProperty("_url");
            this._ionAssetID = this.serializedObject.FindProperty("_ionAssetID");
            this._ionAccessToken = this.serializedObject.FindProperty("_ionAccessToken");
            this._ionServer = this.serializedObject.FindProperty("_ionServer");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            DrawInspectorButtons();
            EditorGUILayout.Space(5);
            DrawSourceProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawInspectorButtons()
        {
            GUILayout.BeginHorizontal();
            var refreshContent = new GUIContent(
                "Refresh",
                "Reloads the GeoJSON document and refreshes all referencing polygon raster overlays.");
            if (GUILayout.Button(refreshContent))
            {
                this._overlay.Refresh();
            }

            CesiumGeoJsonDocumentRasterOverlaySource source =
                (CesiumGeoJsonDocumentRasterOverlaySource)this._source.enumValueIndex;
            if (source == CesiumGeoJsonDocumentRasterOverlaySource.FromCesiumIon)
            {
                var troubleshootTokenContent = new GUIContent(
                    "Troubleshoot Token",
                    "Check if the Cesium ion token used to access this overlay is working " +
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
    }
}
#endif
