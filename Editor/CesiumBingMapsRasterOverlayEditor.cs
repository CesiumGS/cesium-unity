using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumBingMapsRasterOverlay))]
    public class CesiumBingMapsRasterOverlayEditor : Editor
    {
        private CesiumBingMapsRasterOverlay _bingMapsOverlay;
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _bingMapsKey;
        private SerializedProperty _mapStyle;

        private void OnEnable()
        {
            this._bingMapsOverlay = (CesiumBingMapsRasterOverlay)this.target;
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                     this.target,
                                                     typeof(CesiumRasterOverlayEditor));

            this._bingMapsKey = this.serializedObject.FindProperty("_bingMapsKey");
            this._mapStyle = this.serializedObject.FindProperty("_mapStyle");
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
            DrawBingMapsProperties();
            EditorGUILayout.Space(5);
            DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawBingMapsProperties()
        {
            GUIContent bingMapsKeyContent = new GUIContent(
                "Bing Maps Key",
                "The Bing Maps API key to use.");
            EditorGUILayout.DelayedTextField(this._bingMapsKey, bingMapsKeyContent);

            GUIContent mapStyleContent = new GUIContent(
                "Map Style",
                "The map style to use.");
            EditorGUILayout.PropertyField(this._mapStyle, mapStyleContent);
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
