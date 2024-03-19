using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumRasterOverlay))]
    public class CesiumRasterOverlayEditor : Editor
    {
        private const string _overlayPrefix = "_overlayTexture_";

        private Cesium3DTileset _tileset;
        private CesiumRasterOverlay _overlay;

        private SerializedProperty _showCreditsOnScreen;
        private SerializedProperty _materialKey;
        private SerializedProperty _maximumScreenSpaceError;
        private SerializedProperty _maximumTextureSize;
        private SerializedProperty _maximumSimultaneousTileLoads;
        private SerializedProperty _subTileCacheBytes;

        private int _materialCRC = 0;
        private string[] _materialKeys;
        private int _selectedMaterialKeyIndex;
        private bool _showMaterialKeyWarning;

        public bool drawShowCreditsOnScreen = true;
        public bool drawOverlayProperties = true;

        private void OnEnable()
        {
            this._overlay = (CesiumRasterOverlay)target;
            this._tileset = _overlay.gameObject.GetComponent<Cesium3DTileset>();

            this._materialKey = this.serializedObject.FindProperty("_materialKey");
            this._showCreditsOnScreen = this.serializedObject.FindProperty("_showCreditsOnScreen");
            this._maximumScreenSpaceError =
                this.serializedObject.FindProperty("_maximumScreenSpaceError");
            this._maximumTextureSize = this.serializedObject.FindProperty("_maximumTextureSize");
            this._maximumSimultaneousTileLoads =
                this.serializedObject.FindProperty("_maximumSimultaneousTileLoads");
            this._subTileCacheBytes = this.serializedObject.FindProperty("_subTileCacheBytes");

            this._materialKeys = new string[] { };
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawTilesetWarning();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;

            this.UpdateMaterialKeys();

            this.DrawMaterialKeyProperty();

            if (this.drawShowCreditsOnScreen)
            {
                this.DrawShowCreditsOnScreenProperty();
            }

            if (this.drawOverlayProperties)
            {
                this.DrawRasterOverlayProperties();
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawTilesetWarning()
        {
            if (this._tileset == null)
            {
                EditorGUILayout.HelpBox("CesiumRasterOverlay should be used in combination with a " +
                    "Cesium3DTileset component on this GameObject.", MessageType.Warning);
            }
        }

        private void UpdateMaterialKeys()
        {
            if (this._tileset != null)
            {
                Material material = this._tileset.opaqueMaterial;

                if (material == null)
                {
                    material = Resources.Load<Material>("CesiumDefaultTilesetMaterial");
                }

                if (material == null)
                {
                    Debug.LogError("Couldn't find default tileset material in Resources.");
                    return;
                }

                int materialCRC = material.ComputeCRC();
                if (this._materialCRC == materialCRC)
                {
                    return;
                }

                string[] propertyNames = material.GetTexturePropertyNames();
                List<string> materialKeys = new List<string>();

                bool foundMaterialKey = false;
                foreach (string name in propertyNames)
                {
                    if (name.StartsWith(_overlayPrefix))
                    {
                        string key = name.Substring(_overlayPrefix.Length);

                        if (this._materialKey.stringValue == key)
                        {
                            this._selectedMaterialKeyIndex = materialKeys.Count;
                            foundMaterialKey = true;
                            this._showMaterialKeyWarning = false;
                        }

                        materialKeys.Add(key);
                    }
                }

                if (!foundMaterialKey)
                {
                    this._showMaterialKeyWarning = true;
                    this._selectedMaterialKeyIndex = materialKeys.Count;
                    materialKeys.Add(this._materialKey.stringValue);
                }

                this._materialKeys = materialKeys.ToArray();
            }
        }


        private void DrawMaterialKeyProperty()
        {
            GUIContent materialKeyContent = new GUIContent(
                "Material Key",
                "The key to use to match this overlay to the corresponding parameters " +
                "in the tileset's material." +
                "\n\n" +
                "In the tileset's materials, an overlay requires parameters for its texture, " +
                "texture coordinate index, and translation and scale. Overlays must specify a " +
                "string key to match with the correct parameters. The format of these parameters " +
                "is as follows." +
                "\n\n" +
                "- Overlay Texture: _overlayTexture_KEY\n" +
                "- Overlay Texture Coordinate Index: _overlayTextureCoordinateIndex_KEY\n" +
                "- Overlay Translation and Scale: _overlayTranslationScale_KEY\n" +
                "\n\n" +
                "Material keys are useful for specifying the order of the raster overlays, or distinguishing " +
                "them for overlay-specific effects."
                );
            int selectedIndex =
                EditorGUILayout.Popup(materialKeyContent, this._selectedMaterialKeyIndex, this._materialKeys);

            if (this._showMaterialKeyWarning)
            {
                EditorGUILayout.HelpBox("The specified Material Key does not exist in the " +
                    "Cesium3DTileset's material.", MessageType.Warning);
            }

            if (selectedIndex == this._selectedMaterialKeyIndex ||
                selectedIndex < 0 || selectedIndex >= this._materialKeys.Length)
            {
                return;
            }

            this._materialKey.stringValue = this._materialKeys[selectedIndex];
            this._selectedMaterialKeyIndex = selectedIndex;

        }

        private void DrawShowCreditsOnScreenProperty()
        {
            GUIContent showCreditsOnScreenContent = new GUIContent(
                "Show Credits On Screen",
                "Whether or not to show credits of this raster overlay on screen.");
            EditorGUILayout.PropertyField(
                this._showCreditsOnScreen, showCreditsOnScreenContent);
        }

        private void DrawRasterOverlayProperties()
        {
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