#pragma once

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumDebugColorizeTilesRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace Cesium3DTilesSelection {
class DebugColorizeTilesRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumDebugColorizeTilesRasterOverlayImpl {
public:
  CesiumDebugColorizeTilesRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumDebugColorizeTilesRasterOverlay&
          overlay);
  ~CesiumDebugColorizeTilesRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumDebugColorizeTilesRasterOverlay&
          overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumDebugColorizeTilesRasterOverlay&
          overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<
      Cesium3DTilesSelection::DebugColorizeTilesRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
