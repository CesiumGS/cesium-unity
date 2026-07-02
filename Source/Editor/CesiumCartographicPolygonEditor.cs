#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumCartographicPolygon))]
    public class CesiumCartographicPolygonEditor : Editor
    {
#if SUPPORTS_SPLINES
        private CesiumCartographicPolygon _polygon;

        private SerializedProperty _source;
        private SerializedProperty _url;
        private SerializedProperty _ionAssetID;
        private SerializedProperty _ionAccessToken;
        private SerializedProperty _ionServer;

        private void OnEnable()
        {
            this._polygon = (CesiumCartographicPolygon)this.target;

            this._source = this.serializedObject.FindProperty("_source");
            this._url = this.serializedObject.FindProperty("_url");
            this._ionAssetID = this.serializedObject.FindProperty("_ionAssetID");
            this._ionAccessToken = this.serializedObject.FindProperty("_ionAccessToken");
            this._ionServer = this.serializedObject.FindProperty("_ionServer");
        }
#endif

        public override void OnInspectorGUI()
        {
#if !SUPPORTS_SPLINES
#if UNITY_2022_2_OR_NEWER
            EditorGUILayout.HelpBox("CesiumCartographicPolygon requires the Splines package, which is currently " +
                "not installed in the project. Install the Splines package using the Package Manager.", MessageType.Error);
#else
            EditorGUILayout.HelpBox("CesiumCartographicPolygon requires the Splines package, which is not available " +
                "in this version of Unity.", MessageType.Error);
#endif
#else
            this.serializedObject.Update();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            DrawInspectorButtons();
            EditorGUILayout.Space(5);
            DrawSourceProperties();
            EditorGUILayout.Space(5);

            this.serializedObject.ApplyModifiedProperties();
#endif
        }

#if SUPPORTS_SPLINES
        private void DrawInspectorButtons()
        {
            GUILayout.BeginHorizontal();
            var refreshContent = new GUIContent(
                "Refresh",
                "Reloads the polygon's spline from the configured source and " +
                "rebuilds any CesiumPolygonRasterOverlay that uses this polygon, " +
                "so the cutout in the tileset matches the latest data.");
            if (GUILayout.Button(refreshContent))
            {
                Undo.RecordObject(this._polygon, "Refresh Cartographic Polygon");
                this._polygon.Refresh();
                EditorUtility.SetDirty(this._polygon);
            }

            CesiumCartographicPolygonSource source =
                (CesiumCartographicPolygonSource)this._source.enumValueIndex;
            if (source == CesiumCartographicPolygonSource.FromCesiumIon)
            {
                var troubleshootTokenContent = new GUIContent(
                    "Troubleshoot Token",
                    "Check if the Cesium ion token used to access this polygon's GeoJSON is working " +
                    "correctly, and fix it if necessary.");
                if (GUILayout.Button(troubleshootTokenContent))
                {
                    IonTokenTroubleshootingWindow.ShowWindow(this._polygon, false);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSourceProperties()
        {
            GUIContent sourceContent = new GUIContent(
                "Source",
                "The source from which this polygon's shape is derived.");
            EditorGUILayout.PropertyField(this._source, sourceContent);

            CesiumCartographicPolygonSource source =
                (CesiumCartographicPolygonSource)this._source.enumValueIndex;

            if (source == CesiumCartographicPolygonSource.Manual)
            {
                EditorGUILayout.HelpBox(
                    "The spline is being edited manually. Use 'Save as GeoJSON…' to " +
                    "write the current shape to a .geojson file on disk.",
                    MessageType.Info);

                var saveGeoJsonContent = new GUIContent(
                    "Save as GeoJSON…",
                    "Writes the polygon's current spline to a GeoJSON file on disk.");
                if (GUILayout.Button(saveGeoJsonContent))
                {
                    CesiumCartographicPolygonGeoJsonWriter.SaveAsGeoJsonWithDialog(this._polygon);
                }
            }
            else if (source == CesiumCartographicPolygonSource.FromUrl)
            {
                GUIContent urlContent = new GUIContent(
                    "URL",
                    "The URL from which to load the GeoJSON document.");
                EditorGUILayout.DelayedTextField(this._url, urlContent);

                EditorGUILayout.HelpBox(
                    "Changing any knot in the spline changes the source to manual, as it does " +
                    "not match anymore with the geojson file.",
                    MessageType.Info);
            }
            else if (source == CesiumCartographicPolygonSource.FromCesiumIon)
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

                EditorGUILayout.HelpBox(
                    "Changing any knot in the spline changes the source to manual, as it does " +
                    "not match anymore with the geojson file.",
                    MessageType.Info);
            }
        }
#endif
    }
}
#endif
