using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumWebMapTileServiceRasterOverlay))]
    public class CesiumWebMapTileServiceRasterOverlayEditor : Editor
    {
        private CesiumWebMapTileServiceRasterOverlay _webMapTileServiceOverlay;
        private CesiumRasterOverlayEditor _rasterOverlayEditor;
        
        private SerializedProperty _baseUrl;
        private SerializedProperty _tileWidth;
        private SerializedProperty _tileHeight;
        private SerializedProperty _minimumLevel;
        private SerializedProperty _maximumLevel;
        private SerializedProperty _format;
        private SerializedProperty _style;
        private SerializedProperty _layer;
        private SerializedProperty _tileMatrixSetID;
        private SerializedProperty _useGeographicProjection;

        private void OnEnable()
        {
            this._webMapTileServiceOverlay =
                (CesiumWebMapTileServiceRasterOverlay)this.target;
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                     this.target,
                                                     typeof(CesiumRasterOverlayEditor));

            this._baseUrl = this.serializedObject.FindProperty("_baseUrl");
            this._tileWidth = this.serializedObject.FindProperty("_tileWidth");
            this._tileHeight = this.serializedObject.FindProperty("_tileHeight");
            this._minimumLevel = this.serializedObject.FindProperty("_minimumLevel");
            this._maximumLevel = this.serializedObject.FindProperty("_maximumLevel");
            this._format = this.serializedObject.FindProperty("_format");
            this._style = this.serializedObject.FindProperty("_style");
            this._layer = this.serializedObject.FindProperty("_layer");
            this._tileMatrixSetID = this.serializedObject.FindProperty("_tileMatrixSetID");
            this._useGeographicProjection = this.serializedObject.FindProperty("_useGeographicProjection");
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
            DrawWebMapTileServiceProperties();
            EditorGUILayout.Space(5);
            DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawWebMapTileServiceProperties()
        {
            GUIContent baseUrlContent = new GUIContent(
                "Base URL",
                "The base URL of the Web Map Service (WMS)." +
                "\n\n" +
                "e.g." +
                "\n\n" +
                "https://services.ga.gov.au/gis/services/NM_Culture_and_Infrastructure/MapServer/WMSServer");
            EditorGUILayout.DelayedTextField(this._baseUrl, baseUrlContent);

            GUIContent tileWidthContent = new GUIContent(
                "Tile Width",
                "Image width.");
            CesiumInspectorGUI.ClampedIntField(
                this._tileWidth,
                64,
                2048,
                tileWidthContent);

            GUIContent tileHeightContent = new GUIContent(
                "Tile Height",
                "Image height.");
            CesiumInspectorGUI.ClampedIntField(
                this._tileHeight,
                64,
                2048,
                tileHeightContent);

            GUIContent minimumLevelContent = new GUIContent(
                "Minimum Level",
                "Minimum zoom level." +
                "\n\n" +
                "Take care when specifying this that the number of tiles at the " +
                "minimum level is small, such as four or less. A larger number " +
                "is likely to result in rendering problems.");
            EditorGUILayout.PropertyField(this._minimumLevel, minimumLevelContent);

            GUIContent maximumLevelContent = new GUIContent(
                "Maximum Level",
                "Maximum zoom level.");
            EditorGUILayout.PropertyField(this._maximumLevel, maximumLevelContent);
            
            GUIContent formatContent = new GUIContent(
                "Format",
                "The MIME type for images to retrieve from the server, default value is \"image/jpeg\".");
            EditorGUILayout.DelayedTextField(this._format, formatContent);
            
            GUIContent styleContent = new GUIContent(
                "Style",
                "The style name for WMTS requests, default value is \"default\".");
            EditorGUILayout.DelayedTextField(this._style, styleContent);
            
            GUIContent layerContent = new GUIContent(
                "Layer",
                "The layer name for WMTS requests.");
            EditorGUILayout.DelayedTextField(this._layer, layerContent);
            
            GUIContent tileMatrixSetIDContent = new GUIContent(
                "Tile Matrix Set ID",
                "The tile matrix set identifier for WMTS requests.");
            EditorGUILayout.DelayedTextField(this._tileMatrixSetID, tileMatrixSetIDContent);
            
            GUIContent useGeographicProjectionContent = new GUIContent(
                "Use Geographic Projection",
                "If true, the overlay will be projected using a geographic projection. " +
                "If false, the overlay will be projected using a web mercator projection.");
            EditorGUILayout.PropertyField(this._useGeographicProjection, useGeographicProjectionContent);
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

