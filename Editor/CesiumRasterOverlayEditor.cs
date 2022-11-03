using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumRasterOverlay))]
    public class CesiumRasterOverlayEditor : Editor
    {
        private CesiumRasterOverlay _overlay;

        private SerializedProperty _showCreditsOnScreen;
        private SerializedProperty _maximumScreenSpaceError;
        private SerializedProperty _maximumTextureSize;
        private SerializedProperty _maximumSimultaneousTileLoads;
        private SerializedProperty _subTileCacheBytes;

        private void OnEnable()
        {
            this._overlay = (CesiumRasterOverlay)target;

            this._showCreditsOnScreen = this.serializedObject.FindProperty("_showCreditsOnScreen");
            this._maximumScreenSpaceError =
                this.serializedObject.FindProperty("_maximumScreenSpaceError");
            this._maximumTextureSize = this.serializedObject.FindProperty("_maximumTextureSize");
            this._maximumSimultaneousTileLoads =
                this.serializedObject.FindProperty("_maximumSimultaneousTileLoads");
            this._subTileCacheBytes = this.serializedObject.FindProperty("_subTileCacheBytes");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            DrawRasterOverlayProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawRasterOverlayProperties()
        {
            GUIContent showCreditsOnScreenContent = new GUIContent(
                "Show Credits On Screen",
                "Whether or not to show credits of this raster overlay on screen.");
            EditorGUILayout.PropertyField(
                this._showCreditsOnScreen, showCreditsOnScreenContent);

            GUIContent maximumScreenSpaceErrorContent = new GUIContent(
                "Maximum Screen Space Error",
                "The maximum number of pixels of error when rendering this overlay. " +
                "This is used to select an appropriate level-of-detail. " +
                "\n\n" +
                "When this property has its default value, 2.0, it means that raster " +
                "overlay images will be sized so that, when zoomed in closest, a single " +
                "pixel in the raster overlay maps to approximately 2x2 pixels on the " +
                "screen.");
            EditorGUILayout.PropertyField(
                this._maximumScreenSpaceError, maximumScreenSpaceErrorContent);

            GUIContent maximumTextureSizeContent = new GUIContent(
                "Maximum Texture Size",
                "The maximum texel size of raster overlay textures, in either direction." +
                "\n\n" +
                "Images created by this overlay will be no more than this number of " +
                "texels in either direction. This may result in reduced raster overlay " +
                "detail in some cases.");
            EditorGUILayout.PropertyField(
                this._maximumTextureSize, maximumTextureSizeContent);

            GUIContent maximumSimultaneousTileLoadsContent = new GUIContent(
                "Maximum Simultaneous Tile Loads",
                "The maximum number of overlay tiles that may simultaneously be in " +
                "the process of loading.");
            EditorGUILayout.PropertyField(
                this._maximumSimultaneousTileLoads, maximumSimultaneousTileLoadsContent);

            GUIContent subTileCacheBytesContent = new GUIContent(
                "Sub Tile Cache Bytes",
                "The maximum number of bytes to use to cache sub-tiles in memory." +
                "\n\n" +
                "This is used by provider types, that have an underlying tiling " +
                "scheme that may not align with the tiling scheme of the geometry " +
                "tiles on which the raster overlay tiles are draped. Because a " +
                "single sub-tile may overlap multiple geometry tiles, it is useful " +
                "to cache loaded sub-tiles in memory in case they're needed again " +
                "soon. This property controls the maximum size of that cache.");
            EditorGUILayout.PropertyField(
                this._subTileCacheBytes, subTileCacheBytesContent);
        }
    }
}