using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumUrlTemplateRasterOverlay))]
    public class CesiumUrlTemplateRasterOverlayEditor : Editor
    {
        private static readonly (string Template, string Desc)[] TEMPLATE_PARAMS = new (string Template, string Desc)[]
        {
            ("x", "The tile X coordinate in the tiling scheme, where 0 is the westernmost tile."),
            ("y", "The tile Y coordinate in the tiling scheme, where 0 is the nothernmost tile."),
            ("z", "The level of the tile in the tiling scheme, where 0 is the root of the quadtree pyramid."),
            ("reverseX", "The tile X coordinate in the tiling scheme, where 0 is the easternmost tile."),
            ("reverseY", "The tile Y coordinate in the tiling scheme, where 0 is the southernmost tile."),
            ("reverseZ", "The tile Z coordinate in the tiling scheme, where 0 is equivalent to `maximumLevel`."),
            ("westDegrees", "The western edge of the tile in geodetic degrees."),
            ("southDegrees", "The southern edge of the tile in geodetic degrees."),
            ("eastDegrees", "The eastern edge of the tile in geodetic degrees."),
            ("northDegrees", "The northern edge of the tile in geodetic degrees."),
            ("minimumX", "The minimum X coordinate of the tile's projected coordinates."),
            ("minimumY", "The minimum Y coordinate of the tile's projected coordinates."),
            ("maximumX", "The maximum X coordinate of the tile's projected coordinates."),
            ("maximumY", "The maximum Y coordinate of the tile's projected coordinates."),
            ("width", "The width of each tile in pixels."),
            ("height", "The height of each tile in pixels.")
        };

        private CesiumUrlTemplateRasterOverlay _urlTemplateRasterOverlay;
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _templateUrl;
        private SerializedProperty _projection;
        private SerializedProperty _specifyTilingScheme;
        private SerializedProperty _rootTilesX;
        private SerializedProperty _rootTilesY;
        private SerializedProperty _rectangleWest;
        private SerializedProperty _rectangleSouth;
        private SerializedProperty _rectangleEast;
        private SerializedProperty _rectangleNorth;
        private SerializedProperty _minimumLevel;
        private SerializedProperty _maximumLevel;
        private SerializedProperty _tileWidth;
        private SerializedProperty _tileHeight;
        private SerializedProperty _requestHeaders;

        private bool _foldOut = false;

        private void OnEnable()
        {
            this._urlTemplateRasterOverlay =
                (CesiumUrlTemplateRasterOverlay)this.target;
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                    this.target,
                                                    typeof(CesiumRasterOverlayEditor));

            this._templateUrl = this.serializedObject.FindProperty("_templateUrl");
            this._projection = this.serializedObject.FindProperty("_projection");
            this._specifyTilingScheme = this.serializedObject.FindProperty("_specifyTilingScheme");
            this._rootTilesX = this.serializedObject.FindProperty("_rootTilesX");
            this._rootTilesY = this.serializedObject.FindProperty("_rootTilesY");
            this._rectangleWest = this.serializedObject.FindProperty("_rectangleWest");
            this._rectangleSouth = this.serializedObject.FindProperty("_rectangleSouth");
            this._rectangleEast = this.serializedObject.FindProperty("_rectangleEast");
            this._rectangleNorth = this.serializedObject.FindProperty("_rectangleNorth");
            this._minimumLevel = this.serializedObject.FindProperty("_minimumLevel");
            this._maximumLevel = this.serializedObject.FindProperty("_maximumLevel");
            this._tileWidth = this.serializedObject.FindProperty("_tileWidth");
            this._tileHeight = this.serializedObject.FindProperty("_tileHeight");
            this._requestHeaders = this.serializedObject.FindProperty("_requestHeaders");
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
            DrawUrlTemplateProperties();
            EditorGUILayout.Space(5);
            DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawUrlTemplateProperties()
        {
            EditorGUILayout.DelayedTextField(this._templateUrl, new GUIContent(
               "Template URL",
               @"The URL containing template parameters that will be substituted when loading tiles."));
            this._foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(this._foldOut, new GUIContent("Supported URL Parameters"));

            if (this._foldOut)
            {
                float maxLabelSize = TEMPLATE_PARAMS.Max(param => EditorStyles.boldLabel.CalcSize(new GUIContent("{" + param.Template + "}")).x);
                foreach (var (name, desc) in TEMPLATE_PARAMS)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("{" + name + "}", EditorStyles.boldLabel, GUILayout.Width(maxLabelSize));
                    EditorGUILayout.LabelField(desc, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(
                this._projection,
                new GUIContent("Projection", "The type of projection used to protect the imagery onto the globe."));

            EditorGUILayout.PropertyField(
                this._specifyTilingScheme,
                new GUIContent("Specify Tiling Scheme",
                "Set this to true to specify the quadtree tiling scheme according to the specified root tile" +
                "numbers and projected bounding rectangle. If false, the tiling scheme will be deduced from" +
                "the projection."));

            EditorGUI.BeginDisabledGroup(!this._specifyTilingScheme.boolValue);
            GUIContent rootTilesXContent = new GUIContent(
                "Root Tiles X",
                "If specified, this determines the number of tiles at the root of the quadtree tiling scheme in the X direction.");
            EditorGUILayout.PropertyField(this._rootTilesX, rootTilesXContent);
            GUIContent rootTilesYContent = new GUIContent(
                "Root Tiles Y",
                "If specified, this determines the number of tiles at the root of the quadtree tiling scheme in the Y direction.");
            EditorGUILayout.PropertyField(this._rootTilesY, rootTilesYContent);

            CesiumInspectorGUI.ClampedDoubleField(
                this._rectangleWest,
                -180, 180,
                new GUIContent(
                    "Rectangle West",
                    "The west boundary of the bounding rectangle used for the quadtree tiling" +
                    "scheme. Specified in longitude degrees in the range [-180, 180]."
                ));
            CesiumInspectorGUI.ClampedDoubleField(
                this._rectangleSouth,
                -90, 90,
                new GUIContent(
                    "Rectangle South",
                    "The south boundary of the bounding rectangle used for the quadtree tiling" +
                    "scheme. Specified in longitude degrees in the range [-90, 90]."
                ));
            CesiumInspectorGUI.ClampedDoubleField(
                this._rectangleEast,
                -180, 180,
                new GUIContent(
                    "Rectangle East",
                    "The east boundary of the bounding rectangle used for the quadtree tiling" +
                    "scheme. Specified in longitude degrees in the range [-180, 180]."
                ));
            CesiumInspectorGUI.ClampedDoubleField(
                this._rectangleNorth,
                -90, 90,
                new GUIContent(
                    "Rectangle North",
                    "The north boundary of the bounding rectangle used for the quadtree tiling" +
                    "scheme. Specified in longitude degrees in the range [-90, 90]."
                ));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(this._minimumLevel, new GUIContent(
                "Minimum Level",
                "Minimum zoom level.\n\n" +
                "Take care when specifying this that the number of tiles at the minimum" +
                "level is small, such as four or less. A larger number is likely to result" +
                "in rendering problems."
                 ));
            EditorGUILayout.PropertyField(this._maximumLevel, new GUIContent("Maximum Level", "Maximum zoom level."));

            EditorGUILayout.PropertyField(this._tileWidth, new GUIContent("Tile Width", "The pixel width of the image tiles."));
            EditorGUILayout.PropertyField(this._tileHeight, new GUIContent("Tile Height", "The pixel height of the image tiles."));

            EditorGUILayout.PropertyField(
                this._requestHeaders,
                new GUIContent("Request Headers", "HTTP headers to be attached to each request made for this raster overlay."));

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
