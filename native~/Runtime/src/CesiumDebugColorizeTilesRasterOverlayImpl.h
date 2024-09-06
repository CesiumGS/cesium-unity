#pragma once

#include <CesiumUtility/IntrusivePointer.h>
#include <CesiumUtility/ReferenceCounted.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumDebugColorizeTilesRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class DebugColorizeTilesRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumDebugColorizeTilesRasterOverlayImpl
    : public CesiumUtility::ReferenceCountedThreadSafe<
          CesiumDebugColorizeTilesRasterOverlayImpl> {
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
      CesiumRasterOverlays::DebugColorizeTilesRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
