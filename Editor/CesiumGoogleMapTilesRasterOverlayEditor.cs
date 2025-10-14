using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumGoogleMapTilesRasterOverlay))]
    public class CesiumGoogleMapTilesRasterOverlayEditor : Editor
    {
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _apiKey;
        private SerializedProperty _mapType;
        private SerializedProperty _language;
        private SerializedProperty _region;
        private SerializedProperty _scale;
        private SerializedProperty _highDpi;
        private SerializedProperty _layerTypes;
        private SerializedProperty _styles;
        private SerializedProperty _overlay;

        private string[] _scaleOptions;
        private int _selectedScaleIndex;

        private void OnEnable()
        {
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                     this.target,
                                                     typeof(CesiumRasterOverlayEditor));

            this._apiKey = this.serializedObject.FindProperty("_apiKey");
            this._mapType = this.serializedObject.FindProperty("_mapType");
            this._language = this.serializedObject.FindProperty("_language");
            this._region = this.serializedObject.FindProperty("_region");
            this._scale = this.serializedObject.FindProperty("_scale");
            this._highDpi = this.serializedObject.FindProperty("_highDpi");
            this._layerTypes = this.serializedObject.FindProperty("_layerTypes");
            this._styles = this.serializedObject.FindProperty("_styles");
            this._overlay = this.serializedObject.FindProperty("_overlay");

            // Remove "ScaleFactor" prefix from the Scale enum options.
            int nameOffset = ("ScaleFactor").Length;
            this._scaleOptions = this._scale.enumNames;
            for (int i = 0; i < this._scaleOptions.Length; i++)
            {
                this._scaleOptions[i] = this._scaleOptions[i].Substring(nameOffset);
            }
            this._selectedScaleIndex = this._scale.enumValueIndex;
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
            this.DrawGoogleMapTilesProperties();
            EditorGUILayout.Space(5);
            this.DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawGoogleMapTilesProperties()
        {
            GUIContent apiKeyContent = new GUIContent(
                "API Key",
                "The Google Map Tiles API key to use.");
            EditorGUILayout.DelayedTextField(this._apiKey, apiKeyContent);

            GUIContent mapTypeContent = new GUIContent(
                "Map Type",
                "The type of base map.");
            EditorGUILayout.PropertyField(this._mapType, mapTypeContent);

            GUIContent languageContent = new GUIContent(
                "Language",
                "An IETF language tag that specifies the language used to display " +
                "information on the tiles. For example, `en-US` specifies the " +
                "English language as spoken in the United States.");
            EditorGUILayout.DelayedTextField(this._language, languageContent);

            GUIContent regionContent = new GUIContent(
                "Region",
                "A Common Locale Data Repository region identifier (two uppercase " +
                "letters) that represents the physical location of the user. For " +
                "example, `US`.");
            EditorGUILayout.DelayedTextField(this._region, regionContent);

            GUIContent scaleContent = new GUIContent(
                "Scale",
                "Scales-up the size of map elements (such as road labels), while " +
                "retaining the tile size and coverage area of the default tile." +
                "\n\n" +
                "Increasing the scale also reduces the number of labels on the map, " +
                "which reduces clutter.");
            this._selectedScaleIndex =
                EditorGUILayout.Popup(scaleContent, this._selectedScaleIndex, this._scaleOptions);
            this._scale.enumValueIndex = this._selectedScaleIndex;

            GUIContent highDpiContent = new GUIContent(
                "High DPI",
                "Specifies whether to return high-resolution tiles." +
                "\n\n" +
                "If the scale-factor is increased, High DPI is used to increase the " +
                "size of the tile. Normally, increasing the scale factor enlarges the " +
                "resulting tile into an image of the same size, which lowers quality. " +
                "With High DPI, the resulting size is also increased, preserving quality. " +
                "DPI stands for Dots per Inch, and High DPI means the tile renders using " +
                "more dots per inch than normal." +
                "\n\n" +
                "If enabled, the number of pixels in each of the x and y dimensions is " +
                "multiplied by the scale factor (that is, 2x or 4x). The coverage area " +
                "of the tile remains unchanged. This parameter works only with Scale " +
                "values of 2x or 4x. It has no effect on 1x scale tiles.");
            EditorGUILayout.PropertyField(this._highDpi, highDpiContent);

            GUIContent layerTypesContent = new GUIContent(
                "Layer Types",
                "The layer types to be added to the map.");
            EditorGUILayout.PropertyField(this._layerTypes, layerTypesContent);

            GUIContent stylesContent = new GUIContent(
                "Styles",
                "A list of JSON style objects that specify the appearance and detail " +
                "level of map features such as roads, parks, and built-up areas." +
                "\n\n" +
                "Styling is used to customize the standard Google base map. The Styles " +
                "parameter is valid only if the Map Type is Roadmap.");
            EditorGUILayout.PropertyField(this._styles, stylesContent);

            GUIContent overlayContent = new GUIContent(
                "Overlay",
                "Specifies whether Layer Types are rendered as a separate overlay, or " +
                "combined with the base imagery." +
                "\n\n" +
                "When enabled, the base map isn't displayed. If you haven't defined any " +
                "Layer Types, then this value is ignored.");
            EditorGUILayout.PropertyField(this._overlay, overlayContent);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (EditorGUILayout.LinkButton("Map Tiles API Style reference"))
            {
                Application.OpenURL("https://developers.google.com/maps/documentation/tile/style-reference");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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
