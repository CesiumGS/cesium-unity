using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A raster overlay that can be used to debug tilesets by shading each tile with a random color.
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumDebugColorizeTilesRasterOverlayImpl",
        "CesiumDebugColorizeTilesRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Debug Colorize Tiles Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumDebugColorizeTilesRasterOverlay : CesiumRasterOverlay
    {   
        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);
        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
