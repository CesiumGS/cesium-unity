namespace Cesium3DTilesSelection {
class RasterOverlayOptions;
}

namespace DotNet::CesiumForUnity {
class CesiumRasterOverlay;
class CesiumRasterOverlayOptions;
}

namespace CesiumForUnityNative {
class CesiumRasterOverlayUtility {
public:
  static Cesium3DTilesSelection::RasterOverlayOptions GetOverlayOptions(
      const DotNet::CesiumForUnity::CesiumRasterOverlay& overlay);
};
} // namespace CesiumForUnityNative
