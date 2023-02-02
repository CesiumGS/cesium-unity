#pragma once

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumBingMapsRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace Cesium3DTilesSelection {
class BingMapsRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumBingMapsRasterOverlayImpl {
public:
  CesiumBingMapsRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumBingMapsRasterOverlay& overlay);
  ~CesiumBingMapsRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumBingMapsRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumBingMapsRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<Cesium3DTilesSelection::BingMapsRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
