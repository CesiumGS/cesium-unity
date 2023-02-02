#pragma once

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumWebMapServiceRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace Cesium3DTilesSelection {
class WebMapServiceRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumWebMapServiceRasterOverlayImpl {
public:
  CesiumWebMapServiceRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumWebMapServiceRasterOverlay& overlay);
  ~CesiumWebMapServiceRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumWebMapServiceRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumWebMapServiceRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<
      Cesium3DTilesSelection::WebMapServiceRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
