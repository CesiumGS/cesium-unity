#include "CesiumWebMapTileServiceRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumRasterOverlays/WebMapTileServiceRasterOverlay.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumWebMapTileServiceRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumWebMapTileServiceRasterOverlayProjection.h>
#include <DotNet/System/Collections/Generic/List1.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
using namespace DotNet;
using namespace DotNet::CesiumForUnity;
using namespace DotNet::System::Collections::Generic;

namespace CesiumForUnityNative {

CesiumWebMapTileServiceRasterOverlayImpl::
    CesiumWebMapTileServiceRasterOverlayImpl(
        const DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay&
            overlay)
    : _pOverlay(nullptr) {}

CesiumWebMapTileServiceRasterOverlayImpl::
    ~CesiumWebMapTileServiceRasterOverlayImpl() {}

void CesiumWebMapTileServiceRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay&
        overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  if (System::String::IsNullOrEmpty(overlay.baseUrl())) {
    // Don't create an overlay with an empty base URL.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset) {
    // Tileset not valid, cannot add overlay.
    return;
  }

  const CesiumGeospatial::Ellipsoid& ellipsoid = pTileset->getEllipsoid();

  WebMapTileServiceRasterOverlayOptions wmtsOptions;
  wmtsOptions.format = overlay.format().ToStlString();
  wmtsOptions.layer = overlay.layer().ToStlString();
  wmtsOptions.style = overlay.style().ToStlString();
  wmtsOptions.tileMatrixSetID = overlay.tileMatrixSetID().ToStlString();

  if (overlay.specifyZoomLevels() &&
      overlay.maximumLevel() > overlay.minimumLevel()) {
    wmtsOptions.minimumLevel = overlay.minimumLevel();
    wmtsOptions.maximumLevel = overlay.maximumLevel();
  }

  wmtsOptions.tileWidth = overlay.tileWidth();
  wmtsOptions.tileHeight = overlay.tileHeight();

  if (overlay.projection() ==
      CesiumWebMapTileServiceRasterOverlayProjection::Geographic) {
    wmtsOptions.projection = CesiumGeospatial::GeographicProjection(ellipsoid);
  } else {
    wmtsOptions.projection = CesiumGeospatial::WebMercatorProjection(ellipsoid);
  }

  if (overlay.specifyTilingScheme()) {
    CesiumGeospatial::GlobeRectangle globeRectangle =
        CesiumGeospatial::GlobeRectangle::fromDegrees(
            overlay.rectangleWest(),
            overlay.rectangleSouth(),
            overlay.rectangleEast(),
            overlay.rectangleNorth());
    CesiumGeometry::Rectangle coverageRectangle =
        CesiumGeospatial::projectRectangleSimple(
            *wmtsOptions.projection,
            globeRectangle);
    wmtsOptions.coverageRectangle = coverageRectangle;
    wmtsOptions.tilingScheme = CesiumGeometry::QuadtreeTilingScheme(
        coverageRectangle,
        overlay.rootTilesX(),
        overlay.rootTilesY());
  }

  if (overlay.specifyTileMatrixSetLabels()) {
    List1<DotNet::System::String> unityLabels = overlay.tileMatrixSetLabels();
    if (unityLabels.Count() > 0) {
      std::vector<std::string> labels(unityLabels.Count());
      for (int i = 0; i < unityLabels.Count(); i++) {
        DotNet::System::String unityLabel = unityLabels[i];
        labels[i] = unityLabel.ToStlString();
      }
      wmtsOptions.tileMatrixLabels = labels;
    }
  } else {
    if (!DotNet::System::String::IsNullOrEmpty(
            overlay.tileMatrixSetLabelPrefix())) {
      std::string prefix = overlay.tileMatrixSetLabelPrefix().ToStlString();
      std::vector<std::string> labels(26);
      for (size_t level = 0; level <= 25; ++level) {
        labels.emplace_back(prefix + std::to_string(level));
      }
      wmtsOptions.tileMatrixLabels = labels;
    }
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  this->_pOverlay = new WebMapTileServiceRasterOverlay(
      overlay.materialKey().ToStlString(),
      overlay.baseUrl().ToStlString(),
      std::vector<CesiumAsync::IAssetAccessor::THeader>(),
      wmtsOptions,
      options);

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumWebMapTileServiceRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay&
        overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay == nullptr)
    return;

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  pTileset->getOverlays().remove(this->_pOverlay);
  this->_pOverlay = nullptr;
}

} // namespace CesiumForUnityNative
