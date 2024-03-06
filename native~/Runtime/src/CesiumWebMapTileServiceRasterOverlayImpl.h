#pragma once

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumWebMapTileServiceRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class WebMapTileServiceRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumWebMapTileServiceRasterOverlayImpl {
public:
  CesiumWebMapTileServiceRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay& overlay);
  ~CesiumWebMapTileServiceRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<
      CesiumRasterOverlays::WebMapTileServiceRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
