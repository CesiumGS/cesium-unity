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
    public partial class CesiumDebugColorizeTilesRasterOverlay : CesiumRasterOverlay
    {
        protected override partial void AddToTileset(Cesium3DTileset tileset);
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
