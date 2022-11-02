using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumWebMapServiceRasterOverlay))]
    public class CesiumWebMapServiceRasterOverlayEditor : Editor
    {
        private CesiumWebMapServiceRasterOverlay _webMapServiceOverlay;
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _baseUrl;
        private SerializedProperty _layers;
        private SerializedProperty _tileWidth;
        private SerializedProperty _tileHeight;
        private SerializedProperty _minimumLevel;
        private SerializedProperty _maximumLevel;

        private void OnEnable()
        {
            this._webMapServiceOverlay =
                (CesiumWebMapServiceRasterOverlay)this.target;
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                    this.target,
                    typeof(CesiumRasterOverlayEditor));

            this._baseUrl = this.serializedObject.FindProperty("_baseUrl");
            this._layers = this.serializedObject.FindProperty("_layers");
            this._tileWidth = this.serializedObject.FindProperty("_tileWidth");
            this._tileHeight = this.serializedObject.FindProperty("_tileHeight");
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
            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            this.serializedObject.Update();

            DrawWebMapServiceProperties();
            EditorGUILayout.Space(5);
            if (this._rasterOverlayEditor != null)
            {
                this._rasterOverlayEditor.OnInspectorGUI();
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawWebMapServiceProperties()
        {
            GUIContent baseUrlContent = new GUIContent(
                "Base URL",
                "The base URL of the Web Map Service (WMS)." +
                "\n\n" +
                "e.g." +
                "\n\n" +
                "https://services.ga.gov.au/gis/services/NM_Culture_and_Infrastructure/MapServer/WMSServer");
            EditorGUILayout.DelayedTextField(this._baseUrl, baseUrlContent);

            GUIContent layersContent = new GUIContent(
                "Layers",
                "Comma-separated layer names to request from the server.");
            EditorGUILayout.DelayedTextField(this._layers, layersContent);

            GUIContent tileWidthContent = new GUIContent(
                "Tile Width",
                "Image width.");
            EditorGUILayout.PropertyField(this._tileWidth, tileWidthContent);

            GUIContent tileHeightContent = new GUIContent(
                "Tile Height",
                "Image height.");
            EditorGUILayout.PropertyField(this._tileHeight, tileHeightContent);

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
        }
    }
}

