using Reinterop;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A raster overlay that can be used to debug tilesets by shading each tile with a random color.
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumPolygonRasterOverlayImpl", "CesiumPolygonRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Polygon Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumPolygonRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        [Tooltip("The polygons to rasterize for this overlay.")]
        private List<CesiumCartographicPolygon> _polygons;

        public List<CesiumCartographicPolygon> polygons
        {
            get => this._polygons;
            set
            {
                this._polygons = value;
                this.Refresh();
            }
        }

        [SerializeField]
        [Tooltip("Whether to invert the selection specified by the polygons." +
            "\n\n" +
            "If this is true, only the areas outside of all the polygons will be rasterized.")]

        private bool _invertSelection = false;

        public bool invertSelection
        {
            get => this._invertSelection;
            set
            {
                this._invertSelection = value;
                this.Refresh();
            }
        }

        [SerializeField]
        [Tooltip("Whether tiles that fall entirely within the rasterized selection should be " +
            "excluded from loading and rendering. For better performance, this should be enabled " +
            "when this overlay will be used for clipping. But when this overlay is used for other effects," +
            " this option should be disabled to avoid missing tiles." +
            "\n\n" +
            "Note that if InvertSelection is true, this will cull tiles that are outside of all the polygons. " +
            "If it is false, this will cull tiles that are completely inside at least one polygon.")]
        private bool _excludeSelectedTiles = true;

        public bool excludeSelectedTiles
        {
            get => this._excludeSelectedTiles;
            set
            {
                this._excludeSelectedTiles = value;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);
        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
