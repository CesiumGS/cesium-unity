using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumAzureMapsRasterOverlay))]
    public class CesiumAzureMapsRasterOverlayEditor : Editor
    {
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _azureMapsKey;
        private SerializedProperty _mapStyle;

        private void OnEnable()
        {
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                     this.target,
                                                     typeof(CesiumRasterOverlayEditor));

            this._azureMapsKey = this.serializedObject.FindProperty("_azureMapsKey");
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
            this._rasterOverlayEditor?.DrawInspectorButtons();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            this.DrawAzureMapsProperties();
            EditorGUILayout.Space(5);
            this.DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawAzureMapsProperties()
        {
            GUIContent azureMapsKeyContent = new GUIContent(
                "Azure Maps Key",
                "The Azure Maps API key to use.");
            EditorGUILayout.DelayedTextField(this._azureMapsKey, azureMapsKeyContent);

            GUIContent mapStyleContent = new GUIContent(
                "Map Style",
                "The map style to use.");
            EditorGUILayout.PropertyField(this._mapStyle, mapStyleContent);
        }

        private void DrawRasterOverlayProperties() => this._rasterOverlayEditor?.OnInspectorGUI();
    }
}
