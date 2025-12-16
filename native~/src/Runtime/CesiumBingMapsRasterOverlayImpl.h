#pragma once

#include "CesiumImpl.h"

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumBingMapsRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class BingMapsRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumBingMapsRasterOverlayImpl
    : public CesiumImpl<CesiumBingMapsRasterOverlayImpl> {
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
  CesiumUtility::IntrusivePointer<CesiumRasterOverlays::BingMapsRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
