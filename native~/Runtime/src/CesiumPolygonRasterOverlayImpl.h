#pragma once

#include <CesiumUtility/IntrusivePointer.h>
#include <CesiumUtility/ReferenceCounted.h>

#include <DotNet/System/Collections/Generic/List1.h>

#include <memory>

namespace DotNet::CesiumForUnity {
class Cesium3DTileset;
class CesiumPolygonRasterOverlay;
} // namespace DotNet::CesiumForUnity

namespace Cesium3DTilesSelection {
class RasterizedPolygonsTileExcluder;
}

namespace CesiumRasterOverlays {
class RasterizedPolygonsOverlay;
}

namespace CesiumGeospatial {
class CartographicPolygon;
}

namespace CesiumForUnityNative {

class CesiumPolygonRasterOverlayImpl
    : public CesiumUtility::ReferenceCountedThreadSafe<
          CesiumPolygonRasterOverlayImpl> {
public:
  CesiumPolygonRasterOverlayImpl(
      const DotNet::CesiumForUnity::CesiumPolygonRasterOverlay& overlay);
  ~CesiumPolygonRasterOverlayImpl();

  void AddToTileset(
      const ::DotNet::CesiumForUnity::CesiumPolygonRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);
  void RemoveFromTileset(
      const ::DotNet::CesiumForUnity::CesiumPolygonRasterOverlay& overlay,
      const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset);

private:
  static CesiumGeospatial::CartographicPolygon CreateCartographicPolygon(
      DotNet::System::Collections::Generic::List1<
          DotNet::Unity::Mathematics::double2> cartographicPoints);

  CesiumUtility::IntrusivePointer<
      CesiumRasterOverlays::RasterizedPolygonsOverlay>
      _pOverlay;
  std::shared_ptr<Cesium3DTilesSelection::RasterizedPolygonsTileExcluder>
      _pExcluder;
};

} // namespace CesiumForUnityNative
