#pragma once

#include "CesiumImpl.h"

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumUrlTemplateRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class UrlTemplateRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumUrlTemplateRasterOverlayImpl
    : public CesiumImpl<CesiumUrlTemplateRasterOverlayImpl> {
public:
  CesiumUrlTemplateRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumUrlTemplateRasterOverlay& overlay);
  ~CesiumUrlTemplateRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumUrlTemplateRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumUrlTemplateRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<
      CesiumRasterOverlays::UrlTemplateRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
