using Reinterop;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A raster overlay that rasterizes polygons and drapes them over the tileset.
    /// This is useful for clipping out parts of a tileset or applying specific material
    /// effects in an area.
    /// </summary>
    /// <remarks>
    /// Polygon raster overlays will only work in Unity 2022.2 or later.
    /// </remarks>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumPolygonRasterOverlayImpl", "CesiumPolygonRasterOverlayImpl.h")]
#if UNITY_2022_2_OR_NEWER
    [AddComponentMenu("Cesium/Cesium Polygon Raster Overlay")]
#else
    [AddComponentMenu("")]
#endif
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumPolygonRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private List<CesiumCartographicPolygon> _polygons;

        /// <summary>
        /// The polygons to rasterize for this overlay.
        /// </summary>
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
        private bool _invertSelection = false;

        /// <summary>
        /// Whether to invert the selection specified by the polygons.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this setting is false, the areas inside the polygons are rasterized and therefore 
        /// hidden from the rest of the tileset. In other words, they appear to cut holes in the
        /// tileset.
        /// </para>
        /// <para>
        /// When this setting is true, the areas outside of all the polygons will be rasterized instead.
        /// This will hide everything except for the areas inside the polygons.
        /// </para>
        /// </remarks>
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
        private bool _excludeSelectedTiles = true;

        /// <summary>
        /// Whether tiles that fall entirely within the rasterized selection should be 
        /// excluded from loading and rendering.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For better performance, this should be enabled when this overlay will be used for clipping.
        /// But when this overlay is used for other effects, this option should be disabled to avoid 
        /// missing tiles.
        /// </para>
        /// <para>
        /// Note that if <see cref="invertSelection"/> is true, this will cull tiles that are outside 
        /// of all the polygons. If it is false, this will cull tiles that are completely inside at 
        /// least one polygon.
        /// </para>
        /// </remarks>
        public bool excludeSelectedTiles
        {
            get => this._excludeSelectedTiles;
            set
            {
                this._excludeSelectedTiles = value;
                this.Refresh();
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            this.materialKey = "Clipping";
        }
#endif

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);
        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
