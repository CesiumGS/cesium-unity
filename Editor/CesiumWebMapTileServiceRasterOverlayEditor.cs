using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumWebMapTileServiceRasterOverlay))]
    public class CesiumWebMapTileServiceRasterOverlayEditor : Editor
    {
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _baseUrl;
        private SerializedProperty _layer;
        private SerializedProperty _style;
        private SerializedProperty _format;
        private SerializedProperty _tileMatrixSetID;
        private SerializedProperty _tileMatrixSetLabelPrefix;
        private SerializedProperty _specifyTileMatrixSetLabels;
        private SerializedProperty _tileMatrixSetLabels;
        private SerializedProperty _projection;
        private SerializedProperty _specifyTilingScheme;
        private SerializedProperty _rootTilesX;
        private SerializedProperty _rootTilesY;
        private SerializedProperty _rectangleWest;
        private SerializedProperty _rectangleSouth;
        private SerializedProperty _rectangleEast;
        private SerializedProperty _rectangleNorth;
        private SerializedProperty _tileWidth;
        private SerializedProperty _tileHeight;
        private SerializedProperty _specifyZoomLevels;
        private SerializedProperty _minimumLevel;
        private SerializedProperty _maximumLevel;

        private void OnEnable()
        {
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                     this.target,
                                                     typeof(CesiumRasterOverlayEditor));

            this._baseUrl = this.serializedObject.FindProperty("_baseUrl");
            this._layer = this.serializedObject.FindProperty("_layer");
            this._style = this.serializedObject.FindProperty("_style");
            this._format = this.serializedObject.FindProperty("_format");
            this._tileMatrixSetID = this.serializedObject.FindProperty("_tileMatrixSetID");
            this._tileMatrixSetLabelPrefix = this.serializedObject.FindProperty("_tileMatrixSetLabelPrefix");
            this._specifyTileMatrixSetLabels = this.serializedObject.FindProperty("_specifyTileMatrixSetLabels");
            this._tileMatrixSetLabels = this.serializedObject.FindProperty("_tileMatrixSetLabels");
            this._projection = this.serializedObject.FindProperty("_projection");
            this._specifyTilingScheme = this.serializedObject.FindProperty("_specifyTilingScheme");
            this._rootTilesX = this.serializedObject.FindProperty("_rootTilesX");
            this._rootTilesY = this.serializedObject.FindProperty("_rootTilesY");
            this._rectangleWest = this.serializedObject.FindProperty("_rectangleWest");
            this._rectangleSouth = this.serializedObject.FindProperty("_rectangleSouth");
            this._rectangleEast = this.serializedObject.FindProperty("_rectangleEast");
            this._rectangleNorth = this.serializedObject.FindProperty("_rectangleNorth");
            this._tileWidth = this.serializedObject.FindProperty("_tileWidth");
            this._tileHeight = this.serializedObject.FindProperty("_tileHeight");
            this._specifyZoomLevels = this.serializedObject.FindProperty("_specifyZoomLevels");
            this._minimumLevel = this.serializedObject.FindProperty("_minimumLevel");
            this._maximumLevel = this.serializedObject.FindProperty("_maximumLevel");
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
            this.DrawBasicProperties();
            EditorGUILayout.Space(5);
            this.DrawTileMatrixSetProperties();
            EditorGUILayout.Space(5);
            this.DrawTilingSchemeProperties();
            EditorGUILayout.Space(5);
            this.DrawLevelOfDetailContent();
            EditorGUILayout.Space(5);
            this.DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawBasicProperties()
        {
            GUIContent baseUrlContent = new GUIContent(
                "Base URL",
                "The base URL of the Web Map Tile Service (WMTS)." +
                "\n\n" +
                "e.g." +
                "\n\n" +
                "https://tile.openstreetmap.org/{TileMatrix}/{TileCol}/{TileRow}.png");
            EditorGUILayout.DelayedTextField(this._baseUrl, baseUrlContent);

            GUIContent layerContent = new GUIContent(
                "Layer",
                "The layer name for WMTS requests.");
            EditorGUILayout.DelayedTextField(this._layer, layerContent);

            GUIContent styleContent = new GUIContent(
                "Style",
                "The style name for WMTS requests.");
            EditorGUILayout.DelayedTextField(this._style, styleContent);

            GUIContent formatContent = new GUIContent(
                "Format",
                "The MIME type for images to retrieve from the server.");
            EditorGUILayout.DelayedTextField(this._format, formatContent);
        }

        private void DrawTileMatrixSetProperties()
        {
            GUILayout.Label("Tile Matrix Set Descriptors", EditorStyles.boldLabel);

            GUIContent tileMatrixSetIDContent = new GUIContent(
                "Tile Matrix Set ID",
                "The tile matrix set identifier for WMTS requests.");
            EditorGUILayout.DelayedTextField(this._tileMatrixSetID, tileMatrixSetIDContent);

            EditorGUI.BeginDisabledGroup(this._specifyTileMatrixSetLabels.boolValue);
            GUIContent tileMatrixSetLabelPrefixContent = new GUIContent(
                "Tile Matrix Set Label Prefix",
                "The prefix to use for the tile matrix set labels. For instance, setting \"EPSG:4326:\" " +
                "as the prefix generates the list [\"EPSG:4326:0\", \"EPSG:4326:1\", \"EPSG:4326:2\", ...]." +
                "\n\n" +
                "Only applicable when \"Specify Tile Matrix Set Labels\" is false.");
            EditorGUILayout.PropertyField(this._tileMatrixSetLabelPrefix, tileMatrixSetLabelPrefixContent);
            EditorGUI.EndDisabledGroup();

            GUIContent specifyTileMatrixSetLabelsContent = new GUIContent(
                "Specify Tile Matrix Set Labels",
                "Set this to true to manually specify the tile matrix set labels. If false, the labels " +
                "will be constructed from the specified levels and prefix (if one is specified).");
            EditorGUILayout.PropertyField(this._specifyTileMatrixSetLabels, specifyTileMatrixSetLabelsContent);

            EditorGUI.BeginDisabledGroup(!this._specifyTileMatrixSetLabels.boolValue);
            GUIContent tileMatrixSetLabelsContent = new GUIContent(
                "Tile Matrix Set Label",
                "The manually specified tile matrix set labels." +
                "\n\n" +
                "Only applicable when \"Specify Tile Matrix Set Labels\" is true.");
            EditorGUILayout.PropertyField(this._tileMatrixSetLabels, tileMatrixSetLabelsContent);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawTilingSchemeProperties()
        {
            GUILayout.Label("Tiling Scheme", EditorStyles.boldLabel);

            GUIContent projectionContent = new GUIContent(
                "Projection",
                "The type of projection used to project the WMTS imagery onto the globe. " +
                "For instance, EPSG:4326 uses geographic projection and EPSG:3857 uses Web Mercator.");
            EditorGUILayout.PropertyField(this._projection, projectionContent);

            GUIContent specifyTilingSchemeContent = new GUIContent(
                "Specify Tiling Scheme",
                "Set this to true to specify the quadtree tiling scheme according to the specified root " +
                "tile numbers and projected bounding rectangle. If false, the tiling scheme will be " +
                "deduced from the projection.");
            EditorGUILayout.PropertyField(this._specifyTilingScheme, specifyTilingSchemeContent);

            EditorGUI.BeginDisabledGroup(!this._specifyTilingScheme.boolValue);
            GUIContent rootTilesXContent = new GUIContent(
                "Root Tiles X",
                "The number of tiles corresponding to TileCol, also known as TileMatrixWidth. " +
                "If specified, this determines the number of tiles at the root of the quadtree " +
                "tiling scheme in the X direction." +
                "\n\n" +
                "Only applicable if \"Specify Tiling Scheme\" is set to true.");
            EditorGUILayout.PropertyField(this._rootTilesX, rootTilesXContent);

            GUIContent rootTilesYContent = new GUIContent(
                "Root Tiles Y",
                "The number of tiles corresponding to TileRow, also known as TileMatrixHeight. " +
                "If specified, this determines the number of tiles at the root of the quadtree " +
                "tiling scheme in the Y direction." +
                "\n\n" +
                "Only applicable if \"Specify Tiling Scheme\" is set to true.");
            EditorGUILayout.PropertyField(this._rootTilesY, rootTilesYContent);

            GUIContent rectangleWest = new GUIContent(
                "Rectangle West",
                "The west boundary of the bounding rectangle used for the quadtree tiling scheme. " +
                "Specified in longitude degrees in the range [-180, 180]." +
                "\n\n" +
                "Only applicable if \"Specify Tiling Scheme\" is set to true.");
            CesiumInspectorGUI.ClampedDoubleField(this._rectangleWest, -180, 180, rectangleWest, true);

            GUIContent rectangleSouth = new GUIContent(
                "Rectangle South",
                "The south boundary of the bounding rectangle used for the quadtree tiling scheme. " +
                "Specified in latitude degrees in the range [-90, 90]." +
                "\n\n" +
                "Only applicable if \"Specify Tiling Scheme\" is set to true.");
            CesiumInspectorGUI.ClampedDoubleField(this._rectangleSouth, -90, 90, rectangleSouth, true);

            GUIContent rectangleEast = new GUIContent(
                "Rectangle East",
                "The east boundary of the bounding rectangle used for the quadtree tiling scheme. " +
                "Specified in longitude degrees in the range [-180, 180]." +
                "\n\n" +
                "Only applicable if \"Specify Tiling Scheme\" is set to true.");
            CesiumInspectorGUI.ClampedDoubleField(this._rectangleEast, -180, 180, rectangleEast, true);

            GUIContent rectangleNorth = new GUIContent(
                "Rectangle North",
                "The north boundary of the bounding rectangle used for the quadtree tiling scheme. " +
                "Specified in latitude degrees in the range [-90, 90]." +
                "\n\n" +
                "Only applicable if \"Specify Tiling Scheme\" is set to true.");
            CesiumInspectorGUI.ClampedDoubleField(this._rectangleNorth, -90, 90, rectangleNorth, true);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawLevelOfDetailContent()
        {
            GUILayout.Label("Level of Detail", EditorStyles.boldLabel);

            GUIContent specifyZoomLevelsContent = new GUIContent(
                "Specify Zoom Levels",
                "Set this to true to directly specify the minimum and maximum zoom levels available " +
                "from the server. If false, the minimum and maximum zoom levels will be retrieved " +
                "from the server's tilemapresource.xml file."
                );
            EditorGUILayout.PropertyField(this._specifyZoomLevels, specifyZoomLevelsContent);

            EditorGUI.BeginDisabledGroup(!this._specifyZoomLevels.boolValue);
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
            EditorGUI.EndDisabledGroup();

            GUIContent tileWidthContent = new GUIContent(
                "Tile Width",
                "The width of the image tiles in pixels.");
            CesiumInspectorGUI.ClampedIntField(
                this._tileWidth,
                64,
                2048,
                tileWidthContent);

            GUIContent tileHeightContent = new GUIContent(
                "Tile Height",
                "The height of the image tiles in pixels.");
            CesiumInspectorGUI.ClampedIntField(
                this._tileHeight,
                64,
                2048,
                tileHeightContent);
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

