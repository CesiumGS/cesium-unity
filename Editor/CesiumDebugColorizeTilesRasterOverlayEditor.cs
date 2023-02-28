using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumDebugColorizeTilesRasterOverlay))]
    public class CesiumDebugColorizeTilesRasterOverlayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // The CesiumRasterOverlay options don't apply to
            // CesiumDebugColorizeTilesRasterOverlay, so don't show them here.
        }
    }
}
