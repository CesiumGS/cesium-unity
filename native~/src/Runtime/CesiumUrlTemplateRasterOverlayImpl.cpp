#include "CesiumUrlTemplateRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumAsync/IAssetAccessor.h>
#include <CesiumGeometry/Rectangle.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumGeospatial/GlobeRectangle.h>
#include <CesiumRasterOverlays/UrlTemplateRasterOverlay.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumEllipsoid.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumUrlTemplateRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumUrlTemplateRasterOverlayProjection.h>
#include <DotNet/CesiumForUnity/HeaderEntry.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/GameObject.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumUrlTemplateRasterOverlayImpl::CesiumUrlTemplateRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumUrlTemplateRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumUrlTemplateRasterOverlayImpl::~CesiumUrlTemplateRasterOverlayImpl() {}

void CesiumUrlTemplateRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumUrlTemplateRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  DotNet::CesiumForUnity::CesiumGeoreference georeferenceComponent =
      tileset.gameObject()
          .GetComponentInParent<DotNet::CesiumForUnity::CesiumGeoreference>();
  if (georeferenceComponent == nullptr) {
    return;
  }

  const CesiumGeospatial::Ellipsoid& ellipsoid =
      georeferenceComponent.ellipsoid().NativeImplementation().GetEllipsoid();

  if (System::String::IsNullOrEmpty(overlay.templateUrl())) {
    // Don't create an overlay with an empty URL.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  UrlTemplateRasterOverlayOptions overlayOptions;
  if (overlay.maximumLevel() > overlay.minimumLevel()) {
    overlayOptions.minimumLevel = overlay.minimumLevel();
    overlayOptions.maximumLevel = overlay.maximumLevel();
  }

  if (overlay.projection() ==
      ::DotNet::CesiumForUnity::CesiumUrlTemplateRasterOverlayProjection::
          WebMercator) {
    overlayOptions.projection =
        CesiumGeospatial::WebMercatorProjection(ellipsoid);
  } else {
    overlayOptions.projection =
        CesiumGeospatial::GeographicProjection(ellipsoid);
  }

  overlayOptions.tileWidth = overlay.tileWidth();
  overlayOptions.tileHeight = overlay.tileHeight();

  if (overlay.specifyTilingScheme()) {
    CesiumGeospatial::GlobeRectangle globeRectangle =
        CesiumGeospatial::GlobeRectangle::fromDegrees(
            overlay.rectangleWest(),
            overlay.rectangleSouth(),
            overlay.rectangleEast(),
            overlay.rectangleNorth());
    CesiumGeometry::Rectangle coverageRectangle =
        CesiumGeospatial::projectRectangleSimple(
            *overlayOptions.projection,
            globeRectangle);
    overlayOptions.coverageRectangle = coverageRectangle;
    overlayOptions.tilingScheme = CesiumGeometry::QuadtreeTilingScheme(
        coverageRectangle,
        overlay.rootTilesX(),
        overlay.rootTilesY());
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  std::vector<CesiumAsync::IAssetAccessor::THeader> headers;
  if (overlay.requestHeaders() != nullptr) {
    headers.reserve(overlay.requestHeaders().Count());
    for (int i = 0; i < overlay.requestHeaders().Count(); i++) {
      // Skip blank headers that might come up while editing in the inspector.
      if (overlay.requestHeaders()[i].Name().Length() == 0 ||
          overlay.requestHeaders()[i].Value().Length() == 0) {
        continue;
      }
      headers.emplace_back(
          overlay.requestHeaders()[i].Name().ToStlString(),
          overlay.requestHeaders()[i].Value().ToStlString());
    }
  }

  this->_pOverlay = new UrlTemplateRasterOverlay(
      overlay.materialKey().ToStlString(),
      overlay.templateUrl().ToStlString(),
      headers,
      overlayOptions,
      options);

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumUrlTemplateRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumUrlTemplateRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay == nullptr)
    return;

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  CesiumUtility::IntrusivePointer<CesiumRasterOverlays::RasterOverlay>
      pOverlay = this->_pOverlay.get();
  pTileset->getOverlays().remove(pOverlay);
  this->_pOverlay = nullptr;
}

} // namespace CesiumForUnityNative
