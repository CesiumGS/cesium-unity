using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumDebugColorizeTilesRasterOverlay))]
    public class CesiumDebugColorizeTilesRasterOverlayEditor : Editor
    {
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private void OnEnable()
        {
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                     this.target,
                                                     typeof(CesiumRasterOverlayEditor));

            // The other CesiumRasterOverlay options don't apply to
            // CesiumDebugColorizeTilesRasterOverlay, so don't show them here.

            this._rasterOverlayEditor.drawShowCreditsOnScreen = false;
            this._rasterOverlayEditor.drawOverlayProperties = false;
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
            if (this._rasterOverlayEditor != null)
            {
                this._rasterOverlayEditor.OnInspectorGUI();
            }
        }
    }
}
