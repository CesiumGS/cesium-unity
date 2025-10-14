#pragma once

#include "CesiumImpl.h"

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumGoogleMapTilesRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class GoogleMapTilesRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumGoogleMapTilesRasterOverlayImpl
    : public CesiumImpl<CesiumGoogleMapTilesRasterOverlayImpl> {
public:
  CesiumGoogleMapTilesRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumGoogleMapTilesRasterOverlay& overlay);
  ~CesiumGoogleMapTilesRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumGoogleMapTilesRasterOverlay&
          overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumGoogleMapTilesRasterOverlay&
          overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<
      CesiumRasterOverlays::GoogleMapTilesRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
