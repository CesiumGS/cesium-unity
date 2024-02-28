using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumPolygonRasterOverlay))]
    public class CesiumPolygonRasterOverlayEditor : Editor
    {
        private CesiumRasterOverlayEditor _rasterOverlayEditor;

        private SerializedProperty _polygons;
        private SerializedProperty _invertSelection;
        private SerializedProperty _excludeSelectedTiles;

        private void OnEnable()
        {
            this._rasterOverlayEditor =
                (CesiumRasterOverlayEditor)Editor.CreateEditor(
                                                     this.target,
                                                     typeof(CesiumRasterOverlayEditor));

            this._rasterOverlayEditor.drawShowCreditsOnScreen = false;

            this._polygons = this.serializedObject.FindProperty("_polygons");
            this._invertSelection = this.serializedObject.FindProperty("_invertSelection");
            this._excludeSelectedTiles = this.serializedObject.FindProperty("_excludeSelectedTiles");
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

#if !SUPPORTS_SPLINES
#if UNITY_2022_2_OR_NEWER
            EditorGUILayout.HelpBox("CesiumCartographicPolygon requires the Splines package, which is currently " +
             "not installed in the project. CesiumPolygonRasterOverlay will not have any effect until the Splines " +
             "package is installed through the Package Manager.", MessageType.Error);
#else
            EditorGUILayout.HelpBox("CesiumCartographicPolygon requires the Splines package, which is not available " +
                "in this version of Unity. This means CesiumPolygonRasterOverlay will not have any effect.", MessageType.Error);
#endif
#endif

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            this.DrawPolygonRasterOverlayProperties();
            EditorGUILayout.Space(5);
            this.DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawPolygonRasterOverlayProperties()
        {
            GUIContent polygonsContent = new GUIContent(
                "Polygons",
                "The polygons to rasterize for this overlay.");
            EditorGUILayout.PropertyField(this._polygons, polygonsContent);

            GUIContent invertSelectionContent = new GUIContent(
                "Invert Selection",
                "Whether to invert the selection specified by the polygons." +
                "\n\n" +
                "When this setting is false, the areas inside the polygons are rasterized and therefore " +
                "hidden from the rest of the tileset. In other words, they appear to cut holes in the " +
                "tileset." +
                "\n\n" +
                "When this setting is true, the areas outside of all the polygons will be rasterized " +
                "instead. This will hide everything except for the areas inside the polygons.");
            EditorGUILayout.PropertyField(this._invertSelection, invertSelectionContent);

            GUIContent excludeSelectedTilesContent = new GUIContent(
                "Exclude Selected Tiles",
                "Whether tiles that fall entirely within the rasterized selection should be " +
                "excluded from loading and rendering." +
                "\n\n" +
                "For better performance, this should be enabled when this overlay will be used for clipping. " +
                "But when this overlay is used for other effects, this option should be disabled to avoid " +
                "missing tiles." +
                "\n\n" +
                "Note that if Invert Selection is true, this will cull tiles that are outside of all the " +
                "polygons. If it is false, this will cull tiles that are completely inside at least one " +
                "polygon.");
            EditorGUILayout.PropertyField(this._excludeSelectedTiles, excludeSelectedTilesContent);
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
