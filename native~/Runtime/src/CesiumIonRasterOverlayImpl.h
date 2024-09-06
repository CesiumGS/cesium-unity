#pragma once

#include <CesiumUtility/IntrusivePointer.h>
#include <CesiumUtility/ReferenceCounted.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumIonRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class IonRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumIonRasterOverlayImpl
    : public CesiumUtility::ReferenceCountedThreadSafe<
          CesiumIonRasterOverlayImpl> {
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
  CesiumUtility::IntrusivePointer<CesiumRasterOverlays::IonRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
