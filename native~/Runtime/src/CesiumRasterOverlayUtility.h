namespace CesiumRasterOverlays {
class RasterOverlayOptions;
}

namespace DotNet::CesiumForUnity {
class CesiumRasterOverlay;
class CesiumRasterOverlayOptions;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {
class CesiumRasterOverlayUtility {
public:
  static CesiumRasterOverlays::RasterOverlayOptions
  GetOverlayOptions(const DotNet::CesiumForUnity::CesiumRasterOverlay& overlay);
};
} // namespace CesiumForUnityNative
