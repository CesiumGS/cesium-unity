#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

#if SUPPORTS_SPLINES
using UnityEngine.Splines;
#endif

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumCartographicPolygon))]
    public class CesiumCartographicPolygonEditor : Editor
    {
        private const string HelpSplinesMissingInProject =
            "CesiumCartographicPolygon requires the Splines package, which is currently " +
            "not installed in the project. Install the Splines package using the Package Manager.";

        private const string HelpSplinesMissingInVersion =
            "CesiumCartographicPolygon requires the Splines package, which is not available " +
            "in this version of Unity.";

        private const string HelpManualEditSaveGeoJson =
            "The spline is being edited manually. Use 'Save as GeoJSON…' to " +
            "write the current shape to a .geojson file on disk.";

        private const string HelpKnotEditRevertsToManual =
            "Changing any knot in the spline changes the source to manual, as it does " +
            "not match anymore with the geojson file.";

        private static readonly GUIContent RefreshContent = new GUIContent(
            "Refresh",
            "Reloads the polygon's spline from the configured source and " +
            "rebuilds any CesiumPolygonRasterOverlay that uses this polygon, " +
            "so the cutout in the tileset matches the latest data.");

        private static readonly GUIContent TroubleshootTokenContent = new GUIContent(
            "Troubleshoot Token",
            "Check if the Cesium ion token used to access this polygon's GeoJSON is working " +
            "correctly, and fix it if necessary.");

        private static readonly GUIContent SourceContent = new GUIContent(
            "Source",
            "The source from which this polygon's shape is derived.");

        private static readonly GUIContent SaveAsGeoJsonContent = new GUIContent(
            "Save as GeoJSON…",
            "Writes the polygon's current spline to a GeoJSON file on disk.");

        private static readonly GUIContent UrlContent = new GUIContent(
            "URL",
            "The URL from which to load the GeoJSON document.");

        private static readonly GUIContent IonAssetIdContent = new GUIContent(
            "ion Asset ID",
            "The ID of the Cesium ion asset to use.");

        private static readonly GUIContent IonAccessTokenContent = new GUIContent(
            "ion Access Token",
            "The access token to use to access the Cesium ion resource. " +
            "If empty, the default token from the ion server will be used.");

        private static readonly GUIContent IonServerContent = new GUIContent(
            "ion Server",
            "The Cesium ion server to use.");

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

            Spline.Changed += OnSplineChanged;
        }

        private void OnDisable()
        {
            Spline.Changed -= OnSplineChanged;
        }

        private void OnSplineChanged(Spline spline, int i, SplineModification modification)
        {
            // Called once after editing has completed.
            if (this._polygon._isUpdatingSplineInternally)
            {
                return;
            }

            if (!this._polygon.IsSplineOwned(spline))
            {
                return;
            }

            // If the spline was edited manually while it was sourced from Cesium ion or a URL,
            // the spline no longer matches the source GeoJSON file, so fall back to Manual.
            if (this._polygon.source == CesiumCartographicPolygonSource.FromCesiumIon ||
                this._polygon.source == CesiumCartographicPolygonSource.FromUrl)
            {
                Undo.RecordObject(this._polygon, "Revert Cartographic Polygon source to Manual");
                this._polygon.source = CesiumCartographicPolygonSource.Manual;
                this._polygon.Refresh();
                EditorUtility.SetDirty(this._polygon);
            }
        }

#endif

        public override void OnInspectorGUI()
        {
#if !SUPPORTS_SPLINES
#if UNITY_2022_2_OR_NEWER
            EditorGUILayout.HelpBox(HelpSplinesMissingInProject, MessageType.Error);
#else
            EditorGUILayout.HelpBox(HelpSplinesMissingInVersion, MessageType.Error);
#endif
#else
            CesiumCartographicPolygonSource oldSource = this._polygon.source;

            this.serializedObject.Update();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            DrawInspectorButtons();
            EditorGUILayout.Space(5);
            DrawSourceProperties();
            EditorGUILayout.Space(5);

            this.serializedObject.ApplyModifiedProperties();

            // When the Source is changed in the inspector, reload the polygon's spline and rebuild any dependent raster overlays to update the cutout.
            CesiumCartographicPolygonSource newSource = this._polygon.source;
            if (oldSource != newSource)
            {
                Undo.RecordObject(this._polygon, "Change Cartographic Polygon source");
                this._polygon.Refresh();
                EditorUtility.SetDirty(this._polygon);
            }
#endif
        }

#if SUPPORTS_SPLINES
        private void DrawInspectorButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(RefreshContent))
            {
                Undo.RecordObject(this._polygon, "Refresh Cartographic Polygon");
                this._polygon.Refresh();
                EditorUtility.SetDirty(this._polygon);
            }

            CesiumCartographicPolygonSource source =
                (CesiumCartographicPolygonSource)this._source.enumValueIndex;
            if (source == CesiumCartographicPolygonSource.FromCesiumIon)
            {
                if (GUILayout.Button(TroubleshootTokenContent))
                {
                    IonTokenTroubleshootingWindow.ShowWindow(this._polygon, false);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSourceProperties()
        {
            EditorGUILayout.PropertyField(this._source, SourceContent);

            CesiumCartographicPolygonSource source =
                (CesiumCartographicPolygonSource)this._source.enumValueIndex;

            if (source == CesiumCartographicPolygonSource.Manual)
            {
                EditorGUILayout.HelpBox(HelpManualEditSaveGeoJson, MessageType.Info);

                if (GUILayout.Button(SaveAsGeoJsonContent))
                {
                    CesiumCartographicPolygonGeoJsonWriter.SaveAsGeoJsonWithDialog(this._polygon);
                }
            }
            else if (source == CesiumCartographicPolygonSource.FromUrl)
            {
                EditorGUILayout.DelayedTextField(this._url, UrlContent);

                EditorGUILayout.HelpBox(HelpKnotEditRevertsToManual, MessageType.Info);
            }
            else if (source == CesiumCartographicPolygonSource.FromCesiumIon)
            {
                EditorGUILayout.DelayedIntField(this._ionAssetID, IonAssetIdContent);
                EditorGUILayout.DelayedTextField(this._ionAccessToken, IonAccessTokenContent);
                EditorGUILayout.PropertyField(this._ionServer, IonServerContent);

                EditorGUILayout.HelpBox(HelpKnotEditRevertsToManual, MessageType.Info);
            }
        }
#endif
    }
}
#endif