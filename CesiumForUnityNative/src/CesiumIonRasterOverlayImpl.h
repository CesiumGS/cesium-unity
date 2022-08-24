#pragma once

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
  void JustBeforeDelete(
      const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay);
  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  Cesium3DTilesSelection::IonRasterOverlay* _pOverlay;
};

} // namespace CesiumForUnityNative
