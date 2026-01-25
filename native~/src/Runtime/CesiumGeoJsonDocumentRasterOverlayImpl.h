#pragma once

#include "CesiumImpl.h"

#include <CesiumUtility/IntrusivePointer.h>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumGeoJsonDocumentRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace CesiumRasterOverlays {
class GeoJsonDocumentRasterOverlay;
}

namespace CesiumForUnityNative {

class CesiumGeoJsonDocumentRasterOverlayImpl
    : public CesiumImpl<CesiumGeoJsonDocumentRasterOverlayImpl> {
public:
  CesiumGeoJsonDocumentRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumGeoJsonDocumentRasterOverlay& overlay);
  ~CesiumGeoJsonDocumentRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumGeoJsonDocumentRasterOverlay&
          overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumGeoJsonDocumentRasterOverlay&
          overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  CesiumUtility::IntrusivePointer<
      CesiumRasterOverlays::GeoJsonDocumentRasterOverlay>
      _pOverlay;
};

} // namespace CesiumForUnityNative
