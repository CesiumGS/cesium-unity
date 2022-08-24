#pragma once

namespace DotNet::CesiumForUnity {
class CesiumIonRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumIonRasterOverlayImpl {
public:
  CesiumIonRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay);
  void JustBeforeDelete(
      const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay);
  void RecreateRasterOverlay(
      const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay);
};

} // namespace CesiumForUnityNative
