using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumIonRasterOverlay))]
    public class CesiumIonRasterOverlayEditor : Editor
    {
        private CesiumIonRasterOverlay _ionOverlay;
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private void OnEnable()
        {
            this._ionOverlay = (CesiumIonRasterOverlay)target;
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                        target,
                                                        typeof(CesiumRasterOverlayEditor));
        }

        private void OnDisable()
        {
            if(this._rasterOverlayEditor != null)
            {
                DestroyImmediate(this._rasterOverlayEditor);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawTroubleshootButton();
            EditorGUILayout.Space(5);

            if(this._rasterOverlayEditor != null)
            {
               this._rasterOverlayEditor.OnInspectorGUI();
            }
        }

        private void DrawTroubleshootButton()
        {
            GUIContent troubleshootTokenContent = new GUIContent(
               "Troubleshoot Token",
               "Check if the Cesium ion token used to access this raster overlay is working " +
               "correctly, and fix it if necessary.");
            if (GUILayout.Button(troubleshootTokenContent))
            {
                IonTokenTroubleshootingWindow.ShowWindow(this._ionOverlay, false);
            }
        }
    }
}