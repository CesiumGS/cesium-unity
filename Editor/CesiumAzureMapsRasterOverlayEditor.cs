using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumAzureMapsRasterOverlay))]
    public class CesiumAzureMapsRasterOverlayEditor : Editor
    {
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _key;
        private SerializedProperty _apiVersion;
        private SerializedProperty _tilesetId;
        private SerializedProperty _language;
        private SerializedProperty _view;
        

        private void OnEnable()
        {
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                     this.target,
                                                     typeof(CesiumRasterOverlayEditor));

            this._key = this.serializedObject.FindProperty("_key");
            this._apiVersion = this.serializedObject.FindProperty("_apiVersion");
            this._tilesetId = this.serializedObject.FindProperty("_tilesetId");
            this._language = this.serializedObject.FindProperty("_language");
            this._view = this.serializedObject.FindProperty("_view");
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
            var keyContent = new GUIContent(
                "Key",
                "The Azure Maps key to use.");
            EditorGUILayout.DelayedTextField(this._key, keyContent);
            
            var apiVersionContent = new GUIContent(
                "API Version",
                "The Azure Maps API version to use.");
            EditorGUILayout.PropertyField(this._apiVersion, apiVersionContent);

            var tileSetIdContent = new GUIContent(
                "Tile Set ID",
                "The Azure tile set ID to use.");
            EditorGUILayout.PropertyField(this._tilesetId, tileSetIdContent);

            var languageContent = new GUIContent(
                "Language",
                "The language in which search results should be returned. This should be one"
                + " of the supported IETF language tags, case insensitive. When data in the"
                + " specified language is not available for a specific field, default language");
            EditorGUILayout.PropertyField(this._language, languageContent);

            var viewContent = new GUIContent(
                "View",
                "The View parameter (also called the \"user region\" parameter) allows"
                + " you to show the correct maps for a certain country/region for"
                + " geopolitically disputed regions.");
            EditorGUILayout.PropertyField(this._view, viewContent);
        }

        private void DrawRasterOverlayProperties() => this._rasterOverlayEditor?.OnInspectorGUI();
    }
}
