#pragma once

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumTileMapServiceRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class TileMapServiceRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumTileMapServiceRasterOverlayImpl {
public:
  CesiumTileMapServiceRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumTileMapServiceRasterOverlay& overlay);
  ~CesiumTileMapServiceRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumTileMapServiceRasterOverlay&
          overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumTileMapServiceRasterOverlay&
          overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<
      CesiumRasterOverlays::TileMapServiceRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
