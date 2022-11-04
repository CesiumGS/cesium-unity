using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumTileMapServiceRasterOverlay))]
    public class CesiumTileMapServiceRasterOverlayEditor : Editor
    {
        private CesiumTileMapServiceRasterOverlay _tileMapServiceOverlay;
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _url;
        private SerializedProperty _specifyZoomLevels;
        private SerializedProperty _minimumLevel;
        private SerializedProperty _maximumLevel;

        private void OnEnable()
        {
            this._tileMapServiceOverlay =
                (CesiumTileMapServiceRasterOverlay)this.target;
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                    this.target,
                                                    typeof(CesiumRasterOverlayEditor));

            this._url = this.serializedObject.FindProperty("_url");
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
            DrawTileMapServiceProperties();
            EditorGUILayout.Space(5);
            DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawTileMapServiceProperties()
        {
            GUIContent urlContent = new GUIContent(
               "URL",
               "The base URL of the Tile Map Service (TMS).");
            EditorGUILayout.DelayedTextField(this._url, urlContent);

            GUIContent specifyZoomLevelsContent = new GUIContent(
                "Specify Zoom Levels",
                "Set true to directly specify minimum and maximum zoom levels " +
                "available from the server, or false to automatically determine " +
                "the minimum and maximum zoom levels from the server's " +
                "tilemapresource.xml file.");
            EditorGUILayout.PropertyField(this._specifyZoomLevels, specifyZoomLevelsContent);

            EditorGUI.BeginDisabledGroup(!this._specifyZoomLevels.boolValue);
            GUIContent minimumLevelContent = new GUIContent(
                "Minimum Level",
                "Minimum zoom level.");
            EditorGUILayout.PropertyField(this._minimumLevel, minimumLevelContent);

            GUIContent maximumLevelContent = new GUIContent(
                "Maximum Level",
                "Maximum zoom level.");
            EditorGUILayout.PropertyField(this._maximumLevel, maximumLevelContent);
            EditorGUI.EndDisabledGroup();
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
