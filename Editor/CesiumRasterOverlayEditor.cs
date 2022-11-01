using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumRasterOverlay), true)]
    [CanEditMultipleObjects]
    public class CesiumRasterOverlayEditor : Editor
    {
        private CesiumRasterOverlay _overlay;

        internal CesiumRasterOverlay overlay
        {
            get => this._overlay;
            set
            {
                this._overlay = value;
            }
        }

        internal bool showCreditsOnScreen
        {
            get => this._overlay.showCreditsOnScreen;
            set
            {
                if (this._overlay.showCreditsOnScreen != value)
                {
                    this._overlay.showCreditsOnScreen = value;
                }
            }
        }

        internal float maximumScreenSpaceError
        {
            get => this._overlay.maximumScreenSpaceError;
            set
            {
                if (this._overlay.maximumScreenSpaceError != value)
                {
                    this._overlay.maximumScreenSpaceError = value;
                }
            }
        }

        internal int maximumTextureSize
        {
            get => this._overlay.maximumTextureSize;
            set
            {
                if (this._overlay.maximumTextureSize != value)
                {
                    this._overlay.maximumTextureSize = value;
                }
            }
        }

        internal int maximumSimultaneousTileLoads
        {
            get => this._overlay.maximumSimultaneousTileLoads;
            set
            {
                if (this._overlay.maximumSimultaneousTileLoads != value)
                {
                    this._overlay.maximumSimultaneousTileLoads = value;
                }
            }
        }

        internal long subTileCacheBytes
        {
            get => this._overlay.subTileCacheBytes;
            set
            {
                if (this._overlay.subTileCacheBytes != value)
                {
                    this._overlay.subTileCacheBytes = value;
                }
            }
        }

        private void OnEnable()
        {
            this.overlay = (CesiumRasterOverlay)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;

            GUIContent showCreditsOnScreenContent = new GUIContent(
                "Show Credits On Screen",
                "Whether or not to show credits of this raster overlay on screen.");
            this.showCreditsOnScreen =
                EditorGUILayout.Toggle(showCreditsOnScreenContent, this.showCreditsOnScreen);

            GUIContent maximumScreenSpaceErrorContent = new GUIContent(
                "Maximum Screen Space Error",
                "The maximum number of pixels of error when rendering this overlay. " +
                "This is used to select an appropriate level - of - detail. " +
                "\n\n" +
                "When this property has its default value, 2.0, it means that raster " +
                "overlay images will be sized so that, when zoomed in closest, a single " +
                "pixel in the raster overlay maps to approximately 2x2 pixels on the " +
                "screen.");
            this.maximumScreenSpaceError = EditorGUILayout.FloatField(
                maximumScreenSpaceErrorContent,
                this.maximumScreenSpaceError);

            GUIContent maximumTextureSizeContent = new GUIContent(
                "Maximum Texture Size",
                "The maximum texel size of raster overlay textures, in either direction." +
                "\n\n" +
                "Images created by this overlay will be no more than this number of " +
                "texels in either direction. This may result in reduced raster overlay " +
                "detail in some cases.");
            this.maximumTextureSize = EditorGUILayout.IntField(
                maximumTextureSizeContent,
                this.maximumTextureSize);

            GUIContent maximumSimultaneousTileLoadsContent = new GUIContent(
                "Maximum Simultaneous Tile Loads",
                "The maximum number of overlay tiles that may simultaneously be in " +
                "the process of loading.");
            this.maximumSimultaneousTileLoads = EditorGUILayout.IntField(
                maximumSimultaneousTileLoadsContent,
                this.maximumSimultaneousTileLoads);

            GUIContent subTileCacheBytesContent = new GUIContent(
                "Sub-Tile Cache Bytes",
                "The maximum number of bytes to use to cache sub-tiles in memory." +
                "\n\n" +
                "This is used by provider types, that have an underlying tiling " +
                "scheme that may not align with the tiling scheme of the geometry " +
                "tiles on which the raster overlay tiles are draped. Because a " +
                "single sub-tile may overlap multiple geometry tiles, it is useful " +
                "to cache loaded sub-tiles in memory in case they're needed again " +
                "soon. This property controls the maximum size of that cache.");
            this.subTileCacheBytes = EditorGUILayout.LongField(
                subTileCacheBytesContent,
                this.subTileCacheBytes);
        }
    }

}