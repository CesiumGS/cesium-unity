#pragma once

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumIonRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace Cesium3DTilesSelection {
class IonRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumIonRasterOverlayImpl {
public:
  CesiumIonRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay);
  ~CesiumIonRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<Cesium3DTilesSelection::IonRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
